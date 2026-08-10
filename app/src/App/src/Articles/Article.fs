namespace App.Articles

open FSharp.ViewEngine
open System

type ArticleMetadata =
    { permalink: string
      title: string
      summary: string
      cover: string
      tags: string[]
      createdAt: DateTimeOffset }

type Article =
    { permalink: string
      title: string
      summary: string
      cover: string
      tags: string[]
      createdAt: DateTimeOffset
      page: HtmlElement }

module Article =
    let create (metadata: ArticleMetadata) (page: HtmlElement) =
        { permalink = metadata.permalink
          title = metadata.title
          summary = metadata.summary
          cover = metadata.cover
          tags = metadata.tags
          createdAt = metadata.createdAt
          page = page }
