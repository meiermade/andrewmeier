module Domain.Notion

open System
open System.Net.Http
open System.Net.Http.Headers
open System.Text
open System.Text.Json
open System.Threading.Tasks
open FSharp.UMX

// ============================================================
// Phantom Types
// ============================================================

type [<Measure>] databaseId
type DatabaseId = string<databaseId>

module DatabaseId =
    let toString (d:DatabaseId) = UMX.untag d
    let ofString (d:string) : DatabaseId = UMX.tag d

type [<Measure>] pageId
type PageId = string<pageId>

module PageId =
    let toString (p:PageId) = UMX.untag p
    let ofString (p:string) : PageId = UMX.tag p

type [<Measure>] blockId
type BlockId = string<blockId>

module BlockId =
    let toString (b:BlockId) = UMX.untag b
    let ofString (b:string) : BlockId = UMX.tag b

type [<Measure>] cursor
type Cursor = string<cursor>

module Cursor =
    let toString (c:Cursor) = UMX.untag c
    let ofString (c:string) : Cursor = UMX.tag c

// ============================================================
// Config
// ============================================================

type Config =
    { articlesDatabaseId:DatabaseId
      apiKey:string
      baseUrl:string }

module Config =
    let load () =
        { articlesDatabaseId = Env.variable "NOTION_ARTICLES_DATABASE_ID" |> DatabaseId.ofString
          apiKey = Env.variable "NOTION_API_KEY"
          baseUrl = Env.variableOrDefault "NOTION_BASE_URL" "https://api.notion.com/v1" }

// ============================================================
// Types
// ============================================================

type Annotations =
    { bold: bool
      italic: bool
      strikethrough: bool
      underline: bool
      code: bool
      color: string }

type RichText =
    { plainText: string
      href: string option
      annotations: Annotations }

type FileObject =
    { url: string }

[<RequireQualifiedAccess>]
type BlockType =
    | Paragraph of richText: RichText list
    | Heading1 of richText: RichText list
    | Heading2 of richText: RichText list
    | Heading3 of richText: RichText list
    | BulletedListItem of richText: RichText list * children: Block list
    | NumberedListItem of richText: RichText list * children: Block list
    | Code of richText: RichText list * language: string
    | Image of url: string
    | Divider
    | Quote of richText: RichText list
    | Callout of richText: RichText list
    | Unsupported

and Block =
    { id: string
      hasChildren: bool
      blockType: BlockType }

[<RequireQualifiedAccess>]
type PropertyValue =
    | Title of RichText list
    | RichText of RichText list
    | Email of string
    | Checkbox of bool
    | Date of DateTimeOffset option
    | Status of string
    | Select of string
    | MultiSelect of string list
    | Unknown

type Page =
    { id: string
      icon: FileObject option
      cover: FileObject option
      properties: Map<string, PropertyValue> }

type PaginatedResponse<'T> =
    { results: 'T list
      hasMore: bool
      nextCursor: Cursor option }

// ============================================================
// JSON Parsing
// ============================================================

