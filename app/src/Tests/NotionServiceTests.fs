module Tests.NotionServiceTests

open System
open System.Net
open System.Net.Http
open System.Text
open System.Threading
open System.Threading.Tasks
open Domain
open Domain.Notion
open Expecto

// ============================================================
// Test helpers
// ============================================================

let private defaultAnnotations =
    """{"bold":false,"italic":false,"strikethrough":false,"underline":false,"code":false,"color":"default"}"""

let private richTextJson (text: string) =
    $"""{{
        "type":"text",
        "text":{{"content":"{text}"}},
        "annotations":{defaultAnnotations},
        "plain_text":"{text}",
        "href":null
    }}"""

/// HttpMessageHandler that returns a canned response
type MockHandler(handler: HttpRequestMessage -> HttpResponseMessage) =
    inherit HttpMessageHandler()

    override _.SendAsync(request, _ct) =
        Task.FromResult(handler request)

let private mockHttpClient (responses: (string * string) list) =
    let mutable callIndex = 0

    let handler =
        new MockHandler(fun _req ->
            let _url, body =
                if callIndex < responses.Length then
                    responses.[callIndex]
                else
                    "", """{"results":[],"has_more":false,"next_cursor":null}"""

            callIndex <- callIndex + 1

            new HttpResponseMessage(
                HttpStatusCode.OK,
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            ))

    new HttpClient(handler)

let private testConfig: Config =
    { articlesDatabaseId = DatabaseId.ofString "articles-db"
      apiKey = "test-token"
      baseUrl = "http://notion.test/v1" }

let private noopTelemetry: Telemetry.Service =
    { startActiveSpan =
        fun _name ->
            // Return a no-op span using the global no-op tracer
            let tracer =
                OpenTelemetry.Trace.TracerProvider
                    .Default.GetTracer("test")

            tracer.StartActiveSpan("noop") }

// ============================================================
// Tests
// ============================================================

