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


FROM archlinux/archlinux:latest as base
USER root
RUN pacman -Sy
RUN pacman -S --noconfirm rust python git dotnet-sdk dotnet-runtime aspnet-runtime
RUN useradd -m -s /bin/bash app
USER app
RUN cargo install --git https://github.com/trumank/uesave-rs.git
WORKDIR /app
RUN git clone https://github.com/xNul/palworld-host-save-fix.git

RUN export UESAVE=$(ls ~/.cargo/bin/uesave)
RUN export SCRIPT=$(ls $PWD/**/fix-host-save.py)

FROM base AS final
WORKDIR /app
RUN rm -rf palworld-host-save-fix
RUN git clone https://github.com/Deliay/palworld-host-save-fix.git
RUN export SCRIPT=$(ls $PWD/**/fix-host-save.py)
COPY --from=publish /app/publish .
EXPOSE 80

ENTRYPOINT ["dotnet", "pal-save-fix-ui.dll"]
