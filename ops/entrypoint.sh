export UESAVE=$(ls ~/.cargo/bin/uesave)

export SCRIPT=$(ls "$PWD"/**/fix-host-save.py)

dotnet pal-save-fix-ui.dll
