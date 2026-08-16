namespace App

open System

module Env =
    let variable (key:string) =
        match Environment.GetEnvironmentVariable key with
        | value when String.IsNullOrEmpty value -> failwith $"Environment variable '{key}' is required"
        | value -> value

    let variableOrDefault (key:string) (defaultValue:string) =
        match Environment.GetEnvironmentVariable key with
        | value when String.IsNullOrEmpty value -> defaultValue
        | value -> value

type OpenTelemetryConfig =
    { endpoint:string
      publicEndpoint:string }

module OpenTelemetryConfig =
    let load () =
        { endpoint = Env.variableOrDefault "OTEL_EXPORTER_OTLP_ENDPOINT" "http://localhost:4318"
          publicEndpoint = Env.variableOrDefault "PUBLIC_OTEL_EXPORTER_OTLP_ENDPOINT" "http://localhost:4318" }

type ServerConfig =
    { url:string }

module ServerConfig =
    let load () =
        { url = Env.variableOrDefault "SERVER_URL" "https://localhost:5000" }

type Config =
    { debug:bool
      appName:string
      server:ServerConfig
      openTelemetry:OpenTelemetryConfig }

module Config =
    let load () =
        { debug = Env.variableOrDefault "DEBUG" "false" |> Boolean.Parse
          appName = "andymeier"
          server = ServerConfig.load ()
          openTelemetry = OpenTelemetryConfig.load () }
