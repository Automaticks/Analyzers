# Automaticks.Threading.Tasks.Analyzers

Roslyn analyzers for `System.Threading.Tasks` — async method conventions and task observability.

Part of the [Automaticks Analyzers](https://github.com/Automaticks/Analyzers) family.

## Installation

```shell
dotnet add package Automaticks.Threading.Tasks.Analyzers
```

## Rules

| ID | Title | Category | Default Severity | Kind |
|---|---|---|---|---|
| `ATXTA008` | Async-returning methods must accept CancellationToken as the last parameter | Threading.Tasks | Error | Analyzer |
| `ATXTA010` | Unobserved Task invocation | Threading.Tasks | Error | Analyzer |

## License

MIT — see [LICENSE](https://github.com/Automaticks/Analyzers/blob/main/LICENSE).
