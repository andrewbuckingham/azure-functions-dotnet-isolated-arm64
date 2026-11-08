# Azure Functions .NET 10 isolated base image (linux/arm64)

Microsoft publish only amd64 images for the Azure Functions isolated host and no longer publish its Dockerfile sources. This repository builds an arm64 base image from Microsoft's public `custom-container/template.Dockerfile` and publishes it to Docker Hub:

**`andrewbuckingham/azure-functions-dotnet-isolated-arm64`**

## About this image

This is a **community-built** image. It is not affiliated with, endorsed by, or supported by Microsoft. It contains Microsoft's open-source Azure Functions host and .NET runtimes, built from their public sources and images.

Each image carries standard [OCI image labels](https://github.com/opencontainers/image-spec/blob/main/annotations.md), which you can read with `docker inspect`: `title`, `description`, `version` (the Functions host version), `source` (this repository), `revision` (the commit that built it), `created`, and `base.name`.

## Tags

Choose how much change you want:

| Tag | Host version | Updates | Use when |
|---|---|---|---|
| `4-dotnet-isolated10.0` | Newest stable release | Whenever a new host is released, and monthly for base patches | You want the latest host and accept changes as they come |
| `<host>-dotnet-isolated10.0`, e.g. `4.1055.100-dotnet-isolated10.0` | Fixed | Monthly, for Microsoft's base patches only | You want a stable host and patches, and control host upgrades |
| `<host>-dotnet-isolated10.0-<run-id>` | Fixed | Never | Rolling back to an exact build |

Microsoft's .NET base images (SDK and runtime) are rebuilt monthly, so every tag picks up their security patches without any change to the host version.

## What is in here

- `Dockerfile.functions-base`: the image definition. It builds the Functions host from `Azure/azure-functions-host` at the requested version, on top of Microsoft's .NET 10 runtime and ASP.NET Core images.
- `host-versions`: the host versions kept up to date each month, one per line. Add a version when someone pins it, and remove it when nothing pins it.
- `.github/workflows/build-base.yml`: the build workflow.

Nothing else belongs in this repository. It is public, so application code must never be added here.

## When the workflow runs

- **Monthly** (the 5th), to pick up Microsoft's base patches.
- **On a push to `main`** that changes the Dockerfile, `host-versions` or the workflow.
- **Manually**, from the Actions tab.

Each run:
1. Finds the newest stable Functions host release. GitHub's "latest release" skips previews and release candidates.
2. Builds that version and every version in `host-versions`.
3. Moves the rolling tag `4-dotnet-isolated10.0` to the newest stable host.

If the newest host fails to build, the run fails and the rolling tag stays where it was.

## One-off setup

1. Create a Docker Hub access token with read and write access to the repository.
2. In this repository's settings, add secrets:
   - `DOCKERHUB_USERNAME`: `andrewbuckingham`
   - `DOCKERHUB_TOKEN`: the access token

## Pinning and host upgrades

- **To pin a host version for a consumer:** add it to `host-versions`, and the monthly run keeps it patched.
- **To stop tracking an old version:** remove it from `host-versions` once nothing pins it. Its existing tags stay on Docker Hub.
- **Testing a new host:** nothing to do here. Each new release is built and published automatically. Consumers who want to test it pull that version's tag.

If a new host release turns out to be bad, point the rolling tag back at the previous version:

```
docker buildx imagetools create -t andrewbuckingham/azure-functions-dotnet-isolated-arm64:4-dotnet-isolated10.0 andrewbuckingham/azure-functions-dotnet-isolated-arm64:<previous-version>-dotnet-isolated10.0
```

## Maintaining `host-versions`

**What the file is for.** It lists Azure Functions host versions that should keep receiving Microsoft's base-image security patches. The monthly build rebuilds every version in it, plus the newest stable host release, which is always built automatically.

**You usually don't need to touch it.** The newest host release is built automatically, and so are Microsoft's base-image patches. Only older host versions that someone still uses need a line here.

**Add a version when:**
- a consumer has pinned a host version that is no longer the newest, and they want it to keep getting security patches; or
- you're pinning an older host version yourself.

**Remove a version when:**
- nothing pins it any more, or everyone using it has moved on; and
- you're happy for it to stop receiving base patches.

Removing a version does **not** delete its tags from Docker Hub. Existing tags stay pullable. They just stop being rebuilt.

**Don't add:**
- the newest release, which is already built; or
- Microsoft's .NET base image versions (`10.0`, etc.), which are picked up automatically.

**Example.**
1. The host releases `4.1100.0`. The monthly build now builds `4.1100.0` and points the rolling tag at it.
2. A team is still on `4.1055.100` and can't move yet. Add `4.1055.100` to `host-versions`, so it keeps getting base patches.
3. Once the team moves to `4.1100.0`, remove `4.1055.100` from the list at the next change. Its old tags remain on Docker Hub.

**Pinning for consumers.** Pin a host-version tag such as `4.1055.100-dotnet-isolated10.0` if you want to keep that host version. Use the rolling tag `4-dotnet-isolated10.0` if you're happy to follow the newest host. For exact, unchanging bytes, pin a per-run tag or the image digest.

## Changelog

- **4.1055.100**: initial release.

## Licensing

The `LICENSE` in this repository (MIT) covers the Dockerfile, workflow and documentation here. The published image also contains third-party software, including the Azure Functions host and .NET (both MIT-licensed by Microsoft) and a Linux base layer whose packages carry their own licences. Check those projects' terms before redistributing the image.

## Notes

- `Dockerfile.functions-base` follows Microsoft's `Azure/azure-functions-docker` `custom-container/template.Dockerfile` (MIT licensed). See `LICENSE`.
- Docker Hub automated builds are not used. This workflow does the same job without needing those features.
- The SDK build stage runs on the build machine's architecture (`FROM --platform=$BUILDPLATFORM`) and cross-publishes for arm64. The final stage runs a `RUN` step on arm64, which is why QEMU is set up.
