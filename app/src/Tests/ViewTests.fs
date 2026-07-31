module ViewTests

open App.Common.View
open Expecto
open FSharp.ViewEngine
open System.Collections.Generic
open type Html

[<Tests>]
let tests =
    testList "View" [
        testList "Asset" [
            test "uses fingerprinted path from manifest when present" {
                let manifest = Dictionary<string, string>()
                manifest.Add("/css/compiled.css", "/css/compiled.abc123.css")

                let path = Asset.resolveWithManifest manifest "/css/compiled.css"

                Expect.equal path "/css/compiled.abc123.css" "Expected manifest fingerprinted path"
            }

            test "falls back to original path when manifest entry is missing" {
                let manifest = Dictionary<string, string>()
                manifest.Add("/css/other.css", "/css/other.abc123.css")

                let path = Asset.resolveWithManifest manifest "/css/compiled.css"

                Expect.equal path "/css/compiled.css" "Expected original path when manifest entry is missing"
            }
        ]

        testList "Personal site separation" [
            test "links to Meier Made without retaining company services or projects" {
                let navigation = Render.toHtmlDocString (TopNav.primary)
                let profile = Render.toHtmlDocString App.Index.View.aboutMe

                Expect.stringContains navigation "https://meiermade.com" "Expected a Meier Made company link"
                Expect.isFalse (navigation.Contains "Services") "Expected company services to leave personal navigation"
                Expect.isFalse (navigation.Contains "Projects") "Expected company projects to leave personal navigation"
                Expect.isFalse (profile.Contains "Currently working at") "Expected current-employer claim to be removed"
            }
        ]

        testList "Articles" [
            test "uses accessible custom listbox controls for article filters" {
                let page =
                    App.Articles.View.articlesPage {
                        articles = []
                        filters = { search = None; tag = None; publishedYear = None }
                        tags = [ "Finance" ]
                        years = [ 2026 ]
                    }

                let html = Render.toHtmlDocString page

                Expect.stringContains html "<el-select" "Expected Tailwind Plus filter select"
                Expect.stringContains html "<el-options" "Expected accessible options container"
                Expect.stringContains html "<el-option" "Expected accessible filter options"
                Expect.isFalse (html.Contains "<select") "Expected article filters not to render native selects"
            }

            test "encodes filter values as HTML attributes instead of JavaScript literals" {
                let page =
                    App.Articles.View.articlesPage {
                        articles = []
                        filters = { search = None; tag = None; publishedYear = None }
                        tags = [ "Andy's \"Notes\"" ]
                        years = [ 2026 ]
                    }

                let html = Render.toHtmlDocString page

                Expect.stringContains html "value=\"Andy&#39;s &quot;Notes&quot;\"" "Expected safely encoded option value"
                Expect.isFalse (html.Contains "data-select-root") "Expected no generated JavaScript filter literals"
            }

            test "identifies articles as personal writing" {
                let page =
                    App.Articles.View.articlesPage {
                        articles = []
                        filters = { search = None; tag = None; publishedYear = None }
                        tags = []
                        years = []
                    }
                    |> Render.toHtmlDocString

                Expect.stringContains page "Personal notes by Andy Meier. Unless otherwise noted, they reflect my own views—not those of clients, collaborators, or organizations I work with." "Expected personal-writing subtitle"
                Expect.isFalse (page.Contains "My thoughts on finance and technology") "Expected superseded subtitle to be removed"
            }

            test "shows article tags on the detail page" {
                let article : Domain.Article.Article =
                    { id = "article"
                      permalink = "article"
                      title = "Example article"
                      summary = "Summary"
                      icon = ""
                      iconDescription = ""
                      cover = ""
                      coverDescription = ""
                      tags = [| "Engineering"; "Finance" |]
                      createdAt = System.DateTimeOffset(2026, 2, 1, 0, 0, 0, System.TimeSpan.Zero)
                      updatedAt = System.DateTimeOffset(2026, 2, 1, 0, 0, 0, System.TimeSpan.Zero)
                      blocks = []
                      syncedAt = System.DateTimeOffset(2026, 2, 1, 0, 0, 0, System.TimeSpan.Zero) }

                let html = App.Articles.View.articlePage article |> Render.toHtmlDocString

                Expect.stringContains html "Engineering" "Expected article tag on detail page"
                Expect.stringContains html "Finance" "Expected article tag on detail page"
            }

            test "scopes clear actions to search or added filters" {
                let searchOnly =
                    App.Articles.View.articlesPage {
                        articles = []
                        filters = { search = Some "engine"; tag = None; publishedYear = None }
                        tags = [ "Engineering" ]
                        years = [ 2026 ]
                    }
                    |> Render.toHtmlDocString

                let searchAndFilter =
                    App.Articles.View.articlesPage {
                        articles = []
                        filters = { search = Some "engine"; tag = Some "Engineering"; publishedYear = None }
                        tags = [ "Engineering" ]
                        years = [ 2026 ]
                    }
                    |> Render.toHtmlDocString

                Expect.stringContains searchOnly "Clear search" "Expected a dedicated search clear action"
                Expect.isFalse (searchOnly.Contains "Clear filters") "Expected filter clear action only after adding a filter"
                Expect.isFalse (searchOnly.Contains "type=\"search\"") "Expected no browser-native search clear control"
                Expect.stringContains searchAndFilter "Clear filters" "Expected filters-only clear action"
                Expect.stringContains searchAndFilter "href=\"/articles?search=engine\"" "Expected filter clearing to preserve search"
            }
        ]

        testList "Document" [
            test "includes consent banner and delayed analytics loading" {
                let doc = Document.primary(div { "Hello" }, "G-TEST123", "nav-home")

                let html = Render.toHtmlDocString doc

                Expect.stringContains html "<title>Andy Meier</title>" "Expected page to render"
                Expect.stringContains html "selectedNav: 'nav-home'" "Expected nav signal to render"
                Expect.stringContains html "cookie-consent-banner" "Expected consent banner"
                Expect.stringContains html "analytics-consent-title" "Expected consent dialog title"
                Expect.stringContains html "Optional analytics" "Expected Meier Made-style consent heading"
                Expect.stringContains html "rounded-2xl" "Expected compact consent card"
                Expect.stringContains html "Decline" "Expected decline action"
                Expect.stringContains html "Accept analytics" "Expected accept action"
                Expect.stringContains html "gtag('consent','default',{analytics_storage:'denied'" "Expected denied-by-default consent mode"
                Expect.stringContains html "localStorage.setItem('analytics-consent',v)" "Expected consent to be persisted"
                Expect.stringContains html "https://www.googletagmanager.com/gtag/js?id=G-TEST123" "Expected deferred gtag script source"
                Expect.stringContains html "gtag('config','G-TEST123');" "Expected GA config call after consent"
                Expect.stringContains html "https://cdn.jsdelivr.net/npm/@snowplow/javascript-tracker@4/dist/sp.min.js" "Expected deferred Snowplow tracker source"
                Expect.stringContains html "https://c.andymeier.dev" "Expected Snowplow collector endpoint"
                Expect.stringContains html "postPath:'/i/v1'" "Expected custom Snowplow post path"
                Expect.stringContains html "appId:'andymeier-dev'" "Expected Snowplow app id"
            }
        ]
    ]
