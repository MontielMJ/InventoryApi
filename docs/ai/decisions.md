# Architecture Decisions

Status: decision log for the AI-assisted implementation.

Each entry records the context, the options that were considered, what the AI proposed, the decision that was made and the reason. Entries marked as **modified** or **rejected** are the most useful ones: they show where the output was corrected instead of accepted.

## Summary

| # | Decision | AI proposal | Final decision | Status |
|---|---|---|---|---|
| 1 | Runtime | .NET 8 | .NET 10 LTS | Modified |
| 2 | OAuth2 provider | Duende IdentityServer | Keycloak 26 | Replaced |
| 3 | CQRS dispatcher | MediatR 13 | MediatR 12.4.1 | Modified |
| 4 | Assertion library | FluentAssertions | Shouldly | Replaced |
| 5 | Layering | Repository interfaces in Infrastructure | Interfaces in Application | Modified |
| 6 | Stock model | Editable stock column | Ledger plus denormalized stock | Modified |
| 7 | Concurrency | Optimistic concurrency token | `UPDLOCK, ROWLOCK` | Modified |
| 8 | Deletion | Hard delete | Soft delete | Accepted |
| 9 | Movement write path | EF Core read then Dapper write | Single Dapper transaction | Modified |
| 10 | Error handling | Try/catch in controllers | Global middleware plus `ProblemDetails` | Modified |
| 11 | Category deletion | Cascade delete | Block when active products exist | Modified |
| 12 | Swagger security | Hard-coded URLs | Configuration-driven OAuth2 | Modified |
| 13 | Migrations | Manual `database update` | Applied at startup with retries | Accepted |
| 14 | Tests | Mock everything | Test behavior with a few substitutes | Modified |
| 15 | Docker topology | API and database only | API, database and Keycloak | Accepted |

## 1. Runtime: .NET 10 instead of .NET 8

**Context.** The assignment requires .NET 8 or superior. The local SDK is .NET 10, which is the current LTS.

**Options.** Target .NET 8 for maximum compatibility, or target .NET 10 and use the installed SDK.

**AI proposal.** The model first proposed .NET 8 because it is the version named in the assignment.

**Decision.** .NET 10 LTS.

**Why.** The requirement allows a superior version, the SDK is already installed, and the project benefits from the current LTS support window. Pinning `global.json` to `10.0.101` makes the build reproducible. The only cost is that a reviewer with an older SDK must install .NET 10, which is documented in the README.

## 2. Keycloak instead of Duende IdentityServer

**Context.** The API needs a real OAuth2 provider. Standing one up is the largest piece of infrastructure in the exercise.

**Options.** Duende IdentityServer, Keycloak in Docker, or a minimal custom token issuer.

**AI proposal.** Duende IdentityServer, because it is the most common .NET-native option.

**Decision.** Keycloak 26.

**Why.** Duende's community edition has licensing terms that depend on revenue, which is a poor fit for a public technical exercise. A custom issuer would be insecure and would look like a shortcut. Keycloak is free, runs as a container, and its realm can be versioned as JSON, so the environment is reproducible with no code.

**Trade-off.** The realm export and the host/container URL split add configuration complexity. That complexity is documented and contained in `docker/keycloak/realm-export.json` and the Compose file.

## 3. MediatR 12.4.1 instead of 13

**Context.** CQRS needs an in-process dispatcher with pipeline behaviors.

**Options.** MediatR 13, MediatR 12.4.1, or a small custom dispatcher.

**AI proposal.** MediatR 13, as the latest version.

**Decision.** MediatR 12.4.1.

**Why.** MediatR introduced a commercial license for newer versions. Version 12.4.1 is the last release under the previous terms and covers every feature used here. A custom dispatcher would add code that the exercise does not need. The version is pinned in the project file.

## 4. Shouldly instead of FluentAssertions

**Context.** Unit tests need readable assertions.

**Options.** FluentAssertions, Shouldly, or plain xUnit assertions.

**AI proposal.** FluentAssertions, as the most popular choice.

**Decision.** Shouldly.

**Why.** FluentAssertions changed its license for commercial use. Shouldly is free, has a comparable API for the assertions used here, and avoids a licensing question in a public repository.

## 5. Repository interfaces in Application, not Infrastructure

**Context.** Clean Architecture requires dependencies to point inward.

**Options.** Put repository interfaces next to their implementations in Infrastructure, or in Application.

**AI proposal.** The first draft placed the interfaces in Infrastructure for convenience.

**Decision.** Interfaces live in `Inventory.Application/Abstractions`; implementations live in Infrastructure.

**Why.** If Application referenced Infrastructure, the dependency direction would be inverted and the handlers would be coupled to EF Core and Dapper. Keeping the interfaces in Application lets handlers depend only on the contract and lets tests substitute it without a database.

## 6. Stock as a ledger plus a denormalized value

**Context.** The assignment asks for products and inventory movements. It does not say where stock lives.

**Options.**

1. Compute stock on every read from the movement table.
2. Store stock only on the product and treat movements as a log.
3. Store stock on the product and keep the movement ledger as the source of truth.

**AI proposal.** Option 2, because it is the simplest write path.

**Decision.** Option 3.

**Why.** Computing stock from the ledger on every list request does not scale and complicates pagination and filtering. Storing stock without a ledger loses traceability. Option 3 keeps reads fast and preserves the audit trail. The invariant is that stock only changes through the movement endpoint, inside the same transaction that inserts the ledger row.

**Cost.** Two writes instead of one and a consistency rule that must be respected by every future feature. The rule is enforced by not exposing stock in the product commands.

## 7. `UPDLOCK, ROWLOCK` instead of an optimistic concurrency token

**Context.** Two concurrent `Out` movements could both read the same stock and both succeed, producing negative stock.

