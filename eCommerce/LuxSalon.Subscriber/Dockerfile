# Build kontekst mora biti "LuxSalon/" folder (solution root). docker-compose.yml
# ovo radi automatski.

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["LuxSalon.Subscriber/LuxSalon.Subscriber.csproj", "LuxSalon.Subscriber/"]
RUN dotnet restore "LuxSalon.Subscriber/LuxSalon.Subscriber.csproj"

COPY LuxSalon.Subscriber/ LuxSalon.Subscriber/

WORKDIR "/src/LuxSalon.Subscriber"
RUN dotnet build "LuxSalon.Subscriber.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "LuxSalon.Subscriber.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LuxSalon.Subscriber.dll"]
