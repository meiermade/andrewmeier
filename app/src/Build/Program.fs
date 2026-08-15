open Fake.Core
open Fake.Core.TargetOperators
open Fake.IO
open Fake.IO.FileSystemOperators
open System
open System.IO
open System.Net
open System.Net.Sockets
open System.Security.Cryptography
open System.Text.Json
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

Target.create "StartDeps" <| fun _ ->
    Trace.trace "Starting dependencies (seq)"
    exec "docker-compose" rootDir ["up"; "-d"; "seq"] |> Task.WaitAll

Target.create "EnsureDevCert" <| fun _ ->
    Trace.trace "Ensuring trusted ASP.NET Core HTTPS development certificate"
    exec "dotnet" rootDir ["dev-certs"; "https"; "--trust"] |> Task.WaitAll

Target.create "Watch" <| fun _ ->
    let serverUrl = $"https://localhost:{availableLocalPort ()}"
    Trace.trace $"Starting local server at {serverUrl}"

    let env =
        Map.ofList [
            "ASPNETCORE_ENVIRONMENT", "Development"
            "GOOGLE_ANALYTICS_MEASUREMENT_ID", "G-LOCAL"
            "SERVER_URL", serverUrl
        ]
        |> EnvMap.ofMap

    exec "npm" appDir ["ci"; "--ignore-scripts"] |> _.Wait()
    let watchPrism = exec "npm" appDir ["run"; "build:prism"; "--"; "--watch"]
    let watchTelemetry = exec "npm" appDir ["run"; "build:telemetry"; "--"; "--watch"]
    let watchCss = exec "tailwindcss" appDir ["--input"; "./input.css"; "--output"; "./wwwroot/css/compiled.css"; "--watch"]
    let watchServer = execEnv "dotnet" appDir env ["watch"; "run"; "--no-restore"]
    Task.WaitAny(watchPrism, watchTelemetry, watchCss, watchServer) |> ignore

Target.create "BuildCss" <| fun _ ->
    let buildCss = exec "tailwindcss" appDir ["--input"; "./input.css"; "--output"; "./wwwroot/css/compiled.css"; "--minify"]
    buildCss.Wait()

Target.create "BuildBrowser" <| fun _ ->
    if not (Environment.GetEnvironmentVariable("SKIP_BROWSER_BUILD") = "true") then
        exec "npm" appDir ["ci"; "--ignore-scripts"] |> _.Wait()
        exec "npm" appDir ["run"; "check"] |> _.Wait()
        exec "npm" appDir ["run"; "build"] |> _.Wait()

Target.create "Test" <| fun _ ->
    exec "npm" appDir ["ci"; "--ignore-scripts"] |> _.Wait()
    exec "npm" appDir ["run"; "check"] |> _.Wait()
    exec "npm" appDir ["test"] |> _.Wait()
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

"StartDeps" ==>! "EnsureDevCert"
"EnsureDevCert" ==>! "Watch"

"BuildBrowser" ==>! "Publish"
"BuildCss" ==>! "Publish"

Target.runOrDefaultWithArguments "Default"
