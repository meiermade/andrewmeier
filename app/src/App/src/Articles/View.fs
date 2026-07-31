module App.Articles.View

open Domain.Article
open FSharp.ViewEngine
open App.Common.View
open System
open type Datastar
open type Html

type FilterState =
    { search: string option
      tag: string option
      publishedYear: int option }

module FilterState =
    let url (filters:FilterState) =
        [ "search", filters.search
          "tag", filters.tag
          "year", filters.publishedYear |> Option.map string ]
        |> List.choose (fun (key, value) ->
            value
            |> Option.map _.Trim()
            |> Option.filter (String.IsNullOrWhiteSpace >> not)
            |> Option.map (fun value -> $"{Uri.EscapeDataString key}={Uri.EscapeDataString value}"))
        |> String.concat "&"
        |> function
            | "" -> "/articles"
            | query -> $"/articles?{query}"

type ArticlesPageState =
    { articles: Article list
      filters: FilterState
      tags: string list
      years: int list }

module FilterControl =
    let private json (value:string) =
        System.Text.Json.JsonSerializer.Serialize value
        |> fun serialized -> serialized.Replace("\"", "'")

    let select (name:string) (options:(string * string) list) (selected:string) (ariaLabel:string) (onChange:string) (buttonClass:string) =
        let selectedValue =
            options
            |> List.tryFind (fun (value, _) -> value = selected)
            |> Option.orElse (options |> List.tryHead)
            |> Option.map fst
            |> Option.defaultValue ""
        let selectedLabel =
            options
            |> List.tryFind (fun (value, _) -> value = selectedValue)
            |> Option.map snd
            |> Option.defaultValue ""
        let prefix = "article_filter_" + Guid.NewGuid().ToString("N")
        let valueSignal = prefix + "Value"
        let labelSignal = prefix + "Label"
        let openSignal = prefix + "Open"
        let buttonId = prefix + "Button"
        let optionsId = prefix + "Options"

        div {
            _class "relative"
            _data ("select-root", "")
            _data ("signals", $"{{ {valueSignal}: {json selectedValue}, {labelSignal}: {json selectedLabel}, {openSignal}: false }}")
            _data ("on:keydown__window", $"evt.key == 'Escape' && (${openSignal} = false)")
            input { _type "hidden"; _name name; _value selectedValue }
            button {
                _id buttonId
                _type "button"
                _role "combobox"
                _ariaHaspopup "listbox"
                _ariaLabel ariaLabel
                _attr ("aria-controls", optionsId)
                _data ("select-button", "")
                _data ("attr:aria-expanded", $"${openSignal} ? 'true' : 'false'")
                _data ("on:click__stop", $"${openSignal} = !${openSignal}")
                _class buttonClass
                span { _class "block min-w-0 truncate"; _data ("text", $"${labelSignal}"); selectedLabel }
                span {
                    _class "pointer-events-none ml-2 flex size-5 shrink-0 items-center justify-center text-gray-500 dark:text-gray-400"
                    _ariaHidden "true"
                    raw """<svg viewBox="0 0 20 20" fill="currentColor" class="size-4"><path fill-rule="evenodd" d="M5.23 7.21a.75.75 0 0 1 1.06.02L10 11.17l3.71-3.94a.75.75 0 1 1 1.08 1.04l-4.25 4.5a.75.75 0 0 1-1.08 0l-4.25-4.5a.75.75 0 0 1 .02-1.06Z" clip-rule="evenodd" /></svg>"""
                }
            }
            div {
                _id optionsId
                _role "listbox"
                _attr ("aria-labelledby", buttonId)
                _data ("select-options", "")
                _data ("show", $"${openSignal}")
                _data ("on:click__outside", $"${openSignal} = false")
                _style "display:none"
                _class "absolute right-0 top-full z-40 mt-1 min-w-full overflow-hidden rounded-lg border border-gray-300 bg-white py-1 shadow-xl dark:border-gray-600 dark:bg-gray-800"
                for index, (value, label) in options |> List.indexed do
                    button {
                        _id $"{prefix}Option{index}"
                        _type "button"
                        _role "option"
                        _tabindex -1
                        _data ("select-option", "")
                        _data ("attr:aria-selected", $"${valueSignal} == {json value} ? 'true' : 'false'")
                        _data ("on:click", $"${valueSignal} = {json value}; ${labelSignal} = {json label}; el.closest('[data-select-root]').querySelector('input[type=hidden]').value = {json value}; ${openSignal} = false; {onChange}")
                        _data ("class", $"{{ 'bg-gray-100 font-semibold dark:bg-gray-700': ${valueSignal} == {json value} }}")
                        _class "flex w-full items-center justify-between gap-3 px-3 py-2 text-left text-sm text-gray-800 transition hover:bg-gray-100 focus-visible:outline-2 focus-visible:outline-inset focus-visible:outline-emerald-600 dark:text-gray-100 dark:hover:bg-gray-700"
                        span { _class "truncate"; label }
                        span {
                            _class "text-emerald-600 dark:text-emerald-400"
                            _ariaHidden "true"
                            _data ("show", $"${valueSignal} == {json value}")
                            "✓"
                        }
                    }
            }
        }

