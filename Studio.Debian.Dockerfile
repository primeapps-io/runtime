FROM microsoft/dotnet:2.2-aspnetcore-runtime-stretch-slim AS base
WORKDIR /app
EXPOSE 80

ENV ASPNETCORE_ENVIRONMENT Development
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

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

RUN apt-get update
RUN apt-get -y --no-install-recommends install git

FROM base AS final
WORKDIR /app
COPY --from=publish /app .

# Install Visual Studio Remote Debugger
# RUN apt-get update && apt-get install -y --no-install-recommends unzip
# RUN curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l ~/vsdbg  

ENTRYPOINT ["dotnet","PrimeApps.Studio.dll"]
