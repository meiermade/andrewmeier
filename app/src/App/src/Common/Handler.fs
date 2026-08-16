module App.Common.Handler

open App.Common.View
open App.ServiceRegistry
open Giraffe
open FSharp.ViewEngine
open Microsoft.AspNetCore.Http
open StarFederation.Datastar.DependencyInjection
open System.Text.Json

let patchElement (ds:IDatastarService) (element:HtmlElement) = task {
    let html = Render.toString element
    do! ds.PatchElementsAsync(html)
}

let inline patchSignals (ds:IDatastarService) (signals:'T) = task {
    do! ds.PatchSignalsAsync(signals)
}

let historyScript (url:string) =
    let serializedUrl = JsonSerializer.Serialize url
    $"""window.history.pushState(null, '', {serializedUrl});window.meiermadeTelemetry&&window.meiermadeTelemetry.trackPage();"""

let pushUrl (ds:IDatastarService) (url:string) = task {
    do! ds.ExecuteScriptAsync(historyScript url)
}

let renderPage (services:Services) (page:HtmlElement) (selectedNav:string) : HttpHandler =
    fun next ctx -> task {
        let doc = Document.primary(page, services.config.openTelemetry.publicEndpoint, selectedNav)
        let html = Render.toHtmlDocString doc
        return! htmlString html next ctx
    }
