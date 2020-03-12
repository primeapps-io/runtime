FROM microsoft/dotnet:2.2-sdk-stretch AS build
WORKDIR /src
COPY ["PrimeApps.Admin/PrimeApps.Admin.csproj", "PrimeApps.Admin/"]
COPY ["PrimeApps.Model/PrimeApps.Model.csproj", "PrimeApps.Model/"]
RUN dotnet restore "PrimeApps.Admin/PrimeApps.Admin.csproj"
COPY . .

WORKDIR "/src/PrimeApps.Admin"
RUN dotnet build "PrimeApps.Admin.csproj" --no-restore -c Release -o /app

FROM build AS publish
RUN dotnet publish "PrimeApps.Admin.csproj" --no-restore --self-contained false -c Release -o /app

FROM microsoft/dotnet:2.2-aspnetcore-runtime-stretch-slim AS base
WORKDIR /app
COPY --from=publish /app .

EXPOSE 80
EXPOSE 443

ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS="https://+;http://+"
ENV ASPNETCORE_HTTPS_PORT=443
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="1q2w3e4r5t"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="aspnetapp.pfx"

# Trust CA certificate
RUN mkdir -p /usr/local/share/ca-certificates/ && cp ca.crt /usr/local/share/ca-certificates/ca.crt
RUN chmod 777 /usr/local/share/ca-certificates/ca.crt
RUN update-ca-certificates --fresh

# Install PostgreSQL Client
RUN mkdir -p /usr/share/man/man1 && mkdir -p /usr/share/man/man7
RUN apt update && apt -y upgrade && apt -y install wget gnupg2
RUN wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | apt-key add -
RUN echo "deb http://apt.postgresql.org/pub/repos/apt/ stretch-pgdg main" | tee /etc/apt/sources.list.d/pgdg.list
RUN apt update
RUN apt install -y --no-install-recommends postgresql-client-12
RUN psql --version

FROM base AS final
WORKDIR /app

ENTRYPOINT ["dotnet","PrimeApps.Admin.dll"]
