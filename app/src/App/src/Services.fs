module App.ServiceRegistry

open OpenTelemetry.Trace

type Services =
    { config: Config
      telemetry: Telemetry.Service }

module Services =
    let create (config: Config) (tracer: Tracer) =
        { config = config
          telemetry = Telemetry.Service.create tracer }
