using Foundation.Scheduling.Tools.ProtoConsole.Testing;
using Microsoft.Extensions.Hosting;
using TodoWebApp.Foundation.Scheduling.Setup.Extensions;
using TodoWebApp.Foundation.Shared.Setup.Extensions;

var builder = Host.CreateApplicationBuilder(args);
builder.Services
  .AddFoundationShared(builder.Configuration.GetSection("Foundation"))
  .AddFoundationScheduling(builder.Configuration.GetSection("Foundation"));

using var app = builder.Build();
app.Services
  .SetupFoundationShared()
  .SetupFoundationScheduling();

//APITester.Run(app.Services);
StressTester.Run(app.Services);
//TimingTester.Run(app.Services);

app.Run();