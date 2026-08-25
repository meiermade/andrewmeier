module App.Privacy

open Microsoft.AspNetCore.Http

[<RequireQualifiedAccess>]
type AnalyticsMode =
    | DefaultOn
    | OptIn

type BrowserPolicy =
    { analytics:AnalyticsMode }

let private strictCountryCodes =
    set [
        // European Union and European Economic Area
        "AT"; "BE"; "BG"; "HR"; "CY"; "CZ"; "DK"; "EE"; "FI"; "FR"; "DE"
        "GR"; "HU"; "IE"; "IT"; "LV"; "LT"; "LU"; "MT"; "NL"; "PL"; "PT"
        "RO"; "SK"; "SI"; "ES"; "SE"; "IS"; "LI"; "NO"
        // Canada is conservative because country-level geolocation cannot isolate Québec.
        "GB"; "CH"; "BR"; "CA"
    ]

let private analyticsMode (countryCode:string option) =
    match countryCode |> Option.map (fun value -> value.Trim().ToUpperInvariant()) with
    | Some code when strictCountryCodes.Contains code -> AnalyticsMode.OptIn
    | Some code when code.Length = 2 && code <> "XX" && code <> "T1" -> AnalyticsMode.DefaultOn
    | _ -> AnalyticsMode.OptIn

let resolve countryCode =
    { analytics = analyticsMode countryCode }

let fromRequest (ctx:HttpContext) =
    let countryCode =
        match ctx.Request.Headers.TryGetValue "CF-IPCountry" with
        | true, values -> values.ToString() |> Some
        | false, _ -> None

    resolve countryCode

let analyticsModeValue policy =
    match policy.analytics with
    | AnalyticsMode.DefaultOn -> "default-on"
    | AnalyticsMode.OptIn -> "opt-in"

let matchesAnalyticsMode expected policy =
    match expected with
    | "default-on" -> Some (policy.analytics = AnalyticsMode.DefaultOn)
    | "opt-in" -> Some (policy.analytics = AnalyticsMode.OptIn)
    | _ -> None
