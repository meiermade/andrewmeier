module App.Articles.Shared

open App.Articles
open App.Common.View
open FSharp.ViewEngine
open type Datastar
open type Html

module ArticleCard =
    let private tag (text: string) = span {
        _class
            "inline-flex items-center rounded-md bg-gray-50 px-2 py-1 text-xs font-medium text-gray-600 ring-1 ring-inset ring-gray-500/10 dark:bg-gray-800 dark:text-gray-300 dark:ring-gray-600"

        text
    }

    let tags (tags: string[]) = div {
        _class "flex flex-wrap gap-2"

        for tag' in tags do
            tag tag'
    }

    let summary (article': Article) =
        let url = SiteUrl.article article'.permalink

        article {
            _class "py-6 border-b border-gray-300/60 dark:border-gray-700/60"

            div {
                _class "flex items-center flex-wrap gap-x-4 gap-y-1 text-sm text-gray-400 dark:text-gray-500"

                div {
                    _class "inline-flex items-center whitespace-nowrap"

                    span {
                        _class "mr-1.5"
                        MiniIcon.calendar
                    }

                    time {
                        _datetime (article'.createdAt.ToString("yyyy-MM-dd"))
                        article'.createdAt.ToString("MMMM d, yyyy")
                    }
                }
            }

            h2 {
                _class "mt-2 text-xl font-semibold tracking-tight text-gray-900 dark:text-gray-100"

                a {
                    _href url
                    _dataOn ("click__prevent", $"@get('{url}')")
                    _class "hover:text-emerald-600 dark:hover:text-emerald-400"
                    article'.title
                }
            }

            p {
                _class "mt-2 text-base text-gray-600 dark:text-gray-400"
                article'.summary
            }

            div {
                _class "mt-4"
                tags article'.tags
            }
        }

module ArticlePage =
    let primary (metadata: ArticleMetadata) (content: HtmlElement list) =
        let page = div {
            div {
                _class "bg-cover bg-no-repeat bg-center bg-blend-overlay bg-gray-800"

                match SafeOutput.tryBackgroundImageStyle metadata.cover with
                | Some style -> _style style
                | None -> ()

                div {
                    _class "pt-28 pb-20 px-4 mx-auto max-w-5xl flex flex-col justify-end items-start text-gray-50"

                    time {
                        _class "text-base text-gray-50 border-l border-gray-300 pl-2"
                        _datetime (metadata.createdAt.ToString("yyyy-MM-dd"))
                        metadata.createdAt.ToString("MMMM d, yyyy")
                    }

                    h1 {
                        _class "mt-4 text-4xl font-bold tracking-tight text-gray-50"
                        metadata.title
                    }

                    div {
                        _class "mt-5"
                        ArticleCard.tags metadata.tags
                    }
                }
            }

            article {
                _class "mx-auto max-w-5xl px-4"

                div {
                    _class
                        "mt-8 pb-8 prose prose-lg dark:prose-invert prose-code:before:hidden prose-code:after:hidden max-w-none"

                    _dataInit "highlightCode($el)"

                    for element in content do
                        element
                }
            }

            script { _src (Asset.fingerprinted "/scripts/prism.1.29.0.js") }
            script { js "function highlightCode(el){if(el?.querySelectorAll)Prism.highlightAllUnder(el)}" }
        }

        Page.primary page