**Options.** Optimistic concurrency with a row version, pessimistic locking with `UPDLOCK`, or serializable transactions.

**AI proposal.** An optimistic concurrency token on `Product`.

**Decision.** `SELECT ... WITH (UPDLOCK, ROWLOCK)` inside the movement transaction.

**Why.** The write path uses Dapper, so an EF Core concurrency token would not be enforced unless the check were written by hand. `UPDLOCK` takes an update lock on the product row for the duration of the transaction, which serializes the read-compute-write sequence. `ROWLOCK` keeps the lock granularity at the row. Serializable isolation would lock more than necessary.

**Trade-off.** Writers for the same product are serialized. That is acceptable for an inventory system and is the behavior we want.

## 8. Soft delete

**Context.** Products and categories are referenced by historical movements.

**Options.** Hard delete with cascade, hard delete with restrict, or soft delete.

**AI proposal.** Soft delete.

**Decision.** Accepted.

**Why.** A hard delete would either fail because of the foreign key or destroy audit data. Deactivation preserves history and keeps reads simple with an `IsActive` filter. The trade-off is that unique constraints on SKU and category name are global, so a deactivated SKU cannot be reused. This is explicit and documented.

## 9. One Dapper transaction for movements

**Context.** Registering a movement needs to validate stock, insert the movement and update the product.

**Options.** Read the product with EF Core, then write with Dapper; or do the whole operation with Dapper.

**AI proposal.** Read with EF Core and write with Dapper, to follow the "EF for reads, Dapper for writes" rule literally.

**Decision.** One Dapper transaction for the whole operation.

**Why.** The read is part of the write path, not a query. Splitting it across two connections would break the transaction and reintroduce the race condition. The "EF for reads" rule applies to query handlers that serve HTTP reads, which is where the benefit is real.

## 10. Global exception middleware

**Context.** Domain exceptions must map to HTTP status codes consistently.

**Options.** Try/catch in every controller, an exception filter, or middleware.

**AI proposal.** Try/catch in controllers, because it is explicit.

**Decision.** Middleware that returns `ProblemDetails`.

**Why.** Controllers stay thin, the mapping exists in one place, and every endpoint gets the same error shape. Validation exceptions from the MediatR behavior are handled by the same middleware, so there is no second error path.

## 11. Category deletion is blocked by active products

**Context.** Deleting a category that still has products would leave inconsistent data.

**Options.** Cascade deactivate the products, block the deletion, or reassign products to a default category.

**AI proposal.** Cascade deactivate the products.

**Decision.** Block the deletion with `409 Conflict`.

**Why.** Silently deactivating products as a side effect of deleting a category is surprising and destructive. Blocking forces the caller to make the intent explicit. Reassigning to a default category invents business rules that were not requested.

## 12. Configuration-driven Swagger OAuth2

**Context.** The Clean Code rules require configuration from environment variables.

**Options.** Hard-code the Keycloak URLs in `Program.cs`, or read them from configuration.

**AI proposal.** Hard-code them, because they are fixed for the Docker setup.

**Decision.** Read `Swagger__ClientId`, `Swagger__AuthorizationUrl` and `Swagger__TokenUrl` from configuration.

**Why.** Hard-coded URLs violate the rule and break local development, where the URLs differ from the container network. Configuration keeps one code path for Docker and local runs.

## 13. Migrations at startup

**Context.** The reviewer should be able to run one command.

**Options.** Apply migrations at startup, run them manually, or use a separate migration container.

**AI proposal.** Apply them at startup with retries.

**Decision.** Accepted, with a documented limitation.

**Why.** It makes `docker compose up --build` self-contained and removes a manual step that is easy to miss. The retry loop handles SQL Server startup time. The README states that a multi-instance production deployment should move this to a dedicated job.

## 14. Test behavior, not implementation

**Context.** The assignment asks for unit tests and adequate coverage.

**Options.** Mock every dependency and assert every call, or test the observable behavior of handlers and domain rules.

**AI proposal.** Heavy mocking with call assertions.

**Decision.** A small number of substitutes, focused on outcomes.

**Why.** Call assertions make tests brittle and couple them to refactoring. The tests assert returned values and thrown exceptions. The repository interfaces are substituted because they are the boundary; everything inside the handler runs for real.

## 15. Keycloak in the Compose stack

**Context.** The assignment requires the API and the database in Compose and OAuth2 protection.

**Options.** Assume an external identity provider, or include one in the stack.

**AI proposal.** Include Keycloak.

**Decision.** Accepted.

**Why.** An external dependency would make the one-command promise false and would leave the reviewer without a way to obtain a token. Including Keycloak, with an imported realm and a demo user, keeps the stack self-contained.

## Decisions that were rejected

| Proposal | Reason for rejection |
|---|---|
| Separate read database | Operational cost with no benefit at this scale |
| Event sourcing for stock | Far beyond the scope and harder to review |
| Generic repository over EF Core | Hides the query shape and adds little value |
| AutoMapper | The projections are simple and explicit mapping is clearer |
| FluentAssertions | License change |
| MediatR 13 | License change |
| Duende IdentityServer | License terms and unnecessary surface |
| Hard delete | Destroys audit data |
| Swagger Basic authentication | Does not satisfy OAuth2 |
| In-memory database for tests | Would test EF Core, not the handlers |

## Residual risks

1. The Keycloak split-horizon configuration is correct for the provided Compose file but must be revisited for any other hostname.
2. Startup migrations can race if more than one API instance starts at the same time.
3. Soft-deleted SKUs cannot be reused because the unique index is global.
4. There are no integration tests against SQL Server; the concurrency behavior is verified by reasoning and manual testing, not by an automated test.
5. Authorization stops at authentication. Realm roles exist but are not enforced by policies.
