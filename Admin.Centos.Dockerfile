FROM microsoft/dotnet:2.2-sdk-stretch AS build
WORKDIR /src
COPY ["PrimeApps.Admin/PrimeApps.Admin.csproj", "PrimeApps.Admin/"]
COPY ["PrimeApps.Model/PrimeApps.Model.csproj", "PrimeApps.Model/"]
RUN dotnet restore "PrimeApps.Admin/PrimeApps.Admin.csproj"
COPY . .

WORKDIR "/src/PrimeApps.Admin"
RUN dotnet build "PrimeApps.Admin.csproj" --no-restore -c Debug -o /app

FROM build AS publish
RUN dotnet publish "PrimeApps.Admin.csproj" --no-restore -c Debug --self-contained false /p:MicrosoftNETPlatformLibrary=Microsoft.NETCore.App -o  /app

FROM registry.centos.org/dotnet/dotnet-22-runtime-centos7 AS base
SHELL ["/bin/bash", "-c"]
WORKDIR /app
COPY --from=publish /app .

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

# Trust Kubernetes CA certificate
USER root
RUN yum install ca-certificates && update-ca-trust force-enable
RUN cp ca.crt /etc/pki/ca-trust/source/anchors/kubernetes_ca.crt
RUN update-ca-trust extract

# Install System.Drawing native dependencies
RUN ln -s /lib64/libdl.so.2 /lib64/libdl.so

FROM base AS final
CMD ["dotnet","PrimeApps.Admin.dll"]