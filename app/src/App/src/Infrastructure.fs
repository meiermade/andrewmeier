[<AutoOpen>]
module App.Infrastructure

open Giraffe
open Microsoft.AspNetCore.Http

[<AutoOpen>]
module HttpContextExtensions =
    type HttpContext with
        member this.IsDatastar =
            match this.TryGetRequestHeader("Datastar-Request") with
            | Some "true" -> true
            | _ -> false
