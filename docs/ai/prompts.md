# AI Prompts and Reasoning Summary

Status: reasoned summary of the prompts used during the exercise.

The prompts are grouped by phase. For each one this document records what was asked, what the model produced and how the output was reviewed. The intent is to show direction and supervision, not to dump a raw transcript.

## 1. Reading and decomposing the assignment

### Prompt

> Read the assignment PDF at `PruebaTecnica_General.pdf`. Extract every functional and technical requirement, including the Clean Code rules and the AI evidence requirements. Produce a checklist that can be used to verify the deliverable at the end. Do not write code yet.

### Why

The document mixes hard requirements, evaluation criteria and suggestions. The first step was to separate mandatory items from optional ones before choosing a stack.

### Outcome

The output was a checklist with four groups: architecture and patterns, data and persistence, security, and delivery. It also surfaced two details that are easy to miss:

- The Clean Code rules apply to code and comments, while user-facing messages may be in Spanish.
- The AI evidence is graded, so the specification and the decision log have to be produced as real artifacts, not as an afterthought.

The checklist became the acceptance criteria in `docs/ai/spec.md`.

## 2. Choosing the stack

### Prompt

> Propose three viable stacks for this assignment: one conservative, one aligned with the newest LTS, and one that minimizes moving parts. Compare them by delivery risk, reviewability and the likelihood of satisfying the EF Core plus Dapper requirement. Recommend one and justify the trade-offs.

### Why

The assignment says ".NET 8 or superior", which leaves room for interpretation. A wrong choice here affects every later prompt.

### Outcome

The options were:

1. .NET 8 with SQL Server and a self-hosted OAuth2 server.
2. .NET 10 LTS with SQL Server and Keycloak.
3. .NET 8 with SQLite and a minimal token issuer.

Option 2 was selected. .NET 10 is the current LTS, Keycloak gives a real OAuth2/OIDC implementation with almost no code, and SQL Server matches the requirement literally. Option 3 was rejected because SQLite and a hand-rolled issuer would look like shortcuts in a senior evaluation.

The model initially suggested Duende IdentityServer. It was rejected because the community edition has licensing constraints and because a full identity server is more surface area than this exercise needs.

## 3. Spec before code

### Prompt

> Act as a backend architect. Write the specification for the inventory API before implementation. Include scope, out of scope, entities with field-level rules, the HTTP contract with request and response examples, the error contract, authentication, non-functional requirements, the test plan and acceptance criteria. Call out ambiguities and resolve them explicitly.

### Why

This is the Spec-Driven Development evidence. Writing the contract first forces decisions about stock, deletion, error codes and payloads before they are buried in code.

### Outcome

The result is `docs/ai/spec.md`. The review changed four points:

- The first draft allowed stock to be set on product creation. It was changed to "always starts at zero" to preserve the ledger as the only source of stock changes.
- The first draft used `204 No Content` for product creation. It was changed to `201 Created` with a `Location` header.
- The first draft returned the full entity after creation. It was changed to return only the id, which keeps the write response small and avoids leaking persistence details.
- The first draft left category deletion undefined. It was changed to block deletion when active products exist.

These changes were made in the spec first, then implemented.

## 4. Solution skeleton

### Prompt

> Create a .NET 10 solution named `InventoryApi` with four projects: `Inventory.Domain`, `Inventory.Application`, `Inventory.Infrastructure`, `Inventory.Api`, plus an xUnit test project. Enforce the dependency direction Api -> Infrastructure -> Application -> Domain. Use nullable reference types and implicit usings.

### Why

The skeleton is mechanical, but it is also where layering mistakes become expensive. Generating it in one step kept the project references consistent.

### Outcome

The solution was created with `dotnet new` and project references. The model proposed putting repository interfaces in Infrastructure; that was rejected because it would invert the dependency direction. Interfaces stay in Application, implementations in Infrastructure.

## 5. Domain model

### Prompt

> Model `Category`, `Product` and `InventoryMovement` as rich entities with private setters and static factory methods. Add a `StockPolicy` that computes the next stock for `In` and `Out` movements and throws a domain exception when the result would be negative. Keep the domain free of EF Core and ASP.NET Core references.

### Why

Business rules must be testable without a database or an HTTP pipeline. A static policy is the smallest unit that can be tested exhaustively.

### Outcome

The entities use factory methods (`Create`) and behavior methods (`Update`, `Deactivate`) instead of public setters. `StockPolicy.Apply` takes the current stock, the movement type and the quantity, and throws `InsufficientStockException` when the result is negative. EF Core can still materialize the entities because of the private parameterless constructor.

The model first proposed an `ApplyMovement` method on `Product`. It was moved to `StockPolicy` because the write path uses Dapper and does not load the entity. Keeping the rule independent from the entity made it reusable in both paths.

## 6. CQRS with MediatR and FluentValidation

### Prompt

> Implement CQRS with MediatR. One command or query per file, one handler per file, validators for every command. Add a pipeline behavior that runs FluentValidation before the handler and throws a `ValidationException` that the API can translate to `400`. Keep handlers under 25 lines and avoid nested conditionals.

### Why

This is the core of the architecture grade. The prompt had to constrain file layout and method size, otherwise the generated code drifts from the Clean Code rules.

### Outcome

The generated structure follows `Feature/Commands/Operation` and `Feature/Queries/Operation`. Each command is a `record` that implements `IRequest<T>`. Handlers depend on repository interfaces and `TimeProvider`.

