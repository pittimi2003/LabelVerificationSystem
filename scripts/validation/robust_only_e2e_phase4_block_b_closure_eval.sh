#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
API_PROJECT="$ROOT_DIR/source/Backend/Api/LabelVerificationSystem.Api/LabelVerificationSystem.Api.csproj"
DB_PATH="$ROOT_DIR/source/Backend/Api/LabelVerificationSystem.Api/labelverification.db"
BASE_URL="${BASE_URL:-http://localhost:5041}"
LOG_FILE="${LOG_FILE:-$ROOT_DIR/.tmp-robust-only-phase4-closure-e2e.log}"

ADMIN_LOGIN_PAYLOAD='{"usernameOrEmail":"admin","password":"Admin123!","rememberMe":false}'
MANAGER_LOGIN_PAYLOAD='{"usernameOrEmail":"manager","password":"Manager123!","rememberMe":false}'
OPERATOR_LOGIN_PAYLOAD='{"usernameOrEmail":"operator","password":"Operator123!","rememberMe":false}'

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

extract_token() {
  python3 -c 'import json,sys; print(json.loads(sys.stdin.read()).get("accessToken", ""))'
}

admin_login_response="$(curl -sS -w '\n%{http_code}' -H 'Content-Type: application/json' -d "$ADMIN_LOGIN_PAYLOAD" "$BASE_URL/api/auth/login")"
admin_login_code="$(echo "$admin_login_response" | tail -n1)"
admin_login_json="$(echo "$admin_login_response" | sed '$d')"
admin_token="$(echo "$admin_login_json" | extract_token)"

manager_login_response="$(curl -sS -w '\n%{http_code}' -H 'Content-Type: application/json' -d "$MANAGER_LOGIN_PAYLOAD" "$BASE_URL/api/auth/login")"
manager_login_code="$(echo "$manager_login_response" | tail -n1)"
manager_login_json="$(echo "$manager_login_response" | sed '$d')"
manager_token="$(echo "$manager_login_json" | extract_token)"

operator_login_response="$(curl -sS -w '\n%{http_code}' -H 'Content-Type: application/json' -d "$OPERATOR_LOGIN_PAYLOAD" "$BASE_URL/api/auth/login")"
operator_login_code="$(echo "$operator_login_response" | tail -n1)"
operator_login_json="$(echo "$operator_login_response" | sed '$d')"
operator_token="$(echo "$operator_login_json" | extract_token)"

for token_name in admin_token manager_token operator_token; do
  if [[ -z "${!token_name}" ]]; then
    echo "No se obtuvo token válido para $token_name" >&2
    exit 1
  fi
done

admin_me_code="$(curl -sS -o /tmp/phase4_admin_me.json -w '%{http_code}' -H "Authorization: Bearer $admin_token" "$BASE_URL/api/auth/me")"
manager_me_code="$(curl -sS -o /tmp/phase4_manager_me.json -w '%{http_code}' -H "Authorization: Bearer $manager_token" "$BASE_URL/api/auth/me")"
operator_me_code="$(curl -sS -o /tmp/phase4_operator_me.json -w '%{http_code}' -H "Authorization: Bearer $operator_token" "$BASE_URL/api/auth/me")"

admin_users_code="$(curl -sS -o /tmp/phase4_admin_users.json -w '%{http_code}' -H "Authorization: Bearer $admin_token" "$BASE_URL/api/users")"
manager_users_code="$(curl -sS -o /tmp/phase4_manager_users.json -w '%{http_code}' -H "Authorization: Bearer $manager_token" "$BASE_URL/api/users")"
operator_users_code="$(curl -sS -o /tmp/phase4_operator_users.json -w '%{http_code}' -H "Authorization: Bearer $operator_token" "$BASE_URL/api/users")"

