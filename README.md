# LabelVerificationSystem

Solución base con:
- net9.0
- Aspire
- Sqlite
- StandaloneWasm
- Swashbuckle

## Diagnóstico rápido de conectividad frontend ↔ backend (login/auth)

Si en el navegador aparece `TypeError: Failed to fetch` en:
- `GET https://localhost:7131/api/auth/me`
- `POST https://localhost:7131/api/auth/login`

no lo diagnostiques como credenciales o autorización todavía: primero valida conectividad HTTPS.

### 1) Verificar URL real del backend

```bash
dotnet run --project source/Backend/Api/LabelVerificationSystem.Api/LabelVerificationSystem.Api.csproj --launch-profile https
```

Debes ver en consola:
- `Now listening on: https://localhost:7131`
- `Now listening on: http://localhost:5041`

### 2) Verificar `Api:BaseUrl` del frontend

Archivo esperado en Development:
- `source/Frontend/Web/LabelVerificationSystem.Web/wwwroot/appsettings.Development.json`

Valor esperado:
- `"Api": { "BaseUrl": "https://localhost:7131/" }`

### 3) Confirmar estado del certificado dev HTTPS

```bash
dotnet dev-certs https --check --trust
```

Si reporta *"none of them is trusted"*, el navegador puede fallar con `Failed to fetch` antes de recibir HTTP.

### 4) Corregir confianza del certificado

```bash
dotnet dev-certs https --trust
```

Luego vuelve a validar:

```bash
dotnet dev-certs https --check --trust
```

### 5) Verificar que ya hay respuesta HTTP real desde la API

```bash
curl -k -i https://localhost:7131/api/auth/me -H 'Origin: https://localhost:7219'
```

Si ves `401` (u otro código HTTP), la conectividad dejó de estar en estado `Failed to fetch` y ya pasaste a capa HTTP de negocio/autorización.
