#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
API_PROJECT="$ROOT_DIR/source/Backend/Api/LabelVerificationSystem.Api/LabelVerificationSystem.Api.csproj"
BASE_URL="${BASE_URL:-http://localhost:5041}"
LOGIN_PAYLOAD='{"usernameOrEmail":"admin","password":"Admin123!","rememberMe":false}'
MANAGER_LOGIN_PAYLOAD='{"usernameOrEmail":"manager","password":"Manager123!","rememberMe":false}'
LOG_FILE="${LOG_FILE:-$ROOT_DIR/.tmp-robust-only-e2e.log}"

export ASPNETCORE_ENVIRONMENT=Development
export Authorization__UseRobustMatrix=true
export Authorization__EnableLegacyFallback=true
export Authorization__RobustOnlyCutover__Enabled=true
export Authorization__RobustOnlyCutover__UserIds__0=admin-001
export Authorization__RobustOnlyCutover__UserIds__1=manager-001
export Authorization__RobustOnlyCutover__Scopes__0=UsersAdministration:View
export Authorization__RobustOnlyCutover__Scopes__1=UsersAdministration:Create
export Authorization__RobustOnlyCutover__Scopes__2=UsersAdministration:Edit
export Authorization__RobustOnlyCutover__Scopes__3=UsersAdministration:ActivateDeactivate
export Authorization__RobustOnlyCutover__Scopes__4=AuthorizationMatrixAdministration:Manage
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
users_roles_code="$(curl -sS -o /tmp/robust_users_roles.json -w '%{http_code}' -H "Authorization: Bearer $token" "$BASE_URL/api/users/roles")"
users_code="$(curl -sS -o /tmp/robust_users.json -w '%{http_code}' -H "Authorization: Bearer $token" "$BASE_URL/api/users")"
user_detail_code="$(curl -sS -o /tmp/robust_users_admin.json -w '%{http_code}' -H "Authorization: Bearer $token" "$BASE_URL/api/users/admin-001")"
matrix_roles_code="$(curl -sS -o /tmp/robust_matrix_roles.json -w '%{http_code}' -H "Authorization: Bearer $token" "$BASE_URL/api/authorization-matrix/roles")"

manager_login_response="$(curl -sS -w '\n%{http_code}' \
  -H 'Content-Type: application/json' \
  -d "$MANAGER_LOGIN_PAYLOAD" \
  "$BASE_URL/api/auth/login")"
manager_login_code="$(echo "$manager_login_response" | tail -n1)"
manager_login_json="$(echo "$manager_login_response" | sed '$d')"
manager_token="$(echo "$manager_login_json" | python3 -c 'import json,sys; print(json.load(sys.stdin).get("accessToken",""))')"

if [[ -z "$manager_token" ]]; then
  echo "No se obtuvo accessToken en /api/auth/login para manager." >&2
  echo "$manager_login_json" >&2
  exit 1
fi

manager_me_code="$(curl -sS -o /tmp/robust_manager_me.json -w '%{http_code}' -H "Authorization: Bearer $manager_token" "$BASE_URL/api/auth/me")"
manager_users_code="$(curl -sS -o /tmp/robust_manager_users.json -w '%{http_code}' -H "Authorization: Bearer $manager_token" "$BASE_URL/api/users")"
manager_users_roles_code="$(curl -sS -o /tmp/robust_manager_users_roles.json -w '%{http_code}' -H "Authorization: Bearer $manager_token" "$BASE_URL/api/users/roles")"
manager_create_code="$(curl -sS -o /tmp/robust_manager_create.json -w '%{http_code}' \
  -X POST \
  -H 'Content-Type: application/json' \
  -H "Authorization: Bearer $manager_token" \
  -d '{"username":"manager-should-not-create","displayName":"Denied","email":"denied@local.test","password":"Manager123!","roles":["Operators"],"permissions":[],"isActive":true}' \
  "$BASE_URL/api/users")"

