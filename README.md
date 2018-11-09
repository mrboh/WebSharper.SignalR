# WebSharper.SignalR

WebSharper bindings for SignalR

To use:

1. Add NuGet package: weatherwax.io.WebSharper.SignalR
2. Add services.AddSignalR() and app.UseSignalR(..) to the Startup class of a Sitelet project. Using a SiteletService allows you to easily make use of the IHubContext<'THub>.

```f#
member this.ConfigureServices(services: IServiceCollection) =
        // Hook in SignalR
        services.AddSignalR () |> ignore
        ...
        
member this.Configure(app: IApplicationBuilder, env: IHostingEnvironment) =
    // Configure SignalR
    app.UseSignalR(
        fun hubRouteBuilder ->
            hubRouteBuilder.MapHub<SampleHub> (PathString "/signalr")
    ) |> ignore
    ...
```

3. Create Hub

```f#
type SampleHub () =
    inherit Hub ()

    member _this.ServerMessage (message: string) : Task =
        Task.Run (
            fun _ ->
                // Send the message back to the client
                _this.Clients.Caller.SendAsync ("ClientMessage", sprintf "The following message was sent to the server and re-sent back to the client: %s" message)
        )
```

4. Create connection object on the client

```f#
[<JavaScript>]
module Client =
    let private connection = HubConnectionBuilder().WithUrl("/signalr").ConfigureLogging(LogLevel.Trace).Build()

    let private configureSignalR element =
        let handleMessage =
            Action<_>(
                fun (message: string) ->
                    ...
            )

        // Monitor for messages
        connection.On ("ClientMessage", handleMessage)

        // Commence signal monitoring
        connection
            .Start()
            .Catch(fun err -> JavaScript.Console.Error <| err.ToString ())
            |> ignore
            
    let Main () =
        div [ on.afterRender configureSignalR ] [
          ...
```

To see a basic working example, look at the sample project.
