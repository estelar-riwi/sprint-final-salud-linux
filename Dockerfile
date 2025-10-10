# ==============================
# Etapa 1: Build del proyecto
# ==============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copiamos el archivo del proyecto
COPY sprint-final-salud-linux.csproj ./
RUN dotnet restore sprint-final-salud-linux.csproj

# Copiamos todo el código fuente
COPY . .

# Publicamos el proyecto compilado
RUN dotnet publish sprint-final-salud-linux.csproj -c Release -o /publish

# ==============================
# Etapa 2: Runtime (ejecución)
# ==============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /publish .

# Configuramos el puerto de escucha
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

# Comando de ejecución
ENTRYPOINT ["dotnet", "sprint-final-salud-linux.dll"]