let queryDatabaseTests =
    testList "Service.queryDatabase" [
        testTask "uses the configured endpoint and Notion request headers" {
            let mutable request: HttpRequestMessage option = None
            let handler =
                new MockHandler(fun req ->
                    request <- Some req
                    new HttpResponseMessage(
                        HttpStatusCode.OK,
                        Content = new StringContent("""{"results":[],"has_more":false,"next_cursor":null}""", Encoding.UTF8, "application/json")
                    ))
            let httpClient = new HttpClient(handler)
            let svc = Service.create testConfig noopTelemetry httpClient

            let! _ = svc.queryDatabase testConfig.articlesDatabaseId { filter = None; startCursor = None }

            let request = request |> Option.defaultWith (fun () -> failtest "Expected a Notion request")
            Expect.equal request.RequestUri.AbsoluteUri "http://notion.test/v1/databases/articles-db/query" "Expected configured Notion base URL"
            Expect.equal request.Headers.Authorization.Scheme "Bearer" "Expected bearer authentication"
            Expect.equal request.Headers.Authorization.Parameter "test-token" "Expected configured API key"
            Expect.equal (request.Headers.GetValues("Notion-Version") |> Seq.exactlyOne) "2022-06-28" "Expected Notion API version"
        }

        testTask "returns parsed pages" {
            let responseJson =
                $"""{{
                    "object":"list",
                    "results":[{{
                        "object":"page",
                        "id":"page-1",
                        "icon":null,
                        "cover":null,
                        "properties":{{
                            "Title":{{"type":"title","title":[{richTextJson "Article 1"}]}},
                            "Status":{{"type":"status","status":{{"name":"Published"}}}}
                        }}
                    }},{{
                        "object":"page",
                        "id":"page-2",
                        "icon":null,
                        "cover":null,
                        "properties":{{
                            "Title":{{"type":"title","title":[{richTextJson "Article 2"}]}},
                            "Status":{{"type":"status","status":{{"name":"Draft"}}}}
                        }}
                    }}],
                    "has_more":false,
                    "next_cursor":null
                }}"""

            let httpClient = mockHttpClient [ "", responseJson ]
            let svc = Service.create testConfig noopTelemetry httpClient

            let! res =
                svc.queryDatabase (DatabaseId.ofString "articles-db") { filter = None; startCursor = None }

            Expect.equal res.results.Length 2 "two pages"
            Expect.equal (Page.getTitle "Title" res.results.[0]) "Article 1" "first title"
            Expect.equal (Page.getStatus "Status" res.results.[1]) "Draft" "second status"
            Expect.isFalse res.hasMore "no more"
        }

        testTask "handles pagination cursor" {
            let page1Json =
                $"""{{
                    "object":"list",
                    "results":[{{
                        "object":"page","id":"p1","icon":null,"cover":null,
                        "properties":{{"Title":{{"type":"title","title":[{richTextJson "First"}]}}}}
                    }}],
                    "has_more":true,
                    "next_cursor":"cursor-abc"
                }}"""

            let page2Json =
                $"""{{
                    "object":"list",
                    "results":[{{
                        "object":"page","id":"p2","icon":null,"cover":null,
                        "properties":{{"Title":{{"type":"title","title":[{richTextJson "Second"}]}}}}
                    }}],
                    "has_more":false,
                    "next_cursor":null
                }}"""

            let httpClient = mockHttpClient [ "", page1Json; "", page2Json ]
            let svc = Service.create testConfig noopTelemetry httpClient

            let! res1 =
                svc.queryDatabase (DatabaseId.ofString "articles-db") { filter = None; startCursor = None }

            Expect.isTrue res1.hasMore "has more"
            Expect.equal res1.nextCursor (Some (Cursor.ofString "cursor-abc")) "cursor"

            let! res2 =
                svc.queryDatabase
                    (DatabaseId.ofString "articles-db")
                    { filter = None
                      startCursor = res1.nextCursor }

            Expect.equal res2.results.Length 1 "one page"
            Expect.isFalse res2.hasMore "no more"
        }

        testTask "returns empty for no results" {
            let responseJson =
                """{"object":"list","results":[],"has_more":false,"next_cursor":null}"""

            let httpClient = mockHttpClient [ "", responseJson ]
            let svc = Service.create testConfig noopTelemetry httpClient

            let! res =
                svc.queryDatabase (DatabaseId.ofString "articles-db") { filter = None; startCursor = None }

            Expect.isEmpty res.results "empty"
        }
    ]

let retrievePageTests =
    testList "Service.retrievePage" [
        testTask "returns parsed page" {
            let responseJson =
                $"""{{
                    "object":"page",
                    "id":"page-abc",
                    "icon":{{"type":"external","external":{{"url":"https://example.com/icon.png"}}}},
                    "cover":{{"type":"file","file":{{"url":"https://s3.example.com/cover.jpg"}}}},
                    "properties":{{
                        "Title":{{"type":"title","title":[{richTextJson "My Page"}]}},
                        "Permalink":{{"type":"rich_text","rich_text":[{richTextJson "my-page"}]}},
                        "Status":{{"type":"status","status":{{"name":"Published"}}}},
                        "Tags":{{"type":"multi_select","multi_select":[{{"name":"F#"}},{{"name":"Web"}}]}}
                    }}
                }}"""

            let httpClient = mockHttpClient [ "", responseJson ]
            let svc = Service.create testConfig noopTelemetry httpClient

            let! page = svc.retrievePage (PageId.ofString "page-abc")

            Expect.equal page.id "page-abc" "id"
            Expect.equal (Page.getTitle "Title" page) "My Page" "title"
            Expect.equal (Page.getText "Permalink" page) "my-page" "permalink"
            Expect.equal (Page.getStatus "Status" page) "Published" "status"
            Expect.equal (Page.getMultiSelect "Tags" page) [| "F#"; "Web" |] "tags"
            Expect.equal (Page.getIconUrl page) "https://example.com/icon.png" "icon"
            Expect.equal (Page.getCoverUrl page) "https://s3.example.com/cover.jpg" "cover"
        }
    ]

