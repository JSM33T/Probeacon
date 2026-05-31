FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy csproj files first for layer caching on restore
COPY src/ProBeacon.Domain/ProBeacon.Domain.csproj           src/ProBeacon.Domain/
COPY src/ProBeacon.Application/ProBeacon.Application.csproj src/ProBeacon.Application/
COPY src/ProBeacon.Infrastructure/ProBeacon.Infrastructure.csproj src/ProBeacon.Infrastructure/
COPY src/ProBeacon.Api/ProBeacon.Api.csproj                 src/ProBeacon.Api/

RUN dotnet restore src/ProBeacon.Api/ProBeacon.Api.csproj

# Copy everything else and publish
COPY src/ src/
RUN dotnet publish src/ProBeacon.Api/ProBeacon.Api.csproj \
    -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /publish .

ENTRYPOINT ["dotnet", "ProBeacon.Api.dll"]
