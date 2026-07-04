#!/usr/bin/env bash
set -e

echo "==> Downloading .NET 10 installer..."
curl -sSL https://dot.net -o dotnet-install.sh
chmod +x dotnet-install.sh

echo "==> Installing .NET 10 SDK..."
./dotnet-install.sh --channel 10.0 --install-dir ./dotnet

echo "==> Updating system path paths..."
export PATH="$PWD/dotnet:$PATH"

echo "==> Verifying .NET version:"
dotnet --version

echo "==> Installing WebAssembly toolchain workloads..."
dotnet workload install wasm-tools

echo "==> Compiling Blazor WASM App in Release Mode..."
dotnet publish -c Release
