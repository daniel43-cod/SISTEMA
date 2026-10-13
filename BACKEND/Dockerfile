# ETAPA 1: COMPILACIÓN
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copiar archivos del backend
COPY . .

# Restaurar dependencias
RUN dotnet restore API_SISTEMA/API_SISTEMA.csproj

# Compilar y publicar la API
RUN dotnet publish API_SISTEMA/API_SISTEMA.csproj -c Release -o /app/publish --no-restore

# ETAPA 2: EJECUCIÓN
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["dotnet", "API_SISTEMA.dll"]