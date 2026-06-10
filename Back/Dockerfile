# ---- Build ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restaurar primero solo los .csproj (mejor caché de capas)
COPY YCT.Domain/*.csproj YCT.Domain/
COPY YCT.Application/*.csproj YCT.Application/
COPY YCT.Infrastructure/*.csproj YCT.Infrastructure/
COPY YCT.API/*.csproj YCT.API/
RUN dotnet restore YCT.API/YCT.API.csproj

# Copiar el resto y publicar
COPY . .
RUN dotnet publish YCT.API/YCT.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- Runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "YCT.API.dll"]
