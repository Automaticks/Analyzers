# Automaticks.CSharp.Analyzers

Roslyn analyzers for C# language rules — naming, syntax, documentation, complexity, and language feature usage.

Part of the [Automaticks Analyzers](https://github.com/Automaticks/Analyzers) family.

## Installation

```shell
dotnet add package Automaticks.CSharp.Analyzers
```

## Rules

| ID | Title | Category | Default Severity | Kind |
|---|---|---|---|---|
| `ATXCS003` | Async-returning methods must use the Async suffix | CSharp | Error | Analyzer |
| `ATXCS004` | Provider/Factory/Builder/Client/Session types must not expose properties | CSharp | Error | Analyzer |
| `ATXCS007` | EventHandler and `EventHandler<T>` declarations are not allowed | CSharp | Error | Analyzer |
| `ATXCS009` | Methods with the 'Async' suffix must return Task, ValueTask, or `IAsyncEnumerable<T>` | CSharp | Error | Analyzer |
| `ATXCS011` | Static methods must only exist in static classes | CSharp | Error | Analyzer |
| `ATXCS012` | Anonymous tuple types are forbidden | CSharp | Error | Analyzer |
| `ATXCS013` | The 'internal' access modifier is forbidden | CSharp | Error | Analyzer, CodeFix |
| `ATXCS014` | Redundant null check on non-nullable parameter | CSharp | Error | Analyzer |
| `ATXCS017` | Identifier contains an abbreviated segment | CSharp | Error | Analyzer |
| `ATXCS020` | Generic built-in delegate types are forbidden | CSharp | Error | Analyzer |
| `ATXCS021` | Methods must not exceed the maximum line limit | CSharp | Error | Analyzer |
| `ATXCS022` | Callable construct has too many parameters | CSharp | Error | Analyzer |
| `ATXCS023` | Method defines more than one out parameter | CSharp | Error | Analyzer |
| `ATXCS024` | out parameter is not the last parameter | CSharp | Error | Analyzer |
| `ATXCS025` | ref parameter is forbidden | CSharp | Error | Analyzer |
| `ATXCS026` | ref parameter is not the first parameter | CSharp | Error | Analyzer |
| `ATXCS027` | Method defines more than one ref parameter | CSharp | Error | Analyzer |
| `ATXCS028` | Method cyclomatic complexity exceeds the maximum | CSharp | Error | Analyzer |
| `ATXCS029` | Direct cast to reference type is forbidden | CSharp | Error | Analyzer |
| `ATXCS031` | Type name does not match file name | Naming | Error | Analyzer |
| `ATXCS032` | Method nesting depth exceeds the maximum | Maintainability | Error | Analyzer |
| `ATXCS033` | Method cognitive complexity exceeds the maximum | Maintainability | Error | Analyzer |
| `ATXCS034` | Classes must not exceed the maximum lines-of-code limit | Maintainability | Error | Analyzer |
| `ATXCS036` | Inline field or property initialization is forbidden | CSharp | Error | Analyzer |
| `ATXCS037` | Explicit constructors are required | CSharp | Error | Analyzer |
| `ATXCS038` | `<remarks>` is not allowed in XML documentation | CSharp | Error | Analyzer, CodeFix |
| `ATXCS039` | Empty lines between adjacent field or constant declarations are forbidden | Style | Error | Analyzer, CodeFix |
| `ATXCS040` | Missing blank line adjacent to a property or indexer declaration | Style | Error | Analyzer |
| `ATXCS041` | Plain comment is not allowed | CSharp | Warning | Analyzer, CodeFix |
| `ATXCS042` | Type member is declared in the wrong section | Style | Error | Analyzer |
| `ATXCS043` | Missing blank line between using directives and namespace declaration | Style | Error | Analyzer, CodeFix |
| `ATXCS044` | Consecutive blank lines are forbidden | Style | Error | Analyzer, CodeFix |
| `ATXCS045` | Auto-implemented property must be declared on a single line | Style | Error | Analyzer |
| `ATXCS046` | Duplicate using directive | Style | Error | Analyzer, CodeFix |
| `ATXCS047` | Using directives must be sorted alphabetically | Style | Error | Analyzer, CodeFix |
| `ATXCS048` | Unused using directive | Style | Error | Analyzer, CodeFix |
| `ATXCS050` | `<summary>` content must start on a new line and be indented with 4 spaces | CSharp | Warning | Analyzer |
| `ATXCS051` | Public member is missing a `<summary>` XML documentation comment | CSharp | Warning | Analyzer |
| `ATXCS052` | Public member parameter is missing a `<param>` XML documentation element | CSharp | Warning | Analyzer |
| `ATXCS053` | Public non-void method is missing a `<returns>` XML documentation element | CSharp | Warning | Analyzer |
| `ATXCS054` | Missing blank line before XML doc comment | Style | Error | Analyzer, CodeFix |
| `ATXCS055` | The params keyword is forbidden | CSharp | Error | Analyzer |
| `ATXCS057` | Parameter must not have a default value | CSharp | Error | Analyzer |
| `ATXCS058` | Inline 'new' expression is forbidden | CSharp | Error | Analyzer |
| `ATXCS059` | Initializer must use one member per line | Style | Error | Analyzer |
| `ATXCS060` | Empty initializer braces are forbidden | Style | Error | Analyzer |
| `ATXCS061` | Interface default implementations are forbidden | CSharp | Error | Analyzer |
| `ATXCS062` | Boolean fields and properties must use an allowed prefix | CSharp | Error | Analyzer |
| `ATXCS063` | Methods returning bool must use an allowed prefix | CSharp | Error | Analyzer |
| `ATXCS064` | Type member violates within-group ordering | Style | Error | Analyzer |
| `ATXCS065` | Init-only setter is redundant when the property is assigned in the constructor | CSharp | Error | Analyzer |
| `ATXCS066` | Folders must not exceed the maximum number of source files | Maintainability | Error | Analyzer |
| `ATXCS067` | Namespaces must not exceed the maximum number of source files | Maintainability | Error | Analyzer |

## Code fixes

Rules marked `CodeFix` above ship an automated fix. Fixes appear on the IDE light bulb and
support the document, project, and solution Fix All scopes. To apply them in bulk:

```shell
dotnet format analyzers --diagnostics ATXCS048 --severity error
```

| ID | Fix |
|---|---|
| `ATXCS013` | Make the declaration public |
| `ATXCS038` | Remove the `<remarks>` element |
| `ATXCS039` | Remove the blank line between the fields |
| `ATXCS041` | Remove the comment |
| `ATXCS043` | Add a blank line before the namespace declaration |
| `ATXCS044` | Remove the extra blank line |
| `ATXCS046` | Remove the duplicate using directive |
| `ATXCS047` | Sort the using directives alphabetically |
| `ATXCS048` | Remove the unused using directive |
| `ATXCS054` | Add a blank line before the XML doc comment |

Two fixes delete content rather than rewrite it, so review them before committing:
`ATXCS038` drops the `<remarks>` element without merging its prose into `<summary>`, and
`ATXCS041` deletes the comment rather than converting it to XML documentation.

Rules not listed here have no fix because resolving them needs a judgement call — choosing a
descriptive name, splitting an oversized type, or designing a replacement type.

## License

MIT — see [LICENSE](https://github.com/Automaticks/Analyzers/blob/main/LICENSE).