Two corrections were needed:

- The first version of `RegisterMovementCommandHandler` loaded the product through EF Core and then wrote through Dapper. That duplicated the read and defeated the transaction boundary. It was replaced by a single write-repository call that returns a status object.
- The first version of the validators used nested `When` blocks. They were flattened to keep nesting at one level.

MediatR 13 was proposed by the model. It was rejected in favor of 12.4.1 because of the licensing changes announced for newer versions. This is recorded in `docs/ai/decisions.md`.

## 7. Persistence: EF Core for reads, Dapper for writes

### Prompt

> Implement the read side with EF Core using `AsNoTracking` and projections to DTOs. Implement the write side with Dapper and parameterized SQL. Register a movement inside one transaction: select the product row with `UPDLOCK, ROWLOCK`, compute the next stock with `StockPolicy`, insert the movement and update the product stock. Return a result object instead of throwing from the repository.

### Why

The assignment requires both technologies. The concurrency requirement is not explicit, but negative stock under concurrent `Out` movements is the most likely correctness bug in an inventory system.

### Outcome

The read repositories return DTOs directly with `Select`, so no tracking and no navigation fix-up. Write repositories use `CommandDefinition` with cancellation tokens and anonymous parameter objects.

The first draft of the movement repository threw `InsufficientStockException` from inside the transaction. It was changed to return `MovementRegistrationResult` with a status enum, so the handler owns the translation to domain exceptions and the transaction is never left in an ambiguous state.

The model also proposed an optimistic concurrency token on `Product`. It was rejected because Dapper updates bypass EF Core concurrency tokens unless the check is written by hand. `UPDLOCK, ROWLOCK` expresses the intent directly in SQL Server.

## 8. API layer and error handling

### Prompt

> Add thin controllers that only translate HTTP to MediatR requests. Add a global exception middleware that maps domain exceptions to RFC 7807 `ProblemDetails`. Configure JWT bearer authentication against Keycloak and Swagger UI with the authorization code plus PKCE flow.

### Why

Controllers must not contain business logic, and error handling must be consistent across every endpoint.

### Outcome

Controllers contain no try/catch and no business rules. `ExceptionHandlingMiddleware` maps `NotFoundException`, `ConflictException`, `InsufficientStockException` and `ValidationException` to the right status codes.

The first draft hard-coded the OAuth2 URLs in `Program.cs`. They were moved to configuration (`Swagger__ClientId`, `Swagger__AuthorizationUrl`, `Swagger__TokenUrl`) to satisfy the environment-variable rule.

## 9. Tests

### Prompt

> Write unit tests for `StockPolicy`, the movement handler, product creation and update, category deletion, the products query and the validators. Use xUnit, NSubstitute and Shouldly. Test behavior, not implementation details. Keep the tests readable and independent.

### Why

The assignment asks for adequate coverage of critical components. Testing the domain policy and the handlers covers the rules that are most likely to break.

### Outcome

Twenty-eight tests cover the happy path and the failure paths. FluentAssertions was proposed and rejected because of its recent licensing change; Shouldly is used instead. The test project has no database dependency, so it runs in CI without containers.

## 10. Docker and documentation

### Prompt

> Add a multi-stage Dockerfile, a Compose file with SQL Server, Keycloak and the API, a Keycloak realm export with two clients and a demo user, and a README that explains setup, authentication, endpoints, local debugging and migrations.

### Why

The one-command startup is an explicit requirement, and the reviewer will follow the README literally.

### Outcome

The Compose stack uses health checks and `depends_on` conditions. The API waits for SQL Server and applies migrations with retries. The realm export contains the audience mapper that makes the `inventory-api` audience appear in both flows.

Two issues were found during review and fixed:

- Keycloak exposes `localhost:8080` to the host but `keycloak:8080` to the API container. The API validates the issuer against the public URL while downloading metadata from the internal URL. This split-horizon configuration is documented in the README.
- The SQL Server health check had to support both `mssql-tools18` and `mssql-tools`, because the tools path changed across image versions.

## 11. Clean Code enforcement

### Prompt

> Write a PowerShell script that fails when a C# file under `src` contains a blank line that is not immediately before a `return`, a comment, or a non-ASCII character. Run it as part of the final verification and fix every violation.

### Why

The Clean Code rules are explicit and mechanical. Automating the mechanical part makes the review objective and prevents regressions.

### Outcome

`scripts/check-clean-code.ps1` reports zero violations. The final code has no comments, no blank lines except before `return`, and ASCII-only source files. Method length and nesting were reviewed manually because they are not reliably detectable by a simple script.

## 12. What was not delegated to the model

- The final choice of stack and the acceptance of each trade-off.
- The decision to reject MediatR 13 and FluentAssertions.
- The stock model and the concurrency strategy.
- The Keycloak client configuration and the issuer workaround.
- The review of every generated file against the Clean Code rules.
- The decision-making answers, which are personal and were written from experience.

## 13. Lessons from directing the model

1. Specific constraints produce usable code. "Follow Clean Code" is not actionable; "no blank lines except before return, one class per file, handlers under 25 lines" is.
2. Writing the contract first prevents rework. Most corrections happened in the spec, before the code existed.
3. Generated code must be reviewed for architectural drift, especially around layering and transaction boundaries.
4. Version choices need a licensing check. Two popular libraries changed their license recently, and both were replaced.
5. The model is good at boilerplate and at enumerating options; it is not a substitute for judgment about risk, scope and maintainability.
