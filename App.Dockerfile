FROM microsoft/dotnet:2.1.4-aspnetcore-runtime-bionic AS base
WORKDIR /app
EXPOSE 80 443
ENV ASPNETCORE_ENVIRONMENT Production

FROM microsoft/dotnet:2.1.402-sdk-bionic AS build
WORKDIR /src
COPY ["PrimeApps.App/PrimeApps.App.csproj", "PrimeApps.App/"]
COPY ["PrimeApps.Model/PrimeApps.Model.csproj", "PrimeApps.Model/"]

RUN dotnet restore "PrimeApps.App/PrimeApps.App.csproj"
COPY . .
WORKDIR "/src/PrimeApps.App"
RUN dotnet build "PrimeApps.App.csproj" --no-restore -c Debug -o /app

FROM build AS publish
RUN dotnet publish "PrimeApps.App.csproj" --no-restore -c Debug -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PrimeApps.App.dll"]