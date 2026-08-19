#!/usr/bin/env bash
set -e

echo "=== Building JellyPremiere Release ==="
dotnet publish JellyPremiere/JellyPremiere.csproj -c Release -o ./publish

mkdir -p ./dist

ZIP_NAME="JellyPremiere_1.0.0.0.zip"
rm -f "./dist/${ZIP_NAME}"

cd publish
zip -r "../dist/${ZIP_NAME}" *
cd ..

CHECKSUM=$(sha256sum "./dist/${ZIP_NAME}" | awk '{print $1}')
FILE_SIZE=$(stat -c%s "./dist/${ZIP_NAME}")

cat << EOF_MANIFEST > ./dist/manifest.json
[
  {
    "guid": "e4138e6f-70db-40a2-9b21-171b3e839e99",
    "name": "JellyPremiere",
    "description": "Sistema profesional de anuncios, estrenos y notificaciones obligatorias para Jellyfin.",
    "overview": "Crea anuncios, promociona estrenos de la biblioteca y emite avisos importantes u obligatorios directamente en Jellyfin.",
    "owner": "JellyPremiere",
    "category": "General",
    "versions": [
      {
        "version": "1.0.0.0",
        "changelog": "Initial release of JellyPremiere plugin.",
        "targetAbi": "10.10.0.0",
        "sourceUrl": "https://github.com/user/JellyPremiere",
        "checksum": "${CHECKSUM}",
        "timestamp": "$(date -u +"%Y-%m-%dT%H:%M:%SZ")",
        "targetSystem": "All"
      }
    ]
  }
]
EOF_MANIFEST

echo "=== Release Build Complete ==="
echo "ZIP: ./dist/${ZIP_NAME} (Size: ${FILE_SIZE} bytes)"
echo "Checksum: ${CHECKSUM}"