let articlesPage (state:ArticlesPageState) =
    let filters = state.filters
    let hasSearch = filters.search.IsSome
    let hasActiveFilters = filters.tag.IsSome || filters.publishedYear.IsSome
    let hasCriteria = hasSearch || hasActiveFilters
    let clearSearchUrl = FilterState.url { filters with search = None }
    let clearFiltersUrl = FilterState.url { filters with tag = None; publishedYear = None }
    let searchInputClass =
        "w-full rounded-md border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm outline-none transition placeholder:text-gray-400 focus:border-emerald-600 focus:ring-2 focus:ring-emerald-600/20 dark:border-gray-600 dark:bg-gray-900 dark:text-gray-100 dark:placeholder:text-gray-500 dark:focus:border-emerald-400 dark:focus:ring-emerald-400/20"
        + if hasSearch then " pr-10" else ""

    let content =
        div {
            _class "mx-auto max-w-5xl px-4 py-10"
            header {
                h1 { _class "text-4xl font-medium text-gray-900 dark:text-gray-100"; "Articles" }
                p {
                    _class "mt-4 max-w-2xl text-lg leading-7 text-gray-600 dark:text-gray-400"
                    "Personal notes by Andy Meier. Unless otherwise noted, they reflect my own views—not those of clients, collaborators, or organizations I work with."
                }
            }
            section {
                _ariaLabel "Article search and filters"
                _class "mt-10"
                form {
                    _method "get"
                    _action "/articles"
                    _class "border-b border-gray-300/60 py-4 dark:border-gray-700/60"
                    div {
                        _class "flex flex-col gap-2 sm:flex-row sm:items-center"
                        label {
                            _class "min-w-0 flex-1"
                            span { _class "sr-only"; "Search articles" }
                            div {
                                _class "relative"
                                input {
                                    _type "text"
                                    _role "searchbox"
                                    _name "search"
                                    _ariaLabel "Search articles"
                                    _value (filters.search |> Option.defaultValue "")
                                    _placeholder "Search articles"
                                    _class searchInputClass
                                }
                                if hasSearch then
                                    a {
                                        _href clearSearchUrl
                                        _ariaLabel "Clear search"
                                        _class "absolute inset-y-0 right-0 inline-flex items-center px-3 text-gray-400 transition hover:text-gray-900 focus-visible:outline-2 focus-visible:outline-inset focus-visible:outline-emerald-600 dark:text-gray-500 dark:hover:text-gray-100 dark:focus-visible:outline-emerald-400"
                                        span { _class "sr-only"; "Clear search" }
                                        raw """<svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5" class="size-4" aria-hidden="true"><path stroke-linecap="round" stroke-linejoin="round" d="m6 6 8 8m0-8-8 8" /></svg>"""
                                    }
                            }
                        }
                        if filters.tag.IsNone || filters.publishedYear.IsNone then
                            details {
                                _class "group relative shrink-0"
                                summary {
                                    _class "flex cursor-pointer list-none items-center justify-center rounded-md border border-gray-300 bg-white px-3 py-2 text-sm font-semibold text-gray-800 shadow-sm transition hover:bg-gray-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-600 dark:border-gray-600 dark:bg-gray-900 dark:text-gray-100 dark:hover:bg-gray-800"
                                    "+ Add filter"
                                }
                                div {
                                    _class "z-30 mt-2 grid w-full gap-3 rounded-lg border border-gray-300 bg-white p-3 shadow-xl sm:absolute sm:right-0 sm:w-auto sm:min-w-64 dark:border-gray-700 dark:bg-gray-900"
                                    if filters.tag.IsNone then
                                        label {
                                            _class "grid gap-1 text-sm"
                                            span { _class "font-medium text-gray-900 dark:text-gray-100"; "Tag" }
                                            FilterControl.select
                                                "tag"
                                                (("", "Select tag") :: (state.tags |> List.map (fun tag -> tag, tag)))
                                                ""
                                                "Tag filter"
                                                "el.closest('form').requestSubmit()"
                                                "inline-flex w-full items-center justify-between rounded-md border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm outline-none transition hover:bg-gray-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-600 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-100 dark:hover:bg-gray-700 dark:focus-visible:outline-emerald-400"
                                        }
                                    if filters.publishedYear.IsNone then
                                        label {
                                            _class "grid gap-1 text-sm"
                                            span { _class "font-medium text-gray-900 dark:text-gray-100"; "Published" }
                                            FilterControl.select
                                                "year"
                                                (("", "Select year") :: (state.years |> List.map (fun year -> string year, string year)))
                                                ""
                                                "Published year filter"
                                                "el.closest('form').requestSubmit()"
                                                "inline-flex w-full items-center justify-between rounded-md border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm outline-none transition hover:bg-gray-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-600 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-100 dark:hover:bg-gray-700 dark:focus-visible:outline-emerald-400"
                                        }
                                }
                            }
                        button { _type "submit"; _class "sr-only"; "Search" }
                    }
                    if hasActiveFilters then
                        div {
                            _class "mt-3 flex flex-wrap items-center gap-2"
                            match filters.tag with
                            | Some tag ->
                                div {
                                    _class "inline-flex items-center rounded-md border border-gray-300 bg-gray-50 text-sm dark:border-gray-600 dark:bg-gray-800"
                                    span { _class "border-r border-gray-300 px-2 py-1.5 font-medium text-gray-500 dark:border-gray-600 dark:text-gray-400"; "Tag" }
                                    FilterControl.select
                                        "tag"
                                        (state.tags |> List.map (fun tagOption -> tagOption, tagOption))
                                        tag
                                        "Tag filter"
                                        "el.closest('form').requestSubmit()"
                                        "inline-flex items-center bg-transparent py-1.5 pl-2 pr-1 font-medium text-gray-900 outline-none focus-visible:outline-2 focus-visible:outline-inset focus-visible:outline-emerald-600 dark:text-gray-100 dark:focus-visible:outline-emerald-400"
                                    a {
                                        _href (FilterState.url { filters with tag = None })
                                        _ariaLabel "Remove tag filter"
                                        _class "inline-flex self-stretch items-center border-l border-gray-300 px-2 text-gray-500 transition hover:bg-gray-100 hover:text-gray-900 dark:border-gray-600 dark:text-gray-400 dark:hover:bg-gray-700 dark:hover:text-gray-100"
                                        "×"
                                    }
                                }
                            | None -> ()
                            match filters.publishedYear with
                            | Some year ->
                                div {
                                    _class "inline-flex items-center rounded-md border border-gray-300 bg-gray-50 text-sm dark:border-gray-600 dark:bg-gray-800"
                                    span { _class "border-r border-gray-300 px-2 py-1.5 font-medium text-gray-500 dark:border-gray-600 dark:text-gray-400"; "Published" }
                                    FilterControl.select
                                        "year"
                                        (state.years |> List.map (fun yearOption -> string yearOption, string yearOption))
                                        (string year)
                                        "Published year filter"
                                        "el.closest('form').requestSubmit()"
                                        "inline-flex items-center bg-transparent py-1.5 pl-2 pr-1 font-medium text-gray-900 outline-none focus-visible:outline-2 focus-visible:outline-inset focus-visible:outline-emerald-600 dark:text-gray-100 dark:focus-visible:outline-emerald-400"
                                    a {
                                        _href (FilterState.url { filters with publishedYear = None })
                                        _ariaLabel "Remove published year filter"
                                        _class "inline-flex self-stretch items-center border-l border-gray-300 px-2 text-gray-500 transition hover:bg-gray-100 hover:text-gray-900 dark:border-gray-600 dark:text-gray-400 dark:hover:bg-gray-700 dark:hover:text-gray-100"
                                        "×"
                                    }
                                }
                            | None -> ()
                            a {
                                _href clearFiltersUrl
                                _class "px-1 py-1.5 text-sm font-medium text-gray-500 transition hover:text-gray-900 hover:underline dark:text-gray-400 dark:hover:text-gray-100"
                                "Clear filters"
                            }
                        }
                }
                if state.articles.IsEmpty then
                    div {
                        _class "py-12 text-center"
                        h2 { _class "text-lg font-semibold text-gray-900 dark:text-gray-100"; "No articles found" }
                        if hasCriteria then
                            p { _class "mt-2 text-sm text-gray-600 dark:text-gray-400"; "Try changing or clearing your search and filters." }
                        else
                            p { _class "mt-2 text-sm text-gray-600 dark:text-gray-400"; "Articles will appear here when they are published." }
                    }
                else
                    div {
                        _class "flex flex-col"
                        for article in state.articles do ArticleCard.summary article
                    }
            }
        }
    Page.primary content

