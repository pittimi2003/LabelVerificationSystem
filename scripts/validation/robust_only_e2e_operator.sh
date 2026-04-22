#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
API_PROJECT="$ROOT_DIR/source/Backend/Api/LabelVerificationSystem.Api/LabelVerificationSystem.Api.csproj"
BASE_URL="${BASE_URL:-http://localhost:5041}"
ADMIN_LOGIN_PAYLOAD='{"usernameOrEmail":"admin","password":"Admin123!","rememberMe":false}'
OPERATOR_LOGIN_PAYLOAD='{"usernameOrEmail":"operator","password":"Operator123!","rememberMe":false}'
LOG_FILE="${LOG_FILE:-$ROOT_DIR/.tmp-robust-only-e2e-operator.log}"

export ASPNETCORE_ENVIRONMENT=Development
export Authorization__UseRobustMatrix=true
export Authorization__EnableLegacyFallback=true
export Authorization__RobustOnlyCutover__Enabled=true
export Authorization__RobustOnlyCutover__UserIds__0=admin-001
export Authorization__RobustOnlyCutover__UserIds__1=manager-001
export Authorization__RobustOnlyCutover__UserIds__2=operator-001
export Authorization__RobustOnlyCutover__Scopes__0=UsersAdministration:View
export Authorization__RobustOnlyCutover__Scopes__1=UsersAdministration:Create
export Authorization__RobustOnlyCutover__Scopes__2=UsersAdministration:Edit
export Authorization__RobustOnlyCutover__Scopes__3=UsersAdministration:ActivateDeactivate
export Authorization__RobustOnlyCutover__Scopes__4=AuthorizationMatrixAdministration:Manage
export Authorization__RobustOnlyCutover__Scopes__5=ExcelUploads:View
export Authorization__RobustOnlyCutover__Scopes__6=ExcelUploads:Upload
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

admin_login_response="$(curl -sS -w '\n%{http_code}' \
  -H 'Content-Type: application/json' \
  -d "$ADMIN_LOGIN_PAYLOAD" \
  "$BASE_URL/api/auth/login")"
admin_login_code="$(echo "$admin_login_response" | tail -n1)"
admin_login_json="$(echo "$admin_login_response" | sed '$d')"
admin_token="$(echo "$admin_login_json" | python3 -c 'import json,sys; print(json.load(sys.stdin).get("accessToken",""))')"

if [[ -z "$admin_token" ]]; then
  echo "No se obtuvo accessToken para admin." >&2
  echo "$admin_login_json" >&2
  exit 1
fi

matrix_operators_code="$(curl -sS -o /tmp/robust_matrix_operators.json -w '%{http_code}' \
  -H "Authorization: Bearer $admin_token" \
  "$BASE_URL/api/authorization-matrix/roles/Operators")"

operator_login_response="$(curl -sS -w '\n%{http_code}' \
  -H 'Content-Type: application/json' \
  -d "$OPERATOR_LOGIN_PAYLOAD" \
  "$BASE_URL/api/auth/login")"
operator_login_code="$(echo "$operator_login_response" | tail -n1)"
operator_login_json="$(echo "$operator_login_response" | sed '$d')"
operator_token="$(echo "$operator_login_json" | python3 -c 'import json,sys; print(json.load(sys.stdin).get("accessToken",""))')"
operator_refresh_token="$(echo "$operator_login_json" | python3 -c 'import json,sys; print(json.load(sys.stdin).get("refreshToken",""))')"

if [[ -z "$operator_token" || -z "$operator_refresh_token" ]]; then
  echo "No se obtuvo accessToken/refreshToken para operator." >&2
  echo "$operator_login_json" >&2
  exit 1
fi

operator_me_code="$(curl -sS -o /tmp/robust_operator_me.json -w '%{http_code}' -H "Authorization: Bearer $operator_token" "$BASE_URL/api/auth/me")"
operator_refresh_response="$(curl -sS -w '\n%{http_code}' \
  -X POST \
  -H 'Content-Type: application/json' \
  -d "{\"refreshToken\":\"$operator_refresh_token\"}" \
  "$BASE_URL/api/auth/refresh")"
