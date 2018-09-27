FROM microsoft/dotnet:2.1-aspnetcore-runtime-alpine  AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk-alpine AS build
ENV ASPNETCORE_ENVIRONMENT Development
COPY . /src
WORKDIR /src
RUN dotnet restore -nowarn:msb3202,nu1503 PrimeApps.App/
RUN dotnet build --no-restore -c Release -o /app PrimeApps.App/

FROM build AS publish
ENV ASPNETCORE_ENVIRONMENT Development
WORKDIR /src/PrimeApps.App/
RUN dotnet publish --no-restore -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PrimeApps.App.dll"]