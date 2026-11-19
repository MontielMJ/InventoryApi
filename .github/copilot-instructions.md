# Inventory API repository instructions

## Project context

This repository contains a .NET 10 inventory API and a React TypeScript frontend. The API manages products, categories and immutable inventory movements. The frontend consumes the API through OAuth2/OIDC authentication provided by Keycloak.

## General rules

- Read `docs/ai/spec.md` before changing behavior or API contracts.
- Keep production code, identifiers and technical documentation in English.
- Preserve the existing Clean Architecture boundaries.
- Keep domain rules in `Inventory.Domain` and orchestration in `Inventory.Application`.
- Use EF Core for read operations and Dapper for write operations.
- Do not modify stock through product commands; stock changes only through movements.
- Preserve soft-delete behavior and the inventory audit trail.
- Do not introduce production secrets into source control.
- Do not delete or rewrite applied migrations without explicit approval.
- Do not run destructive Git commands or push changes automatically.
- Run the smallest relevant validation commands after every change.

## Required validation

Backend changes should pass:

```powershell
dotnet build
dotnet test
```

Frontend changes should pass:

```powershell
cd frontend
npm run lint
npm run test
npm run build
```

Cross-cutting changes should also pass:

```powershell
docker compose config
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\check-clean-code.ps1
```
