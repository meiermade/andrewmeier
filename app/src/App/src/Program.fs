open App
open App.ServiceRegistry
open Domain
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open OpenTelemetry
open OpenTelemetry.Exporter
open OpenTelemetry.Resources
open OpenTelemetry.Trace
open Serilog
open Serilog.Events
open Serilog.Sinks.OpenTelemetry
open StarFederation.Datastar.DependencyInjection
open System

let configureTracerProvider (config: Config) =
    Sdk
        .CreateTracerProviderBuilder()
        .AddSource(config.appName)
        .ConfigureResource(fun resourceBuilder ->
            resourceBuilder.AddService(serviceName = config.appName) |> ignore)
        .AddAspNetCoreInstrumentation(fun opts ->
            opts.Filter <- fun ctx -> ctx.Request.Path.Value <> "/health")
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(fun opts ->
            opts.Endpoint <- Uri(config.seq.endpoint + "/ingest/otlp/v1/traces")
            opts.Protocol <- OtlpExportProtocol.HttpProtobuf)
        .Build()

let configureLogger (config: Config) =
    let initialLogLevel =
        if config.debug then LogEventLevel.Debug
        else LogEventLevel.Information

    let logger =
        LoggerConfiguration()
            .MinimumLevel.Is(initialLogLevel)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .WriteTo.Console()
            .WriteTo.OpenTelemetry(fun opts ->
                opts.Endpoint <- config.seq.endpoint + "/ingest/otlp/v1/logs"
                opts.Protocol <- OtlpProtocol.HttpProtobuf
                opts.ResourceAttributes.Add("service.name", box config.appName))
            .CreateLogger()

    Log.Logger <- logger

let configureServices (serviceCollection: IServiceCollection) (tracerProvider: TracerProvider) (services: Services) =
    serviceCollection
        .AddSerilog()
        .AddSingleton(tracerProvider)
        .AddHostedService(fun _ -> Article.SyncBackgroundService(services.article))
        .AddDatastar()
        .AddGiraffe()
    |> ignore

let private addSecurityHeaders (ctx:HttpContext) =
    ctx.Response.Headers["Content-Security-Policy"] <- "base-uri 'self'; frame-ancestors 'none'; object-src 'none'"
    ctx.Response.Headers["Permissions-Policy"] <- "camera=(), geolocation=(), microphone=()"
    ctx.Response.Headers["Referrer-Policy"] <- "strict-origin-when-cross-origin"
    ctx.Response.Headers["Strict-Transport-Security"] <- "max-age=31536000; includeSubDomains"
    ctx.Response.Headers["X-Content-Type-Options"] <- "nosniff"

let configureApp (services: Services) (app: WebApplication) =
    app
        .Use(fun (ctx:HttpContext) (next:RequestDelegate) ->
            addSecurityHeaders ctx
            next.Invoke ctx)
        .UseSerilogRequestLogging(fun opts ->
            opts.GetLevel <- fun ctx _ _ ->
                if ctx.Request.Path.Value = "/health" then LogEventLevel.Verbose
                else LogEventLevel.Information)
        .UseStaticFiles()
        .UseGiraffe(Index.Handler.handler services)

[<EntryPoint>]
let main _args =
    let config = Config.load ()
    configureLogger config

    try
        try
            let tracerProvider = configureTracerProvider config
            let tracer = tracerProvider.GetTracer(config.appName)
            let services = Services.create config tracer

            let builder = WebApplication.CreateBuilder()
            configureServices builder.Services tracerProvider services
            let app = builder.Build()

            configureApp services app
            Log.Information("Starting {AppName}", config.appName)
            app.Run(config.server.url)
            0
        with ex ->
            Log.Fatal(ex, "Application start-up failed")
            1
    finally
        Log.CloseAndFlush()
