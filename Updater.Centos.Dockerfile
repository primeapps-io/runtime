FROM microsoft/dotnet:2.2-sdk-stretch AS build
WORKDIR /src
COPY ["PrimeApps.Migrator/PrimeApps.Migrator.csproj", "PrimeApps.Migrator/"]
COPY ["PrimeApps.Model/PrimeApps.Model.csproj", "PrimeApps.Model/"]
COPY . .

WORKDIR "/src/PrimeApps.Migrator"
RUN dotnet build "PrimeApps.Migrator.csproj" -c Release

FROM build AS publish
RUN dotnet publish "PrimeApps.Migrator.csproj" --self-contained false --runtime linux-x64 -c Release -o /migrator

FROM registry.centos.org/dotnet/dotnet-22-runtime-centos7 AS base
SHELL ["/bin/bash", "-c"]
WORKDIR /root
COPY --from=publish /migrator migrator/
COPY ["artifacts/update.sh", "/root"]
RUN chmod +x update.sh