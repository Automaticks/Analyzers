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
| `ATXCS003` | Async-returning methods must use the Async suffix | CSharp | Error | Analyzer, CodeFix |
| `ATXCS004` | Provider/Factory/Builder/Client/Session types must not expose properties | CSharp | Error | Analyzer |
| `ATXCS007` | EventHandler and `EventHandler<T>` declarations are not allowed | CSharp | Error | Analyzer |
| `ATXCS009` | Methods with the 'Async' suffix must return Task, ValueTask, or `IAsyncEnumerable<T>` | CSharp | Error | Analyzer, CodeFix |
| `ATXCS011` | Static methods must only exist in static classes | CSharp | Error | Analyzer |
| `ATXCS012` | Anonymous tuple types are forbidden | CSharp | Error | Analyzer |
| `ATXCS013` | The 'internal' access modifier is forbidden | CSharp | Error | Analyzer, CodeFix |
| `ATXCS014` | Redundant null check on non-nullable parameter | CSharp | Error | Analyzer, CodeFix |
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
| `ATXCS031` | Type name does not match file name | Naming | Error | Analyzer, CodeFix |
| `ATXCS032` | Method nesting depth exceeds the maximum | Maintainability | Error | Analyzer |
| `ATXCS033` | Method cognitive complexity exceeds the maximum | Maintainability | Error | Analyzer |
| `ATXCS034` | Classes must not exceed the maximum lines-of-code limit | Maintainability | Error | Analyzer |
| `ATXCS036` | Inline field or property initialization is forbidden | CSharp | Error | Analyzer |
| `ATXCS037` | Explicit constructors are required | CSharp | Error | Analyzer |
| `ATXCS038` | `<remarks>` is not allowed in XML documentation | CSharp | Error | Analyzer, CodeFix |
| `ATXCS039` | Empty lines between adjacent field or constant declarations are forbidden | Style | Error | Analyzer, CodeFix |
| `ATXCS040` | Missing blank line adjacent to a property or indexer declaration | Style | Error | Analyzer, CodeFix |
| `ATXCS041` | Plain comment is not allowed | CSharp | Warning | Analyzer, CodeFix |
| `ATXCS042` | Type member is declared in the wrong section | Style | Error | Analyzer, CodeFix |
| `ATXCS043` | Missing blank line between using directives and namespace declaration | Style | Error | Analyzer, CodeFix |
| `ATXCS044` | Consecutive blank lines are forbidden | Style | Error | Analyzer, CodeFix |
| `ATXCS045` | Auto-implemented property must be declared on a single line | Style | Error | Analyzer, CodeFix |
| `ATXCS046` | Duplicate using directive | Style | Error | Analyzer, CodeFix |
| `ATXCS047` | Using directives must be sorted alphabetically | Style | Error | Analyzer, CodeFix |
| `ATXCS048` | Unused using directive | Style | Error | Analyzer, CodeFix |
| `ATXCS050` | `<summary>` content must start on a new line and be indented with 4 spaces | CSharp | Warning | Analyzer, CodeFix |
| `ATXCS051` | Public member is missing a `<summary>` XML documentation comment | CSharp | Warning | Analyzer, CodeFix |
| `ATXCS052` | Public member parameter is missing a `<param>` XML documentation element | CSharp | Warning | Analyzer, CodeFix |
| `ATXCS053` | Public non-void method is missing a `<returns>` XML documentation element | CSharp | Warning | Analyzer, CodeFix |
| `ATXCS054` | Missing blank line before XML doc comment | Style | Error | Analyzer, CodeFix |
| `ATXCS055` | The params keyword is forbidden | CSharp | Error | Analyzer |
| `ATXCS057` | Parameter must not have a default value | CSharp | Error | Analyzer |
| `ATXCS058` | Inline 'new' expression is forbidden | CSharp | Error | Analyzer |
| `ATXCS059` | Initializer must use one member per line | Style | Error | Analyzer, CodeFix |
| `ATXCS060` | Empty initializer braces are forbidden | Style | Error | Analyzer, CodeFix |
| `ATXCS061` | Interface default implementations are forbidden | CSharp | Error | Analyzer, CodeFix |
| `ATXCS062` | Boolean fields and properties must use an allowed prefix | CSharp | Error | Analyzer, CodeFix |
| `ATXCS063` | Methods returning bool must use an allowed prefix | CSharp | Error | Analyzer, CodeFix |
| `ATXCS064` | Type member violates within-group ordering | Style | Error | Analyzer, CodeFix |
| `ATXCS065` | Init-only setter is redundant when the property is assigned in the constructor | CSharp | Error | Analyzer |
| `ATXCS066` | Folders must not exceed the maximum number of source files | Maintainability | Error | Analyzer |
| `ATXCS067` | Namespaces must not exceed the maximum number of source files | Maintainability | Error | Analyzer |
| `ATXCS068` | No-op discard statement is forbidden | CSharp | Error | Analyzer, CodeFix |
| `ATXCS069` | Static methods must only exist in static classes, not records or structs | CSharp | Error | Analyzer |
| `ATXCS070` | Mutable static state must not exist in a non-static class | CSharp | Error | Analyzer |
| `ATXCS071` | XML documentation element is too long | Documentation | Error | Analyzer |
| `ATXCS072` | Namespace-qualified type reference is forbidden | CSharp | Error | Analyzer, CodeFix |
| `ATXCS073` | 'global::' alias qualifier is forbidden | CSharp | Error | Analyzer |
| `ATXCS074` | Alias directive is forbidden | CSharp | Error | Analyzer |
| `ATXCS075` | Expression-bodied methods are forbidden | CSharp | Error | Analyzer, CodeFix |
| `ATXCS076` | Expression-bodied local functions are forbidden | CSharp | Error | Analyzer, CodeFix |
| `ATXCS077` | Expression-bodied properties are forbidden | CSharp | Error | Analyzer, CodeFix |
| `ATXCS078` | Expression-bodied indexers are forbidden | CSharp | Error | Analyzer, CodeFix |
| `ATXCS079` | Expression-bodied operators are forbidden | CSharp | Error | Analyzer, CodeFix |
| `ATXCS080` | Expression-bodied conversion operators are forbidden | CSharp | Error | Analyzer, CodeFix |
| `ATXCS081` | Expression-bodied constructors are forbidden | CSharp | Error | Analyzer, CodeFix |
| `ATXCS082` | Expression-bodied destructors are forbidden | CSharp | Error | Analyzer, CodeFix |
| `ATXCS083` | Expression-bodied accessors are forbidden | CSharp | Error | Analyzer, CodeFix |

