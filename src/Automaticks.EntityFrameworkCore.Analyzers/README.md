# Automaticks.EntityFrameworkCore.Analyzers

Roslyn analyzers and suppressors for Entity Framework Core.

Part of the [Automaticks Analyzers](https://github.com/Automaticks/Analyzers) family.

## Installation

```shell
dotnet add package Automaticks.EntityFrameworkCore.Analyzers
```

## Rules

This package defines no rules of its own — it contains only a suppressor that relaxes a rule owned by another package.

| ID | Title | Category | Default Severity | Kind |
|---|---|---|---|---|
| `ATXLQ002` | Suppresses `ATXLQ002` (LINQ is not allowed, owned by `Automaticks.Linq.Analyzers`) for files that import `Microsoft.EntityFrameworkCore` | Linq | Warning | Suppressor |

## License

MIT — see [LICENSE](https://github.com/Automaticks/Analyzers/blob/main/LICENSE).
