# Security policy

This is a community-built image, maintained on a best-effort basis with no support or service level.
See the [README](README.md#no-warranty-or-support).

## Reporting a vulnerability

Please report security problems privately, using GitHub's
[private vulnerability reporting](https://github.com/andrewbuckingham/azure-functions-dotnet-isolated-arm64/security/advisories/new),
not a public issue.

Useful to know first:

- **Vulnerabilities in the Azure Functions host or .NET** belong upstream, with Microsoft:
  [Microsoft Security Response Center](https://msrc.microsoft.com/create-report).
  Once Microsoft release a fix, this image picks it up at the next build: a new host release is
  built when the workflow next runs, and .NET and OS patches arrive with the monthly rebuild.
- **Known CVEs in OS packages** that Ubuntu or Microsoft haven't patched yet can't be fixed here.
- **Problems with this repository's own build** (the Dockerfile, the workflow, or how the image is
  put together) are the right thing to report here.
