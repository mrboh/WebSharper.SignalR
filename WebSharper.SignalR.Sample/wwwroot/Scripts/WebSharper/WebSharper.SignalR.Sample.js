(function()
{
 "use strict";
 var Global,WebSharper,SignalR,Sample,Client,SC$1,IntelliFactory,Runtime,UI,Doc,AttrProxy,console,signalR;
 Global=self;
 WebSharper=Global.WebSharper=Global.WebSharper||{};
 SignalR=WebSharper.SignalR=WebSharper.SignalR||{};
 Sample=SignalR.Sample=SignalR.Sample||{};
 Client=Sample.Client=Sample.Client||{};
 SC$1=Global.StartupCode$WebSharper_SignalR_Sample$Client=Global.StartupCode$WebSharper_SignalR_Sample$Client||{};
 IntelliFactory=Global.IntelliFactory;
 Runtime=IntelliFactory&&IntelliFactory.Runtime;
 UI=WebSharper&&WebSharper.UI;
 Doc=UI&&UI.Doc;
 AttrProxy=UI&&UI.AttrProxy;
 console=Global.console;
 signalR=Global.signalR;
 Client.Main$47$59=Runtime.Curried3(function($1,e,e$1)
 {
  return Client.sendMessage(e,e$1);
 });
 Client.Main$39$84=function()
 {
  return function(e)
  {
   Client.configureSignalR(e);
  };
 };
 Client.Main=function()
 {
  return Doc.Element("div",[Client.attrClass("container"),AttrProxy.Create("style","margin-top: 15px;"),AttrProxy.OnAfterRenderImpl(function(e)
  {
   Client.configureSignalR(e);
  })],[Doc.Element("form",[],[Doc.Element("div",[Client.attrClass("form-group")],[Doc.Element("label",[],[Doc.TextNode("Enter a message ")]),Doc.Element("input",[AttrProxy.Create("id","textbox"),AttrProxy.Create("type","text"),Client.attrClass("form-control"),AttrProxy.Create("value","Hello, world!")],[]),Doc.Element("small",[Client.attrClass("form-text text-muted")],[Doc.TextNode("See the application console and the Javascript log for results")])])]),Doc.Element("button",[Client.attrClass("btn btn-primary"),AttrProxy.HandlerImpl("click",function(e)
  {
   return function(e$1)
   {
    return Client.sendMessage(e,e$1);
   };
  })],[Doc.TextNode("Send")])]);
 };
 Client.sendMessage=function(el,ev)
 {
  Client.connection().send("ServerMessage",Global.jQuery("#textbox").val());
 };
 Client.configureSignalR=function(element)
 {
  Client.connection().on("ClientMessage",function(message)
  {
   return Global.alert(message);
  });
  (Client.connection().start())["catch"](function(err)
  {
   return console.error(Global.String(err));
  });
 };
 Client.connection=function()
 {
  SC$1.$cctor();
  return SC$1.connection;
 };
 Client.attrClass=function(a)
 {
  return AttrProxy.Create("class",a);
 };
 SC$1.$cctor=function()
 {
  SC$1.$cctor=Global.ignore;
  SC$1.connection=(new signalR.HubConnectionBuilder()).withUrl("/signalr").configureLogging(0).build();
 };
}());
