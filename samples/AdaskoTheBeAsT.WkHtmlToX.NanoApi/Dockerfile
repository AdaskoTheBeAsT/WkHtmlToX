# escape=`
ARG NANO_IMAGE=adaskothebeast/nanoserver:aspnet-x64-11.0.0-ltsc2025-v2.2.1

FROM mcr.microsoft.com/dotnet/sdk:11.0-windowsservercore-ltsc2025 AS build

SHELL ["cmd", "/S", "/C"]
WORKDIR C:\src
COPY Directory.Build.props Directory.Build.targets DeterministicBuild.targets AdaskoTheBeAsT.ruleset stylecop.json SecurityCodeScan.config.yml .editorconfig global.json LICENSE README.md ./
COPY src/AdaskoTheBeAsT.WkHtmlToX/AdaskoTheBeAsT.WkHtmlToX.csproj src/AdaskoTheBeAsT.WkHtmlToX/
COPY src/AdaskoTheBeAsT.WkHtmlToX.DependencyInjection/AdaskoTheBeAsT.WkHtmlToX.DependencyInjection.csproj src/AdaskoTheBeAsT.WkHtmlToX.DependencyInjection/
COPY src/AdaskoTheBeAsT.WkHtmlToX.Hosting/AdaskoTheBeAsT.WkHtmlToX.Hosting.csproj src/AdaskoTheBeAsT.WkHtmlToX.Hosting/
COPY samples/AdaskoTheBeAsT.WkHtmlToX.NanoApi/AdaskoTheBeAsT.WkHtmlToX.NanoApi.csproj samples/AdaskoTheBeAsT.WkHtmlToX.NanoApi/

RUN dotnet restore samples/AdaskoTheBeAsT.WkHtmlToX.NanoApi/AdaskoTheBeAsT.WkHtmlToX.NanoApi.csproj -p:UseAppHost=false

COPY src/ src/
COPY samples/AdaskoTheBeAsT.WkHtmlToX.NanoApi/ samples/AdaskoTheBeAsT.WkHtmlToX.NanoApi/

RUN dotnet publish samples/AdaskoTheBeAsT.WkHtmlToX.NanoApi/AdaskoTheBeAsT.WkHtmlToX.NanoApi.csproj `
    -c Release `
    --no-restore `
    --self-contained false `
    -p:UseAppHost=false `
    -p:ErrorLog= `
    -o C:\out

FROM ${NANO_IMAGE} AS nano

WORKDIR C:\app
COPY --from=build ["C:/out/", "C:/app/"]
# Keep the spooler app-local beside the published win-x64 wkhtmltox.dll.
COPY --from=build ["C:/Windows/System32/winspool.drv", "C:/app/winspool.drv"]
USER ContainerUser
ENV ASPNETCORE_URLS=http://+:8080
ENV NANOSERVER_ENABLE_HEADLESS_FONTS=1
EXPOSE 8080
ENTRYPOINT ["dotnet", "AdaskoTheBeAsT.WkHtmlToX.NanoApi.dll"]
