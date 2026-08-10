module ArticleCatalogTests

open App.Articles
open Expecto
open FSharp.ViewEngine

[<Tests>]
let articleCatalogTests =
    testList
        "Article catalog"
        [ test "contains the three published articles in reverse chronological order" {
              let permalinks = Catalog.all |> List.map _.permalink

              Expect.equal
                  permalinks
                  [ "fsharp-semantic-kernel"; "personal-infrastructure"; "dev-env" ]
                  "Expected the published article catalog"
          }

          test "renders source-controlled article pages with durable assets" {
              let rendered =
                  Catalog.all
                  |> List.map (fun article -> article, Render.toHtmlDocString article.page)

              for article, html in rendered do
                  Expect.stringContains html article.title $"Expected {article.permalink} title"

                  Expect.stringContains
                      html
                      "https://assets.meiermade.com/andymeier/articles/"
                      $"Expected {article.permalink} GCS assets"

                  Expect.isFalse
                      (html.Contains "prod-files-secure.s3")
                      $"Expected {article.permalink} not to use signed Notion assets"

                  Expect.isFalse
                      (html.Contains "app.notion.com")
                      $"Expected {article.permalink} not to use Notion assets"

              let semanticKernel =
                  rendered
                  |> List.find (fun (article, _) -> article.permalink = "fsharp-semantic-kernel")
                  |> snd

              let infrastructure =
                  rendered
                  |> List.find (fun (article, _) -> article.permalink = "personal-infrastructure")
                  |> snd

              let developmentEnvironment =
                  rendered |> List.find (fun (article, _) -> article.permalink = "dev-env") |> snd

              Expect.stringContains semanticKernel "Semantic Kernel SDK" "Expected Semantic Kernel content"

              Expect.stringContains
                  infrastructure
                  "My current infrastructure runs on 3 Raspberry Pis"
                  "Expected infrastructure content"

              Expect.stringContains
                  developmentEnvironment
                  "Windows Subsystem for Linux"
                  "Expected development environment content"
          }

          test "finds articles by permalink" {
              Expect.isSome (Catalog.tryFind "personal-infrastructure") "Expected a published article"
              Expect.isNone (Catalog.tryFind "missing") "Expected an unknown permalink not to resolve"
          } ]
