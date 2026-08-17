module App.Consent

open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Hosting
open System
open System.Text.Json

[<Literal>]
let cookieName = "analytics-consent"

[<Literal>]
let private policyVersion = "2026-08-16"

[<CLIMutable>]
type ConsentRequest = { analytics:string }

type Choice =
    | Accepted
    | Declined

let private tryChoice value =
    match value with
    | "accepted" -> Some Accepted
    | "declined" -> Some Declined
    | _ -> None

let private choiceValue choice =
    match choice with
    | Accepted -> "accepted"
    | Declined -> "declined"

let private isSameOrigin (ctx:HttpContext) =
    let origin = ctx.Request.Headers["Origin"].ToString()
    let fetchSite = ctx.Request.Headers["Sec-Fetch-Site"].ToString()
    match Uri.TryCreate(origin, UriKind.Absolute) with
    | true, originUri ->
        String.Equals(originUri.Authority, ctx.Request.Host.Value, StringComparison.OrdinalIgnoreCase)
        && not (String.Equals(fetchSite, "cross-site", StringComparison.OrdinalIgnoreCase))
    | false, _ -> false

let private appendCookie (ctx:HttpContext) choice =
    let options = CookieOptions()
    options.HttpOnly <- false
    options.IsEssential <- true
    options.MaxAge <- Nullable(TimeSpan.FromDays 180.)
    options.Path <- "/"
    options.SameSite <- SameSiteMode.Lax
    let environment = ctx.GetService<IHostEnvironment>()
    options.Secure <- not (environment.IsDevelopment())

    let timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    let value = $"v1.{choiceValue choice}.{policyVersion}.{timestamp}"
    ctx.Response.Cookies.Append(cookieName, value, options)

let persist : HttpHandler =
    fun next ctx -> task {
        if not (ctx.Request.HasJsonContentType()) then
            ctx.SetStatusCode StatusCodes.Status415UnsupportedMediaType
            return! text "Analytics consent requires JSON." next ctx
        elif not (isSameOrigin ctx) then
            return! RequestErrors.FORBIDDEN "Analytics consent requires a same-origin request." next ctx
        else
            try
                let! request = ctx.BindJsonAsync<ConsentRequest>()
                if isNull (box request) then
                    return! RequestErrors.BAD_REQUEST "Invalid consent request." next ctx
                else
                    match tryChoice request.analytics with
                    | Some choice ->
                        appendCookie ctx choice
                        ctx.SetStatusCode StatusCodes.Status204NoContent
                        return Some ctx
                    | None ->
                        return! RequestErrors.BAD_REQUEST "Invalid analytics consent choice." next ctx
            with :? JsonException ->
                return! RequestErrors.BAD_REQUEST "Invalid consent request." next ctx
    }
