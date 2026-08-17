module App.Common.Handler

open App.Common.View
open App.Infrastructure
open App.ServiceRegistry
open FSharp.ViewEngine
open Giraffe
open Microsoft.AspNetCore.Http
open StarFederation.Datastar.DependencyInjection
open System.Text.Json

let patchElement (ds:IDatastarService) (element:HtmlElement) = task {
    do! ds.PatchElementsAsync(Render.toString element)
}

let inline patchSignals (ds:IDatastarService) (signals:'T) = task {
    do! ds.PatchSignalsAsync(signals)
}

let private afterNavigationScript updateHistory =
    $"""{updateHistory}window.scrollTo(0, 0);requestAnimationFrame(function(){{document.getElementById('page-content')?.focus({{preventScroll:true}});window.meiermadeTelemetry?.trackPage();}});"""

let historyScript (url:string) =
    let serializedUrl = JsonSerializer.Serialize url
    afterNavigationScript $"window.history.pushState(null, '', {serializedUrl});"

let private restoreHistoryScript = afterNavigationScript ""

let private updateHistory (ctx:HttpContext) (ds:IDatastarService) (url:string) = task {
    let script = if ctx.IsHistoryRestore then restoreHistoryScript else historyScript url
    do! ds.ExecuteScriptAsync(script)
}

let patchPage (ctx:HttpContext) (ds:IDatastarService) (metadata:PageMetadata) (page:HtmlElement) (selectedNav:string) = task {
    do! patchSignals ds {| selectedNav = selectedNav |}
    for element in PageHead.patchableElements metadata do
        do! patchElement ds element
    do! patchElement ds page
    do! updateHistory ctx ds metadata.canonicalPath
}

let renderPage (services:Services) (metadata:PageMetadata) (page:HtmlElement) (selectedNav:string) : HttpHandler =
    fun next ctx -> task {
        let doc = Document.primary(metadata, page, services.config.openTelemetry.publicEndpoint, selectedNav)
        let html = Render.toHtmlDocString doc
        return! htmlString html next ctx
    }
