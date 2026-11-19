#!/usr/bin/env bash
# Compila y arranca la demo de eVidencia dentro de GitHub Codespaces.
# La aplicación queda en el puerto 5080; Codespaces lo publica en una dirección del navegador.
set -u
cd "$(dirname "$0")/../eVidencia.NET"

echo "== Compilando la solución (la primera vez tarda uno o dos minutos) =="
OPCIONES=""
if ! dotnet build eVidencia.sln -nologo -v q; then
  echo
  echo "== La compilación con el conector de SQL Server ha fallado. =="
  echo "== Se compila sin él: la demo funciona con los datos de ejemplo en memoria. =="
  OPCIONES="-p:SinSqlServer=true"
  dotnet build eVidencia.sln -nologo -v q $OPCIONES || { echo "ERROR: la solución no compila."; exit 1; }
fi

echo
echo "== Pruebas de la lógica de negocio =="
dotnet run --project pruebas/eVidencia.Pruebas --no-build $OPCIONES | tail -1

echo
echo "== Arrancando la demo en el puerto 5080 =="
echo "== Si no se abre sola: pestaña PORTS (Puertos) > fila 5080 > icono del globo =="
echo "== Para pararla: Ctrl+C en este terminal. Para volver a arrancarla: bash .devcontainer/arrancar.sh =="
export ASPNETCORE_ENVIRONMENT=Development
export ASPNETCORE_URLS=http://0.0.0.0:5080
exec dotnet run --project src/eVidencia.Web --no-build --no-launch-profile $OPCIONES
