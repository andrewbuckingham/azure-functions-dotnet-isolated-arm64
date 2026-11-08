using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace SmokeApp;

public class Ping
{
    // Anonymous, so the test needs no function keys or storage account.
    [Function("ping")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest request) =>
        new OkObjectResult($"pong: .NET {Environment.Version} on {RuntimeInformation.OSArchitecture}");
}
