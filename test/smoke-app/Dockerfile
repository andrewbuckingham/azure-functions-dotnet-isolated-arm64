# Builds the smoke-test app on top of a base image under test. Used only by the build workflow.
ARG BASE_IMAGE=andrewbuckingham/azure-functions-dotnet-isolated-arm64:4-dotnet-isolated10.0
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src
COPY . .
RUN dotnet publish SmokeApp.csproj -c Release -o /out

FROM ${BASE_IMAGE}
COPY --from=build /out /home/site/wwwroot