module RichTextView =
    let private notionBlockLinkRegex = System.Text.RegularExpressions.Regex(@"^/[0-9a-f]{32}#([0-9a-f]{32})$")

    let private toAnchorHref (href:string) =
        let m = notionBlockLinkRegex.Match(href)
        if m.Success then $"#{m.Groups.[1].Value}"
        else href

    let toHtml (t: Domain.Notion.RichText) =
        let inner =
            if t.annotations.code then
                code { _class "language-none"; t.plainText }
            else
                span {
                    _class [
                        if t.annotations.bold then "font-bold"
                        if t.annotations.italic then "italic"
                        if t.annotations.underline then "underline"
                        if t.annotations.strikethrough then "line-through"
                    ]
                    t.plainText
                }

        match t.href with
        | Some href -> a { _href (toAnchorHref href); inner }
        | None -> inner

module Block =
    let (|Bulleted|Numbered|Other|) (block:Domain.Notion.Block) =
        match block.blockType with
        | Domain.Notion.BlockType.BulletedListItem _ -> Bulleted
        | Domain.Notion.BlockType.NumberedListItem _ -> Numbered
        | _ -> Other

    let rec toHtml (block:Domain.Notion.Block) : HtmlElement =
        let cleanId = block.id.Replace("-", "")

        match block.blockType with
        | Domain.Notion.BlockType.Heading1 richText ->
            h2 {
                _class "mt-8"; _id cleanId
                for t in richText do RichTextView.toHtml t
            }
        | Domain.Notion.BlockType.Heading2 richText ->
            h3 {
                _class "mt-6"; _id cleanId
                for t in richText do RichTextView.toHtml t
            }
        | Domain.Notion.BlockType.Heading3 richText ->
            h4 {
                _class "mt-4"; _id cleanId
                for t in richText do RichTextView.toHtml t
            }
        | Domain.Notion.BlockType.Paragraph richText ->
            div {
                if List.isEmpty richText then br
                else for t in richText do RichTextView.toHtml t
            }
        | Domain.Notion.BlockType.BulletedListItem(richText, children) ->
            li {
                for t in richText do RichTextView.toHtml t
                for child in children do toHtml child
            }
        | Domain.Notion.BlockType.NumberedListItem(richText, children) ->
            li {
                for t in richText do RichTextView.toHtml t
                for child in children do toHtml child
            }
        | Domain.Notion.BlockType.Code(richText, language) ->
            let language =
                match language with
                | "f#" -> "fsharp"
                | "JSON" -> "json"
                | "TOML" -> "toml"
                | other -> other
            pre {
                _class $"language-{language}"
                code {
                    _class $"language-{language}"
                    for t in richText do RichTextView.toHtml t
                }
            }
        | Domain.Notion.BlockType.Image url ->
            img { _class "drop-shadow-xl rounded"; _src url }
        | Domain.Notion.BlockType.Divider ->
            div { _class "border-b-2 border-gray-300/60 dark:border-gray-700/60" }
        | Domain.Notion.BlockType.Quote richText ->
            blockquote { for t in richText do RichTextView.toHtml t }
        | Domain.Notion.BlockType.Callout richText ->
            div {
                _class "bg-gray-200 dark:bg-gray-800 rounded p-2"
                for t in richText do RichTextView.toHtml t
            }
        | Domain.Notion.BlockType.Unsupported -> empty

