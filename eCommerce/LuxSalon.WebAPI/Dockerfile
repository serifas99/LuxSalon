# Build kontekst mora biti "LuxSalon/" folder (solution root), npr:
#   docker build -f LuxSalon.WebAPI/Dockerfile -t luxsalon-webapi .
# docker-compose.yml vec ovo radi automatski.

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Prvo samo .csproj fajlovi - omogucava Docker layer cache za "dotnet restore"
# dok se ne promjene reference/paketi (brzi rebuild pri promjeni samo koda).
COPY ["LuxSalon.WebAPI/LuxSalon.WebAPI.csproj", "LuxSalon.WebAPI/"]
COPY ["LuxSalon.Services/LuxSalon.Services.csproj", "LuxSalon.Services/"]
COPY ["LuxSalon.Common.Services/LuxSalon.Common.Services.csproj", "LuxSalon.Common.Services/"]
COPY ["LuxSalon.Model/LuxSalon.Model.csproj", "LuxSalon.Model/"]
RUN dotnet restore "LuxSalon.WebAPI/LuxSalon.WebAPI.csproj"

COPY LuxSalon.WebAPI/ LuxSalon.WebAPI/
COPY LuxSalon.Services/ LuxSalon.Services/
COPY LuxSalon.Common.Services/ LuxSalon.Common.Services/
COPY LuxSalon.Model/ LuxSalon.Model/

WORKDIR "/src/LuxSalon.WebAPI"
RUN dotnet build "LuxSalon.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LuxSalon.WebAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LuxSalon.WebAPI.dll"]
