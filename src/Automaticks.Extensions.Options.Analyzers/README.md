# Automaticks.Extensions.Options.Analyzers

Roslyn analyzers for `Microsoft.Extensions.Options` usage.

Part of the [Automaticks Analyzers](https://github.com/Automaticks/Analyzers) family.

## Installation

```shell
dotnet add package Automaticks.Extensions.Options.Analyzers
```

## Rules

| ID | Title | Category | Default Severity | Kind |
|---|---|---|---|---|
| `ATXEO049` | BindConfiguration is forbidden | Extensions.Options | Error | Analyzer, CodeFix |

## Code fixes

Fixes are offered on the IDE light bulb at document, project, and solution scope, and in bulk:

```shell
dotnet format analyzers --diagnostics ATXEO049 --severity error
```

| ID | Fix |
|---|---|
| `ATXEO049` | Use `Configure` with `GetRequiredSection` |

The fix rewrites `services.AddOptions<T>().BindConfiguration("X")` into
`services.Configure<T>(configuration.GetRequiredSection("X"))`.

It is offered only when the call has that chained shape and an `IConfiguration` is in scope, since
there is otherwise nothing to pass to `GetRequiredSection`. A bare `OptionsBuilder` receiver still
reports the rule but must be rewritten by hand.

## License

MIT — see [LICENSE](https://github.com/Automaticks/Analyzers/blob/main/LICENSE).
