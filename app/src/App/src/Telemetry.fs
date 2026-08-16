module App.Telemetry

open OpenTelemetry.Trace
open System.Diagnostics

let removeUrlQuery (activity:Activity) =
    activity.SetTag("url.query", null) |> ignore

type StartActiveSpan = string -> TelemetrySpan

type Service = { startActiveSpan: StartActiveSpan }

module Service =
    let create (tracer: Tracer) : Service =
        { startActiveSpan = fun name -> tracer.StartActiveSpan name }
