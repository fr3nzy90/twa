using Foundation.Shared.Tools.ProtoConsole.Testing;
using Microsoft.Extensions.Hosting;
using TodoWebApp.Foundation.Shared.Setup.Extensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
  .AddFoundationShared(builder.Configuration.GetSection("Foundation"));

using var app = builder.Build();
app.Services
  .SetupFoundationShared();

LogTester.Run(app.Services);

//app.Run();