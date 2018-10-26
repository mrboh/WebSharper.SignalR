namespace weatherwax.io.WebSharper.SignalR

open WebSharper
open WebSharper.JavaScript
open WebSharper.InterfaceGenerator

module Definition =

    let I1 =
        Interface "I1"
        |+> [
                "test1" => T<string> ^-> T<string>
            ]

    let I2 =
        Generic -- fun t1 t2 ->
            Interface "I2"
            |+> [
                    Generic - fun m1 -> "foo" => m1 * t1 ^-> t2
                ]

    let C1 =
        Class "C1"
        |+> Instance [
                "foo" =@ T<int>
            ]
        |+> Static [
                Constructor (T<unit> + T<int>)
                "mem"   => (T<unit> + T<int> ^-> T<unit>)
                "test2" => (TSelf -* T<int> ^-> T<unit>) * T<string> ^-> T<string>
                "radius2" =? T<float>
                |> WithSourceName "R2"
            ]

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

    let HubConnectionBuilder =
        Class "HubConnectionBuilder"
        |+> Instance [
            "build"                 => T<unit> ^-> TSelf
            "configureLogging"      => LogLevel?logLevel ^-> TSelf
            "configureLogging"      => ILogger?logger ^-> TSelf
            "withHubProtocol"       => IHubProtocol?protocol ^-> TSelf
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
