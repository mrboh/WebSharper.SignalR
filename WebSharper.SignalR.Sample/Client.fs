namespace WebSharper.SignalR.Sample

open System
open weatherwax.io.WebSharper.SignalR
open WebSharper
open WebSharper.JavaScript
open WebSharper.JQuery
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html

[<JavaScript>]
module Client =

    let private attrClass = attr.``class``

    let private connection = HubConnectionBuilder().WithUrl("/signalr").ConfigureLogging(LogLevel.Trace).Build()

    let private configureSignalR element =
        let handleMessage =
            Action<_>(
                fun (message: string) ->
                    JS.Alert message
            )

        // Monitor for messages
        connection.On ("ClientMessage", handleMessage)

        // Commence signal monitoring
        connection
            .Start()
            .Catch(fun err -> JavaScript.Console.Error <| err.ToString ())
            |> ignore

    let private sendMessage el ev = 
        connection.Send ("ServerMessage", JQuery("#textbox").Val()) |> ignore

    let Main () =
        div [ attrClass "container"; attr.style "margin-top: 15px;"; on.afterRender configureSignalR ] [
            form [] [
                div [ attrClass "form-group" ] [
                    label [] [ text "Enter a message " ]
                    input [ attr.id "textbox"; attr.``type`` "text"; attrClass "form-control"; attr.value "Hello, world!" ] [ ]
                    small [ attrClass "form-text text-muted" ] [ text "Message will be sent to the server then re-sent back to the client" ]
                ]
            ]
            button [ attrClass "btn btn-primary"; on.click sendMessage ] [ text "Send" ]
        ]