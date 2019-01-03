FROM registry.access.redhat.com/dotnet/dotnet-22-runtime-rhel7  AS base
SHELL ["/bin/bash", "-c"]
ENV ASPNETCORE_ENVIRONMENT Development
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV DOTNET_CORE_VERSION=2.2
ENV DOTNET_FRAMEWORK=netcoreapp2.2

FROM microsoft/dotnet:2.2-sdk-stretch AS build
WORKDIR /src
COPY ["PrimeApps.Auth/PrimeApps.Auth.csproj", "PrimeApps.Auth/"]
COPY ["PrimeApps.Model/PrimeApps.Model.csproj", "PrimeApps.Model/"]
RUN dotnet restore "PrimeApps.Auth/PrimeApps.Auth.csproj"
COPY . .
WORKDIR "/src/PrimeApps.Auth"
RUN dotnet build "PrimeApps.Auth.csproj" --no-restore -c Debug -o /app

FROM build AS publish
RUN dotnet publish "PrimeApps.Auth.csproj" --no-restore -c Debug --self-contained false /p:MicrosoftNETPlatformLibrary=Microsoft.NETCore.App -o  /app

FROM base AS final
COPY --from=publish /app .
CMD ["dotnet","PrimeApps.Auth.dll"]