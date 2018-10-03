FROM microsoft/dotnet:2.1.5-aspnetcore-runtime-stretch-slim AS base
WORKDIR /app
EXPOSE 80 443
ENV ASPNETCORE_ENVIRONMENT Production
ENV ASPNETCORE_URLS="https://+;http://+"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password="pWd"
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/app/tls.pfx"
ENV DOTNET_RUNNING_IN_CONTAINER=true

FROM microsoft/dotnet:2.1.403-sdk-stretch AS build
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

ENTRYPOINT openssl pkcs12 -inkey /domain-cert/tls.key -in /domain-cert/tls.crt -export -out /app/tls.pfx -passout pass:pWd && dotnet PrimeApps.Auth.dll