## Code fixes

Rules marked `CodeFix` above ship an automated fix. Fixes appear on the IDE light bulb and
support the document, project, and solution Fix All scopes. To apply them in bulk:

```shell
dotnet format analyzers --diagnostics ATXCS048 --severity error
```

| ID | Fix |
|---|---|
| `ATXCS003` | Append the Async suffix (renames references) |
| `ATXCS009` | Remove the Async suffix (renames references) |
| `ATXCS013` | Make the declaration public |
| `ATXCS014` | Remove the redundant null check |
| `ATXCS031` | Rename the file to match the type |
| `ATXCS038` | Remove the `<remarks>` element |
| `ATXCS039` | Remove the blank line between the fields |
| `ATXCS040` | Add a blank line between the members |
| `ATXCS041` | Remove the comment |
| `ATXCS042` | Sort the type members into canonical order |
| `ATXCS043` | Add a blank line before the namespace declaration |
| `ATXCS044` | Remove the extra blank line |
| `ATXCS045` | Put the property on a single line |
| `ATXCS046` | Remove the duplicate using directive |
| `ATXCS047` | Sort the using directives alphabetically |
| `ATXCS048` | Remove the unused using directive |
| `ATXCS050` | Reformat the `<summary>` block |
| `ATXCS051` | Add an empty `<summary>` documentation block |
| `ATXCS052` | Add an empty `<param>` element |
| `ATXCS053` | Add an empty `<returns>` element |
| `ATXCS054` | Add a blank line before the XML doc comment |
| `ATXCS059` | Put each initializer member on its own line |
| `ATXCS060` | Remove the empty initializer braces |
| `ATXCS061` | Remove the default implementation |
| `ATXCS062` | Prefix the member name with `is` |
| `ATXCS063` | Prefix the method name with `can` |
| `ATXCS064` | Sort the type members into canonical order |
| `ATXCS068` | Remove the no-op discard statement |
| `ATXCS072` | Simplify to the type name, adding a `using` if needed |
| `ATXCS075` | Convert the expression body to a block body |
| `ATXCS076` | Convert the expression body to a block body |
| `ATXCS077` | Convert the expression body to a block body |
| `ATXCS078` | Convert the expression body to a block body |
| `ATXCS079` | Convert the expression body to a block body |
| `ATXCS080` | Convert the expression body to a block body |
| `ATXCS081` | Convert the expression body to a block body |
| `ATXCS082` | Convert the expression body to a block body |
| `ATXCS083` | Convert the expression body to a block body |

Review these before committing, because they remove or invent content rather than rewrite it:
`ATXCS038` drops `<remarks>` without merging its prose into `<summary>`, `ATXCS041` deletes the
comment rather than converting it, and `ATXCS051`, `ATXCS052` and `ATXCS053` insert **empty**
elements — they satisfy the rule but still need you to write the documentation.

`ATXCS062` and `ATXCS063` each accept two prefixes; the fix offers one (`is` and `can`) and you
may want the other.

Some fixes are deliberately narrower than the rule:

| ID | Not fixed |
|---|---|
| `ATXCS060` | Array creation such as `new int[] { }`, which has no valid brace-free form |
| `ATXCS061` | Fields and static members, which cannot become a contract without being deleted |
| `ATXCS072` | A collision needing a rename, which a fix can't invent |

Rules with no fix need a judgement a tool should not make: choosing a descriptive name
(`ATXCS017`), splitting an oversized type (`ATXCS021`, `ATXCS034`), designing a replacement type
(`ATXCS012`, `ATXCS020`), picking an alias-free name (`ATXCS073`, `ATXCS074`), or changing a
signature and every call site (`ATXCS024`, `ATXCS026`, `ATXCS055`, `ATXCS057`). `ATXCS029` is
excluded because `as` returns `null` where a cast throws, and `ATXCS058` because extracting a
`new` expression reorders argument evaluation.

`ATXCS042` and `ATXCS064` describe the same ordering, so one fix settles both. It rewrites the
whole member list rather than nudging the reported member, and documentation moves with its
member. Blank-line spacing between the moved members is left to `ATXCS039`, `ATXCS040`
and `ATXCS044`.

## License

MIT — see [LICENSE](https://github.com/Automaticks/Analyzers/blob/main/LICENSE).
