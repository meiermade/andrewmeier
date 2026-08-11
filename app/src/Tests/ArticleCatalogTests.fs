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
                  [ "personal-infrastructure"; "dev-env"; "fsharp-semantic-kernel" ]
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

              for topic in
                  [ "Google Kubernetes Engine"
                    "Pulumi ESC"
                    "Cloudflare Tunnel"
                    "Seq"
                    "Snowplow"
                    "zonal"
                    "Meier Made Platform"
                    "platform-identity/"
                    "application/"
                    "privateClusterConfig"
                    "fn::open::pulumi-stacks"
                    "new k8s.apps.v1.Deployment"
                    "mermaid.11.16.0.min.js"
                    "accTitle: Meier Made platform system context" ] do
                  Expect.stringContains infrastructure topic $"Expected infrastructure content for {topic}"

              for obsoleteTopic in [ "Raspberry Pi"; "Penpot"; "Amazon Web Services" ] do
                  Expect.isFalse
                      (infrastructure.Contains obsoleteTopic)
                      $"Expected obsolete infrastructure content for {obsoleteTopic} to be removed"

              for tool in
                  [ "Homebrew"
                    "Ghostty"
                    "Starship"
                    "tmux"
                    "GitHub CLI"
                    "1Password CLI"
                    "fnm"
                    "Node.js"
                    "uv"
                    ".NET SDK"
                    "Pi coding agent"
                    "PyCharm"
                    "Rider"
                    "WebStorm"
                    "Docker Desktop"
                    "gcloud"
                    "kubectl"
                    "kubectx"
                    "kubens"
                    "Pulumi"
                    "cloudflared"
                    "Google Workspace CLI"
                    "ripgrep"
                    "jq"
                    "Playwright" ] do
                  Expect.stringContains
                      developmentEnvironment
                      tool
                      $"Expected development environment content for {tool}"

              Expect.isFalse
                  (developmentEnvironment.Contains "Windows Subsystem for Linux")
                  "Expected obsolete Windows setup content to be removed"
          }

          test "finds articles by permalink" {
              Expect.isSome (Catalog.tryFind "personal-infrastructure") "Expected a published article"
              Expect.isNone (Catalog.tryFind "missing") "Expected an unknown permalink not to resolve"
          } ]
