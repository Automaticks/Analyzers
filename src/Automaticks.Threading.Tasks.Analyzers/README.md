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
| `ATXTA010` | Unobserved Task invocation | Threading.Tasks | Error | Analyzer, CodeFix |
| `ATXTA011` | CancellationToken parameter is never used | Threading.Tasks | Warning | Analyzer |

`ATXTA008` checks only the declared type of the last parameter. `ATXTA011` reads the body, so a
token that is accepted to satisfy the signature and then ignored is still reported. Methods
carrying a test attribute are exempt, because test frameworks inject the token.

## Code fixes

Fixes are offered on the IDE light bulb at document, project, and solution scope, and in bulk:

```shell
dotnet format analyzers --diagnostics ATXTA010 --severity error
```

| ID | Fix |
|---|---|
| `ATXTA010` | Await the returned task |

The `ATXTA010` fix is offered only when the enclosing method or lambda is already `async`;
otherwise resolving it would change the signature and every call site. An existing
`_ = DoAsync();` discard is rewritten into a real `await`, because discarding is itself a
violation rather than a fix.

## License

MIT — see [LICENSE](https://github.com/Automaticks/Analyzers/blob/main/LICENSE).
