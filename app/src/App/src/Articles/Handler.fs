module App.Articles.Handler

open App.Infrastructure
open App.ServiceRegistry
open App.Articles.View
open App.Common.Handler
open Giraffe
open Microsoft.AspNetCore.Http
open StarFederation.Datastar.DependencyInjection
open System

let private optionalQueryValue (ctx:HttpContext) name =
    match ctx.Request.Query.TryGetValue name with
    | true, value when not (String.IsNullOrWhiteSpace(string value)) -> Some(string value)
    | _ -> None

let private filtersFromRequest (ctx:HttpContext) =
    { FilterState.search = optionalQueryValue ctx "search"
      tag = optionalQueryValue ctx "tag"
      publishedYear = optionalQueryValue ctx "year" |> Option.bind (fun value ->
          match Int32.TryParse value with
          | true, year -> Some year
          | false, _ -> None) }

let filterArticles (filters:FilterState) (articles:Domain.Article.Article list) =
    let matchesSearch (article:Domain.Article.Article) =
        filters.search
        |> Option.forall (fun query ->
            let query = query.Trim()
            article.title.Contains(query, StringComparison.OrdinalIgnoreCase)
            || article.summary.Contains(query, StringComparison.OrdinalIgnoreCase)
            || (article.tags |> Array.exists (fun tag -> tag.Contains(query, StringComparison.OrdinalIgnoreCase))))

    let matchesTag (article:Domain.Article.Article) =
        filters.tag
        |> Option.forall (fun selectedTag ->
            article.tags |> Array.exists (fun tag -> String.Equals(tag, selectedTag, StringComparison.OrdinalIgnoreCase)))

    let matchesYear (article:Domain.Article.Article) =
        filters.publishedYear
        |> Option.forall (fun year -> article.createdAt.Year = year)

    articles |> List.filter (fun article -> matchesSearch article && matchesTag article && matchesYear article)

let private getArticlesPage (services:Services) : HttpHandler =
    fun next ctx -> task {
        use _span = services.telemetry.startActiveSpan "app.articles.get_articles_page"
        let filters = filtersFromRequest ctx
        let canonicalUrl = FilterState.url filters
        let requestedUrl = ctx.Request.Path.Value + ctx.Request.QueryString.ToString()

        if not ctx.IsDatastar && requestedUrl <> canonicalUrl then
            return! redirectTo false canonicalUrl next ctx
        else
            let! allArticles = services.article.listArticles ()
            let page =
                { articles = filterArticles filters allArticles
                  filters = filters
                  tags = allArticles |> List.collect (fun article -> article.tags |> Array.toList) |> List.distinct |> List.sort
                  years = allArticles |> List.map (fun article -> article.createdAt.Year) |> List.distinct |> List.sortDescending }
                |> articlesPage

            if ctx.IsDatastar then
                let ds = ctx.GetService<IDatastarService>()
                do! patchSignals ds {| selectedNav = "nav-articles" |}
                do! patchElement ds page
                do! pushUrl ds canonicalUrl
                return Some ctx
            else
                return! renderPage services page "nav-articles" next ctx
    }

let private getArticlePage (services:Services) (id:string) : HttpHandler =
    fun next ctx -> task {
        use _span = services.telemetry.startActiveSpan "app.articles.get_article_page"
        match! services.article.tryGetArticle id with
        | Some article ->
            let page = articlePage article
            let url = $"/articles/{article.permalink}"

            if ctx.IsDatastar then
                let ds = ctx.GetService<IDatastarService>()
                do! patchSignals ds {| selectedNav = "nav-articles" |}
                do! patchElement ds page
                do! pushUrl ds url
                return Some ctx
            else
                return! renderPage services page "nav-articles" next ctx
        | None ->
            let page = notFoundPage
            if ctx.IsDatastar then
                let ds = ctx.GetService<IDatastarService>()
                do! patchElement ds page
                return Some ctx
            else
                return! renderPage services page "nav-articles" next ctx
    }

let handler (services:Services) : HttpHandler =
    choose [
        routex "(/?)" >=> GET >=> getArticlesPage services
        routef "/%s" (fun id -> GET >=> getArticlePage services id)
    ]
