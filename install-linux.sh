#!/usr/bin/env bash
# Install the runtime dependencies into the Subnautica install: BepInEx 5.4.23
# win_x64 and this mod's two DLLs. Idempotent: re-running overwrites in place.
#
# Linux note: Subnautica is a Windows game running under Proton, so the
# win_x64 BepInEx build is correct even on Linux — its doorstop proxy is what
# Proton loads.
#
# Usage: ./install-linux.sh [game-dir]
#   game-dir defaults to ~/.local/share/Steam/Steamapps/common/Subnautica

set -euo pipefail

GAME_DIR="${1:-$HOME/.local/share/Steam/Steamapps/common/Subnautica}"
BEPINEX_VERSION="5.4.23.5"
BEPINEX_URL="https://github.com/BepInEx/BepInEx/releases/download/v${BEPINEX_VERSION}/BepInEx_win_x64_${BEPINEX_VERSION}.zip"
MOD_OUT="$(dirname "$0")/src/SpawnOnDamage.Plugin/bin/Release/net472"

die() { echo "install-linux: $*" >&2; exit 1; }

[ -d "$GAME_DIR/Subnautica_Data/Managed" ] || die "no Subnautica install at '$GAME_DIR' (pass the dir as argument)"
[ -f "$MOD_OUT/SpawnOnDamage.dll" ] || die "no built plugin at '$MOD_OUT' — run: nix develop -c dotnet build SpawnOnDamage.slnx -c Release"

echo "==> BepInEx ${BEPINEX_VERSION} (win_x64)"
if [ ! -f "$GAME_DIR/BepInEx/core/BepInEx.dll" ]; then
    TMP="$(mktemp -d)"
    trap 'rm -rf "$TMP"' EXIT
    curl -sL -o "$TMP/bepinex.zip" "$BEPINEX_URL"
    python3 - "$TMP/bepinex.zip" "$GAME_DIR" <<'EOF'
import sys, zipfile
zip_path, game_dir = sys.argv[1], sys.argv[2]
with zipfile.ZipFile(zip_path) as z:
    z.extractall(game_dir)
EOF
    echo "    installed"
else
    echo "    already present, skipping"
fi

echo "==> mod DLLs"
mkdir -p "$GAME_DIR/BepInEx/plugins"
cp "$MOD_OUT/SpawnOnDamage.dll" "$MOD_OUT/SpawnOnDamage.Core.dll" "$GAME_DIR/BepInEx/plugins/"

echo "==> done. Start the game once; the config appears at"
echo "    $GAME_DIR/BepInEx/config/caesarakalaeii.spawnondamage.cfg"