module Parse =

    let private str (el: JsonElement) =
        match el.ValueKind with
        | JsonValueKind.String -> el.GetString()
        | _ -> ""

    let private optStr (el: JsonElement) =
        match el.ValueKind with
        | JsonValueKind.String ->
            let s = el.GetString()
            if String.IsNullOrEmpty(s) then None else Some s
        | _ -> None

    let private tryGetProp (name: string) (el: JsonElement) =
        match el.TryGetProperty(name) with
        | true, v -> Some v
        | _ -> None

    let richText (el: JsonElement) : RichText =
        let annotations =
            match tryGetProp "annotations" el with
            | Some a ->
                { bold = a.GetProperty("bold").GetBoolean()
                  italic = a.GetProperty("italic").GetBoolean()
                  strikethrough = a.GetProperty("strikethrough").GetBoolean()
                  underline = a.GetProperty("underline").GetBoolean()
                  code = a.GetProperty("code").GetBoolean()
                  color = str (a.GetProperty("color")) }
            | None ->
                { bold = false
                  italic = false
                  strikethrough = false
                  underline = false
                  code = false
                  color = "default" }

        { plainText = el.GetProperty("plain_text") |> str
          href = tryGetProp "href" el |> Option.bind optStr
          annotations = annotations }

    let richTextList (el: JsonElement) : RichText list =
        [ for item in el.EnumerateArray() do
              richText item ]

    let fileObject (el: JsonElement) : FileObject option =
        match tryGetProp "type" el |> Option.map str with
        | Some "file" -> tryGetProp "file" el |> Option.map (fun f -> { url = str (f.GetProperty("url")) })
        | Some "external" ->
            tryGetProp "external" el |> Option.map (fun f -> { url = str (f.GetProperty("url")) })
        | _ -> None

    let propertyValue (el: JsonElement) : PropertyValue =
        match tryGetProp "type" el |> Option.map str with
        | Some "title" -> PropertyValue.Title(el.GetProperty("title") |> richTextList)
        | Some "rich_text" -> PropertyValue.RichText(el.GetProperty("rich_text") |> richTextList)
        | Some "email" ->
            match tryGetProp "email" el |> Option.bind optStr with
            | Some e -> PropertyValue.Email e
            | None -> PropertyValue.Email ""
        | Some "checkbox" -> PropertyValue.Checkbox(el.GetProperty("checkbox").GetBoolean())
        | Some "date" ->
            match tryGetProp "date" el with
            | Some d when d.ValueKind <> JsonValueKind.Null ->
                match tryGetProp "start" d |> Option.bind optStr with
                | Some s ->
                    match DateTimeOffset.TryParse(s) with
                    | true, dt -> PropertyValue.Date(Some dt)
                    | _ -> PropertyValue.Date None
                | None -> PropertyValue.Date None
            | _ -> PropertyValue.Date None
        | Some "status" ->
            match tryGetProp "status" el with
            | Some s when s.ValueKind <> JsonValueKind.Null -> PropertyValue.Status(str (s.GetProperty("name")))
            | _ -> PropertyValue.Status ""
        | Some "select" ->
            match tryGetProp "select" el with
            | Some s when s.ValueKind <> JsonValueKind.Null -> PropertyValue.Select(str (s.GetProperty("name")))
            | _ -> PropertyValue.Select ""
        | Some "multi_select" ->
            match tryGetProp "multi_select" el with
            | Some ms ->
                PropertyValue.MultiSelect
                    [ for item in ms.EnumerateArray() do
                          str (item.GetProperty("name")) ]
            | None -> PropertyValue.MultiSelect []
        | _ -> PropertyValue.Unknown

    let page (el: JsonElement) : Page =
        let icon =
            tryGetProp "icon" el
            |> Option.bind (fun v ->
                if v.ValueKind = JsonValueKind.Null then None
                else fileObject v)
        let cover =
            tryGetProp "cover" el
            |> Option.bind (fun v ->
                if v.ValueKind = JsonValueKind.Null then None
                else fileObject v)

        let properties =
            match tryGetProp "properties" el with
            | Some props ->
                [ for prop in props.EnumerateObject() do
                      prop.Name, propertyValue prop.Value ]
                |> Map.ofList
            | None -> Map.empty

        { id = str (el.GetProperty("id"))
          icon = icon
          cover = cover
          properties = properties }

    let block (el: JsonElement) : Block =
        let id = str (el.GetProperty("id"))

        let hasChildren =
            match tryGetProp "has_children" el with
            | Some v -> v.GetBoolean()
            | None -> false

        let blockType =
            match tryGetProp "type" el |> Option.map str with
            | Some "paragraph" -> BlockType.Paragraph(el.GetProperty("paragraph").GetProperty("rich_text") |> richTextList)
            | Some "heading_1" -> BlockType.Heading1(el.GetProperty("heading_1").GetProperty("rich_text") |> richTextList)
            | Some "heading_2" -> BlockType.Heading2(el.GetProperty("heading_2").GetProperty("rich_text") |> richTextList)
            | Some "heading_3" -> BlockType.Heading3(el.GetProperty("heading_3").GetProperty("rich_text") |> richTextList)
            | Some "bulleted_list_item" ->
                BlockType.BulletedListItem(el.GetProperty("bulleted_list_item").GetProperty("rich_text") |> richTextList, [])
            | Some "numbered_list_item" ->
                BlockType.NumberedListItem(el.GetProperty("numbered_list_item").GetProperty("rich_text") |> richTextList, [])
            | Some "code" ->
                let codeEl = el.GetProperty("code")
                let lang = str (codeEl.GetProperty("language"))
                BlockType.Code(codeEl.GetProperty("rich_text") |> richTextList, lang)
            | Some "image" ->
                let imageEl = el.GetProperty("image")
                let url =
                    match tryGetProp "type" imageEl |> Option.map str with
                    | Some "file" -> str (imageEl.GetProperty("file").GetProperty("url"))
                    | Some "external" -> str (imageEl.GetProperty("external").GetProperty("url"))
                    | _ -> ""
                BlockType.Image url
            | Some "divider" -> BlockType.Divider
            | Some "quote" -> BlockType.Quote(el.GetProperty("quote").GetProperty("rich_text") |> richTextList)
            | Some "callout" -> BlockType.Callout(el.GetProperty("callout").GetProperty("rich_text") |> richTextList)
            | _ -> BlockType.Unsupported

        { id = id
          hasChildren = hasChildren
          blockType = blockType }

    let paginatedPages (doc: JsonDocument) : PaginatedResponse<Page> =
        let root = doc.RootElement

        { results =
            [ for item in root.GetProperty("results").EnumerateArray() do
                  page item ]
          hasMore = root.GetProperty("has_more").GetBoolean()
          nextCursor =
            match tryGetProp "next_cursor" root with
            | Some v when v.ValueKind = JsonValueKind.String -> Some(v.GetString() |> Cursor.ofString)
            | _ -> None }

    let paginatedBlocks (doc: JsonDocument) : PaginatedResponse<Block> =
        let root = doc.RootElement

        { results =
            [ for item in root.GetProperty("results").EnumerateArray() do
                  block item ]
          hasMore = root.GetProperty("has_more").GetBoolean()
          nextCursor =
            match tryGetProp "next_cursor" root with
            | Some v when v.ValueKind = JsonValueKind.String -> Some(v.GetString() |> Cursor.ofString)
            | _ -> None }

