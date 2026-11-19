---
applyTo: "src/**/*.cs,tests/**/*.cs"
---

# Backend instructions

- Target .NET 10 and preserve the four-layer architecture.
- Keep domain rules framework-independent.
- Use MediatR commands and queries with one handler per request.
- Use EF Core and `AsNoTracking` for reads.
- Use parameterized Dapper SQL for writes.
- Keep controllers thin and use the validation pipeline.
- Map domain exceptions through RFC 7807 `ProblemDetails`.
- Use UTC and the existing time abstraction.
- Preserve transaction and locking behavior for inventory movements.
- Add unit tests for every changed rule or handler.
