namespace weatherwax.io.WebSharper.SignalR

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator
open System.Net.Cache

module Definition =

    let numericEnum startIndex name strings =
        let inlines =
            strings
            |> List.mapi (
                fun i s ->
                    (s, string <| i + startIndex)
            )
        Pattern.EnumInlines name inlines

    let LogLevel = 
        numericEnum 0 "signalR.LogLevel"
            [ "Trace"; "Debug"; "Information"; "Warning"; "Error"; "Critical"; "None" ]

    // Unclear if this is correct
    let TransferFormat =
        numericEnum 0 "signalR.TransferFormat"
            [ "Binary"; "Text" ]

    let MessageType =
        numericEnum 1 "signalR.MessageType"
            [ "Invocation"; "StreamItem"; "Completion"; "StreamInvocation"; "CancelInvocation"; "Ping"; "Close" ]

    let HttpTransportType =
        numericEnum 0 "signalR.HttpTransportType"
            [ "None"; "WebSockets"; "ServerSentEvents"; "LongPolling" ]

    let HubMessageBase =
        Interface "signalR.HubMessageBase"
        |+> [
            "type" =@ MessageType
        ]

    let MessageHeaders = T<System.Collections.Generic.Dictionary<string,string>>

    let HubInvocationMessage =
        Interface "signalR.HubInvocationMessage"
        |=> Extends [ HubMessageBase ]
        |+> [
            "headers"       =@ MessageHeaders
            "invocationId"  =@ T<string>
        ]

    let InvocationMessage =
        Interface "signalR.InvocationMessage"
        |=> Extends [ HubInvocationMessage ]
        |+> [
            "arguments" =@ Type.ArrayOf T<obj>
            "target"    =@ T<string>
            "type"      =@ T<obj>       // Type is 'Invocation'?
        ]

    let StreamInvocationMessage =
        Interface "signalR.StreamInvocationMessage"
        |=> Extends [ HubInvocationMessage ]
        |+> [
            "arguments"     =@ Type.ArrayOf T<obj>
            "invocationId"  =@ T<string>
            "target"        =@ T<string>
            "type"          =@ T<obj>       // Type is 'StreamInvocation'?
        ]

    let StreamItemMessage =
        Interface "signalR.StreamItemMessage"
        |=> Extends [ HubInvocationMessage ]
        |+> [
            "invocationId"  =@ T<string>
            "item"          =@ T<obj>
            "type"          =@ T<obj>       // Type is 'StreamItem'?
        ]

    let CompletionMessage =
        Interface "signalR.CompletionMessage"
        |=> Extends [ HubInvocationMessage ]
        |+> [
            "error"         =@ T<string>
            "invocationId"  =@ T<string>
            "result"        =@ T<obj>
            "type"          =@ T<obj>       // Type is 'Completion'?
        ]

    let CancelInvocationMessage =
        Interface "signalR.CancelInvocationMessage"
        |=> Extends [ HubInvocationMessage ]
        |+> [
            "invocationId"  =@ T<string>
            "type"          =@ T<obj>       // Type is 'CancelInvocation'?
        ]

    let PingMessage = 
        Interface "signalR.PingMessage"
        |=> Extends [ HubInvocationMessage ]
        |+> [
            "type"  =@ T<obj>       // Type is 'Ping'?
        ]

    let CloseMessage =
        Interface "signalR.CloseMessage"
        |=> Extends [ HubInvocationMessage ]
        |+> [
            "error" =@ T<string>
            "type"  =@ T<obj>       // Type is 'Close'?
        ]

    let HubMessage = InvocationMessage + StreamInvocationMessage + StreamItemMessage + CompletionMessage + CancelInvocationMessage + PingMessage + CloseMessage

    let ILogger =
        Interface "signalR.ILogger"
        |+> [
            "log"   => LogLevel?logLevel * T<string>?message ^-> T<unit>
        ]

    let IHubProtocol =
        Interface "signalR.IHubProtocol"
        |+> [
            "name"              =@ T<string>
            "transferFormat"    =@ TransferFormat
            "version"           =@ T<Number>
        ]
        |+> [
            "parseMessages"     => (T<string> + T<ArrayBuffer>)?input * ILogger?logger ^-> Type.ArrayOf HubMessage
            "writeMessage"      => HubMessage ^-> (T<string> + T<ArrayBuffer>)
        ]

    let ITransport =
        Interface "signalR.ITransport"
        |+> [
            "onclose"       =@ T<Error> ^-> T<unit>
            "onreceive"     =@ T<string> + T<ArrayBuffer> ^-> T<unit>
        ]
        |+> [
            "connect"       => T<string>?url * TransferFormat?transferFormat ^-> T<Promise<unit>>
            "send"          => T<obj>?data ^-> T<Promise<unit>>
            "stop"          => T<unit> ^-> T<Promise<unit>>
        ]

    let AbortSignal =
        Interface "signalR.AbortSignal"
        |+> [
            "aborted"   =@ T<bool>
            "onabort"   =@ T<unit -> unit>
        ]

    let HttpRequest =
        Interface "signalR.HttpRequest"
        |+> [
            "abortSignal"       =@ AbortSignal
            "content"           =@ T<string> + T<ArrayBuffer>
            "headers"           =@ T<Function>
            "method"            =@ T<string>
            "responseType"      =@ T<XMLHttpRequestResponseType>
            "timeout"           =@ T<Number>
            "url"               =@ T<string>
        ]

    let HttpResponse =
        Class "signalR.HttpResponse"
        |+> Instance [
            "content"       =@ T<string> + T<ArrayBuffer>
            "statusCode"    =@ T<Number>
            "statusText"    =@ T<string>
        ]
        |+> Static [
            Constructor (T<Number>?statusCode)
            Constructor (T<Number>?statusCode * T<string>?statusText)
            Constructor (T<Number>?statusCode * T<string>?statusText * T<string>?content)
            Constructor (T<Number>?statusCode * T<string>?statusText * T<ArrayBuffer>?content)
        ]

    let HttpClient =
        Class "signalR.HttpClient"
        |+> Instance [
            "delete"        => T<string>?url ^-> T<Promise<_>>.[HttpResponse]
            "get"           => T<string>?url ^-> T<Promise<_>>.[HttpResponse]
            "get"           => T<string>?url * HttpRequest?options ^-> T<Promise<_>>.[HttpResponse]
            "post"          => T<string>?url ^-> T<Promise<_>>.[HttpResponse]
            "post"          => T<string>?url * HttpRequest?options ^-> T<Promise<_>>.[HttpResponse]
            "send"          => HttpRequest?request ^-> T<Promise<_>>.[HttpResponse]
        ]

    let IHttpConnectionOptions =
        Interface "signalR.IHttpConnectionOptions"
        |+> [
            "httpClient"        =@ HttpClient
            "logger"            =@ ILogger + LogLevel
            "logMessageContent" =@ T<bool>
            "skipNegotiation"   =@ T<bool>
            "transport"         =@ HttpTransportType + ITransport
        ]
        |+> [
            "accessTokenFactory" => T<unit> ^-> T<string> + T<Promise<string>>
        ]

    let IStreamSubscriber =
        Generic - fun t ->
            Interface "signalR.IStreamSubscriber"
            |+> [
                "closed"        =@ T<bool>
            ]
            |+> [
                "complete"      => T<unit> ^-> T<unit>
                "error"         => T<obj>?err ^-> T<obj>
                "next"          => t?value ^-> T<unit>
            ]

    let ISubscription =
        Generic - fun t ->
            Interface "signalR.ISubscription"
            |+> [
                "dispose" => T<unit> ^-> T<unit>
            ]

    let IStreamResult  =
        Generic - fun t ->
            Interface "signalR.IStreamResult"
            |+> [
                "subscribe" => IStreamSubscriber.[t]?subscriber ^-> ISubscription.[t]
            ]

    let HttpError =
        Class "signalR.HttpError"
        |=> Inherits T<Error>
        |+> Instance [
            "statusCode"    =@ T<Number>
        ]
        |+> Static [
            Constructor (T<string>?errorMessage * T<Number>?statusCode)
            "Error"         =@ T<obj>       // Type is documented as 'ErrorConstructor'?
        ]

    let TimeoutError =
        Class "signalR.TimeoutError"
        |=> Inherits T<Error>
        |+> Instance []
        |+> Static [
            Constructor (T<string>?errorMessage)
            "Error" =@ T<obj>       // Type is documented as 'ErrorConstructor'
        ]
        
    let DefaultHttpClient =
        Class "signalR.DefaultHttpClient"
        |=> Inherits HttpClient
        |+> Instance [
            "send"      => HttpRequest?request ^-> T<Promise<_>>.[HttpResponse]
        ]
        |+> Static [
            Constructor (ILogger?logger)
        ]

    let HubConnection =
        Class "signalR.HubConnection"
        |+> Instance [
            "serverTimeoutInMilliseconds"   =@ T<Number>
            "off"                           => T<string>?methodName ^-> T<unit>
            "off"                           => T<string>?methodName * T<obj[] -> unit>?method ^-> T<unit>
            "on"                            => T<string>?methodName * T<obj[] -> unit>?newMethod ^-> T<unit>
            "onclose"                       => T<Error -> unit>?callback ^-> T<unit>
            "send"                          => T<string>?methodName * (Type.ArrayOf T<obj>)?args ^-> T<Promise<unit>>
            "start"                         => T<unit> ^-> T<Promise<unit>>
            "stop"                          => T<unit> ^-> T<Promise<unit>>

            Generic - fun t ->
                "invoke"                    => T<string>?methodName * (Type.ArrayOf T<obj>)?args ^-> T<Promise<_>>.[t]

            Generic - fun t ->
                "stream"                    => T<string>?methodName * (Type.ArrayOf T<obj>)?args ^-> IStreamResult.[t]
        ]

    let HubConnectionBuilder =
        Class "signalR.HubConnectionBuilder"
        |+> Instance [
            "build"                 => T<unit> ^-> HubConnection
            "configureLogging"      => LogLevel?logLevel ^-> TSelf
            "configureLogging"      => ILogger?logger ^-> TSelf
            "withHubProtocol"       => IHubProtocol?protocol ^-> TSelf
            "withUrl"               => T<string>?url ^-> TSelf
            "withUrl"               => T<string>?url * IHttpConnectionOptions?options ^-> TSelf
        ]
        |+> Static [
            Constructor (T<unit>)
        ]

    let JsonHubProtocol =
        Class "signalR.JsonHubProtocol"
        |+> Instance [
            "name"              =@ T<string>
            "transferFormat"    =@ TransferFormat
            "version"           =@ T<Number>

            "parseMessages"     => T<string>?input * ILogger?logger ^-> Type.ArrayOf HubMessage
            "writeMessage"      => HubMessage?message ^-> T<string>
        ]

    let NullLogger =
        Class "signalR.NullLogger"
        |+> Instance [
            "log"       => LogLevel?logLevel * T<string>?message ^-> T<unit>
        ]
        |+> Static [
            "instance"  =@ ILogger
        ]

    let Assembly =
        Assembly [
            Namespace "weatherwax.io.WebSharper.SignalR" [
                LogLevel
                TransferFormat
                MessageType
                HttpTransportType
                HubMessageBase

                HubInvocationMessage
                InvocationMessage
                StreamInvocationMessage
                StreamItemMessage
                CompletionMessage
                CancelInvocationMessage
                PingMessage
                CloseMessage

                AbortSignal
                ILogger
                IHubProtocol
                ITransport
                HttpClient
                IHttpConnectionOptions
                IStreamSubscriber
                ISubscription
                IStreamResult
                HttpRequest

                HttpError
                TimeoutError
                DefaultHttpClient
                HttpResponse
                HubConnection
                HubConnectionBuilder
                JsonHubProtocol
                NullLogger
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
