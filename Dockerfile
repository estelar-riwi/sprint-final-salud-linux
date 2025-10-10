# ========================
#   Etapa 1: Compilación
# ========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiamos todo el código
COPY . .

# Restauramos dependencias y compilamos
RUN dotnet restore
RUN dotnet publish -c Release -o out

# ========================
#   Etapa 2: Ejecución
# ========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copiamos el resultado de compilación
COPY --from=build /app/out .

# Render usa el puerto 8080 por defecto
EXPOSE 8080

# Configuramos la variable de entorno
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "sprint-final-salud-linux.dll"]
