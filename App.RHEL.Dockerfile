FROM registry.access.redhat.com/dotnet/dotnet-22-runtime-rhel7  AS base
SHELL ["/bin/bash", "-c"]

ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV DOTNET_CORE_VERSION=2.2
ENV DOTNET_FRAMEWORK=netcoreapp2.2
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS="https://+;http://+"
ENV ASPNETCORE_HTTPS_PORT=443
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="1q2w3e4r5t"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="aspnetapp.pfx"

FROM microsoft/dotnet:2.2-sdk-stretch AS build
WORKDIR /src
COPY ["PrimeApps.App/PrimeApps.App.csproj", "PrimeApps.App/"]
COPY ["PrimeApps.Model/PrimeApps.Model.csproj", "PrimeApps.Model/"]
RUN dotnet restore "PrimeApps.App/PrimeApps.App.csproj"
COPY . .

ADD PrimeApps.App/ca.crt /usr/local/share/ca-certificates/ca.crt
RUN chmod 644 /usr/local/share/ca-certificates/ca.crt
RUN update-ca-certificates

WORKDIR "/src/PrimeApps.App"
RUN dotnet build "PrimeApps.App.csproj" --no-restore -c Debug -o /app

FROM build AS publish
RUN dotnet publish "PrimeApps.App.csproj" --no-restore -c Debug --self-contained false /p:MicrosoftNETPlatformLibrary=Microsoft.NETCore.App -o  /app

FROM base AS final
COPY --from=publish /app .
CMD ["dotnet","PrimeApps.App.dll"]