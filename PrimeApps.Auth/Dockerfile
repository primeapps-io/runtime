#Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
#For more information, please see http://aka.ms/containercompat

FROM microsoft/dotnet:2.1-aspnetcore-runtime-nanoserver-1709 AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk-nanoserver-1709 AS build
WORKDIR /src
COPY PrimeApps.Auth/PrimeApps.Auth.csproj PrimeApps.Auth/
COPY PrimeApps.Model/PrimeApps.Model.csproj PrimeApps.Model/
RUN dotnet restore PrimeApps.Auth/PrimeApps.Auth.csproj
COPY . .
WORKDIR /src/PrimeApps.Auth
RUN dotnet build PrimeApps.Auth.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish PrimeApps.Auth.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "PrimeApps.Auth.dll"]