module Content =
    let toHtml (blocks:Domain.Notion.Block list) =
        let elements = ResizeArray<HtmlElement>()
        let bulletedListItems = ResizeArray<HtmlElement>()
        let numberedListItems = ResizeArray<HtmlElement>()

        let flushBulletedListItems () =
            let children = List.ofSeq bulletedListItems
            let unorderedList = ul { _class "list-disc"; for c in children do c }
            elements.Add(unorderedList)
            bulletedListItems.Clear()

        let flushNumberedListItems () =
            let children = List.ofSeq numberedListItems
            let orderedList = ol { _class "list-decimal"; for c in children do c }
            elements.Add(orderedList)
            numberedListItems.Clear()

        for block in blocks do
            match block with
            | Block.Bulleted ->
                if numberedListItems.Count > 0 then flushNumberedListItems()
                bulletedListItems.Add(Block.toHtml block)
            | Block.Numbered ->
                if bulletedListItems.Count > 0 then flushBulletedListItems()
                numberedListItems.Add(Block.toHtml block)
            | Block.Other ->
                if numberedListItems.Count > 0 then flushNumberedListItems()
                if bulletedListItems.Count > 0 then flushBulletedListItems()
                elements.Add(Block.toHtml block)

        if numberedListItems.Count > 0 then flushNumberedListItems()
        if bulletedListItems.Count > 0 then flushBulletedListItems()
        List.ofSeq elements

