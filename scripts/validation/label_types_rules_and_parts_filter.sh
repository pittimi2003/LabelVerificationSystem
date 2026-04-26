#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
API_PROJECT="$ROOT_DIR/source/Backend/Api/LabelVerificationSystem.Api/LabelVerificationSystem.Api.csproj"
BASE_URL="${BASE_URL:-http://localhost:5041}"
LOG_FILE="${LOG_FILE:-$ROOT_DIR/.tmp-label-types-rules.log}"
DB_FILE_DEV="$ROOT_DIR/labelverification.db"
DB_FILE_BASE="$ROOT_DIR/label-verification.db"
DB_FILE_API="$ROOT_DIR/source/Backend/Api/LabelVerificationSystem.Api/labelverification.db"

export ASPNETCORE_ENVIRONMENT=Development

python3 - <<PY
from pathlib import Path
for base in [Path(r"$DB_FILE_DEV"), Path(r"$DB_FILE_BASE"), Path(r"$DB_FILE_API")]:
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
  if grep -q "Now listening on" "$LOG_FILE"; then break; fi
  sleep 1
done

grep -q "Now listening on" "$LOG_FILE" || { echo "API no inició correctamente. Revisar log: $LOG_FILE" >&2; exit 1; }

api_call() {
  local method="$1"; local url="$2"; local token="${3:-}"; local payload="${4:-}"
  local args=(-sS -X "$method" -w '\n%{http_code}' "$url")
  [[ -n "$token" ]] && args+=(-H "Authorization: Bearer $token")
  [[ -n "$payload" ]] && args+=(-H 'Content-Type: application/json' -d "$payload")
  curl "${args[@]}"
}
extract_code(){ echo "$1" | tail -n1; }
extract_body(){ echo "$1" | sed '$d'; }

login_resp="$(api_call POST "$BASE_URL/api/auth/login" "" '{"usernameOrEmail":"admin","password":"Admin123!","rememberMe":false}')"
[[ "$(extract_code "$login_resp")" == "200" ]] || { echo "Login admin falló" >&2; exit 1; }
admin_token="$(extract_body "$login_resp" | python3 -c 'import json,sys;print(json.load(sys.stdin)["accessToken"])')"

lt_base="$(api_call POST "$BASE_URL/api/label-types" "$admin_token" '{"name":"LT Base","rules":[{"columnName":"Model","expectedValue":" model-x "}]}')"
[[ "$(extract_code "$lt_base")" == "201" ]] || { echo "No se pudo crear LT Base" >&2; echo "$(extract_body "$lt_base")" >&2; exit 1; }

lt_specific="$(api_call POST "$BASE_URL/api/label-types" "$admin_token" '{"name":"LT Specific","rules":[{"columnName":"Model","expectedValue":"MODEL-X"},{"columnName":"Cco","expectedValue":"CCO-1"}]}')"
[[ "$(extract_code "$lt_specific")" == "201" ]] || { echo "No se pudo crear LT Specific" >&2; echo "$(extract_body "$lt_specific")" >&2; exit 1; }

lt_duplicate="$(api_call POST "$BASE_URL/api/label-types" "$admin_token" '{"name":"LT Duplicate","rules":[{"columnName":"cco","expectedValue":"cco-1"},{"columnName":"MODEL","expectedValue":"model-x"}]}')"
[[ "$(extract_code "$lt_duplicate")" == "409" ]] || { echo "Se esperaba rechazo por duplicidad exacta de reglas" >&2; exit 1; }

lt_duplicate_col="$(api_call POST "$BASE_URL/api/label-types" "$admin_token" '{"name":"LT Col Dup","rules":[{"columnName":"Model","expectedValue":"A"},{"columnName":"model","expectedValue":"B"}]}')"
[[ "$(extract_code "$lt_duplicate_col")" == "400" ]] || { echo "Se esperaba rechazo por columna duplicada" >&2; exit 1; }

lt_empty_val="$(api_call POST "$BASE_URL/api/label-types" "$admin_token" '{"name":"LT Empty","rules":[{"columnName":"Model","expectedValue":"  "}]}')"
[[ "$(extract_code "$lt_empty_val")" == "400" ]] || { echo "Se esperaba rechazo por valor esperado vacío" >&2; exit 1; }

part_specific="$(api_call POST "$BASE_URL/api/parts" "$admin_token" '{"partNumber":"PN-1","model":" model-x ","minghuaDescription":"Desc 1","caducidad":10,"cco":"cco-1","certificationEac":true,"firstFourNumbers":1234}')"
[[ "$(extract_code "$part_specific")" == "201" ]] || { echo "No se pudo crear part específica" >&2; exit 1; }
part_specific_label="$(extract_body "$part_specific" | python3 -c 'import json,sys;print(json.load(sys.stdin)["labelTypeName"])')"
[[ "$part_specific_label" == "LT Specific" ]] || { echo "Se esperaba LT Specific por regla más específica" >&2; exit 1; }

part_base="$(api_call POST "$BASE_URL/api/parts" "$admin_token" '{"partNumber":"PN-2","model":"model-x","minghuaDescription":"Desc 2","caducidad":20,"cco":"other","certificationEac":false,"firstFourNumbers":5678}')"
[[ "$(extract_code "$part_base")" == "201" ]] || { echo "No se pudo crear part base" >&2; exit 1; }
part_base_label="$(extract_body "$part_base" | python3 -c 'import json,sys;print(json.load(sys.stdin)["labelTypeName"])')"
[[ "$part_base_label" == "LT Base" ]] || { echo "Se esperaba LT Base" >&2; exit 1; }

part_fallback="$(api_call POST "$BASE_URL/api/parts" "$admin_token" '{"partNumber":"PN-3","model":"none","minghuaDescription":"Desc 3","caducidad":30,"cco":"none","certificationEac":null,"firstFourNumbers":9999}')"
[[ "$(extract_code "$part_fallback")" == "201" ]] || { echo "No se pudo crear part fallback" >&2; exit 1; }
part_fallback_label="$(extract_body "$part_fallback" | python3 -c 'import json,sys;print(json.load(sys.stdin)["labelTypeName"])')"
[[ "$part_fallback_label" == "Por asignar" ]] || { echo "Se esperaba fallback Por asignar" >&2; exit 1; }

parts_filter_resp="$(api_call GET "$BASE_URL/api/parts?labelTypeName=LT%20Specific&page=1&pageSize=20" "$admin_token")"
[[ "$(extract_code "$parts_filter_resp")" == "200" ]] || { echo "Falló filtro parts por labelTypeName" >&2; exit 1; }
count_specific="$(extract_body "$parts_filter_resp" | python3 -c 'import json,sys;print(json.load(sys.stdin)["totalItems"])')"
[[ "$count_specific" -ge 1 ]] || { echo "Filtro por LabelTypeName no devolvió resultados" >&2; exit 1; }

echo "Validación reglas LabelType + filtro Parts completada correctamente."
