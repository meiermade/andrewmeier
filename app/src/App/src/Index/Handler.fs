module App.Index.Handler

open App.Infrastructure
open App.ServiceRegistry
open App.Articles
open App.Common.Handler
open App.Index.View
open Giraffe
open Microsoft.AspNetCore.Http
open StarFederation.Datastar.DependencyInjection

let private getHomePage (services:Services) : HttpHandler =
    fun next ctx -> task {
        use _span = services.telemetry.startActiveSpan "app.index.get_home_page"
        let recentArticles = Catalog.all |> List.truncate 3
        let page = homePage recentArticles

        if ctx.IsDatastar then
            let ds = ctx.GetService<IDatastarService>()
            do! patchPage ctx ds metadata page "nav-home"
            return Some ctx
        else
            return! renderPage services metadata page "nav-home" next ctx
    }

let private privacyPolicyCheck expected : HttpHandler =
    fun _ ctx -> task {
        ctx.Response.Headers.CacheControl <- "no-store"
        let policy = App.Privacy.fromRequest ctx
        ctx.Response.StatusCode <-
            match App.Privacy.matchesAnalyticsMode expected policy with
            | Some true -> StatusCodes.Status200OK
            | Some false -> StatusCodes.Status409Conflict
            | None -> StatusCodes.Status400BadRequest
        return Some ctx
    }

let handler (services:Services) : HttpHandler =
    choose [
        route "/health" >=> GET >=> text "Healthy"
        route "/privacy/consent" >=> POST >=> App.Consent.persist
        routef "/privacy/policy-check/%s" (fun expected -> GET >=> privacyPolicyCheck expected)
        route "/privacy" >=> GET >=> renderPage services App.PrivacyPage.metadata App.PrivacyPage.page ""
        routex "(/?)" >=> GET >=> getHomePage services
        route "/services" >=> GET >=> redirectTo true "https://meiermade.com/services"
        route "/projects" >=> GET >=> redirectTo true "https://meiermade.com/projects"
        subRoute "/articles" (App.Articles.Handler.handler services)
    ]
