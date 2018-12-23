FROM registry.access.redhat.com/dotnet/dotnet-22-runtime-rhel7 AS base
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_ENVIRONMENT Docker
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

FROM registry.access.redhat.com/dotnet/dotnet-22-rhel7 AS build
WORKDIR /src
COPY ["PrimeApps.Auth/PrimeApps.Auth.csproj", "PrimeApps.Auth/"]
COPY ["PrimeApps.Model/PrimeApps.Model.csproj", "PrimeApps.Model/"]

RUN dotnet restore "PrimeApps.Auth/PrimeApps.Auth.csproj"
COPY . .
WORKDIR "/src/PrimeApps.Auth"
RUN dotnet build "PrimeApps.Auth.csproj" --no-restore -c Debug -o /app

FROM build AS publish
RUN dotnet publish "PrimeApps.Auth.csproj" --no-restore -c Debug -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

ENTRYPOINT ["dotnet","PrimeApps.Auth.dll"]
