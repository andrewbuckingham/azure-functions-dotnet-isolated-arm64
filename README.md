# Azure Functions .NET 10 isolated base image (linux/arm64)

Microsoft publish only amd64 images for the Azure Functions isolated host and no longer publish its Dockerfile sources. This repository builds an arm64 base image from Microsoft's public `custom-container/template.Dockerfile` and publishes it to Docker Hub:

**`andrewbuckingham/azure-functions-dotnet-isolated-arm64`**

- `4-dotnet-isolated10.0`: latest build (rolling)
- `<host-version>-dotnet-isolated10.0`: latest build for that host version
- `<host-version>-dotnet-isolated10.0-<run-id>`: a specific build, for pinning or rollback

## What is in here

- `Dockerfile.functions-base`: the image definition. It builds the Functions host from `Azure/azure-functions-host` at `HOST_VERSION`, and takes the .NET 10 runtime and ASP.NET Core from Microsoft's images.
- `.github/workflows/rebuild-base.yml`: builds and pushes the image on the 5th of each month, and on demand.

Nothing else belongs in this repository. It is public, so application code must never be added here.

## One-off setup

1. Create a Docker Hub access token with write access to the repository (Account settings → Personal access tokens).
2. In this repository's settings, add secrets:
   - `DOCKERHUB_USERNAME`: `andrewbuckingham`
   - `DOCKERHUB_TOKEN`: the access token
3. Run the workflow once by hand (Actions → Rebuild Functions base image → Run workflow) to check it works.

## Upgrading the host

When a new Azure Functions host release appears at https://github.com/Azure/azure-functions-host/releases:

1. Update `ARG HOST_VERSION` in `Dockerfile.functions-base` and the default `host_version` in the workflow.
2. Run the workflow.
3. Tell the consuming application's repository to pick it up (its `Dockerfile` pins `4-dotnet-isolated10.0`, which moves with each build).

The monthly run does not change the host version. It refreshes the .NET SDK and runtime images, which is where Microsoft's security patches arrive.

## Notes

- `Dockerfile.functions-base` follows Microsoft's `Azure/azure-functions-docker` `custom-container/template.Dockerfile` (MIT licensed). See `LICENSE`.

- Docker Hub automated builds are not used. They need a linked GitHub repository and plan-level features; this workflow does the same job without needing those features.
- The SDK build stage runs on the build machine's own architecture (`FROM --platform=$BUILDPLATFORM`) and cross-publishes for arm64. The final stage runs a `RUN` step on arm64, which is why QEMU is set up.
