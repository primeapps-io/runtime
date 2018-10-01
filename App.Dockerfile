FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine AS base
WORKDIR /app
EXPOSE 80 443
ENV ASPNETCORE_ENVIRONMENT Production
ENV ASPNETCORE_URLS="https://+;http://+"
ENV ASPNETCORE_HTTPS_PORT=443

FROM microsoft/dotnet:2.1-sdk-alpine AS build
WORKDIR /src
COPY ["PrimeApps.App/PrimeApps.App.csproj", "PrimeApps.App/"]
COPY ["PrimeApps.Model/PrimeApps.Model.csproj", "PrimeApps.Model/"]

RUN dotnet restore "PrimeApps.App/PrimeApps.App.csproj"
COPY . .
WORKDIR "/src/PrimeApps.App"
RUN dotnet build "PrimeApps.App.csproj" -c Debug -o /app

FROM build AS publish
RUN dotnet publish "PrimeApps.App.csproj" -c Debug -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PrimeApps.App.dll"]