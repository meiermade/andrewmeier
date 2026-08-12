module App.Articles.Catalog

open App.Articles

let all: Article list =
    [ Posts.FSharpSemanticKernel.article
      Posts.PersonalInfrastructure.article
      Posts.DevelopmentEnvironment.article ]
    |> List.sortByDescending _.createdAt

let tryFind permalink =
    all |> List.tryFind (fun article -> article.permalink = permalink)