let retrieveBlockChildrenTests =
    testList "Service.retrieveBlockChildren" [
        testTask "returns parsed blocks" {
            let responseJson =
                $"""{{
                    "object":"list",
                    "results":[
                        {{"object":"block","id":"b1","type":"heading_1","has_children":false,
                          "heading_1":{{"rich_text":[{richTextJson "Introduction"}]}}}},
                        {{"object":"block","id":"b2","type":"paragraph","has_children":false,
                          "paragraph":{{"rich_text":[{richTextJson "Some content here."}]}}}},
                        {{"object":"block","id":"b3","type":"code","has_children":false,
                          "code":{{"rich_text":[{richTextJson "let x = 42"}],"language":"f#"}}}},
                        {{"object":"block","id":"b4","type":"divider","has_children":false,"divider":{{}}}},
                        {{"object":"block","id":"b5","type":"image","has_children":false,
                          "image":{{"type":"external","external":{{"url":"https://example.com/img.png"}}}}}}
                    ],
                    "has_more":false,
                    "next_cursor":null
                }}"""

            let httpClient = mockHttpClient [ "", responseJson ]
            let svc = Service.create testConfig noopTelemetry httpClient

            let! res = svc.retrieveBlockChildren (BlockId.ofString "block-parent") None

            Expect.equal res.results.Length 5 "five blocks"

            match res.results.[0].blockType with
            | BlockType.Heading1 rt -> Expect.equal rt.[0].plainText "Introduction" "h1"
            | other -> failtestf "expected Heading1, got %A" other

            match res.results.[1].blockType with
            | BlockType.Paragraph rt -> Expect.equal rt.[0].plainText "Some content here." "paragraph"
            | other -> failtestf "expected Paragraph, got %A" other

            match res.results.[2].blockType with
            | BlockType.Code(rt, lang) ->
                Expect.equal rt.[0].plainText "let x = 42" "code"
                Expect.equal lang "f#" "language"
            | other -> failtestf "expected Code, got %A" other

            Expect.equal res.results.[3].blockType BlockType.Divider "divider"

            match res.results.[4].blockType with
            | BlockType.Image url -> Expect.equal url "https://example.com/img.png" "image"
            | other -> failtestf "expected Image, got %A" other
        }

        testTask "handles pagination" {
            let page1Json =
                $"""{{
                    "object":"list",
                    "results":[
                        {{"object":"block","id":"b1","type":"paragraph","has_children":false,
                          "paragraph":{{"rich_text":[{richTextJson "First"}]}}}}
                    ],
                    "has_more":true,
                    "next_cursor":"block-cursor"
                }}"""

            let page2Json =
                $"""{{
                    "object":"list",
                    "results":[
                        {{"object":"block","id":"b2","type":"paragraph","has_children":false,
                          "paragraph":{{"rich_text":[{richTextJson "Second"}]}}}}
                    ],
                    "has_more":false,
                    "next_cursor":null
                }}"""

            let httpClient = mockHttpClient [ "", page1Json; "", page2Json ]
            let svc = Service.create testConfig noopTelemetry httpClient

            let! res1 = svc.retrieveBlockChildren (BlockId.ofString "parent-id") None
            Expect.equal res1.results.Length 1 "first page"
            Expect.isTrue res1.hasMore "has more"

            let! res2 = svc.retrieveBlockChildren (BlockId.ofString "parent-id") res1.nextCursor
            Expect.equal res2.results.Length 1 "second page"
            Expect.isFalse res2.hasMore "no more"
        }
    ]

[<Tests>]
let tests =
    testList
        "Notion.Service"
        [ queryDatabaseTests
          retrievePageTests
          retrieveBlockChildrenTests ]
