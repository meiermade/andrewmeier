module App.Telemetry

open OpenTelemetry.Trace

type StartActiveSpan = string -> TelemetrySpan

type Service = { startActiveSpan: StartActiveSpan }

module Service =
    let create (tracer: Tracer) : Service =
        { startActiveSpan = fun name -> tracer.StartActiveSpan name }
