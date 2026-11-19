---
applyTo: "src/Inventory.Infrastructure/Persistence/**/*.{cs,sql},docker-compose.yml"
---

# Database instructions

- Target SQL Server 2022.
- Preserve EF Core mappings, migrations and Dapper repository separation.
- Use global uniqueness rules for SKU and category names.
- Preserve restrict-delete relationships and soft-delete semantics.
- Use parameterized SQL for every Dapper command.
- Keep stock update and movement insert in one transaction.
- Preserve `UPDLOCK, ROWLOCK` for concurrent stock updates.
- Add new migrations instead of rewriting applied migrations.
- Never commit production credentials or remove database volumes without explicit approval.
