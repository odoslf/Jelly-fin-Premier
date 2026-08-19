#!/usr/bin/env bash
set -e

VERSION="1.0.0.0"
TAG="v${VERSION}"
REPO_OWNER="odoslf"
REPO_NAME="Jelly-fin-Premier"

echo "=== Building JellyPremiere ${VERSION} Release ==="

rm -rf publish dist
mkdir -p publish dist

dotnet publish JellyPremiere/JellyPremiere.csproj -c Release -o ./publish

ZIP_NAME="JellyPremiere_${VERSION}.zip"

cd publish
zip -q -r "../dist/${ZIP_NAME}" *
cd ..

# Compute checksums
MD5_CHECKSUM=$(md5sum "./dist/${ZIP_NAME}" | awk '{print $1}')
SHA256_CHECKSUM=$(sha256sum "./dist/${ZIP_NAME}" | awk '{print $1}')

echo "${MD5_CHECKSUM}" > "./dist/${ZIP_NAME}.md5"
echo "${SHA256_CHECKSUM}" > "./dist/${ZIP_NAME}.sha256"

# Jellyfin plugin repository uses MD5 checksum in hex for manifest.json
DOWNLOAD_URL="https://github.com/${REPO_OWNER}/${REPO_NAME}/releases/download/${TAG}/${ZIP_NAME}"
TIMESTAMP=$(date -u +"%Y-%m-%dT%H:%M:%SZ")

cat << EOF_MANIFEST > ./dist/manifest.json
[
  {
    "guid": "e4138e6f-70db-40a2-9b21-171b3e839e99",
    "name": "JellyPremiere",
    "description": "Sistema profesional de anuncios, estrenos y notificaciones obligatorias para Jellyfin.",
    "overview": "Crea anuncios, promociona estrenos de la biblioteca y emite avisos importantes u obligatorios directamente en Jellyfin.",
    "owner": "odoslf",
    "category": "General",
    "versions": [
      {
        "version": "${VERSION}",
        "changelog": "Initial release of JellyPremiere plugin v1.0.0.0.",
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
echo "Source URL: ${DOWNLOAD_URL}"
