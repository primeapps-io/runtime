FROM microsoft/dotnet:2.2-sdk-stretch AS build
WORKDIR /src
COPY ["PrimeApps.Studio/PrimeApps.Studio.csproj", "PrimeApps.Studio/"]
COPY ["PrimeApps.Model/PrimeApps.Model.csproj", "PrimeApps.Model/"]
RUN dotnet restore "PrimeApps.Studio/PrimeApps.Studio.csproj"
COPY . .
WORKDIR "/src/PrimeApps.Studio"
RUN dotnet build "PrimeApps.Studio.csproj" --no-restore -c Debug -o /app

FROM build AS publish
RUN dotnet publish "PrimeApps.Studio.csproj" --no-restore --self-contained false -c Debug -o /app

FROM microsoft/dotnet:2.2-aspnetcore-runtime-stretch-slim AS base
WORKDIR /app
COPY --from=publish /app . 

EXPOSE 80
EXPOSE 443

ENV ASPNETCORE_ENVIRONMENT Development
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Install PostgreSQL Client
RUN mkdir -p /usr/share/man/man1 && mkdir -p /usr/share/man/man7
RUN apt-get update && apt-get install -y --no-install-recommends postgresql-client-9.6
RUN psql --version

FROM base AS final
WORKDIR /app

ENTRYPOINT ["dotnet","PrimeApps.Studio.dll"]