let articlePage (article':Article) =
    let content =
        div {
            div {
                _class "bg-cover bg-no-repeat bg-center bg-blend-overlay bg-gray-800"
                _style $"background-image: url('{article'.cover}')"
                div {
                    _class "pt-28 pb-20 px-4 mx-auto max-w-5xl flex flex-col justify-end items-start text-gray-50"
                    time {
                        _class "text-base text-gray-50 border-l border-gray-300 pl-2"
                        _datetime (article'.createdAt.ToString("yyyy-MM-dd"))
                        article'.createdAt.ToString("MMMM d, yyyy")
                    }
                    h1 {
                        _class "mt-4 text-4xl font-bold tracking-tight text-gray-50"
                        article'.title
                    }
                    div {
                        _class "mt-5"
                        ArticleCard.tags article'.tags
                    }
                }
            }
            article {
                _class "mx-auto max-w-5xl px-4"
                div {
                    _class "mt-8 prose prose-lg dark:prose-invert prose-code:before:hidden prose-code:after:hidden max-w-none"
                    _dataInit "highlightCode($el)"
                    for el in Content.toHtml article'.blocks do el
                }
            }
            script { _src (Asset.fingerprinted "/scripts/prism.1.29.0.js") }
            script { js "function highlightCode(el){if(el?.querySelectorAll)Prism.highlightAllUnder(el)}" }
        }
    Page.primary content

let notFoundPage =
    let content =
        div {
            _class "flex flex-col items-center"
            h1 { _class "text-3xl text-gray-800 dark:text-gray-100"; "Could not find page." }
            p { _class "mt-2 text-md text-gray-600 dark:text-gray-400"; "Something went wrong. Try refreshing the page." }
        }
    Page.primary content
