[<AutoOpen>]
module App.Infrastructure

open Giraffe
open Microsoft.AspNetCore.Http

[<Literal>]
let NavigationHistoryHeader = "X-MeierMade-Navigation"

[<AutoOpen>]
module HttpContextExtensions =
    type HttpContext with
        member this.IsDatastar =
            match this.TryGetRequestHeader("Datastar-Request") with
            | Some "true" -> true
            | _ -> false

        member this.IsHistoryRestore =
            match this.TryGetRequestHeader(NavigationHistoryHeader) with
            | Some "restore" -> true
            | _ -> false
