module BuildProcess

open System
open System.Diagnostics

[<Struct>]
type Command =
    { executable:string
      arguments:string list
      workingDirectory:string
      environment:Map<string, string>
      standardInput:string option
      timeout:TimeSpan }

[<Struct>]
type Result =
    { exitCode:int
      standardOutput:string
      standardError:string }

let create executable arguments =
    { executable = executable
      arguments = arguments
      workingDirectory = Environment.CurrentDirectory
      environment = Map.empty
      standardInput = None
      timeout = TimeSpan.FromMinutes 15. }

let run command =
    let startInfo = ProcessStartInfo(command.executable)
    startInfo.UseShellExecute <- false
    startInfo.RedirectStandardOutput <- true
    startInfo.RedirectStandardError <- true
    startInfo.RedirectStandardInput <- command.standardInput.IsSome
    startInfo.WorkingDirectory <- command.workingDirectory

    for argument in command.arguments do
        startInfo.ArgumentList.Add argument

    for KeyValue(key, value) in command.environment do
        startInfo.Environment[key] <- value

    use childProcess = new Process(StartInfo = startInfo)
    if not (childProcess.Start()) then invalidOp $"Unable to start {command.executable}."

    let output = childProcess.StandardOutput.ReadToEndAsync()
    let error = childProcess.StandardError.ReadToEndAsync()

    match command.standardInput with
    | Some input ->
        childProcess.StandardInput.Write input
        childProcess.StandardInput.Close()
    | None -> ()

    if not (childProcess.WaitForExit command.timeout) then
        childProcess.Kill(entireProcessTree = true)
        childProcess.WaitForExit(TimeSpan.FromSeconds 5.) |> ignore
        invalidOp $"{command.executable} exceeded its {command.timeout} timeout."

    { exitCode = childProcess.ExitCode
      standardOutput = output.GetAwaiter().GetResult().Trim()
      standardError = error.GetAwaiter().GetResult().Trim() }

let runChecked runner command =
    let result = runner command

    if result.exitCode <> 0 then
        let details =
            [ result.standardError; result.standardOutput ]
            |> List.filter (String.IsNullOrWhiteSpace >> not)
            |> String.concat Environment.NewLine
        invalidOp $"{command.executable} failed with exit code {result.exitCode}. {details}"

    result
