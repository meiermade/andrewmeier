module App.PrivacyPage

open App.Common.View
open FSharp.ViewEngine
open type Html

let metadata : PageMetadata =
    { canonicalPath = "/privacy"
      description = "How andymeier.dev handles browser analytics and analytics preferences."
      title = "Privacy | Andy Meier" }

let page =
    Page.primary (
        main {
            _class "mx-auto max-w-3xl px-4 py-10 sm:py-14"
            header {
                h1 { _class "text-4xl font-medium text-gray-900 dark:text-gray-100"; "Privacy" }
                p {
                    _class "mt-4 text-lg leading-7 text-gray-600 dark:text-gray-400"
                    "This page explains the limited browser analytics used on andymeier.dev and how you can control them."
                }
            }
            section {
                _id "analytics"
                _class "mt-10 scroll-mt-20 border-t border-gray-300/60 pt-8 dark:border-gray-700/60"
                h2 { _class "text-2xl font-medium text-gray-900 dark:text-gray-100"; "Browser analytics" }
                div {
                    _class "mt-4 space-y-4 text-base/7 text-gray-700 dark:text-gray-300"
                    p {
                        "I use limited first-party browser analytics to understand traffic sources, which pages and articles are used, site performance, and browser errors. The data is sent to Meier Made infrastructure using OpenTelemetry."
                    }
                    p {
                        "The analytics events use a random browser-session identifier. They do not include form content, email addresses, phone numbers, raw query strings, full referring URLs, or cross-site tracking identifiers."
                    }
                    p {
                        "In locations where consent is required, analytics remains off until you accept. Elsewhere, limited analytics may run by default. An explicit acceptance or decline applies wherever you visit from, and declining does not affect the site."
                    }
                    p {
                        "A first-party cookie remembers an explicit choice for six months. Session identifiers and sanitized traffic attribution remain in browser session storage only for the current browser session."
                    }
                    p {
                        "You can change or withdraw your choice at any time with the Analytics settings control in the footer. Withdrawal stops subsequent browser telemetry and clears its session storage."
                    }
                }
            }
        }
    )
