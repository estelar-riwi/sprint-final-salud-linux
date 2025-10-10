# ==============================
# Etapa 1: Build del proyecto
# ==============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiamos el archivo del proyecto y restauramos dependencias
COPY sprint-final-salud-linux.csproj ./
RUN dotnet restore sprint-final-salud-linux.csproj

# Copiamos todo el código fuente
COPY . .

# Publicamos el proyecto compilado en Release
RUN dotnet publish sprint-final-salud-linux.csproj -c Release -o /publish

# ==============================
# Etapa 2: Runtime (ejecución)
# ==============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Instalamos libgdiplus para soporte de System.Drawing en Linux
RUN apt-get update && apt-get install -y libgdiplus libc6-dev \
    && rm -rf /var/lib/apt/lists/*

# Copiamos la app compilada desde la etapa de build
COPY --from=build /publish .

# Configuramos el puerto de escucha
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# Comando para iniciar la app
ENTRYPOINT ["dotnet", "sprint-final-salud-linux.dll"]
