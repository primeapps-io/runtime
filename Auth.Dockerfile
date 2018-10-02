FROM microsoft/dotnet:2.1.4-aspnetcore-runtime-bionic AS base
WORKDIR /app
EXPOSE 80 443
ENV ASPNETCORE_ENVIRONMENT Production
ENV ASPNETCORE_URLS="https://+;http://+"
ENV ASPNETCORE_URLS="https://+;http://+"
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=""
ENV ASPNETCORE_Kestrel__Certificates__Default__Path="/app/tls.pfx"

FROM microsoft/dotnet:2.1.402-sdk-bionic AS build
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

CMD openssl pkcs12 -inkey /domain-cert/tls.key -in /domain-cert/tls.crt -export -out /app/tls.pfx -passout pass:
ENTRYPOINT ["dotnet", "PrimeApps.Auth.dll"]