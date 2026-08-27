#!/usr/bin/env bash
set -euo pipefail

VERSION="${VERSION:-1.0.1.0}"
TAG="v${VERSION}"
REPO_OWNER="odoslf"
REPO_NAME="Jelly-fin-Premier"

echo "=== Building JellyPremiere ${VERSION} Release ==="

rm -rf publish dist
mkdir -p publish dist

dotnet publish JellyPremiere/JellyPremiere.csproj -c Release --no-restore -o ./publish

test -s publish/JellyPremiere.dll
ZIP_NAME="JellyPremiere_${VERSION}.zip"
(
  cd publish
  zip -q "../dist/${ZIP_NAME}" JellyPremiere.dll
)

unzip -t "dist/${ZIP_NAME}"
test "$(unzip -Z1 "dist/${ZIP_NAME}" | wc -l)" -eq 1
test "$(unzip -Z1 "dist/${ZIP_NAME}")" = "JellyPremiere.dll"

MD5_CHECKSUM=$(md5sum "./dist/${ZIP_NAME}" | awk '{print $1}')
SHA256_CHECKSUM=$(sha256sum "./dist/${ZIP_NAME}" | awk '{print $1}')

echo "${MD5_CHECKSUM}" > "./dist/${ZIP_NAME}.md5"
echo "${SHA256_CHECKSUM}" > "./dist/${ZIP_NAME}.sha256"

DOWNLOAD_URL="https://github.com/${REPO_OWNER}/${REPO_NAME}/releases/download/${TAG}/${ZIP_NAME}"
TIMESTAMP=$(date -u +"%Y-%m-%dT%H:%M:%SZ")

cat << EOF_MANIFEST > ./dist/manifest.json
[
  {
    "guid": "e4138e6f-70db-40a2-9b21-171b3e839e99",
    "name": "JellyPremiere",
    "description": "Sistema de anuncios, estrenos y avisos para Jellyfin.",
    "overview": "Banners y avisos automáticos en Jellyfin Web/WebView, más un canal nativo Estrenos para contenidos reproducibles vinculados a la biblioteca.",
    "owner": "odoslf",
    "category": "General",
    "versions": [
      {
        "version": "${VERSION}",
        "changelog": "Carga automática del cliente Web, convivencia con Community, canal Estrenos reproducible con fuentes reales de Jellyfin y validación E2E en Jellyfin 10.10.7.",
        "targetAbi": "10.10.7.0",
        "sourceUrl": "${DOWNLOAD_URL}",
        "checksum": "${MD5_CHECKSUM}",
        "timestamp": "${TIMESTAMP}",
        "targetSystem": "All"
      }
    ]
  }
]
EOF_MANIFEST

echo "=== Release Build Complete ==="
echo "ZIP: ./dist/${ZIP_NAME}"
echo "MD5 Checksum: ${MD5_CHECKSUM}"
echo "SHA256 Checksum: ${SHA256_CHECKSUM}"
