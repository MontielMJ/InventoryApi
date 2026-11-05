# Instrucciones de ejecución

## 1. Requisitos

- Windows 10/11.
- Docker Desktop iniciado.
- .NET SDK 10.0, solo necesario para ejecutar la API fuera de Docker.
- Git, opcional.

## 2. Configuración inicial

Desde la carpeta raíz del proyecto:

```powershell
cd C:\Users\Unico\Desktop\Angular\InventoryApi
```

Opcionalmente, crear el archivo `.env` a partir de `.env.example`:

```powershell
Copy-Item .env.example .env
```

Los valores locales predeterminados son:

| Servicio | Dirección |
|---|---|
| API | `http://localhost:8081` |
| Swagger | `http://localhost:8081/swagger` |
| Health check | `http://localhost:8081/health` |
| Keycloak | `http://localhost:8080` |
| SQL Server | `localhost,14330` |

## 3. Ejecutar con Docker Compose

Construir las imágenes y levantar todos los servicios:

```powershell
docker compose up --build -d
```

Verificar el estado de los contenedores:

```powershell
docker compose ps
```

Esperar hasta que SQL Server aparezca como `healthy` y la API esté en estado `Up`.

Verificar la API:

```powershell
Invoke-WebRequest http://localhost:8081/health -UseBasicParsing
```

La respuesta esperada es:

```json
{"status":"Healthy"}
```

## 4. Ejecutar el smoke test

El smoke test valida autenticación, Swagger, CRUD, movimientos de inventario, control de stock y borrado lógico:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\smoke-test.ps1
```

La ejecución termina correctamente cuando muestra:

```text
All smoke checks passed.
```

## 5. Usar Swagger

Abrir:

```text
http://localhost:8081/swagger
```

Presionar **Authorize** y autenticarse con:

```text
Usuario: inventory-admin
Contraseña: Admin123!
```

Después de autorizar, se pueden ejecutar los endpoints protegidos desde Swagger.

## 6. Conectarse a SQL Server desde SSMS

Usar estos valores en SQL Server Management Studio:

| Campo | Valor |
|---|---|
| Server name | `localhost,14330` |
| Authentication | SQL Server Authentication |
| Login | `sa` |
| Password | `Your_password123` |
| Database | `InventoryDb` |

El puerto externo es `14330` porque la instancia local de SQL Server normalmente utiliza el puerto `1433`.

## 7. Ejecutar la API localmente

Primero iniciar las dependencias:

```powershell
docker compose up -d sqlserver keycloak
```

La configuración de desarrollo ya utiliza SQL Server en `localhost,14330`.

Ejecutar la API:

```powershell
dotnet run --project .\src\Inventory.Api
```

La API estará disponible en:

```text
http://localhost:8081
```

No ejecutar simultáneamente la API de Docker y la API local en el mismo puerto.

## 8. Ejecutar compilación y pruebas

Compilar la solución:

```powershell
dotnet build
```

Ejecutar las pruebas unitarias:

```powershell
dotnet test
```

Validar las reglas de Clean Code:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\check-clean-code.ps1
```

## 9. Detener los servicios

Detener los contenedores conservando los datos:

```powershell
docker compose down
```

Detener los contenedores y eliminar también el volumen de SQL Server:

```powershell
docker compose down -v
```

> El comando `docker compose down -v` elimina la base de datos y todos sus datos.

## 10. Ver logs

Logs de la API:

```powershell
docker compose logs -f api
```

Logs de SQL Server:

```powershell
docker compose logs -f sqlserver
```

Logs de Keycloak:

```powershell
docker compose logs -f keycloak
```

## 11. Solución de problemas frecuentes

### El puerto 1433 está ocupado

Usar `localhost,14330` para conectarse al SQL Server de Docker. No cambiar la instancia local de SQL Server.

### La API permanece cargando

Revisar el estado y los logs:

```powershell
docker compose ps
docker compose logs api --tail 100
```

Confirmar que SQL Server esté en estado `healthy`.

### Login failed for user `sa`

Verificar que la conexión utilice:

```text
Server=localhost,14330
User=sa
Password=Your_password123
```

Si se cambió `SQL_SA_PASSWORD` después de crear el volumen, eliminar y recrear el volumen:

```powershell
docker compose down -v
docker compose up --build -d
```

### El puerto 8081 está ocupado

Detener el proceso que utiliza el puerto o ejecutar la API con otro puerto:

```powershell
dotnet run --project .\src\Inventory.Api --urls http://localhost:8082
```

En ese caso, la API estará disponible en `http://localhost:8082`.
