module Tests.MockNotionTests

open System.Text.Json
open Expecto
open MockNotion

[<Tests>]
let tests =
    testList "MockNotion" [
        test "returns two fictional published articles" {
            use doc = JsonDocument.Parse(Fixture.queryDatabase)
            let articles = doc.RootElement.GetProperty("results")

            Expect.equal (articles.GetArrayLength()) 2 "Expected two local articles"

            let titles =
                articles.EnumerateArray()
                |> Seq.map (fun page ->
                    page.GetProperty("properties").GetProperty("Title").GetProperty("title").[0].GetProperty("plain_text").GetString())
                |> Set.ofSeq

            Expect.equal titles (Set.ofList [ "Mock engineering notes"; "Mock finance systems" ]) "Expected fictional article titles"
        }

        test "returns deterministic blocks for each fictional article" {
            let financeBlocks = Fixture.tryFindBlocks "mock-finance-systems"
            let engineeringBlocks = Fixture.tryFindBlocks "mock-engineering-notes"

            Expect.isSome financeBlocks "Expected finance article blocks"
            Expect.isSome engineeringBlocks "Expected engineering article blocks"
            Expect.isNone (Fixture.tryFindBlocks "unknown") "Expected unknown article to have no blocks"
        }
    ]
