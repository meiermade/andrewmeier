module MockNotion

open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Http
open System
open System.Threading.Tasks

module Fixture =
    [<Literal>]
    let apiKey = "mock-notion-token"

    [<Literal>]
    let databaseId = "mock-articles"

    [<Literal>]
    let apiVersion = "2022-06-28"

    let private annotations =
        """{"bold":false,"italic":false,"strikethrough":false,"underline":false,"code":false,"color":"default"}"""

    let private richText text =
        $"""{{"type":"text","text":{{"content":"{text}"}},"annotations":{annotations},"plain_text":"{text}","href":null}}"""

    let private page id title permalink summary tags createdAt updatedAt =
        let tags = tags |> List.map (fun tag -> $"""{{"name":"{tag}"}}""") |> String.concat ","

        $"""{{
            "object":"page",
            "id":"{id}",
            "icon":null,
            "cover":null,
            "properties":{{
                "Title":{{"type":"title","title":[{richText title}]}},
                "Permalink":{{"type":"rich_text","rich_text":[{richText permalink}]}},
                "Summary":{{"type":"rich_text","rich_text":[{richText summary}]}},
                "Tags":{{"type":"multi_select","multi_select":[{tags}]}},
                "Created At":{{"type":"date","date":{{"start":"{createdAt}","end":null}}}},
                "Updated At":{{"type":"date","date":{{"start":"{updatedAt}","end":null}}}},
                "Status":{{"type":"status","status":{{"name":"Published"}}}}
            }}
        }}"""

    let private financeArticle =
        page
            "mock-finance-systems"
            "Mock finance systems"
            "mock-finance-systems"
            "A fictional local article about dependable finance operations."
            [ "Finance"; "Local development" ]
            "2026-01-15T00:00:00.000+00:00"
            "2026-01-15T00:00:00.000+00:00"

    let private engineeringArticle =
        page
            "mock-engineering-notes"
            "Mock engineering notes"
            "mock-engineering-notes"
            "A fictional local article used to exercise the article index and detail view."
            [ "Engineering"; "Mock" ]
            "2026-02-01T00:00:00.000+00:00"
            "2026-02-01T00:00:00.000+00:00"

    let queryDatabase =
        $"""{{"object":"list","results":[{engineeringArticle},{financeArticle}],"has_more":false,"next_cursor":null}}"""

    let private blocks heading paragraph =
        $"""{{
            "object":"list",
            "results":[
                {{"object":"block","id":"{heading}-heading","type":"heading_1","has_children":false,"heading_1":{{"rich_text":[{richText heading}]}}}},
                {{"object":"block","id":"{heading}-paragraph","type":"paragraph","has_children":false,"paragraph":{{"rich_text":[{richText paragraph}]}}}}
            ],
            "has_more":false,
            "next_cursor":null
        }}"""

    let tryFindPage id =
        match id with
        | "mock-finance-systems" -> Some financeArticle
        | "mock-engineering-notes" -> Some engineeringArticle
        | _ -> None

    let tryFindBlocks id =
        match id with
        | "mock-finance-systems" -> Some(blocks "Mock finance systems" "This fictional article is available only through the local MockNotion service.")
        | "mock-engineering-notes" -> Some(blocks "Mock engineering notes" "This fictional article verifies local article synchronization without a live Notion workspace.")
        | _ -> None

let private json (body:string) : HttpHandler =
    fun _next ctx -> task {
        ctx.Response.ContentType <- "application/json"
        do! ctx.Response.WriteAsync body
        return Some ctx
    }

let private unauthorized : HttpHandler =
    fun _next ctx -> task {
        ctx.SetStatusCode StatusCodes.Status401Unauthorized
        return! json """{"object":"error","status":401,"code":"unauthorized","message":"Invalid mock Notion credentials"}""" _next ctx
    }

let private requireNotionRequest (handler:HttpHandler) : HttpHandler =
    fun next ctx -> task {
        let authorization = ctx.Request.Headers.Authorization.ToString()
        let version = ctx.Request.Headers["Notion-Version"].ToString()

        if authorization = $"Bearer {Fixture.apiKey}" && version = Fixture.apiVersion then
            return! handler next ctx
        else
            return! unauthorized next ctx
    }

let private notFound : HttpHandler =
    fun next ctx -> task {
        ctx.SetStatusCode StatusCodes.Status404NotFound
        return! json """{"object":"error","status":404,"code":"object_not_found","message":"Mock Notion object was not found"}""" next ctx
    }

let private queryDatabase : HttpHandler =
    json Fixture.queryDatabase

let private retrievePage id : HttpHandler =
    match Fixture.tryFindPage id with
    | Some page -> json page
    | None -> notFound

let private retrieveBlockChildren id : HttpHandler =
    match Fixture.tryFindBlocks id with
    | Some blocks -> json blocks
    | None -> notFound

let app : HttpHandler =
    choose [
        GET >=> route "/healthz" >=> text "ok"
        requireNotionRequest (
            choose [
                POST >=> route $"/v1/databases/{Fixture.databaseId}/query" >=> queryDatabase
                GET >=> routef "/v1/pages/%s" retrievePage
                GET >=> routef "/v1/blocks/%s/children" retrieveBlockChildren
                notFound
            ])
    ]

[<EntryPoint>]
let main _args =
    let url = Environment.GetEnvironmentVariable("MOCK_NOTION_URL") |> Option.ofObj |> Option.defaultValue "http://0.0.0.0:5015"
    let builder = WebApplication.CreateBuilder()
    builder.Services.AddGiraffe() |> ignore
    let webApp = builder.Build()
    webApp.UseGiraffe app
    webApp.Run url
    0
