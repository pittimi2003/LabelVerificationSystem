#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
API_PROJECT="$ROOT_DIR/source/Backend/Api/LabelVerificationSystem.Api/LabelVerificationSystem.Api.csproj"
BASE_URL="${BASE_URL:-http://localhost:5041}"
LOG_FILE="${LOG_FILE:-$ROOT_DIR/.tmp-label-types-smoke.log}"
DB_FILE_DEV="$ROOT_DIR/labelverification.db"
DB_FILE_BASE="$ROOT_DIR/label-verification.db"

export ASPNETCORE_ENVIRONMENT=Development
export Authorization__UseRobustMatrix=true
export Authorization__EnableLegacyFallback=true
export Authentication__ConfiguredUsersRobustBridge__Enabled=true

python3 - <<PY
from pathlib import Path
for base in [Path(r"$DB_FILE_DEV"), Path(r"$DB_FILE_BASE")]:
    for suffix in ["", "-shm", "-wal"]:
        p = Path(str(base) + suffix)
        if p.exists():
            p.unlink()
PY

dotnet run --project "$API_PROJECT" >"$LOG_FILE" 2>&1 &
API_PID=$!
cleanup() { kill "$API_PID" >/dev/null 2>&1 || true; }
trap cleanup EXIT

for _ in $(seq 1 90); do
  if grep -q "Now listening on" "$LOG_FILE"; then
    break
  fi
  sleep 1
done

grep -q "Now listening on" "$LOG_FILE" || { echo "API no inició correctamente. Revisar log: $LOG_FILE" >&2; exit 1; }

api_call() {
  local method="$1"
  local url="$2"
  local token="${3:-}"
  local payload="${4:-}"

  local args=(-sS -X "$method" -w '\n%{http_code}' "$url")
  if [[ -n "$token" ]]; then
    args+=(-H "Authorization: Bearer $token")
  fi
  if [[ -n "$payload" ]]; then
    args+=(-H 'Content-Type: application/json' -d "$payload")
  fi

  curl "${args[@]}"
}

extract_code() { echo "$1" | tail -n1; }
extract_body() { echo "$1" | sed '$d'; }

login_payload() {
  printf '{"usernameOrEmail":"%s","password":"%s","rememberMe":false}' "$1" "$2"
}

admin_login="$(api_call POST "$BASE_URL/api/auth/login" "" "$(login_payload admin 'Admin123!')")"
manager_login="$(api_call POST "$BASE_URL/api/auth/login" "" "$(login_payload manager 'Manager123!')")"
operator_login="$(api_call POST "$BASE_URL/api/auth/login" "" "$(login_payload operator 'Operator123!')")"

[[ "$(extract_code "$admin_login")" == "200" ]] || { echo "Login admin falló" >&2; exit 1; }
[[ "$(extract_code "$manager_login")" == "200" ]] || { echo "Login manager falló" >&2; exit 1; }
[[ "$(extract_code "$operator_login")" == "200" ]] || { echo "Login operator falló" >&2; exit 1; }

admin_token="$(extract_body "$admin_login" | python3 -c 'import json,sys; print(json.load(sys.stdin)["accessToken"])')"
manager_token="$(extract_body "$manager_login" | python3 -c 'import json,sys; print(json.load(sys.stdin)["accessToken"])')"
operator_token="$(extract_body "$operator_login" | python3 -c 'import json,sys; print(json.load(sys.stdin)["accessToken"])')"

available_resp="$(api_call GET "$BASE_URL/api/label-types/available-columns" "$admin_token")"
list_resp="$(api_call GET "$BASE_URL/api/label-types" "$admin_token")"

unique_name="Tipo QA $RANDOM"
create_payload=$(printf '{"name":"%s","columns":["PartNumber","Model","Cco"]}' "$unique_name")
create_resp="$(api_call POST "$BASE_URL/api/label-types" "$admin_token" "$create_payload")"

create_code="$(extract_code "$create_resp")"
[[ "$create_code" == "201" ]] || { echo "Create label-type falló: $create_code" >&2; echo "$(extract_body "$create_resp")" >&2; exit 1; }
created_id="$(extract_body "$create_resp" | python3 -c 'import json,sys; print(json.load(sys.stdin)["id"])')"

edited_name="$unique_name Editado"
update_payload=$(printf '{"name":"%s","columns":["PartNumber","Model","Cco"],"isActive":true}' "$edited_name")
update_resp="$(api_call PUT "$BASE_URL/api/label-types/$created_id" "$admin_token" "$update_payload")"

duplicate_payload=$(printf '{"name":"%s","columns":["PartNumber"]}' "$edited_name")
duplicate_resp="$(api_call POST "$BASE_URL/api/label-types" "$admin_token" "$duplicate_payload")"
no_columns_resp="$(api_call POST "$BASE_URL/api/label-types" "$admin_token" '{"name":"Invalido","columns":[]}')"

manager_list_resp="$(api_call GET "$BASE_URL/api/label-types" "$manager_token")"
manager_create_resp="$(api_call POST "$BASE_URL/api/label-types" "$manager_token" '{"name":"x","columns":["PartNumber"]}')"
operator_list_resp="$(api_call GET "$BASE_URL/api/label-types" "$operator_token")"

[[ "$(extract_code "$available_resp")" == "200" && "$(extract_code "$list_resp")" == "200" && "$(extract_code "$update_resp")" == "200" ]] || { echo "Falló smoke principal LabelTypes" >&2; exit 1; }
[[ "$(extract_code "$duplicate_resp")" == "409" && "$(extract_code "$no_columns_resp")" == "400" ]] || { echo "Fallaron validaciones LabelTypes" >&2; exit 1; }
[[ "$(extract_code "$manager_list_resp")" == "200" && "$(extract_code "$manager_create_resp")" == "403" && "$(extract_code "$operator_list_resp")" == "403" ]] || { echo "Fallaron permisos LabelTypes" >&2; exit 1; }

echo "GET /api/label-types/available-columns (admin) => $(extract_code "$available_resp")"
echo "GET /api/label-types (admin) => $(extract_code "$list_resp")"
echo "POST /api/label-types (admin) => $create_code"
echo "PUT /api/label-types/{id} (admin) => $(extract_code "$update_resp")"
echo "POST duplicado => $(extract_code "$duplicate_resp")"
echo "POST sin columnas => $(extract_code "$no_columns_resp")"
echo "GET /api/label-types (manager) => $(extract_code "$manager_list_resp")"
echo "POST /api/label-types (manager esperado 403) => $(extract_code "$manager_create_resp")"
echo "GET /api/label-types (operator esperado 403) => $(extract_code "$operator_list_resp")"
