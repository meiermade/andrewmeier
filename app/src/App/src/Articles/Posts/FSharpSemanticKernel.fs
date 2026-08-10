module App.Articles.Posts.FSharpSemanticKernel

open App.Articles
open App.Articles.Shared
open FSharp.ViewEngine
open System
open type Html

let private metadata =
    { permalink = "fsharp-semantic-kernel"
      title = "F# Semantic Kernel"
      summary = "Create an AI assistant using F# and Microsoft’s Semantic Kernel"
      cover = "https://assets.meiermade.com/andymeier/articles/shared/gradient-purple-4776537cdf89.webp"
      tags = [| "AI"; "Programming"; "F#" |]
      createdAt = DateTimeOffset(2024, 1, 23, 0, 0, 0, TimeSpan.Zero) }

let private content =
    [ div {
          span { text "Microsoft’s " }

          a {
              _href "https://github.com/microsoft/semantic-kernel"
              span { text "Semantic Kernel SDK" }
          }

          span {
              text
                  " is a set of libraries which allows you to easily build applications which integrate with Large Language Model (LLM) APIs such as "
          }

          a {
              _href "https://platform.openai.com/docs/introduction"
              span { text "OpenAI" }
          }

          span {
              text
                  ". It allows you to automatically orchestrate calls between the connected APIs and your code. This is extremely powerful as you can effectively build ‘skills’ for AI assistants to help understand and solve problems in your specific domain."
          }
      }
      div { br }
      div {
          span {
              text
                  "When building with the Semantic Kernel SDK, a ‘skill’ is effectively a class with a set of methods with annotations on the method itself as well as the input and output values. You can then add this to the kernel and it will be available as a tool when making requests in OpenAI chat completion. The real magic happens when the kernel receives the response as it will then automatically call the method based on the tool call in the OpenAI response."
          }
      }
      div { br }
      div {
          span {
              text
                  "To get a better understanding of how this works I created a short F# script using the Semantic Kernel SDK. In this script I created a Widget Plugin which can manage widget resources. In this example I am just using an in memory database (a dictionary) but you can imagine that this could be your own application database or API. I am using F#’s built in MailboxProcessor to help manage the console input and output as it helps me reason about the state of the program. The "
          }

          a {
              _href "https://learn.microsoft.com/en-us/semantic-kernel/agents/"
              span { text "Semantic Kernel documentation" }
          }

          span { text " has other examples of how to create agents." }
      }
      pre {
          _class "language-fsharp"

          code {
              _class "language-fsharp"

              span {
                  text
                      "#r \"nuget: Microsoft.Extensions.Logging\"\n#r \"nuget: Microsoft.Extensions.Logging.Console\"\n#r \"nuget: Microsoft.SemanticKernel\"\n#r \"nuget: FSharp.Control.AsyncSeq\"\n\nopen Microsoft.Extensions.Logging\nopen Microsoft.Extensions.DependencyInjection\nopen Microsoft.SemanticKernel\nopen Microsoft.SemanticKernel.ChatCompletion\nopen Microsoft.SemanticKernel.Connectors.OpenAI\nopen FSharp.Control\nopen System\nopen System.Text\nopen System.Threading\nopen System.Threading.Tasks\nopen System.ComponentModel\nopen System.Collections.Generic\n\ntype AgentStatus =\n    | ReadyForUser\n    | ReadyForAssistant\n\ntype AgentState =\n    { history:ChatMessageContent list\n      asisstantBuffer:StringBuilder\n      status:AgentStatus }\n\ntype AgentAction =\n    | GetUserInput\n    | GetAssistantResponse of history:ChatMessageContent list\n\ntype AgentMessage =\n    | UserMessage of content:string\n    | StreamingAssistantMessage of content:string\n    | StreamingAssistantMessageFinished\n    | GetNextAction of channel:AsyncReplyChannel<AgentAction>\n\ntype AgentMailbox = MailboxProcessor<AgentMessage>\n\ntype Evolve = AgentState -> AgentMessage -> AgentState\ntype NextAction = AgentState -> AgentAction\n\nlet getNextAction : NextAction =\n    fun state ->\n        match state.status with\n        | ReadyForUser -> GetUserInput\n        | ReadyForAssistant -> GetAssistantResponse state.history\n\nlet evolve : Evolve =\n    fun state message ->\n        match message with\n        | UserMessage content ->\n            { state with\n                history = ChatMessageContent(AuthorRole.User, content) :: state.history\n                status = ReadyForAssistant }\n        | StreamingAssistantMessage content ->\n            { state with asisstantBuffer = state.asisstantBuffer.Append(content) }\n        | StreamingAssistantMessageFinished ->\n            let content = state.asisstantBuffer.ToString()\n            { state with\n                asisstantBuffer = StringBuilder()\n                history =  ChatMessageContent(AuthorRole.Assistant, content) :: state.history\n                status = ReadyForUser }\n        | GetNextAction channel -> channel.Reply(getNextAction state); state\n\ntype Agent(kernel:Kernel) =\n    let settings = OpenAIPromptExecutionSettings(ToolCallBehavior=ToolCallBehavior.AutoInvokeKernelFunctions)\n    let chatService = kernel.GetRequiredService<IChatCompletionService>()\n\n    let startMailbox initialState cancellationToken =\n        AgentMailbox.Start((fun inbox ->\n            AsyncSeq.initInfiniteAsync (fun _ -> inbox.Receive())\n            |> AsyncSeq.fold evolve initialState\n            |> Async.Ignore), cancellationToken)\n\n    let rec runAsync (mailbox:AgentMailbox) = async {\n        match! mailbox.PostAndAsyncReply(GetNextAction) with\n        | GetUserInput ->\n            do! Console.Out.WriteAsync(\"user > \") |> Async.AwaitTask\n            let! content = Console.In.ReadLineAsync() |> Async.AwaitTask\n            match content with\n            | content when String.IsNullOrEmpty(content) ->\n                return ()\n            | content when content = Environment.NewLine ->\n                do! runAsync mailbox\n            | content ->\n                mailbox.Post(UserMessage content)\n        | GetAssistantResponse history ->\n            let chunks = chatService.GetStreamingChatMessageContentsAsync(\n                ChatHistory(List.rev history),\n                executionSettings=settings,\n                kernel=kernel)\n            for chunk in AsyncSeq.ofAsyncEnum chunks do \n                if chunk.Role.HasValue then\n                    do! Console.Out.WriteAsync($\"{chunk.Role} > \") |> Async.AwaitTask\n                do! Console.Out.WriteAsync(chunk.Content) |> Async.AwaitTask\n                mailbox.Post(StreamingAssistantMessage chunk.Content)\n            do! Console.Out.WriteLineAsync() |> Async.AwaitTask\n            mailbox.Post(StreamingAssistantMessageFinished)\n        return! runAsync mailbox\n    }\n\n    member _.RunAsync(systemMessage:string) = async {\n        let! cancellationToken = Async.CancellationToken\n        let initialState =\n            { history = List.singleton (ChatMessageContent(AuthorRole.System, systemMessage))\n              asisstantBuffer = StringBuilder()\n              status = ReadyForAssistant }\n        let mailbox = startMailbox initialState cancellationToken\n        do! runAsync mailbox\n    }\n\ntype Widget = { id:Guid; name:string; widgetType:string }\n\ntype WidgetPlugin(loggerFactory:ILoggerFactory) =\n    let logger = loggerFactory.CreateLogger<WidgetPlugin>()\n    let widgets = Dictionary<Guid,Widget>()\n\n    [<KernelFunction>]\n    [<Description(\"Create a widget.\")>]\n    member _.CreateWidget\n            ([<Description(\"The name of the widget.\")>] name:string,\n             [<Description(\"The type of widget. One of 'Foo', 'Bar', or 'Baz'.\")>] widgetType:string)\n            :[<Description(\"Result of creating widget.\")>] Task<string> = task {\n        logger.LogInformation(\"Creating widget: {name}\", name)\n        let widget = { id = Guid.NewGuid(); name = name; widgetType = widgetType } \n        widgets.Add(widget.id, widget)\n        return \"Successfully created widget\"\n    }\n\n    [<KernelFunction>]\n    [<Description(\"Update a widget's name.\")>]\n    member _.UpdateWidgetName\n            ([<Description(\"Id of the widget to update.\")>] id:Guid,\n             [<Description(\"Name of the widget.\")>] name:string)\n            :[<Description(\"Result of updating widget name.\")>] Task<string> = task {\n        logger.LogInformation(\"Updating widget name: {id} {name}\", id, name)\n        return\n            match widgets.TryGetValue(id) with\n            | false, _ ->\n                \"Widget not found\"\n            | true, widget ->\n                widgets[id] <- { widget with name = name }\n                \"Successfully updated widget name\"\n    }\n\n    [<KernelFunction>]\n    [<Description(\"Delete a widget.\")>]\n    member _.DeleteWidget\n            ([<Description(\"Id of the widget to delete.\")>] id:Guid)\n            :[<Description(\"Result of deleting widget.\")>] Task<string> = task {\n        logger.LogInformation(\"Deleting widget: {id}\", id)\n        let removed = widgets.Remove(id)\n        return if removed then \"Widget deleted\" else \"Widget not found\"\n    }\n\n    [<KernelFunction>]\n    [<Description(\"List all the widgets.\")>]\n    member _.ListWidgets() : [<Description(\"List of widgets.\")>] Task<Widget seq> = task {\n        return widgets.Values\n    }\n\nlet main () =\n    let cts = new CancellationTokenSource()\n    let cancellationToken = cts.Token\n    Console.CancelKeyPress.Add(fun _ -> cts.Cancel())\n    let apiKey = Environment.GetEnvironmentVariable(\"OPENAI_API_KEY\")\n    let loggerFactory = LoggerFactory.Create(fun b -> b.AddConsole().SetMinimumLevel(LogLevel.Information) |> ignore)\n    let builder = Kernel.CreateBuilder().AddOpenAIChatCompletion(\"gpt-4-1106-preview\", apiKey)\n    builder.Services.AddSingleton(loggerFactory) |> ignore\n    builder.Plugins.AddFromType<WidgetPlugin>() |> ignore\n    let kernel = builder.Build()\n\n    let systemMessage = \"\"\"\n    You are a friendly assistant. Your job is help client's manage their widgets.\n    If you are not given enough information to complete the task then ask for more information.\n    Start by asking the client how you can help them with their widgets.\n    \"\"\"\n    let agent = Agent(kernel)\n    let work = agent.RunAsync(systemMessage)\n    try Async.RunSynchronously(work, cancellationToken=cancellationToken)\n    with\n    | :? OperationCanceledException -> Console.WriteLine(\"Agent stopped\")\n    | ex -> raise ex\n\nmain()"
              }
          }
      }
      div {
          span { text "Here is the output after running the script (you can run with " }

          code {
              _class "language-none"
              text "dotnet fsi script.fsx"
          }

          span { text ")" }
      }
      pre {
          _class "language-plain text"

          code {
              _class "language-plain text"

              span {
                  text
                      "assistant > How can I assist you with your widgets today?\nuser > Could you create a widget named W1\nassistant > Of course! To go ahead with creating the widget, I'll just need to know the type of widget you would like me to create. There are three types available: 'Foo', 'Bar', and 'Baz'. Which one would you like for your widget named W1?\nuser > Foo please\nassistant > info: CreateWidget[0]\n      Function CreateWidget invoking.\ninfo: FSI_0002.WidgetPlugin[0]\n      Creating widget: W1\ninfo: CreateWidget[0]\n      Function CreateWidget succeeded.\ninfo: CreateWidget[0]\n      Function completed. Duration: 0.0049415s\nassistant > The widget named W1 of type 'Foo' has been successfully created. Is there anything else I can help you with regarding your widgets?\nuser > Could you create another widget named W2 of type Baz\nassistant > info: CreateWidget[0]\n      Function CreateWidget invoking.\ninfo: FSI_0002.WidgetPlugin[0]\n      Creating widget: W2\ninfo: CreateWidget[0]\n      Function CreateWidget succeeded.\ninfo: CreateWidget[0]\n      Function completed. Duration: 0.0001877s\nassistant > The widget named W2 of type 'Baz' has been successfully created. Is there anything else I can assist you with?\nuser > Could you list my widgets\nassistant > info: ListWidgets[0]\n      Function ListWidgets invoking.\ninfo: ListWidgets[0]\n      Function ListWidgets succeeded.\ninfo: ListWidgets[0]\n      Function completed. Duration: 0.002207s\nassistant > You currently have the following widgets:\n\n1. Widget Name: W1\n   - ID: fc39cda8-835b-4426-8c95-331b911560ed\n   - Type: Foo\n\n2. Widget Name: W2\n   - ID: 00a79541-09d1-423b-ad9f-6280145fe28b\n   - Type: Baz\n\nIf you need further assistance with these widgets, feel free to let me know!\nuser > Thanks. Could you delete widget W1\nassistant > info: DeleteWidget[0]\n      Function DeleteWidget invoking.\ninfo: FSI_0002.WidgetPlugin[0]\n      Deleting widget: fc39cda8-835b-4426-8c95-331b911560ed\ninfo: DeleteWidget[0]\n      Function DeleteWidget succeeded.\ninfo: DeleteWidget[0]\n      Function completed. Duration: 0.0065881s\nassistant > Widget W1 has been successfully deleted. If there's anything else I can do for you, just let me know!\nuser > List widgets again please\nassistant > info: ListWidgets[0]\n      Function ListWidgets invoking.\ninfo: ListWidgets[0]\n      Function ListWidgets succeeded.\ninfo: ListWidgets[0]\n      Function completed. Duration: 0.001004s\nassistant > After deleting Widget W1, here's the current list of your widgets:\n\n1. Widget Name: W2\n   - ID: 00a79541-09d1-423b-ad9f-6280145fe28b\n   - Type: Baz\n\nPlease let me know if there's anything more I can help you with your widgets.\nuser >"
              }
          }
      }
      div {
          span {
              text
                  "Hopefully this gives you a sense of how the Semantic Kernel works. The plugins are extremely powerful as you can take basically any API and connect it with natural language queries."
          }
      }
      div { br }
      div { br } ]

let article = Article.create metadata (ArticlePage.primary metadata content)
