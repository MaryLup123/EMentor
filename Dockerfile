# ===== Etapa 1: Construcción de la aplicación .NET =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el archivo .csproj y restaurar dependencias
COPY eduMentor/eduMentor.csproj eduMentor/
RUN dotnet restore "eduMentor/eduMentor.csproj"

# Copiar todo el código y compilar
COPY . .
WORKDIR "/src/eduMentor"
RUN dotnet publish "eduMentor.csproj" -c Release -o /app/publish /p:UseAppHost=false


# ===== Etapa 2: Imagen final con Nginx + ASP.NET =====
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Instalar Nginx
RUN apt-get update && apt-get install -y nginx && rm -rf /var/lib/apt/lists/*

# Copiar la aplicación publicada
COPY --from=build /app/publish /app

# Copiar el archivo de configuración de Nginx
COPY nginx/default.conf /etc/nginx/conf.d/default.conf

# Eliminar configuración por defecto de Nginx
RUN rm /etc/nginx/sites-enabled/default || true

# Exponer el puerto para Render
EXPOSE 8080

# Variable de entorno para ASP.NET
ENV ASPNETCORE_URLS=http://localhost:5000

# Ejecutar Kestrel y Nginx juntos
CMD ["sh", "-c", "dotnet eduMentor.dll & nginx -g 'daemon off;'"]