suffix="$(date +%s)"
create_payload="$(cat <<JSON
{"username":"robust-cutover-$suffix","displayName":"Robust Cutover $suffix","email":"robust-cutover-$suffix@local.test","password":"Admin123!","roles":["Operators"],"permissions":[],"isActive":true}
JSON
)"
create_response="$(curl -sS -w '\n%{http_code}' \
  -H 'Content-Type: application/json' \
  -H "Authorization: Bearer $token" \
  -d "$create_payload" \
  "$BASE_URL/api/users")"
create_code="$(echo "$create_response" | tail -n1)"
create_json="$(echo "$create_response" | sed '$d')"
created_user_id="$(echo "$create_json" | python3 -c 'import json,sys; print(json.load(sys.stdin).get("userId",""))')"

if [[ -z "$created_user_id" ]]; then
  echo "No se obtuvo userId en creación robust-only selectiva (/api/users POST)." >&2
  echo "$create_json" >&2
  exit 1
fi

update_payload="$(cat <<JSON
{"displayName":"Robust Cutover Updated $suffix","email":"robust-cutover-$suffix@local.test","roles":["Operators"],"permissions":[],"isActive":true,"newPassword":null}
JSON
)"
update_code="$(curl -sS -o /tmp/robust_user_update.json -w '%{http_code}' \
  -X PUT \
  -H 'Content-Type: application/json' \
  -H "Authorization: Bearer $token" \
  -d "$update_payload" \
  "$BASE_URL/api/users/$created_user_id")"

activation_code="$(curl -sS -o /tmp/robust_user_activation.json -w '%{http_code}' \
  -X PATCH \
  -H 'Content-Type: application/json' \
  -H "Authorization: Bearer $token" \
  -d '{"isActive":false}' \
  "$BASE_URL/api/users/$created_user_id/activation")"

for expected_ok in \
  "$login_code" \
  "$manager_login_code" \
  "$me_code" \
  "$manager_me_code" \
  "$users_roles_code" \
  "$manager_users_roles_code" \
  "$users_code" \
  "$manager_users_code" \
  "$user_detail_code" \
  "$create_code" \
  "$update_code" \
  "$activation_code" \
  "$matrix_roles_code"; do
  if [[ ! "$expected_ok" =~ ^2 ]]; then
    echo "Validación robust-only selectiva falló con código HTTP: $expected_ok" >&2
    exit 1
  fi
done

if [[ "$manager_create_code" -ne 403 ]]; then
  echo "Se esperaba 403 para POST /api/users con manager robust-only (scope fuera de perímetro). Código: $manager_create_code" >&2
  exit 1
fi

echo "CONFIG: ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT Authorization__UseRobustMatrix=$Authorization__UseRobustMatrix Authorization__EnableLegacyFallback=$Authorization__EnableLegacyFallback Authentication__ConfiguredUsersRobustBridge__Enabled=$Authentication__ConfiguredUsersRobustBridge__Enabled Authorization__RobustOnlyCutover__Enabled=$Authorization__RobustOnlyCutover__Enabled"
echo "POST /api/auth/login => $login_code"
echo "POST /api/auth/login (manager) => $manager_login_code"
echo "GET /api/auth/me => $me_code"
echo "GET /api/auth/me (manager) => $manager_me_code"
echo "GET /api/users/roles => $users_roles_code"
echo "GET /api/users/roles (manager) => $manager_users_roles_code"
echo "GET /api/users => $users_code"
echo "GET /api/users (manager) => $manager_users_code"
echo "GET /api/users/admin-001 => $user_detail_code"
echo "POST /api/users => $create_code"
echo "POST /api/users (manager, expected 403) => $manager_create_code"
echo "PUT /api/users/{created_user_id} => $update_code"
echo "PATCH /api/users/{created_user_id}/activation => $activation_code"
echo "GET /api/authorization-matrix/roles => $matrix_roles_code"
echo "ME sample: $(head -c 180 /tmp/robust_me.json)"
echo "USERS ROLES sample: $(head -c 180 /tmp/robust_users_roles.json)"
echo "USERS sample: $(head -c 180 /tmp/robust_users.json)"
echo "USER DETAIL sample: $(head -c 180 /tmp/robust_users_admin.json)"
echo "MATRIX ROLES sample: $(head -c 180 /tmp/robust_matrix_roles.json)"
