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

            test "uses Datastar navigation disclosures without emulated menus" {
                let navigation = Render.toHtmlDocString TopNav.primary

                Expect.stringContains navigation "data-disclosure-root" "Expected hand-rolled Datastar disclosures"
                Expect.stringContains navigation "aria-expanded" "Expected accessible disclosure triggers"
                Expect.stringContains navigation "aria-controls" "Expected triggers to identify disclosure panels"
                Expect.isFalse (navigation.Contains "role=\"menu\"") "Expected native links and buttons instead of emulated menus"
                Expect.isFalse (navigation.Contains "role=\"menuitem") "Expected native links and buttons instead of emulated menu items"
                Expect.isFalse (navigation.Contains "<el-") "Expected no Tailwind Elements navigation"
            }
        ]

        testList "Articles" [
            test "uses accessible Datastar disclosures with canonical filter links" {
                let page =
                    App.Articles.View.articlesPage {
                        articles = []
                        filters = { search = None; tag = None; publishedYear = None }
                        tags = [ "Finance" ]
                        years = [ 2026 ]
                    }

                let html = Render.toHtmlDocString page

                Expect.stringContains html "data-filter-control=\"tag\"" "Expected a hand-rolled Datastar tag disclosure"
                Expect.stringContains html "data-filter-control=\"year\"" "Expected a hand-rolled Datastar year disclosure"
                Expect.stringContains html "href=\"/articles?tag=Finance\"" "Expected canonical tag filter link"
                Expect.stringContains html "href=\"/articles?year=2026\"" "Expected canonical year filter link"
                Expect.isFalse (html.Contains "role=\"combobox\"") "Expected no emulated combobox"
                Expect.isFalse (html.Contains "role=\"listbox\"") "Expected no emulated listbox"
                Expect.isFalse (html.Contains "data-select-root") "Expected no custom select state machine"
                Expect.isFalse (html.Contains "<el-") "Expected no Tailwind Elements controls"
                Expect.isFalse (html.Contains "<select") "Expected article filters not to render native selects"
            }

            test "encodes filter values as HTML attributes instead of JavaScript literals" {
                let unsafeValue = "Andy's \"Notes\""
                let page =
                    App.Articles.View.articlesPage {
                        articles = []
                        filters = { search = None; tag = None; publishedYear = None }
                        tags = [ unsafeValue ]
                        years = [ 2026 ]
                    }

                let html = Render.toHtmlDocString page
                let expressions =
                    System.Text.RegularExpressions.Regex.Matches(html, "data-on:[^=]+=\"([^\"]*)\"")
                    |> Seq.cast<System.Text.RegularExpressions.Match>
                    |> Seq.map _.Groups.[1].Value
                    |> String.concat "\n"

                Expect.stringContains html "Andy&#39;s &quot;Notes&quot;" "Expected safely encoded option text"
                Expect.isFalse (expressions.Contains unsafeValue) "Expected user-derived values to stay out of Datastar expressions"
                Expect.isFalse (expressions.Contains "Andy&#39;s") "Expected encoded user-derived values to stay out of Datastar expressions"
                Expect.isFalse (html.Contains "data-select-root") "Expected no custom select state machine"
            }

            test "does not let article permalinks escape attributes or Datastar expressions" {
                let unsafePermalink = "\" onmouseover=\"alert(1)"
                let article : Domain.Article.Article =
                    { id = "article"
                      permalink = unsafePermalink
                      title = "Example article"
                      summary = "Summary"
                      icon = ""
                      iconDescription = ""
                      cover = ""
                      coverDescription = ""
                      tags = [||]
                      createdAt = System.DateTimeOffset(2026, 2, 1, 0, 0, 0, System.TimeSpan.Zero)
                      updatedAt = System.DateTimeOffset(2026, 2, 1, 0, 0, 0, System.TimeSpan.Zero)
                      blocks = []
                      syncedAt = System.DateTimeOffset(2026, 2, 1, 0, 0, 0, System.TimeSpan.Zero) }

                let html = ArticleCard.summary article |> Render.toHtmlDocString

                Expect.isFalse (html.Contains "onmouseover=") "Expected permalink not to create an HTML event attribute"
                Expect.stringContains html "/articles/%22%20onmouseover%3D%22alert%281%29" "Expected permalink to be encoded as one URL segment"
            }

            test "allows safe Notion links without rendering unsafe URL schemes or attributes" {
                let richText href : Domain.Notion.RichText =
                    { plainText = "Example"
                      href = Some href
                      annotations =
                        { bold = false
                          italic = false
                          strikethrough = false
                          underline = false
                          code = false
                          color = "default" } }

                let safeHtml = App.Articles.View.RichTextView.toHtml (richText "https://example.com/?a=1&b=2") |> Render.toHtmlDocString
                let unsafeHtml = App.Articles.View.RichTextView.toHtml (richText "javascript:alert(1)\" onclick=\"alert(2)") |> Render.toHtmlDocString

                Expect.stringContains safeHtml "href=\"https://example.com/?a=1&amp;b=2\"" "Expected a safe, encoded HTTPS link"
                Expect.isFalse (unsafeHtml.Contains "<a") "Expected unsafe URL schemes to render as plain text"
                Expect.isFalse (unsafeHtml.Contains "onclick=") "Expected Notion content not to create an event attribute"
            }

            test "restricts image and background URLs to encoded HTTP resources" {
                let safeImage = SafeOutput.tryImageAttribute "https://example.com/image.png?a=1&b=2"
                let unsafeImage = SafeOutput.tryImageAttribute "javascript:alert(1)"
                let background = SafeOutput.tryBackgroundImageStyle "https://example.com/image.png?value=')"

                Expect.equal safeImage (Some "https://example.com/image.png?a=1&amp;b=2") "Expected an encoded HTTPS image URL"
                Expect.isNone unsafeImage "Expected an unsafe image URL scheme to be rejected"
                Expect.isSome background "Expected an HTTPS background image"
                Expect.isFalse (background.Value.Contains "')") "Expected CSS delimiters to be encoded"
                Expect.stringContains background.Value "%27%29" "Expected quote and parenthesis CSS characters to be encoded"
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

        testList "Navigation" [
            test "serializes pushed URLs as JavaScript data" {
                let script = App.Common.Handler.historyScript "');alert(1)//"

                Expect.isFalse (script.Contains "pushState(null, '', '');alert(1)//')") "Expected URL not to break out of the JavaScript string"
                Expect.stringContains script "\\u0027);alert(1)//" "Expected the quote to be JavaScript encoded"
                Expect.stringContains script "window.history.pushState" "Expected history update script"
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
                Expect.isFalse (html.Contains "tailwindplus-elements") "Expected no Tailwind Elements runtime"
                Expect.isFalse (html.Contains "const itemsFor") "Expected no global interaction state machine"
                Expect.isFalse (html.Contains "data-select-root") "Expected no custom select state machine"
                Expect.isFalse (html.Contains "<el-") "Expected no Tailwind Elements markup"
            }
        ]
    ]
