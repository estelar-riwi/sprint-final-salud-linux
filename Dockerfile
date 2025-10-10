# ========================
#   Etapa 1: Compilación
# ========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos el archivo de proyecto (.csproj)
COPY *.sln ./
COPY sprint_final_salud_linux/*.csproj ./sprint_final_salud_linux/

# Restauramos dependencias
RUN dotnet restore

# Copiamos el resto del código
COPY . .

# Compilamos y publicamos en modo Release
RUN dotnet publish sprint_final_salud_linux/sprint_final_salud_linux.csproj -c Release -o /app/out

# ========================
#   Etapa 2: Ejecución
# ========================
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copiamos los archivos publicados
COPY --from=build /app/out .

# Render usa el puerto 8080 por defecto
EXPOSE 8080

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Ejecutamos la app
ENTRYPOINT ["dotnet", "sprint_final_salud_linux.dll"]
