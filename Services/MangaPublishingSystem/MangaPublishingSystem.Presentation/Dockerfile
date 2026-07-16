FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Services/MangaPublishingSystem/MangaPublishingSystem.Presentation/MangaPublishingSystem.Presentation.csproj"
WORKDIR "/src/Services/MangaPublishingSystem/MangaPublishingSystem.Presentation"
RUN dotnet publish "MangaPublishingSystem.Presentation.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Cài đặt libgdiplus và các thư viện cần thiết cho SelectPdf / System.Drawing trên Linux
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgdiplus \
    libc6-dev \
    libx11-dev \
    fontconfig \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MangaPublishingSystem.Presentation.dll"]
