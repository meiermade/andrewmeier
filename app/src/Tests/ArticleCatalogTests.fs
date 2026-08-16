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
                  [ "personal applications"
                    "andymeier.dev"
                    "Google Kubernetes Engine"
                    "Google Workspace"
                    "GKE free tier"
                    "$74.40"
                    "Namespaces, Deployments, and Services"
                    "Cloud Logging"
                    "Cloud Monitoring"
                    "Cloudflare Access"
                    "outbound-only"
                    "Pulumi ESC"
                    "IntelliSense"
                    "Secret Manager"
                    "OpenID Connect"
                    "GitHub Actions"
                    "GitHub CLI"
                    "Benji"
                    "Minnie"
                    "StatefulSet"
                    "OpenTelemetry"
                    "OpenTelemetry Collector"
                    "EventName"
                    "ClickHouse"
                    "ClickStack"
                    "Seq"
                    "strict CORS"
                    "privateClusterConfig"
                    "fn::open::gcp-secrets"
                    "new k8s.apps.v1.Deployment"
                    "mermaid.11.16.0.min.js"
                    "flowchart TB"
                    "accTitle: Personal infrastructure system context"
                    "accTitle: Personal infrastructure runtime"
                    "accTitle: Personal infrastructure deployment"
                    "Personal applications"
                    "strong APIs"
                    "agent-driven workflow"
                    "System context"
                    "Runtime"
                    "Deployment"
                    "same discipline I bring to client infrastructure"
                    "proving ground for new tools and architectural patterns"
                    "experience here informs those decisions"
                    "https://pi.dev/"
                    "https://assets.meiermade.com/andymeier/articles/personal-infrastructure/system-context-dec390cb7efb.webp"
                    "Google Workspace account"
                    "narrowly scoped viewer roles"
                    "Routes website traffic"
                    "Routes agent traffic"
                    "Opens protected interface" ] do
                  Expect.stringContains infrastructure topic $"Expected infrastructure content for {topic}"

              for excludedTopic in
                  [ "Meier Made Platform"
                    "PostgreSQL"
                    "Cloud SQL"
                    "Auth0"
                    "Dagster"
                    "Airbyte"
                    "Metabase"
                    "Raspberry Pi"
                    "Penpot"
                    "Amazon Web Services"
                    "Redis"
                    "Memorystore" ] do
                  Expect.isFalse
                      (infrastructure.Contains excludedTopic)
                      $"Expected out-of-scope infrastructure content for {excludedTopic} to be removed"

              Expect.isFalse
                  (infrastructure.Contains "subgraph personal")
                  "Expected runtime relationships to terminate at containers rather than a decorative boundary"

              for metaLanguage in
                  [ "the example I will follow through this article"
                    "The runtime view names"
                    "The deployment view makes"
                    "simplest representative workload"
                    "better explanation for the platform"
                    "Identifiers and account details are intentionally omitted"
                    "A VPN would still be appropriate"
                    "The application repository has a deliberately ordinary shape"
                    "Kubernetes often gets a bad reputation"
                    "The point is not to turn infrastructure"
                    "The goal is not to use the fewest technologies" ] do
                  Expect.isFalse
                      (infrastructure.Contains metaLanguage)
                      $"Expected meta language for {metaLanguage} to be removed"

              Expect.isLessThan
                  (infrastructure.IndexOf "How it is organized")
                  (infrastructure.IndexOf "Personal applications and agents")
                  "Expected infrastructure ownership to be explained before individual technologies"

              for applicationDetail in
                  [ "href=\"#example\""
                    "https://github.com/meiermade/andymeier"
                    "source for andymeier.dev"
                    "The site keeps article content in source control"
                    "I keep application code, infrastructure, tests, and workflows together" ] do
                  Expect.isFalse
                      (infrastructure.Contains applicationDetail)
                      $"Expected the andymeier.dev application section to omit {applicationDetail}"

              Expect.isFalse
                  (infrastructure.Contains "https://github.com/meiermade/agent")
                  "Expected the private agent repository not to be linked"

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
