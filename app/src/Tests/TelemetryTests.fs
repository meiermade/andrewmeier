module TelemetryTests

open Expecto
open System.Diagnostics

[<Tests>]
let tests =
    testList "server telemetry" [
        test "removes raw URL queries from request activities" {
            use activity = new Activity("request")
            activity.SetTag("url.query", "utm_source=linkedin&email=private@example.com") |> ignore

            App.Telemetry.removeUrlQuery activity

            Expect.isNull (activity.GetTagItem "url.query") "Expected the raw URL query tag to be removed"
        }
    ]
