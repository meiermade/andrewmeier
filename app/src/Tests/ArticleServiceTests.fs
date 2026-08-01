module ArticleServiceTests

open Domain
open Expecto
open Microsoft.Data.Sqlite
open OpenTelemetry
open OpenTelemetry.Trace
open System
open System.IO
open System.Threading.Tasks

let private withTempDb (run: Sqlite.Config -> Task<'a>) =
    task {
        let path = Path.Combine(Path.GetTempPath(), $"andymeier-tests-{Guid.NewGuid():N}.db")

        try
            return! run { path = path }
        finally
            if File.Exists(path) then
                try
                    File.Delete(path)
                with
                | :? IOException -> ()
    }

let private telemetry () =
    let tracerProvider = Sdk.CreateTracerProviderBuilder().AddSource("tests").Build()
    let tracer = tracerProvider.GetTracer("tests")
    Telemetry.Service.create tracer

let private notionConfig: Notion.Config =
    { articlesDatabaseId = Notion.DatabaseId.ofString "db"
      apiKey = "token"
      baseUrl = "https://api.notion.com/v1" }

let private emptyNotionService: Notion.Service =
    { queryDatabase =
        fun _ _ ->
            task {
                return
                    { Notion.PaginatedResponse.results = []
                      hasMore = false
                      nextCursor = None }
            }
      retrievePage =
        fun _ ->
            task {
                return
                    { id = ""
                      icon = None
                      cover = None
                      properties = Map.empty }
            }
      retrieveBlockChildren =
        fun _ _ ->
            task {
                return
                    { Notion.PaginatedResponse.results = []
                      hasMore = false
                      nextCursor = None }
            } }

let private seedArticle (connectionString: string) =
    task {
        use conn = new SqliteConnection(connectionString)
        do! conn.OpenAsync() |> Task.Ignore

        // language=sqlite
        let schemaSql =
            """
            CREATE TABLE IF NOT EXISTS articles (
                id TEXT PRIMARY KEY,
                permalink TEXT NOT NULL UNIQUE,
                title TEXT NOT NULL,
                summary TEXT NOT NULL,
                icon TEXT,
                icon_description TEXT,
                cover TEXT,
                cover_description TEXT,
                tags_json TEXT NOT NULL,
                created_at_utc TEXT NOT NULL,
                updated_at_utc TEXT NOT NULL,
                blocks_json TEXT NOT NULL,
                synced_at_utc TEXT NOT NULL
            );
            """

        do!
            Sqlite.Sql.connect conn
            |> Sqlite.Sql.sql schemaSql
            |> Sqlite.Sql.executeNonQuery

        // language=sqlite
        let insertSql =
            """
            INSERT INTO articles (
                id, permalink, title, summary, icon, icon_description, cover, cover_description,
                tags_json, created_at_utc, updated_at_utc, blocks_json, synced_at_utc
            )
            VALUES (
                $id, $permalink, $title, $summary, $icon, $icon_description, $cover, $cover_description,
                $tags_json, $created_at_utc, $updated_at_utc, $blocks_json, $synced_at_utc
            );
            """

        do!
            Sqlite.Sql.connect conn
            |> Sqlite.Sql.sql insertSql
            |> Sqlite.Sql.parameters [
                "$id", Sqlite.Sql.text "a1"
                "$permalink", Sqlite.Sql.text "hello-world"
                "$title", Sqlite.Sql.text "Hello"
                "$summary", Sqlite.Sql.text "Summary"
                "$icon", Sqlite.Sql.text ""
                "$icon_description", Sqlite.Sql.text ""
                "$cover", Sqlite.Sql.text ""
                "$cover_description", Sqlite.Sql.text ""
                "$tags_json", Sqlite.Sql.text "[]"
                "$created_at_utc", Sqlite.Sql.text "2026-01-01T00:00:00.0000000+00:00"
                "$updated_at_utc", Sqlite.Sql.text "2026-01-01T00:00:00.0000000+00:00"
                "$blocks_json", Sqlite.Sql.text "[]"
                "$synced_at_utc", Sqlite.Sql.text "2026-01-01T00:00:00.0000000+00:00"
            ]
            |> Sqlite.Sql.executeNonQuery
    }

[<Tests>]
let articleServiceTests =
    testList "Article Service" [
        testTask "listArticles and tryGetArticle read from sqlite" {
            let! _ =
                withTempDb (fun sqliteConfig ->
                    let connectionString = Sqlite.connectionString sqliteConfig

                    let service =
                        Article.Service.create notionConfig (telemetry()) sqliteConfig emptyNotionService

                    task {
                        do! seedArticle connectionString
                        let! articles = service.listArticles ()
                        let! article = service.tryGetArticle "hello-world"

                        Expect.equal articles.Length 1 "Expected one article"
                        Expect.equal articles.[0].permalink "hello-world" "Expected permalink"
                        Expect.isSome article "Expected article by permalink"
                    })
            return ()
        }

        testTask "syncDatabase creates schema when missing" {
            let! _ =
                withTempDb (fun sqliteConfig ->
                    let service =
                        Article.Service.create notionConfig (telemetry()) sqliteConfig emptyNotionService

                    task {
                        do! service.syncDatabase ()
                        let! articles = service.listArticles ()
                        Expect.equal articles [] "Expected empty list after empty sync"
                    })
            return ()
        }
    ]