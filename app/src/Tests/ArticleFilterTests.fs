module ArticleFilterTests

open App.Articles
open App.Articles.Handler
open Expecto
open FSharp.ViewEngine
open System
open type Html

let private article (title:string) (summary:string) (tags:string[]) (createdAt:DateTimeOffset) : Article =
    { permalink = title.ToLowerInvariant().Replace(" ", "-")
      title = title
      summary = summary
      cover = "https://assets.meiermade.com/andymeier/articles/shared/cover.webp"
      tags = tags
      createdAt = createdAt
      page = empty }

let private articles =
    [ article "Finance systems" "Notes on dependable operations." [| "Finance"; "Architecture" |] (DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero))
      article "Engineering notes" "Patterns for resilient software." [| "Engineering" |] (DateTimeOffset(2025, 6, 10, 0, 0, 0, TimeSpan.Zero))
      article "Capital planning" "Finance and engineering trade-offs." [| "Capital markets" |] (DateTimeOffset(2025, 1, 15, 0, 0, 0, TimeSpan.Zero)) ]

[<Tests>]
let articleFilterTests =
    testList "Article filters" [
        test "searches title, summary, and tags case-insensitively" {
            let titleMatches = filterArticles { search = Some "systems"; tag = None; publishedYear = None } articles
            let summaryMatches = filterArticles { search = Some "resilient"; tag = None; publishedYear = None } articles
            let tagMatches = filterArticles { search = Some "architecture"; tag = None; publishedYear = None } articles

            Expect.equal (titleMatches |> List.map _.title) [ "Finance systems" ] "Expected title search match"
            Expect.equal (summaryMatches |> List.map _.title) [ "Engineering notes" ] "Expected summary search match"
            Expect.equal (tagMatches |> List.map _.title) [ "Finance systems" ] "Expected tag search match"
        }

        test "combines tag and published-year filters" {
            let filtered = filterArticles { search = Some "finance"; tag = Some "capital markets"; publishedYear = Some 2025 } articles

            Expect.equal (filtered |> List.map _.title) [ "Capital planning" ] "Expected filters to combine with AND semantics"
        }
    ]
