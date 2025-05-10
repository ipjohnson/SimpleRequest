// See https://aka.ms/new-console-template for more information

using SimpleRequest.Web.AspNetHost;
using TestApp.AspNet.JsonRpc;
using TestApp.JsonHandlers;

[assembly: JsonHandlersModule]

AspNetWebHost.Run<ApplicationModule>();