FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Services/MangaPublishingSystem/MangaPublishingSystem.Presentation/MangaPublishingSystem.Presentation.csproj"
WORKDIR "/src/Services/MangaPublishingSystem/MangaPublishingSystem.Presentation"
RUN dotnet publish "MangaPublishingSystem.Presentation.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Cài đặt wkhtmltopdf (engine HTML→PDF cross-platform) và các font cần thiết cho tiếng Việt
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgdiplus \
    libc6-dev \
    wkhtmltopdf \
    fontconfig \
    libfontconfig1 \
    libxrender1 \
    libxext6 \
    libx11-6 \
    libjpeg62-turbo \
    fonts-liberation \
    fonts-freefont-ttf \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Rebuild font cache để hệ thống nhận font bundled
RUN fc-cache -fv

RUN chmod -R 755 /app
ENTRYPOINT ["dotnet", "MangaPublishingSystem.Presentation.dll"]

