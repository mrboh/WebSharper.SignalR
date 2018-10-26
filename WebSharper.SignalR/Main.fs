namespace weatherwax.io.WebSharper.SignalR

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator

module Definition =

    let LogLevel = 
        Pattern.EnumStrings "LogLevel"
            [ "Trace"; "Debug"; "Information"; "Warning"; "Error"; "Critical"; "None" ]

    let TransferFormat =
        Pattern.EnumStrings "TransferFormat"
            [ "Binary"; "Text" ]

    let MessageType =
        Pattern.EnumStrings "MessageType"
            [ "Invocation"; "StreamItem"; "Completion"; "StreamInvocation"; "CancelInvocation"; "Ping"; "Close" ]

    let HttpTransportType =
        Pattern.EnumStrings "HttpTransportType"
            [ "None"; "WebSockets"; "ServerSentEvents"; "LongPolling" ]

    let HubMessageBase =
        Interface "HubMessageBase"
        |+> [
            "type" =@ MessageType
        ]

    let MessageHeaders = T<System.Collections.Generic.Dictionary<string,string>>

    let HubInvocationMessage =
        Interface "HubInvocationMessage"
        |=> Extends [ HubMessageBase ]
        |+> [
            "headers"       =@ MessageHeaders
            "invocationId"  =@ T<string>
        ]

    let InvocationMessage =
        Interface "InvocationMessage"
        |=> Extends [ HubInvocationMessage ]
        |+> [
            "arguments" =@ Type.ArrayOf T<obj>
            "target"    =@ T<string>
            "type"      =@ T<obj>       // Type is 'Invocation'?
        ]

    let StreamInvocationMessage =
        Interface "StreamInvocationMessage"
        |=> Extends [ HubInvocationMessage ]
        |+> [
            "arguments"     =@ Type.ArrayOf T<obj>
            "invocationId"  =@ T<string>
            "target"        =@ T<string>
            "type"          =@ T<obj>       // Type is 'StreamInvocation'?
        ]

    let StreamItemMessage =
        Interface "StreamItemMessage"
        |=> Extends [ HubInvocationMessage ]
        |+> [
            "invocationId"  =@ T<string>
            "item"          =@ T<obj>
            "type"          =@ T<obj>       // Type is 'StreamItem'?
        ]

    let CompletionMessage =
        Interface "CompletionMessage"
        |=> Extends [ HubInvocationMessage ]
        |+> [
            "error"         =@ T<string>
            "invocationId"  =@ T<string>
            "result"        =@ T<obj>
            "type"          =@ T<obj>       // Type is 'Completion'?
        ]

    let CancelInvocationMessage =
        Interface "CancelInvocationMessage"
        |=> Extends [ HubInvocationMessage ]
        |+> [
            "invocationId"  =@ T<string>
            "type"          =@ T<obj>       // Type is 'CancelInvocation'?
        ]

    let PingMessage = 
        Interface "PingMessage"
        |=> Extends [ HubInvocationMessage ]
        |+> [
            "type"  =@ T<obj>       // Type is 'Ping'?
        ]

    let CloseMessage =
        Interface "CloseMessage"
        |=> Extends [ HubInvocationMessage ]
        |+> [
            "error" =@ T<string>
            "type"  =@ T<obj>       // Type is 'Close'?
        ]

    let HubMessage = InvocationMessage + StreamInvocationMessage + StreamItemMessage + CompletionMessage + CancelInvocationMessage + PingMessage + CloseMessage

    let ILogger =
        Interface "ILogger"
        |+> [
            "log"   => LogLevel?logLevel * T<string>?message ^-> T<unit>
        ]

    let IHubProtocol =
        Interface "IHubProtocol"
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
        Interface "ITransport"

    let HttpClient =
        Class "HttpClient"

    let IHttpConnectionOptions =
        Interface "IHttpConnectionOptions"
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
            Interface "IStreamSubscriber"
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
            Interface "ISubscription"
            |+> [
                "dispose" => T<unit> ^-> T<unit>
            ]

    let IStreamResult  =
        Generic - fun t ->
            Interface "IStreamResult"
            |+> [
                "subscribe" => IStreamSubscriber.[t]?subscriber ^-> ISubscription.[t]
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

    let Assembly =
        Assembly [
            Namespace "WebSharper.SignalR" [
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

                ILogger
                IHubProtocol
                ITransport
                HttpClient
                IHttpConnectionOptions
                IStreamSubscriber
                ISubscription
                IStreamResult

                HubConnection
                HubConnectionBuilder
            ]
        ]

[<Sealed>]
type Extension() =
    interface IExtension with
        member ext.Assembly =
            Definition.Assembly

[<assembly: Extension(typeof<Extension>)>]
do ()
