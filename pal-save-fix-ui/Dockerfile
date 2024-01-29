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
RUN rm -rf palworld-host-save-fix
RUN git clone https://github.com/Deliay/palworld-host-save-fix.git
RUN export SCRIPT=$(ls $PWD/**/fix-host-save.py)
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS http://*:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "pal-save-fix-ui.dll"]
