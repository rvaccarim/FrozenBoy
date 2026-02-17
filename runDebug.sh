#!/bin/bash

[ -z "$1" ] && { echo "Usage: $0 <romfile.gb>"; exit 1; }

dotnet run -c Debug --project FrozenBoyUI/FrozenBoyUI.csproj -- "$1"
