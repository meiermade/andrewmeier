open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO
open Fake.IO.FileSystemOperators
open System
open System.IO
open System.Net
open System.Net.Http
open System.Net.Sockets
open System.Security.Cryptography
open System.Text.Json
open System.Threading
open System.Threading.Tasks

Environment.GetCommandLineArgs()
|> Array.tail
|> Array.toList
|> Context.FakeExecutionContext.Create false "build.fsx"
|> Context.Fake
|> Context.setExecutionContext

let srcDir = Path.getDirectory __SOURCE_DIRECTORY__
let rootDir = Path.getDirectory srcDir
let appDir = srcDir </> "App"
let outDir = appDir </> "out"
let wwwrootDir = outDir </> "wwwroot"
let hashedAssetExtensions =
    set [ ".css"; ".gif"; ".ico"; ".jpeg"; ".jpg"; ".js"; ".png"; ".svg"; ".webp"; ".woff"; ".woff2" ]

let toWebPath (rootDir:string) (filePath:string) =
    let relativePath = Path.GetRelativePath(rootDir, filePath).Replace(Path.DirectorySeparatorChar, '/')
    "/" + relativePath

let fingerprintedFilePath (filePath:string) (hash:string) =
    let dir = Path.GetDirectoryName(filePath)
    let name = Path.GetFileNameWithoutExtension(filePath)
    let ext = Path.GetExtension(filePath)
    Path.Combine(dir, $"{name}.{hash}{ext}")

let hashFileContents (filePath:string) =
    use stream = File.OpenRead(filePath)
    use sha256 = SHA256.Create()
    sha256.ComputeHash(stream)
    |> Convert.ToHexString
    |> fun hash -> hash.ToLowerInvariant().Substring(0, 12)

let fingerprintAssets (rootDir:string) =
    let files =
        Directory.EnumerateFiles(rootDir, "*", SearchOption.AllDirectories)
        |> Seq.filter (fun path -> hashedAssetExtensions.Contains(Path.GetExtension(path).ToLowerInvariant()))
        |> Seq.sort
        |> Seq.toList

    let manifest =
        files
        |> Seq.map (fun path ->
            let hash = hashFileContents path
            let fingerprintedPath = fingerprintedFilePath path hash
            File.Copy(path, fingerprintedPath, true)
            toWebPath rootDir path, toWebPath rootDir fingerprintedPath)
        |> Map.ofSeq

    let manifestPath = Path.Combine(rootDir, "asset-manifest.json")
    let json = JsonSerializer.Serialize(manifest)
    File.WriteAllText(manifestPath, json)

let inline (==>!) x y = x ==> y |> ignore

let execEnv command workDir env args =
    CreateProcess.fromRawCommand command args
    |> CreateProcess.withWorkingDirectory workDir
    |> CreateProcess.withEnvironmentMap env
    |> CreateProcess.ensureExitCode
    |> Proc.start

let exec command workDir args =
    CreateProcess.fromRawCommand command args
    |> CreateProcess.withWorkingDirectory workDir
    |> CreateProcess.ensureExitCode
    |> Proc.start
    
let availableLocalPort () =
    use listener = new TcpListener(IPAddress.Loopback, 0)
    listener.Start()
    (listener.LocalEndpoint :?> IPEndPoint).Port

let waitForHttpHealth (name:string) (url:string) =
    use httpClient = new HttpClient()
    let mutable healthy = false
    let mutable attempt = 1

    while not healthy && attempt <= 30 do
        try
            use response = httpClient.GetAsync(url).GetAwaiter().GetResult()
            healthy <- response.IsSuccessStatusCode
        with _ ->
            ()

        if not healthy then Thread.Sleep 500
        attempt <- attempt + 1

    if not healthy then failwith $"{name} did not become healthy at {url}"

Target.create "StartDeps" <| fun _ ->
    Trace.trace "Starting dependencies (seq)"
    exec "docker-compose" rootDir ["up"; "-d"; "seq"] |> Task.WaitAll

Target.create "StartMockNotion" <| fun _ ->
    Trace.trace "Starting MockNotion"
    exec "docker-compose" rootDir ["up"; "-d"; "--build"; "mock-notion"] |> Task.WaitAll
    waitForHttpHealth "MockNotion" "http://localhost:5015/healthz"

Target.create "EnsureDevCert" <| fun _ ->
    Trace.trace "Ensuring trusted ASP.NET Core HTTPS development certificate"
    exec "dotnet" rootDir ["dev-certs"; "https"; "--trust"] |> Task.WaitAll

Target.create "Watch" <| fun _ ->
    let sqlitePath = rootDir </> ".data" </> "app.db"
    let serverUrl = $"https://localhost:{availableLocalPort ()}"
    Trace.trace $"Starting local server at {serverUrl}"

    let env =
        Map.ofList [
            "ASPNETCORE_ENVIRONMENT", "Development"
            "GOOGLE_ANALYTICS_MEASUREMENT_ID", "G-LOCAL"
            "NOTION_API_KEY", "mock-notion-token"
            "NOTION_ARTICLES_DATABASE_ID", "mock-articles"
            "NOTION_BASE_URL", "http://localhost:5015/v1"
            "SERVER_URL", serverUrl
            "SQLITE_PATH", sqlitePath
        ]
        |> EnvMap.ofMap

    let watchCss = exec "tailwindcss" appDir ["--input"; "./input.css"; "--output"; "./wwwroot/css/compiled.css"; "--watch"]
    let watchServer = execEnv "dotnet" appDir env ["watch"; "run"; "--no-restore"]
    Task.WaitAny(watchCss, watchServer) |> ignore

Target.create "BuildCss" <| fun _ ->
    let buildCss = exec "tailwindcss" appDir ["--input"; "./input.css"; "--output"; "./wwwroot/css/compiled.css"; "--minify"]
    buildCss.Wait()

Target.create "Test" <| fun _ ->
    let tests = exec "dotnet" rootDir ["run"; "--project"; "src/Tests/Tests.fsproj"]
    tests.Wait()

Target.create "Publish" <| fun _ ->
    Shell.cleanDir outDir
    let publish = exec "dotnet" appDir [
        "publish"
        "--output"; "./out"
        "--self-contained"; "false"
    ]
    publish.Wait()
    fingerprintAssets wwwrootDir

Target.create "Default" (fun _ -> Target.listAvailable())

"StartDeps" ==>! "StartMockNotion"
"StartMockNotion" ==>! "EnsureDevCert"
"EnsureDevCert" ==>! "Watch"

"BuildCss" ==>! "Publish"

Target.runOrDefaultWithArguments "Default"
