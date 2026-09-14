#!/usr/bin/env bash
#
# Idempotent environment bootstrap for the Impact Rush Unity 6 project.
#
# Installs, if not already present:
#   * Unity Editor 6000.3.18f1 (Linux) at $UNITY_INSTALL_DIR
#   * .NET SDK (for Roslyn / C# tooling and the compile smoke test) at $DOTNET_ROOT
#   * The system libraries the Linux Unity Editor needs to run in batch mode
#
# Safe to re-run: every step checks for an existing install before doing work.
set -euo pipefail

UNITY_VERSION="6000.3.18f1"
UNITY_CHANGESET="5ebeb53e4c07"
UNITY_INSTALL_DIR="${UNITY_INSTALL_DIR:-/opt/unity}"
DOTNET_ROOT="${DOTNET_ROOT:-/opt/dotnet}"
DOTNET_CHANNEL="8.0"

log() { printf '\n\033[1;34m==>\033[0m %s\n' "$*"; }

# --- root helper (works whether or not we already run as root) --------------
if [ "$(id -u)" -eq 0 ]; then
  SUDO=""
else
  SUDO="sudo"
fi

# --- 1. system libraries required by the Linux Unity Editor -----------------
log "Installing system dependencies for the Unity Editor"
export DEBIAN_FRONTEND=noninteractive
$SUDO apt-get update -qq
# Package names are for Ubuntu 24.04 (t64 ABI transition). Missing optional
# packages are tolerated so the script stays portable across base images.
$SUDO apt-get install -y --no-install-recommends \
  ca-certificates curl xz-utils \
  xvfb libgtk-3-0t64 libnss3 libgbm1 libasound2t64 libxtst6 libxss1 libglu1-mesa \
  || $SUDO apt-get install -y --no-install-recommends \
  ca-certificates curl xz-utils \
  xvfb libgtk-3-0 libnss3 libgbm1 libasound2 libxtst6 libxss1 libglu1-mesa

# --- 2. .NET SDK ------------------------------------------------------------
if [ -x "$DOTNET_ROOT/dotnet" ]; then
  log ".NET SDK already present at $DOTNET_ROOT"
else
  log "Installing .NET SDK $DOTNET_CHANNEL to $DOTNET_ROOT"
  $SUDO mkdir -p "$DOTNET_ROOT"
  $SUDO chown "$(id -u):$(id -g)" "$DOTNET_ROOT"
  curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
  bash /tmp/dotnet-install.sh --channel "$DOTNET_CHANNEL" --install-dir "$DOTNET_ROOT"
fi

# --- 3. Unity Editor --------------------------------------------------------
UNITY_BIN="$UNITY_INSTALL_DIR/Editor/Unity"
if [ -x "$UNITY_BIN" ]; then
  log "Unity Editor already present at $UNITY_BIN"
else
  log "Downloading Unity Editor $UNITY_VERSION ($UNITY_CHANGESET) — this is ~4.3 GB"
  $SUDO mkdir -p "$UNITY_INSTALL_DIR"
  $SUDO chown "$(id -u):$(id -g)" "$UNITY_INSTALL_DIR"
  url="https://download.unity3d.com/download_unity/${UNITY_CHANGESET}/LinuxEditorInstaller/Unity.tar.xz"
  curl -fL --retry 4 --retry-delay 4 -o "$UNITY_INSTALL_DIR/Unity.tar.xz" "$url"
  log "Extracting Unity Editor"
  tar -xf "$UNITY_INSTALL_DIR/Unity.tar.xz" -C "$UNITY_INSTALL_DIR"
  rm -f "$UNITY_INSTALL_DIR/Unity.tar.xz"
fi

# --- 4. convenience symlinks ------------------------------------------------
$SUDO ln -sf "$UNITY_BIN" /usr/local/bin/unity-editor
$SUDO ln -sf "$DOTNET_ROOT/dotnet" /usr/local/bin/dotnet

log "Environment ready"
echo "  Unity Editor : $UNITY_BIN"
echo "  .NET SDK     : $($DOTNET_ROOT/dotnet --version 2>/dev/null || echo 'n/a')"
cat <<'NOTE'

NOTE: Running the Unity Editor itself (batch-mode builds, PlayMode tests,
Android APK export) requires an activated Unity license. Provide one through
Cloud Agent secrets, e.g.:

  UNITY_EMAIL / UNITY_PASSWORD  (+ UNITY_SERIAL for Pro/Plus), or
  UNITY_LICENSE                 (contents of a .ulf license file)

Then activate with:
  unity-editor -batchmode -nographics -quit \
    -username "$UNITY_EMAIL" -password "$UNITY_PASSWORD" [-serial "$UNITY_SERIAL"]

Compiling the C# source against the installed Unity reference assemblies
(see .cursor/verify.sh) does NOT require a license.
NOTE
