# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/ToptanciECommerce.Web/ToptanciECommerce.Web.csproj", "src/ToptanciECommerce.Web/"]
COPY ["src/ToptanciECommerce.Application/ToptanciECommerce.Application.csproj", "src/ToptanciECommerce.Application/"]
COPY ["src/ToptanciECommerce.Domain/ToptanciECommerce.Domain.csproj", "src/ToptanciECommerce.Domain/"]
COPY ["src/ToptanciECommerce.Infrastructure/ToptanciECommerce.Infrastructure.csproj", "src/ToptanciECommerce.Infrastructure/"]

RUN dotnet restore "src/ToptanciECommerce.Web/ToptanciECommerce.Web.csproj"

COPY . .

RUN dotnet publish "src/ToptanciECommerce.Web/ToptanciECommerce.Web.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install fonts required by QuestPDF
RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 \
    fontconfig \
    fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

# Create uploads directory
RUN mkdir -p /app/wwwroot/uploads

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ToptanciECommerce.Web.dll"]
