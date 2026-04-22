#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
API_PROJECT="$ROOT_DIR/source/Backend/Api/LabelVerificationSystem.Api/LabelVerificationSystem.Api.csproj"
BASE_URL="${BASE_URL:-http://localhost:5041}"
LOGIN_PAYLOAD='{"usernameOrEmail":"admin","password":"Admin123!","rememberMe":false}'
LOG_FILE="${LOG_FILE:-$ROOT_DIR/.tmp-robust-only-e2e.log}"

export ASPNETCORE_ENVIRONMENT=Development
export Authorization__UseRobustMatrix=true
export Authorization__EnableLegacyFallback=false
export Authentication__ConfiguredUsersRobustBridge__Enabled=true

mkdir -p "$(dirname "$LOG_FILE")"
rm -f "$LOG_FILE"

dotnet run --project "$API_PROJECT" >"$LOG_FILE" 2>&1 &
API_PID=$!
cleanup() {
  kill "$API_PID" >/dev/null 2>&1 || true
}
trap cleanup EXIT

for _ in $(seq 1 90); do
  if grep -q "Now listening on" "$LOG_FILE"; then
    break
  fi
  sleep 1
done

if ! grep -q "Now listening on" "$LOG_FILE"; then
  echo "API no inició correctamente. Revisar log: $LOG_FILE" >&2
  exit 1
fi

login_response="$(curl -sS -w '\n%{http_code}' \
  -H 'Content-Type: application/json' \
  -d "$LOGIN_PAYLOAD" \
  "$BASE_URL/api/auth/login")"
login_code="$(echo "$login_response" | tail -n1)"
login_json="$(echo "$login_response" | sed '$d')"
token="$(echo "$login_json" | python3 -c 'import json,sys; print(json.load(sys.stdin).get("accessToken",""))')"

if [[ -z "$token" ]]; then
  echo "No se obtuvo accessToken en /api/auth/login" >&2
  echo "$login_json" >&2
  exit 1
fi

me_code="$(curl -sS -o /tmp/robust_me.json -w '%{http_code}' -H "Authorization: Bearer $token" "$BASE_URL/api/auth/me")"
users_code="$(curl -sS -o /tmp/robust_users.json -w '%{http_code}' -H "Authorization: Bearer $token" "$BASE_URL/api/users")"
matrix_roles_code="$(curl -sS -o /tmp/robust_matrix_roles.json -w '%{http_code}' -H "Authorization: Bearer $token" "$BASE_URL/api/authorization-matrix/roles")"

echo "CONFIG: ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT Authorization__UseRobustMatrix=$Authorization__UseRobustMatrix Authorization__EnableLegacyFallback=$Authorization__EnableLegacyFallback Authentication__ConfiguredUsersRobustBridge__Enabled=$Authentication__ConfiguredUsersRobustBridge__Enabled"
echo "POST /api/auth/login => $login_code"
echo "GET /api/auth/me => $me_code"
echo "GET /api/users => $users_code"
echo "GET /api/authorization-matrix/roles => $matrix_roles_code"
echo "ME sample: $(head -c 180 /tmp/robust_me.json)"
echo "USERS sample: $(head -c 180 /tmp/robust_users.json)"
echo "MATRIX ROLES sample: $(head -c 180 /tmp/robust_matrix_roles.json)"

