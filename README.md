# Azure Functions .NET 10 isolated base image (linux/arm64)

Microsoft publish only amd64 images for the Azure Functions isolated host and no longer publish its Dockerfile sources. This repository builds an arm64 base image from Microsoft's public `custom-container/template.Dockerfile` and publishes it to Docker Hub:

**`andrewbuckingham/azure-functions-dotnet-isolated-arm64`**

## Tags

- `4-dotnet-isolated10.0`: the **rolling** tag. This is the stable release. It moves only when a host version has been tested and promoted, and monthly when Microsoft patch the .NET base images.
- `<host-version>-dotnet-isolated10.0`: fixed to one Functions host version, e.g. `4.1055.100-dotnet-isolated10.0`. Use this to pin or to test a new host before it's promoted.
- `<host-version>-dotnet-isolated10.0-<run-id>`: a specific build, for rollback.

## What is in here

- `Dockerfile.functions-base`: the image definition. It builds the Functions host from `Azure/azure-functions-host` at the version in the build, and takes the .NET 10 runtime and ASP.NET Core from Microsoft's images.
- `host-version`: the host version built on every push to `main`. Publishes the versioned tag only.
- `stable-host-version`: the host version rebuilt monthly and promoted to the rolling tag.
- `.github/workflows/build-base.yml`: the build and promotion workflow.

Nothing else belongs in this repository. It is public, so application code must never be added here.

## Workflow triggers

| Trigger | Builds | Moves the rolling tag? |
|---|---|---|
| Push to `main` changing the version files or Dockerfile | `host-version` | No |
| Monthly schedule (5th) | `stable-host-version` | Yes |
| Manual run | `host-version` (or the `version` input) | Only if `promote` is ticked |

## One-off setup

1. Create a Docker Hub access token with read and write access to the repository.
2. In this repository's settings, add secrets:
   - `DOCKERHUB_USERNAME`: `andrewbuckingham`
   - `DOCKERHUB_TOKEN`: the access token

## Upgrading the host

1. Check the release on https://github.com/Azure/azure-functions-host/releases. Use a stable release, not a preview or release candidate, and read it for breaking changes.
2. Change `host-version` and `ARG HOST_VERSION` in `Dockerfile.functions-base` to the new version, in a pull request. Merging builds the versioned tag only.
3. Test that versioned tag in a consuming application's own test environment, before promoting it.
4. When it's proven, change `stable-host-version` to the same version in a pull request, and run the workflow with **promote** ticked. This moves the rolling tag.
5. Add a line to the changelog below.

Monthly runs rebuild whatever is in `stable-host-version`, so they never move the rolling tag to an untested host.

## Changelog

- **4.1055.100**: initial release.

## Notes

- `Dockerfile.functions-base` follows Microsoft's `Azure/azure-functions-docker` `custom-container/template.Dockerfile` (MIT licensed). See `LICENSE`.
- Docker Hub automated builds are not used. This workflow does the same job without needing those features.
- The SDK build stage runs on the build machine's architecture (`FROM --platform=$BUILDPLATFORM`) and cross-publishes for arm64. The final stage runs a `RUN` step on arm64, which is why QEMU is set up.
