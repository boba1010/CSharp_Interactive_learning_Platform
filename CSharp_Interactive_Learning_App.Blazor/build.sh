#!/usr/bin/env bash
set -euo pipefail

echo "==> Downloading .NET 10 installer..."
curl -sSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x dotnet-install.sh

echo "==> Installing .NET 10 SDK..."
DOTNET_INSTALL_DIR="$PWD/dotnet"
./dotnet-install.sh --channel 10.0 --install-dir "$DOTNET_INSTALL_DIR"

echo "==> Updating PATH..."
export DOTNET_ROOT="$DOTNET_INSTALL_DIR"
export PATH="$DOTNET_INSTALL_DIR:$PATH"

echo "==> Verifying .NET version:"
dotnet --version

echo "==> Installing WebAssembly toolchain workloads..."
dotnet workload install wasm-tools --skip-manifest-update

echo "==> Compiling Blazor WASM App in Release Mode..."
PROJECT_PATH="./YourApp.csproj"   # <-- set this to your actual project file
dotnet publish "$PROJECT_PATH" -c Release
