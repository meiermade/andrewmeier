module PrivacyTests

open App.Privacy
open Expecto

let private expectAnalytics expected countryCode =
    let actual = (resolve countryCode).analytics
    Expect.equal actual expected $"Unexpected analytics mode for {countryCode}"

[<Tests>]
let tests =
    testList "regional browser privacy policy" [
        testCase "defaults ordinary US analytics on" <| fun _ ->
            expectAnalytics AnalyticsMode.DefaultOn (Some "US")

        testCase "requires opt-in for strict countries" <| fun _ ->
            for countryCode in [ "DE"; "FR"; "GB"; "CH"; "BR"; "CA" ] do
                expectAnalytics AnalyticsMode.OptIn (Some countryCode)

        testCase "normalizes trusted Cloudflare country codes" <| fun _ ->
            expectAnalytics AnalyticsMode.DefaultOn (Some " us ")
            expectAnalytics AnalyticsMode.OptIn (Some " de ")

        testCase "fails closed when location is unavailable or special" <| fun _ ->
            for countryCode in [ None; Some ""; Some "XX"; Some "T1"; Some "invalid" ] do
                expectAnalytics AnalyticsMode.OptIn countryCode

        testCase "defaults other known country codes on" <| fun _ ->
            for countryCode in [ "AU"; "JP"; "MX"; "NZ" ] do
                expectAnalytics AnalyticsMode.DefaultOn (Some countryCode)

        test "matches only supported external policy expectations" {
            let us = resolve (Some "US")
            let strict = resolve (Some "DE")

            Expect.equal (matchesAnalyticsMode "default-on" us) (Some true) "Expected U.S. match"
            Expect.equal (matchesAnalyticsMode "opt-in" strict) (Some true) "Expected strict match"
            Expect.equal (matchesAnalyticsMode "opt-in" us) (Some false) "Expected mismatch"
            Expect.equal (matchesAnalyticsMode "other" strict) None "Expected unsupported value"
        }
    ]
