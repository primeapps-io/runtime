FROM registry.access.redhat.com/dotnet/dotnet-22-runtime-rhel7  AS base
SHELL ["/bin/bash", "-c"]
ENV ASPNETCORE_ENVIRONMENT Development
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV DOTNET_CORE_VERSION=2.2
ENV DOTNET_FRAMEWORK=netcoreapp2.2

FROM microsoft/dotnet:2.2-sdk-stretch AS build
WORKDIR /src
COPY ["PrimeApps.Studio/PrimeApps.Studio.csproj", "PrimeApps.Studio/"]
COPY ["PrimeApps.Model/PrimeApps.Model.csproj", "PrimeApps.Model/"]
RUN dotnet restore "PrimeApps.Studio/PrimeApps.Studio.csproj"
COPY . .
WORKDIR "/src/PrimeApps.Studio"
RUN dotnet build "PrimeApps.Studio.csproj" --no-restore -c Debug -o /app

FROM build AS publish
RUN dotnet publish "PrimeApps.Studio.csproj" --no-restore -c Debug --self-contained false /p:MicrosoftNETPlatformLibrary=Microsoft.NETCore.App -o  /app

sudo yum install git

FROM base AS final
COPY --from=publish /app .
CMD ["dotnet","PrimeApps.Studio.dll"]