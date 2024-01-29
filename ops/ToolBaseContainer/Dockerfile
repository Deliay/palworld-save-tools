
FROM archlinux/archlinux:latest
USER root
RUN pacman -Sy
RUN pacman -S --noconfirm rust python git dotnet-sdk dotnet-runtime aspnet-runtime
RUN useradd -m -s /bin/bash app

USER app
RUN cargo install --git https://github.com/trumank/uesave-rs.git
WORKDIR /app
RUN git clone https://github.com/Deliay/palworld-host-save-fix.git

RUN export UESAVE=$(ls ~/.cargo/bin/uesave)
RUN export SCRIPT=$(ls $PWD/**/fix-host-save.py)
