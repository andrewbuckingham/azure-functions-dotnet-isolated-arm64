# Azure Functions .NET 10 isolated base image (linux/arm64)

Microsoft publish only amd64 images for the Azure Functions isolated host and no longer publish its Dockerfile sources. This repository builds an arm64 base image from Microsoft's public `custom-container/template.Dockerfile` and publishes it to Docker Hub:

**`andrewbuckingham/azure-functions-dotnet-isolated-arm64`**

## About this image

This is a **community-built** image. It is not affiliated with, endorsed by, or supported by Microsoft. It contains Microsoft's open-source Azure Functions host and .NET runtimes, built from their public sources and images.

- **Base OS:** Ubuntu 24.04 ("noble"), from Microsoft's `mcr.microsoft.com/dotnet/runtime-deps:10.0-noble`. Note that Microsoft's own amd64 Functions images are Debian-based.
- **Runtime:** the Functions host (self-contained), plus the .NET 10 runtime and ASP.NET Core shared framework for your isolated worker app. No extension bundle: isolated apps reference extensions as NuGet packages.
- **Port and user:** listens on port 80 and runs as root, like Microsoft's Functions images. See [Running as a non-root user](#running-as-a-non-root-user).

Each image carries standard [OCI image labels](https://github.com/opencontainers/image-spec/blob/main/annotations.md), which you can read with `docker inspect`: `title`, `description`, `version` (the Functions host version), `source` (this repository), `revision` (the commit that built it), `created`, and `base.name`.

## No warranty or support

These images are provided **as is**, with no warranty of any kind and no support or service level. Nothing here is a guarantee that an image is secure, fit for any purpose, or free of vulnerabilities. Docker Scout scans each image on Docker Hub, and the results change as new vulnerabilities are published. Most findings come from Microsoft's base images and are fixed when Microsoft patch them, which the monthly rebuild picks up.

- **You are responsible for your own assurance.** Scan, test and approve the image against your own requirements before using it, and again whenever you update it.
- **Images and tags may change or be removed at any time, without notice.** That includes the rolling tag, version tags, and old run-ID tags.
- **If you depend on it, don't depend on us.** Pin a version (or the image digest) and mirror it into your own registry, or build it yourself from the Dockerfile in this repository.

## Tags

Choose how much change you want:

| Tag | Host version | Updates | Use when |
|---|---|---|---|
| `4-dotnet-isolated10.0` | Newest stable release | Whenever a new host is released, and monthly for base patches | You want the latest host and accept changes as they come |
| `<host>-dotnet-isolated10.0`, e.g. `4.1055.100-dotnet-isolated10.0` | Fixed | Monthly, for Microsoft's base patches only | You want a stable host and patches, and control host upgrades |
| `<host>-dotnet-isolated10.0-<run-id>-<attempt>`, e.g. `4.1055.100-dotnet-isolated10.0-42415879772-1` | Fixed | Never | Rolling back to an exact build |

Microsoft's .NET base images (SDK and runtime) are rebuilt monthly, so every tag picks up their security patches without any change to the host version.

Tags created before November 2026 end in `-<run-id>` without the attempt number.

Every image pushed by the workflow has an SBOM and a build provenance attestation attached. You can read them with `docker buildx imagetools inspect <image> --format '{{ json .SBOM }}'` (or `.Provenance`).

## What is in here

- `Dockerfile.functions-base`: the image definition. It builds the Functions host from `Azure/azure-functions-host` at the requested version, on top of Microsoft's .NET 10 runtime and ASP.NET Core images.
- `host-versions`: the host versions kept up to date each month, one per line. Add a version when someone pins it, and remove it when nothing pins it.
- `.github/workflows/build-base.yml`: the build workflow.
- `.github/dependabot.yml`: keeps the workflow's actions up to date.
- `SECURITY.md`: how to report a security problem.
- `THIRD-PARTY-NOTICES.md`: the licence for Microsoft's template that the Dockerfile is adapted from.
- `DOCKERHUB.md`: the description shown on Docker Hub.

Nothing else belongs in this repository. It is public, so application code must never be added here.

## When the workflow runs

- **Monthly** (the 5th), to pick up Microsoft's base patches.
- **On a push to `main`** that changes the Dockerfile, `host-versions` or the workflow.
- **Manually**, from the Actions tab.
- **On a pull request** that changes those files. Pull requests build and test every version, but never push.

Each run:
1. Finds the newest stable Functions host release: the highest `4.x` version number, ignoring previews and release candidates. (GitHub's "latest release" flag isn't used, because Microsoft publish more than one release line at once.)
2. Builds that version and every version in `host-versions`, each in its own job on a native arm64 runner.
3. Smoke-tests each image: `dotnet` and the ASP.NET Core 10 runtime are present, and the host starts and answers on port 80.
4. Pushes each image that passed, with an SBOM and provenance attestation.
5. Moves the rolling tag `4-dotnet-isolated10.0` to the newest stable host.

Each version is built separately. If one fails, the others are still built and pushed. If the newest host fails, the rolling tag stays where it was.

## One-off setup

1. Create a Docker Hub personal access token with **Read & Write** access. It doesn't need Delete.
2. In this repository's settings, add the secret `DOCKERHUB_TOKEN`: the access token. The Docker Hub username isn't secret, and is set in the workflow.
3. In this repository's settings, under **Security**, turn on **Private vulnerability reporting**, which `SECURITY.md` points to.
4. On Docker Hub, set the repository's description by hand:
   - **Short description:** `Unofficial Azure Functions host 4 for .NET 10 isolated apps on linux/arm64 (e.g. Raspberry Pi)`
   - **Full description:** the contents of `DOCKERHUB.md`. Update it there first whenever it changes.

   It isn't synced automatically, because Docker Hub's API needs a token with Delete scope to change it, and the build workflow shouldn't hold one.

## Pinning and host upgrades

- **To pin a host version for a consumer:** add it to `host-versions`, and the monthly run keeps it patched.
- **To stop tracking an old version:** remove it from `host-versions` once nothing pins it. Its existing tags stay on Docker Hub.
- **Testing a new host:** nothing to do here. Each new release is built and published automatically. Consumers who want to test it pull that version's tag.

If a new host release turns out to be bad, point the rolling tag back at the previous version:

```
docker buildx imagetools create -t andrewbuckingham/azure-functions-dotnet-isolated-arm64:4-dotnet-isolated10.0 andrewbuckingham/azure-functions-dotnet-isolated-arm64:<previous-version>-dotnet-isolated10.0
```

This only lasts until the next run, which moves the rolling tag back to the newest release. Don't run the workflow, and don't push changes to the files it watches, until Microsoft publish a fixed release.

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

## Running as a non-root user

The image runs as root, like Microsoft's Functions images, because the host writes to `/home` and `/azure-functions-host`. The base image already contains an `app` user (UID 1654). To run as that user, give it those directories in your own image, as Microsoft's template does:

```dockerfile
FROM andrewbuckingham/azure-functions-dotnet-isolated-arm64:4-dotnet-isolated10.0
COPY --chown=app:app ./publish /home/site/wwwroot
RUN mkdir -p /azure-functions-host/Secrets /tmp/metrics &&     chown -R app:app /home /azure-functions-host/Secrets /tmp/metrics
USER app
ENV ASPNETCORE_URLS=http://+:8080
```

Non-root processes can't listen on port 80, so this example listens on 8080. Map your ports to match.

## Licensing

The `LICENSE` in this repository (MIT) covers the Dockerfile, workflow and documentation here, except where `THIRD-PARTY-NOTICES.md` says otherwise. The published image also contains third-party software, including the Azure Functions host (MIT, with some components under Apache 2.0), .NET (MIT) and a Linux base layer whose packages carry their own licences. Check those projects' terms before redistributing the image.

## Notes

- `Dockerfile.functions-base` follows Microsoft's `Azure/azure-functions-docker` `custom-container/template.Dockerfile` (MIT licensed). See `THIRD-PARTY-NOTICES.md`.
- Docker Hub automated builds are not used. This workflow does the same job without needing those features.
- The SDK build stage runs on the build machine's architecture (`FROM --platform=$BUILDPLATFORM`) and cross-publishes for arm64. The final stage has no `RUN` steps, so you can build the image on an amd64 machine without arm64 emulation. The workflow uses native arm64 runners so it can also run the image to test it.