// ============================================================
// Page Helpers
// ============================================================

module Page =

    let tryGetProperty (key: string) (page: Page) =
        Map.tryFind key page.properties

    let getIconUrl (page: Page) =
        page.icon |> Option.map _.url |> Option.defaultValue ""

    let getCoverUrl (page: Page) =
        page.cover |> Option.map _.url |> Option.defaultValue ""

    let getTitle (key: string) (page: Page) =
        match tryGetProperty key page with
        | Some(PropertyValue.Title texts) -> texts |> List.tryHead |> Option.map _.plainText |> Option.defaultValue ""
        | _ -> ""

    let getText (key: string) (page: Page) =
        match tryGetProperty key page with
        | Some(PropertyValue.RichText texts) ->
            texts |> List.tryHead |> Option.map _.plainText |> Option.defaultValue ""
        | _ -> ""

    let getCheckbox (key: string) (page: Page) =
        match tryGetProperty key page with
        | Some(PropertyValue.Checkbox v) -> v
        | _ -> false

    let getDate (key: string) (page: Page) =
        match tryGetProperty key page with
        | Some(PropertyValue.Date(Some dt)) -> dt
        | _ -> DateTimeOffset.MinValue

    let getStatus (key: string) (page: Page) =
        match tryGetProperty key page with
        | Some(PropertyValue.Status s) -> s
        | _ -> ""

    let getSelect (key: string) (page: Page) =
        match tryGetProperty key page with
        | Some(PropertyValue.Select s) -> s
        | _ -> ""

    let getMultiSelect (key: string) (page: Page) =
        match tryGetProperty key page with
        | Some(PropertyValue.MultiSelect items) -> items |> Array.ofList
        | _ -> Array.empty

// ============================================================
// HTTP Client
// ============================================================

[<Literal>]
let private NotionApiVersion = "2022-06-28"

let private apiUrl (config:Config) (path:string) =
    config.baseUrl.TrimEnd('/') + path

let private createRequest (config: Config) (method: HttpMethod) (url: string) =
    let req = new HttpRequestMessage(method, url)
    req.Headers.Authorization <- AuthenticationHeaderValue("Bearer", config.apiKey)
    req.Headers.Add("Notion-Version", NotionApiVersion)
    req

