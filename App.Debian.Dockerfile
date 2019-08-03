FROM microsoft/dotnet:2.2-aspnetcore-runtime-stretch-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

ENV ASPNETCORE_ENVIRONMENT Development
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

FROM microsoft/dotnet:2.2-sdk-stretch AS build
WORKDIR /src
COPY ["PrimeApps.App/PrimeApps.App.csproj", "PrimeApps.App/"]
COPY ["PrimeApps.Model/PrimeApps.Model.csproj", "PrimeApps.Model/"]
RUN dotnet restore "PrimeApps.App/PrimeApps.App.csproj"
COPY . .
WORKDIR "/src/PrimeApps.App"
RUN dotnet build "PrimeApps.App.csproj" --no-restore -c Debug -o /app

dotnet dev-certs https -ep ${HOME}/.aspnet/https/aspnetapp.pfx -p crypticpassword
dotnet user-secrets -p PrimeApps.App.csproj set "Kestrel:Certificates:Development:Password" "crypticpassword"

FROM build AS publish
RUN dotnet publish "PrimeApps.App.csproj" --no-restore --self-contained false -c Debug -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app . 

ENTRYPOINT ["dotnet","PrimeApps.App.dll"]
