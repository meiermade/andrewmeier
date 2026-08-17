module ConsentTests

open Expecto
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.FileProviders
open Microsoft.Extensions.Hosting
open System.IO
open System.Text

let private environment =
    { new IHostEnvironment with
        member _.ApplicationName with get () = "Tests" and set _ = ()
        member _.EnvironmentName with get () = Environments.Production and set _ = ()
        member _.ContentRootPath with get () = "." and set _ = ()
        member _.ContentRootFileProvider with get () = NullFileProvider() and set _ = () }

let private services =
    ServiceCollection()
        .AddGiraffe()
        .AddSingleton<IHostEnvironment>(environment)
        .BuildServiceProvider()

let private execute (scheme:string) (contentType:string) (origin:string) (fetchSite:string) (body:string) = task {
    let bytes = Encoding.UTF8.GetBytes(body)
    let context = DefaultHttpContext()
    context.RequestServices <- services
    context.Request.Scheme <- scheme
    context.Request.Host <- HostString("andymeier.dev")
    context.Request.ContentType <- contentType
    context.Request.ContentLength <- int64 bytes.Length
    context.Request.Headers["Origin"] <- origin
    context.Request.Headers["Sec-Fetch-Site"] <- fetchSite
    context.Request.Body <- new MemoryStream(bytes)
    context.Response.Body <- new MemoryStream()

    let next (context:HttpContext) = task { return Some context }
    let! _ = App.Consent.persist next context
    return context :> HttpContext
}

let private sameOrigin body =
    execute "https" "application/json" "https://andymeier.dev" "same-origin" body

let private setCookie (context:HttpContext) =
    context.Response.Headers.SetCookie.ToString().ToLowerInvariant()

[<Tests>]
let tests =
    testList "analytics consent endpoint" [
        testTask "sets the production consent cookie for a same-origin JSON request" {
            let! (context:HttpContext) = sameOrigin "{\"analytics\":\"accepted\"}"
            let cookie = setCookie context

            Expect.equal context.Response.StatusCode StatusCodes.Status204NoContent "Expected successful persistence"
            Expect.stringContains cookie "analytics-consent=v1.accepted.2026-08-16." "Expected versioned accepted choice"
            Expect.stringContains cookie "max-age=15552000" "Expected six-month lifetime"
            Expect.stringContains cookie "path=/" "Expected site-wide cookie"
            Expect.stringContains cookie "secure" "Expected production-only transport"
            Expect.stringContains cookie "samesite=lax" "Expected same-site request protection"
        }

        testTask "rejects a cross-site consent request" {
            let! (context:HttpContext) =
                execute "https" "application/json" "https://attacker.example" "cross-site" "{\"analytics\":\"accepted\"}"

            Expect.equal context.Response.StatusCode StatusCodes.Status403Forbidden "Expected the same-origin boundary"
            Expect.isEmpty (setCookie context) "Expected no consent cookie"
        }

        testTask "rejects a consent request with a mismatched scheme" {
            let! (context:HttpContext) =
                execute "http" "application/json" "https://andymeier.dev" "same-site" "{\"analytics\":\"accepted\"}"

            Expect.equal context.Response.StatusCode StatusCodes.Status403Forbidden "Expected the complete same-origin boundary"
            Expect.isEmpty (setCookie context) "Expected no consent cookie"
        }

        testTask "rejects JSON sent with an unsupported content type" {
            let! (context:HttpContext) =
                execute "https" "text/plain" "https://andymeier.dev" "same-origin" "{\"analytics\":\"accepted\"}"

            Expect.equal context.Response.StatusCode StatusCodes.Status415UnsupportedMediaType "Expected JSON content type enforcement"
            Expect.isEmpty (setCookie context) "Expected no consent cookie"
        }

        testTask "rejects null and invalid analytics choices" {
            for body in [ "null"; "{\"analytics\":\"maybe\"}" ] do
                let! (context:HttpContext) = sameOrigin body
                Expect.equal context.Response.StatusCode StatusCodes.Status400BadRequest "Expected a controlled client error"
                Expect.isEmpty (setCookie context) "Expected no consent cookie"
        }
    ]
