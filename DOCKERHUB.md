# Azure Functions .NET 10 isolated – linux/arm64 (unofficial)

An **unofficial, community-built** Azure Functions host 4 image for **.NET 10 isolated worker** apps on **linux/arm64**, such as the Raspberry Pi (64-bit OS).

Microsoft publish `mcr.microsoft.com/azure-functions/dotnet-isolated:4-dotnet-isolated10.0` for amd64 only. This image fills the gap for arm64, built from Microsoft's public sources and base images.

> **Not affiliated with, endorsed by, or supported by Microsoft.** Provided as is, with no warranty or support. Tags may change or be removed without notice: if you depend on it, pin a digest and mirror it.

## Tags

| Tag | Use when |
|---|---|
| `4-dotnet-isolated10.0` | You want the newest stable host (same tag name as Microsoft's image) |
| `<host>-dotnet-isolated10.0`, e.g. `4.1055.100-dotnet-isolated10.0` | You want a fixed host version that still gets monthly base patches |
| `<host>-dotnet-isolated10.0-<run-id>-<attempt>` | You want an exact build that never changes |

## What's in the image

- Azure Functions host 4, built from [Microsoft's source](https://github.com/Azure/azure-functions-host) and published self-contained for `linux-arm64`
- .NET 10 runtime and ASP.NET Core 10 shared framework, for isolated worker apps
- Ubuntu 24.04, from `mcr.microsoft.com/dotnet/runtime-deps:10.0-noble`
- Listens on port 80 and runs as root, like Microsoft's image. No extension bundle: isolated apps reference extensions as NuGet packages.
- An SBOM and build provenance attestation on every image

```dockerfile
FROM andrewbuckingham/azure-functions-dotnet-isolated-arm64:4-dotnet-isolated10.0
COPY ./publish /home/site/wwwroot
```

## More

Source, build workflow, running as non-root, licensing and security reporting:
**https://github.com/andrewbuckingham/azure-functions-dotnet-isolated-arm64**
