# Automaticks.CommunityToolkit.Mvvm.Analyzers

Roslyn analyzers for `CommunityToolkit.Mvvm` usage conventions.

Part of the [Automaticks Analyzers](https://github.com/Automaticks/Analyzers) family.

## Installation

```shell
dotnet add package Automaticks.CommunityToolkit.Mvvm.Analyzers
```

## Rules

| ID | Title | Category | Default Severity | Kind |
|---|---|---|---|---|
| `ATXMV001` | Command constructors must use method groups, not lambdas | CommunityToolkit.Mvvm | Error | Analyzer, CodeFix |

## Code fixes

Fixes are offered on the IDE light bulb at document, project, and solution scope, and in bulk:

```shell
dotnet format analyzers --diagnostics ATXMV001 --severity error
```

| ID | Fix |
|---|---|
| `ATXMV001` | Extract the lambda into a named method |

The extracted method is named after the assignment target, so `SaveCommand = new RelayCommand(...)`
yields `Save`, falling back to `ExecuteCommand` when no name can be derived.

No fix is offered when the lambda reads a local or parameter from the enclosing scope, because the
extracted method could not reach it — rewrite those by hand.

## License

MIT — see [LICENSE](https://github.com/Automaticks/Analyzers/blob/main/LICENSE).
