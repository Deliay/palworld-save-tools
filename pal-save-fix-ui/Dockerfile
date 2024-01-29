FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["pal-save-fix-ui/pal-save-fix-ui.csproj", "pal-save-fix-ui/"]
RUN dotnet restore "./pal-save-fix-ui/./pal-save-fix-ui.csproj"
COPY . .
WORKDIR "/src/pal-save-fix-ui"
RUN dotnet build "./pal-save-fix-ui.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./pal-save-fix-ui.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM zeroash/pal-save-tools-base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY ./ops/entrypoint.sh .
ENV ASPNETCORE_URLS http://*:8080

ENTRYPOINT ["sh", "entrypoint.sh"]
