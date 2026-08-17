# Rule catalogue

Every diagnostic rule and suppressor across all [Automaticks Analyzers](../README.md) packages,
sorted by rule ID.

Rows marked `Suppressor` don't introduce a new rule - they suppress a rule owned by another
package under specific conditions. Rows marked `CodeFix` ship an automated fix; see the
[code fixes summary](../README.md#code-fixes) and each package README for details.



| ID | Title | Category | Default Severity | Kind | Package |
|---|---|---|---|---|---|
| `ATXCS003` | Async-returning methods must use the Async suffix | CSharp | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS004` | Provider/Factory/Builder/Client/Session types must not expose properties | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS007` | EventHandler and `EventHandler<T>` declarations are not allowed | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS009` | Methods with the 'Async' suffix must return Task, ValueTask, or `IAsyncEnumerable<T>` | CSharp | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS011` | Static methods must only exist in static classes | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS012` | Anonymous tuple types are forbidden | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS013` | The 'internal' access modifier is forbidden | CSharp | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS014` | Redundant null check on non-nullable parameter | CSharp | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS017` | Identifier contains an abbreviated segment | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS020` | Generic built-in delegate types are forbidden | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS021` | Methods must not exceed the maximum line limit | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS022` | Callable construct has too many parameters | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS023` | Method defines more than one out parameter | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS024` | out parameter is not the last parameter | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS025` | ref parameter is forbidden | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS026` | ref parameter is not the first parameter | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS027` | Method defines more than one ref parameter | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS028` | Method cyclomatic complexity exceeds the maximum | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS029` | Direct cast to reference type is forbidden | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS031` | Type name does not match file name | Naming | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS032` | Method nesting depth exceeds the maximum | Maintainability | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS033` | Method cognitive complexity exceeds the maximum | Maintainability | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS034` | Classes must not exceed the maximum lines-of-code limit | Maintainability | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS036` | Inline field or property initialization is forbidden | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS037` | Explicit constructors are required | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS038` | `<remarks>` is not allowed in XML documentation | CSharp | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS039` | Empty lines between adjacent field or constant declarations are forbidden | Style | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS040` | Missing blank line adjacent to a property or indexer declaration | Style | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS041` | Plain comment is not allowed | CSharp | Warning | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS042` | Type member is declared in the wrong section | Style | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS043` | Missing blank line between using directives and namespace declaration | Style | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS044` | Consecutive blank lines are forbidden | Style | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS045` | Auto-implemented property must be declared on a single line | Style | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS046` | Duplicate using directive | Style | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS047` | Using directives must be sorted alphabetically | Style | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS048` | Unused using directive | Style | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS050` | `<summary>` content must start on a new line and be indented with 4 spaces | CSharp | Warning | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS051` | Public member is missing a `<summary>` XML documentation comment | CSharp | Warning | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS052` | Public member parameter is missing a `<param>` XML documentation element | CSharp | Warning | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS053` | Public non-void method is missing a `<returns>` XML documentation element | CSharp | Warning | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS054` | Missing blank line before XML doc comment | Style | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS055` | The params keyword is forbidden | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS057` | Parameter must not have a default value | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS058` | Inline 'new' expression is forbidden | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS059` | Initializer must use one member per line | Style | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS060` | Empty initializer braces are forbidden | Style | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS061` | Interface default implementations are forbidden | CSharp | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS062` | Boolean fields and properties must use an allowed prefix | CSharp | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS063` | Methods returning bool must use an allowed prefix | CSharp | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS064` | Type member violates within-group ordering | Style | Error | Analyzer, CodeFix | `Automaticks.CSharp.Analyzers` |
| `ATXCS065` | Init-only setter is redundant when the property is assigned in the constructor | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS066` | Folders must not exceed the maximum number of source files | Maintainability | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS067` | Namespaces must not exceed the maximum number of source files | Maintainability | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS068` | No-op discard statement is forbidden | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXCS069` | Static methods must only exist in static classes, not records or structs | CSharp | Error | Analyzer | `Automaticks.CSharp.Analyzers` |
| `ATXDC018` | #pragma warning disable is not allowed | Diagnostics.CodeAnalysis | Error | Analyzer, CodeFix | `Automaticks.Diagnostics.CodeAnalysis.Analyzers` |
| `ATXDC019` | // ReSharper disable is not allowed | Diagnostics.CodeAnalysis | Error | Analyzer, CodeFix | `Automaticks.Diagnostics.CodeAnalysis.Analyzers` |
| `ATXDC056` | [SuppressMessage] is not allowed | Diagnostics.CodeAnalysis | Error | Analyzer, CodeFix | `Automaticks.Diagnostics.CodeAnalysis.Analyzers` |
| `ATXEO049` | BindConfiguration is forbidden | Extensions.Options | Error | Analyzer, CodeFix | `Automaticks.Extensions.Options.Analyzers` |
| `ATXLQ002` | Suppresses `ATXLQ002` (LINQ is not allowed) for files that import `Microsoft.EntityFrameworkCore` | Linq | Warning | Suppressor | `Automaticks.EntityFrameworkCore.Analyzers` |
| `ATXLQ002` | LINQ is not allowed | Linq | Warning | Analyzer | `Automaticks.Linq.Analyzers` |
| `ATXLQ003` | LINQ operator calls are not allowed | Linq | Warning | Analyzer | `Automaticks.Linq.Analyzers` |
| `ATXMV001` | Command constructors must use method groups, not lambdas | CommunityToolkit.Mvvm | Error | Analyzer, CodeFix | `Automaticks.CommunityToolkit.Mvvm.Analyzers` |
| `ATXRF030` | Reflection is forbidden | Reflection | Error | Analyzer | `Automaticks.Reflection.Analyzers` |
| `ATXTA008` | Async-returning methods must accept CancellationToken as the last parameter | Threading.Tasks | Error | Analyzer | `Automaticks.Threading.Tasks.Analyzers` |
| `ATXTA010` | Unobserved Task invocation | Threading.Tasks | Error | Analyzer, CodeFix | `Automaticks.Threading.Tasks.Analyzers` |
| `ATXTA011` | CancellationToken parameter is never used | Threading.Tasks | Warning | Analyzer | `Automaticks.Threading.Tasks.Analyzers` |
| `ATXTST001` | Mocking frameworks are not allowed | Testing | Error | Analyzer | `Automaticks.Testing.Analyzers` |
| `ATXTST002` | Test class name must match the class under test | Testing | Warning | Analyzer | `Automaticks.Testing.Analyzers` |
| `ATXTST003` | Test method name must follow the three-part convention | Testing | Warning | Analyzer | `Automaticks.Testing.Analyzers` |
| `ATXTST004` | Task.Delay without TimeProvider is not allowed | Testing | Error | Analyzer | `Automaticks.Testing.Analyzers` |
| `ATXTST006` | Bitmask test must use a single-bit mask | Testing | Warning | Analyzer | `Automaticks.Testing.Analyzers` |
| `ATXTST009` | Debug.Assert condition must not perform side effects | Testing | Error | Analyzer | `Automaticks.Testing.Analyzers` |
| `ATXTST010` | Ambient dependency must be reached through an injectable seam | Testing | Warning | Analyzer | `Automaticks.Testing.Analyzers` |
| `ATXTST012` | Public member must be covered by a test | Testing | Warning | Analyzer | `Automaticks.Testing.Analyzers` |
| `ATXTST013` | File line coverage must meet the configured minimum | Testing | Warning | Analyzer | `Automaticks.Testing.Analyzers` |
| `ATXTST014` | TimeProvider.System is not allowed in tests | Testing | Warning | Analyzer | `Automaticks.Testing.Analyzers` |
