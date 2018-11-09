namespace WebSharper.SignalR.Sample

open Microsoft.AspNetCore.SignalR
open System
open System.Threading.Tasks
open System.Timers
open WebSharper
open WebSharper.AspNetCore
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Server

type EndPoint =
    | [<EndPoint "/">] Home

type MainTemplate = Templating.Template<"Main.html">

type SampleHub ()=
    inherit Hub ()

    member _this.ServerMessage (message: string) : Task =
        Task.Run (
            fun _ ->
                // Send the message back to the client
                _this.Clients.Caller.SendAsync ("ClientMessage", sprintf "The following message was sent to the server and re-sent back to the client: %s" message)
        )

type SampleService (hubContext: IHubContext<SampleHub>) =
    inherit SiteletService<EndPoint> ()

    // The IHubContext allows you to send messages to clients from outside the hub
    let serverMessageTimer = new Timer (10000., AutoReset = false)
    do serverMessageTimer.Elapsed.Add (
        fun _ -> 
            hubContext.Clients.All.SendAsync ("ClientMessage", "This message was automatically sent to all clients after 10 seconds") 
            |> Async.AwaitTask 
            |> Async.Start
    )
    do serverMessageTimer.Start ()

    let buildTemplate context endPoint =
        match endPoint with
            | Home -> 
                let title = "Sample Home"
                let body = div [] [ client <@ Client.Main () @> ]

                MainTemplate()
                    .Title(title)
                    .Body(body)
                    .Doc()
        |> Content.Page

    let sitelet = Application.MultiPage buildTemplate

    override _this.Sitelet = sitelet