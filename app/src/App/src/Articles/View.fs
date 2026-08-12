module App.Articles.View

open App.Articles
open App.Articles.Shared
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

type FilterOption =
    { label: string
      url: string
      selected: bool }

module FilterControl =
    let private optionLink (option:FilterOption) =
        a {
            _href option.url
            _class "group flex w-full items-center justify-between gap-3 px-3 py-2 text-left text-sm text-gray-800 transition hover:bg-gray-100 focus:bg-gray-100 focus-visible:outline-2 focus-visible:outline-inset focus-visible:outline-emerald-600 dark:text-gray-100 dark:hover:bg-gray-700 dark:focus:bg-gray-700 dark:focus-visible:outline-emerald-400"
            span { _class "truncate"; option.label }
            span {
                _class (if option.selected then "text-emerald-600 dark:text-emerald-400" else "invisible")
                _ariaHidden "true"
                "✓"
            }
        }

    let disclosure (name:string) (ariaLabel:string) (selectedLabel:string) (options:FilterOption list) (buttonClass:string) =
        div {
            _class "contents"
            _data ("filter-control", name)
            Disclosure.panel {
                id = $"filter-{name}"
                openSignal = $"{name}FilterOpen"
                triggerLabel = ariaLabel
                rootClass = "relative"
                triggerClass = buttonClass
                triggerContent =
                    span {
                        _class "contents"
                        span { _class "block min-w-0 truncate"; selectedLabel }
                        span {
                            _class "pointer-events-none ml-2 flex size-5 shrink-0 items-center justify-center text-gray-500 dark:text-gray-400"
                            _ariaHidden "true"
                            raw """<svg viewBox="0 0 20 20" fill="currentColor" class="size-4"><path fill-rule="evenodd" d="M5.23 7.21a.75.75 0 0 1 1.06.02L10 11.17l3.71-3.94a.75.75 0 1 1 1.08 1.04l-4.25 4.5a.75.75 0 0 1-1.08 0l-4.25-4.5a.75.75 0 0 1 .02-1.06Z" clip-rule="evenodd" /></svg>"""
                        }
                    }
                panelLabel = ariaLabel
                panelClass = "absolute right-0 top-full z-40 mt-1 max-h-64 w-full min-w-48 overflow-auto rounded-lg border border-gray-300 bg-white py-1 shadow-xl dark:border-gray-600 dark:bg-gray-800"
                panelContent = options |> List.map optionLink
            }
        }

let articlesPage (state:ArticlesPageState) =
    let filters = state.filters
    let hasSearch = filters.search.IsSome
    let hasActiveFilters = filters.tag.IsSome || filters.publishedYear.IsSome
    let hasCriteria = hasSearch || hasActiveFilters
    let clearSearchUrl = FilterState.url { filters with search = None }
    let clearFiltersUrl = FilterState.url { filters with tag = None; publishedYear = None }
    let tagOptions =
        state.tags
        |> List.map (fun tag ->
            { label = tag
              url = FilterState.url { filters with tag = Some tag }
              selected = filters.tag = Some tag })
    let yearOptions =
        state.years
        |> List.map (fun year ->
            { label = string year
              url = FilterState.url { filters with publishedYear = Some year }
              selected = filters.publishedYear = Some year })
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
                    match filters.tag with
                    | Some tag -> input { _type "hidden"; _name "tag"; _value tag }
                    | None -> ()
                    match filters.publishedYear with
                    | Some year -> input { _type "hidden"; _name "year"; _value (string year) }
                    | None -> ()
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
                                        div {
                                            _class "grid gap-1 text-sm"
                                            span { _class "font-medium text-gray-900 dark:text-gray-100"; "Tag" }
                                            FilterControl.disclosure
                                                "tag"
                                                "Tag filter"
                                                "Select tag"
                                                tagOptions
                                                "inline-flex w-full items-center justify-between rounded-md border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition hover:bg-gray-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-600 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-100 dark:hover:bg-gray-700 dark:focus-visible:outline-emerald-400"
                                        }
                                    if filters.publishedYear.IsNone then
                                        div {
                                            _class "grid gap-1 text-sm"
                                            span { _class "font-medium text-gray-900 dark:text-gray-100"; "Published" }
                                            FilterControl.disclosure
                                                "year"
                                                "Published year filter"
                                                "Select year"
                                                yearOptions
                                                "inline-flex w-full items-center justify-between rounded-md border border-gray-300 bg-white px-3 py-2 text-sm text-gray-900 shadow-sm transition hover:bg-gray-50 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-emerald-600 dark:border-gray-600 dark:bg-gray-800 dark:text-gray-100 dark:hover:bg-gray-700 dark:focus-visible:outline-emerald-400"
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
                                    FilterControl.disclosure
                                        "tag"
                                        "Tag filter"
                                        tag
                                        tagOptions
                                        "inline-flex items-center bg-transparent py-1.5 pl-2 pr-1 font-medium text-gray-900 focus-visible:outline-2 focus-visible:outline-inset focus-visible:outline-emerald-600 dark:text-gray-100 dark:focus-visible:outline-emerald-400"
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
                                    FilterControl.disclosure
                                        "year"
                                        "Published year filter"
                                        (string year)
                                        yearOptions
                                        "inline-flex items-center bg-transparent py-1.5 pl-2 pr-1 font-medium text-gray-900 focus-visible:outline-2 focus-visible:outline-inset focus-visible:outline-emerald-600 dark:text-gray-100 dark:focus-visible:outline-emerald-400"
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

let notFoundPage =
    let content =
        div {
            _class "flex flex-col items-center"
            h1 { _class "text-3xl text-gray-800 dark:text-gray-100"; "Could not find page." }
            p { _class "mt-2 text-md text-gray-600 dark:text-gray-400"; "Something went wrong. Try refreshing the page." }
        }
    Page.primary content
