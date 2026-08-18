# Automaticks.Linq.Analyzers

Roslyn analyzers for `System.Linq` usage.

Part of the [Automaticks Analyzers](https://github.com/Automaticks/Analyzers) family.

## Installation

```shell
dotnet add package Automaticks.Linq.Analyzers
```

## Rules

| ID | Title | Category | Default Severity | Kind |
|---|---|---|---|---|
| `ATXLQ002` | LINQ is not allowed | Linq | Warning | Analyzer |
| `ATXLQ003` | LINQ operator calls are not allowed | Linq | Warning | Analyzer |

`ATXLQ002` inspects using directives. `ATXLQ003` resolves the called symbol instead, so it still
fires when `ImplicitUsings` puts `System.Linq` in scope through a generated global using and no
`using System.Linq;` directive appears in the file.

## License

MIT — see [LICENSE](https://github.com/Automaticks/Analyzers/blob/main/LICENSE).
