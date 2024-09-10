# См. статью по ссылке https://aka.ms/customizecontainer, чтобы узнать как настроить контейнер отладки и как Visual Studio использует этот Dockerfile для создания образов для ускорения отладки.

# Этот этап используется при запуске из VS в быстром режиме (по умолчанию для конфигурации отладки)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root
WORKDIR /app
EXPOSE 8080

COPY PdfMicroService/fonts/ /usr/share/fonts/truetype/

# Устанавливаем пакеты
RUN apt-get update && apt-get install -y \
    ffmpeg \
    libgdiplus \
    fontconfig

# Обновляем кэш шрифтов
RUN fc-cache -f -v 

# Чистим кеш и удаляем временные файлы
RUN apt-get clean && rm -rf /var/lib/apt/lists/*

# Этот этап используется для сборки проекта службы
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PdfMicroService/PdfMicroService.csproj", "PdfMicroService/"]
RUN dotnet restore "./PdfMicroService/PdfMicroService.csproj"
COPY . .
WORKDIR "/src/PdfMicroService"
RUN dotnet build "./PdfMicroService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этот этап используется для публикации проекта службы, который будет скопирован на последний этап
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PdfMicroService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Этот этап используется в рабочей среде или при запуске из VS в обычном режиме (по умолчанию, когда конфигурация отладки не используется)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PdfMicroService.dll"]