let private jsonContent (json: string) =
    new StringContent(json, Encoding.UTF8, "application/json")

// ============================================================
// Service
// ============================================================

[<RequireQualifiedAccess>]
type PropertyFilter =
    | StatusEquals of property: string * value: string
    | RichTextEquals of property: string * value: string

module PropertyFilter =
    let toObj (filter: PropertyFilter) : obj =
        match filter with
        | PropertyFilter.StatusEquals(property, value) ->
            {| property = property; status = {| equals = value |} |}
        | PropertyFilter.RichTextEquals(property, value) ->
            {| property = property; rich_text = {| equals = value |} |}

type QueryDatabaseParams =
    { filter: PropertyFilter option
      startCursor: Cursor option }

type Service =
    { queryDatabase: DatabaseId -> QueryDatabaseParams -> Task<PaginatedResponse<Page>>
      retrievePage: PageId -> Task<Page>
      retrieveBlockChildren: BlockId -> Cursor option -> Task<PaginatedResponse<Block>> }

module Service =

    let private queryDatabase
        (telemetry: Telemetry.Service)
        (config: Config)
        (httpClient: HttpClient)
        : DatabaseId -> QueryDatabaseParams -> Task<PaginatedResponse<Page>> =
        fun databaseId queryParams ->
            task {
                let dbId = DatabaseId.toString databaseId
                use span = telemetry.startActiveSpan "domain.notion.query_database"
                span.SetAttribute("database_id", dbId) |> ignore
                let url = apiUrl config $"/databases/{dbId}/query"
                let req = createRequest config HttpMethod.Post url

                let body = dict [
                    match queryParams.filter with
                    | Some f -> "filter", PropertyFilter.toObj f
                    | None -> ()
                    match queryParams.startCursor with
                    | Some c -> "start_cursor", Cursor.toString c :> obj
                    | None -> ()
                ]
                req.Content <- jsonContent (Json.serialize body)
                let! resp = httpClient.SendAsync(req)
                resp.EnsureSuccessStatusCode() |> ignore
                let! body = resp.Content.ReadAsStringAsync()
                use doc = JsonDocument.Parse(body)
                return Parse.paginatedPages doc
            }

    let private retrievePage
        (telemetry: Telemetry.Service)
        (config: Config)
        (httpClient: HttpClient)
        : PageId -> Task<Page> =
        fun pageId ->
            task {
                let pgId = PageId.toString pageId
                use span = telemetry.startActiveSpan "domain.notion.retrieve_page"
                span.SetAttribute("page_id", pgId) |> ignore
                let url = apiUrl config $"/pages/{pgId}"
                let req = createRequest config HttpMethod.Get url
                let! resp = httpClient.SendAsync(req)
                resp.EnsureSuccessStatusCode() |> ignore
                let! body = resp.Content.ReadAsStringAsync()
                use doc = JsonDocument.Parse(body)
                return Parse.page doc.RootElement
            }

    let private retrieveBlockChildren
        (telemetry: Telemetry.Service)
        (config: Config)
        (httpClient: HttpClient)
        : BlockId -> Cursor option -> Task<PaginatedResponse<Block>> =
        fun blockId startCursor ->
            task {
                let blkId = BlockId.toString blockId
                use span = telemetry.startActiveSpan "domain.notion.retrieve_block_children"
                span.SetAttribute("block_id", blkId) |> ignore

                let mutable url = apiUrl config $"/blocks/{blkId}/children?page_size=100"

                match startCursor with
                | Some c -> url <- url + $"&start_cursor={Cursor.toString c}"
                | None -> ()

                let req = createRequest config HttpMethod.Get url
                let! resp = httpClient.SendAsync(req)
                resp.EnsureSuccessStatusCode() |> ignore
                let! body = resp.Content.ReadAsStringAsync()
                use doc = JsonDocument.Parse(body)
                return Parse.paginatedBlocks doc
            }

    let create (config: Config) (telemetry: Telemetry.Service) (httpClient: HttpClient) =
        { queryDatabase = queryDatabase telemetry config httpClient
          retrievePage = retrievePage telemetry config httpClient
          retrieveBlockChildren = retrieveBlockChildren telemetry config httpClient }