admin_matrix_roles_code="$(curl -sS -o /tmp/phase4_admin_matrix_roles.json -w '%{http_code}' -H "Authorization: Bearer $admin_token" "$BASE_URL/api/authorization-matrix/roles")"
manager_matrix_roles_code="$(curl -sS -o /tmp/phase4_manager_matrix_roles.json -w '%{http_code}' -H "Authorization: Bearer $manager_token" "$BASE_URL/api/authorization-matrix/roles")"
operator_matrix_roles_code="$(curl -sS -o /tmp/phase4_operator_matrix_roles.json -w '%{http_code}' -H "Authorization: Bearer $operator_token" "$BASE_URL/api/authorization-matrix/roles")"

admin_excel_history_code="$(curl -sS -o /tmp/phase4_admin_excel_history.json -w '%{http_code}' -H "Authorization: Bearer $admin_token" "$BASE_URL/api/excel-uploads")"
manager_excel_history_code="$(curl -sS -o /tmp/phase4_manager_excel_history.json -w '%{http_code}' -H "Authorization: Bearer $manager_token" "$BASE_URL/api/excel-uploads")"
operator_excel_history_code="$(curl -sS -o /tmp/phase4_operator_excel_history.json -w '%{http_code}' -H "Authorization: Bearer $operator_token" "$BASE_URL/api/excel-uploads")"

admin_excel_upload_code="$(curl -sS -o /tmp/phase4_admin_excel_upload.json -w '%{http_code}' -X POST -H "Authorization: Bearer $admin_token" -F "file=@/dev/null;filename=empty.xlsx;type=application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" "$BASE_URL/api/excel-uploads")"
manager_excel_upload_code="$(curl -sS -o /tmp/phase4_manager_excel_upload.json -w '%{http_code}' -X POST -H "Authorization: Bearer $manager_token" -F "file=@/dev/null;filename=empty.xlsx;type=application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" "$BASE_URL/api/excel-uploads")"
operator_excel_upload_code="$(curl -sS -o /tmp/phase4_operator_excel_upload.json -w '%{http_code}' -X POST -H "Authorization: Bearer $operator_token" -F "file=@/dev/null;filename=empty.xlsx;type=application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" "$BASE_URL/api/excel-uploads")"

for expected_ok in \
  "$admin_login_code" "$manager_login_code" "$operator_login_code" \
  "$admin_me_code" "$manager_me_code" "$operator_me_code" \
  "$admin_users_code" "$manager_users_code" \
  "$admin_matrix_roles_code" \
  "$admin_excel_history_code" "$manager_excel_history_code" "$operator_excel_history_code"; do
  if [[ ! "$expected_ok" =~ ^2 ]]; then
    echo "Validación base falló (se esperaba 2xx): $expected_ok" >&2
    exit 1
  fi
done

for expected_forbidden in \
  "$operator_users_code" \
  "$manager_matrix_roles_code" \
  "$operator_matrix_roles_code" \
  "$manager_excel_upload_code"; do
  if [[ "$expected_forbidden" -ne 403 ]]; then
    echo "Validación deny falló (se esperaba 403): $expected_forbidden" >&2
    exit 1
  fi
done

if [[ "$admin_excel_upload_code" -ne 400 ]]; then
  echo "Se esperaba 400 para POST /api/excel-uploads con admin (archivo vacío): $admin_excel_upload_code" >&2
  exit 1
fi

if [[ "$operator_excel_upload_code" -ne 400 ]]; then
  echo "Se esperaba 400 para POST /api/excel-uploads con operator (archivo vacío): $operator_excel_upload_code" >&2
  exit 1
fi

python3 - <<PY
import sqlite3
con = sqlite3.connect(r"$DB_PATH")
cur = con.cursor()
cur.execute("""
update SystemUsers
set RolesJson='["SuperAdmin"]',
    PermissionsJson='["users.manage","authorization.matrix.manage","excel.upload.create"]'
where UserId='operator-001'
""")
con.commit()
print('db_tamper_rows', cur.rowcount)
PY

tampered_login_response="$(curl -sS -w '\n%{http_code}' -H 'Content-Type: application/json' -d "$OPERATOR_LOGIN_PAYLOAD" "$BASE_URL/api/auth/login")"
tampered_login_code="$(echo "$tampered_login_response" | tail -n1)"
tampered_login_json="$(echo "$tampered_login_response" | sed '$d')"
tampered_operator_token="$(echo "$tampered_login_json" | extract_token)"