operator_refresh_code="$(echo "$operator_refresh_response" | tail -n1)"

operator_users_code="$(curl -sS -o /tmp/robust_operator_users.json -w '%{http_code}' -H "Authorization: Bearer $operator_token" "$BASE_URL/api/users")"
operator_users_roles_code="$(curl -sS -o /tmp/robust_operator_users_roles.json -w '%{http_code}' -H "Authorization: Bearer $operator_token" "$BASE_URL/api/users/roles")"
operator_user_detail_code="$(curl -sS -o /tmp/robust_operator_user_admin.json -w '%{http_code}' -H "Authorization: Bearer $operator_token" "$BASE_URL/api/users/admin-001")"
operator_matrix_roles_code="$(curl -sS -o /tmp/robust_operator_matrix_roles.json -w '%{http_code}' -H "Authorization: Bearer $operator_token" "$BASE_URL/api/authorization-matrix/roles")"
operator_excel_history_code="$(curl -sS -o /tmp/robust_operator_excel_history.json -w '%{http_code}' -H "Authorization: Bearer $operator_token" "$BASE_URL/api/excel-uploads")"
operator_excel_upload_code="$(curl -sS -o /tmp/robust_operator_excel_upload.json -w '%{http_code}' \
  -X POST \
  -H "Authorization: Bearer $operator_token" \
  -F "file=@/dev/null;filename=empty.xlsx;type=application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" \
  "$BASE_URL/api/excel-uploads")"

for expected_ok in \
  "$admin_login_code" \
  "$matrix_operators_code" \
  "$operator_login_code" \
  "$operator_me_code" \
  "$operator_refresh_code" \
  "$operator_excel_history_code"; do
  if [[ ! "$expected_ok" =~ ^2 ]]; then
    echo "Validación robust-only operator falló con código HTTP: $expected_ok" >&2
    exit 1
  fi
done

for expected_forbidden in \
  "$operator_users_code" \
  "$operator_users_roles_code" \
  "$operator_user_detail_code" \
  "$operator_matrix_roles_code"; do
  if [[ "$expected_forbidden" -ne 403 ]]; then
    echo "Se esperaba 403 en endpoint restringido para operator robust-only. Código: $expected_forbidden" >&2
    exit 1
  fi
done

if [[ "$operator_excel_upload_code" -ne 400 ]]; then
  echo "Se esperaba 400 para POST /api/excel-uploads con operator robust-only (autorizado; request inválido por archivo vacío). Código: $operator_excel_upload_code" >&2
  exit 1
fi

echo "CONFIG: ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT Authorization__UseRobustMatrix=$Authorization__UseRobustMatrix Authorization__EnableLegacyFallback=$Authorization__EnableLegacyFallback Authentication__ConfiguredUsersRobustBridge__Enabled=$Authentication__ConfiguredUsersRobustBridge__Enabled Authorization__RobustOnlyCutover__Enabled=$Authorization__RobustOnlyCutover__Enabled"
echo "POST /api/auth/login (admin) => $admin_login_code"
echo "GET /api/authorization-matrix/roles/Operators (admin) => $matrix_operators_code"
echo "POST /api/auth/login (operator) => $operator_login_code"
echo "GET /api/auth/me (operator) => $operator_me_code"
echo "POST /api/auth/refresh (operator) => $operator_refresh_code"
echo "GET /api/excel-uploads (operator) => $operator_excel_history_code"
echo "POST /api/excel-uploads (operator, expected 400 by invalid file) => $operator_excel_upload_code"
echo "GET /api/users (operator, expected 403) => $operator_users_code"
echo "GET /api/users/roles (operator, expected 403) => $operator_users_roles_code"
echo "GET /api/users/admin-001 (operator, expected 403) => $operator_user_detail_code"
echo "GET /api/authorization-matrix/roles (operator, expected 403) => $operator_matrix_roles_code"
echo "MATRIX Operators sample: $(head -c 220 /tmp/robust_matrix_operators.json)"
echo "ME operator sample: $(head -c 220 /tmp/robust_operator_me.json)"
