module App.Index.Handler

open App.Infrastructure
open App.ServiceRegistry
open App.Articles
open App.Common.Handler
open App.Index.View
open Giraffe
open StarFederation.Datastar.DependencyInjection

let private getHomePage (services:Services) : HttpHandler =
    fun next ctx -> task {
        use _span = services.telemetry.startActiveSpan "app.index.get_home_page"
        let recentArticles = Catalog.all |> List.truncate 3
        let page = homePage recentArticles

        if ctx.IsDatastar then
            let ds = ctx.GetService<IDatastarService>()
            do! patchSignals ds {| selectedNav = "nav-home" |}
            do! patchElement ds page
            do! pushUrl ds "/"
            return Some ctx
        else
            return! renderPage services page "nav-home" next ctx
    }

let handler (services:Services) : HttpHandler =
    choose [
        route "/health" >=> GET >=> text "Healthy"
        route "/privacy/consent" >=> POST >=> App.Consent.persist
        routex "(/?)" >=> GET >=> getHomePage services
        route "/services" >=> GET >=> redirectTo true "https://meiermade.com/services"
        route "/projects" >=> GET >=> redirectTo true "https://meiermade.com/projects"
        subRoute "/articles" (App.Articles.Handler.handler services)
    ]