if [[ ! "$tampered_login_code" =~ ^2 ]] || [[ -z "$tampered_operator_token" ]]; then
  echo "No se pudo reloguear operator tras tampering legacy." >&2
  exit 1
fi

tampered_operator_me_code="$(curl -sS -o /tmp/phase4_operator_me_tampered.json -w '%{http_code}' -H "Authorization: Bearer $tampered_operator_token" "$BASE_URL/api/auth/me")"
tampered_operator_users_code="$(curl -sS -o /tmp/phase4_operator_users_tampered.json -w '%{http_code}' -H "Authorization: Bearer $tampered_operator_token" "$BASE_URL/api/users")"
tampered_operator_matrix_roles_code="$(curl -sS -o /tmp/phase4_operator_matrix_roles_tampered.json -w '%{http_code}' -H "Authorization: Bearer $tampered_operator_token" "$BASE_URL/api/authorization-matrix/roles")"

if [[ ! "$tampered_operator_me_code" =~ ^2 ]]; then
  echo "GET /api/auth/me con operator tampered debía ser 2xx: $tampered_operator_me_code" >&2
  exit 1
fi

if [[ "$tampered_operator_users_code" -ne 403 ]] || [[ "$tampered_operator_matrix_roles_code" -ne 403 ]]; then
  echo "Tampering legacy elevó permisos indebidamente. users=$tampered_operator_users_code matrix=$tampered_operator_matrix_roles_code" >&2
  exit 1
fi

python3 - <<'PY'
import json
from pathlib import Path
me = json.loads(Path('/tmp/phase4_operator_me_tampered.json').read_text())
roles = set(me.get('user', {}).get('roles', []))
permissions = set(me.get('user', {}).get('permissions', []))
if 'SuperAdmin' in roles or 'users.manage' in permissions or 'authorization.matrix.manage' in permissions:
    raise SystemExit('Se detectó contaminación de sesión por RolesJson/PermissionsJson en cutover robust-only.')
print('tampered_roles', sorted(roles))
print('tampered_permissions', sorted(permissions))
PY

python3 - <<PY
import sqlite3
con = sqlite3.connect(r"$DB_PATH")
cur = con.cursor()
cur.execute("update SystemUsers set RolesJson='[]', PermissionsJson='[]' where UserId in ('admin-001','manager-001','operator-001')")
con.commit()
print('db_restore_rows', cur.rowcount)
PY

echo "CONFIG: ASPNETCORE_ENVIRONMENT=$ASPNETCORE_ENVIRONMENT Authorization__UseRobustMatrix=$Authorization__UseRobustMatrix Authorization__EnableLegacyFallback=$Authorization__EnableLegacyFallback Authorization__RobustOnlyCutover__Enabled=$Authorization__RobustOnlyCutover__Enabled"
echo "POST /api/auth/login (admin|manager|operator) => $admin_login_code|$manager_login_code|$operator_login_code"
echo "GET /api/auth/me (admin|manager|operator) => $admin_me_code|$manager_me_code|$operator_me_code"
echo "GET /api/users (admin|manager|operator) => $admin_users_code|$manager_users_code|$operator_users_code"
echo "GET /api/authorization-matrix/roles (admin|manager|operator) => $admin_matrix_roles_code|$manager_matrix_roles_code|$operator_matrix_roles_code"
echo "GET /api/excel-uploads (admin|manager|operator) => $admin_excel_history_code|$manager_excel_history_code|$operator_excel_history_code"
echo "POST /api/excel-uploads (admin|manager|operator) => $admin_excel_upload_code|$manager_excel_upload_code|$operator_excel_upload_code"
echo "TAMPERED /api/users (operator) => $tampered_operator_users_code"
echo "TAMPERED /api/authorization-matrix/roles (operator) => $tampered_operator_matrix_roles_code"
