# Inventory Frontend

React dashboard for the Inventory API.

## Local development

```powershell
Copy-Item .env.example .env
npm install
npm run dev
```

Open `http://localhost:5173`. The API and Keycloak services must be running.

## Validation

```powershell
npm run build
npm run lint
npm run test
```

## Features

- Keycloak OAuth2 Authorization Code flow with PKCE.
- Protected routes and automatic token refresh.
- Product search, creation, update and soft delete.
- Category management.
- Inventory movement registration and history.
- Dashboard with stock alerts and recent movements.

The frontend client is configured in `docker/keycloak/realm-export.json` as `inventory-frontend`.
