# Automaticks Analyzers

Roslyn analyzers for C#, Entity Framework Core, CommunityToolkit.Mvvm, and more.

## Packages

| Package | Description | NuGet |
|---|---|---|
| `Automaticks.CSharp.Analyzers` | Analyzers for C# language rules — naming, syntax, documentation, complexity, and language feature usage | [![NuGet](https://img.shields.io/nuget/v/Automaticks.CSharp.Analyzers.svg)](https://www.nuget.org/packages/Automaticks.CSharp.Analyzers/) [![Downloads](https://img.shields.io/nuget/dt/Automaticks.CSharp.Analyzers.svg)](https://www.nuget.org/packages/Automaticks.CSharp.Analyzers/) |
| `Automaticks.Threading.Tasks.Analyzers` | Analyzers targeting `System.Threading.Tasks` — async method conventions and task observability | [![NuGet](https://img.shields.io/nuget/v/Automaticks.Threading.Tasks.Analyzers.svg)](https://www.nuget.org/packages/Automaticks.Threading.Tasks.Analyzers/) [![Downloads](https://img.shields.io/nuget/dt/Automaticks.Threading.Tasks.Analyzers.svg)](https://www.nuget.org/packages/Automaticks.Threading.Tasks.Analyzers/) |
| `Automaticks.Linq.Analyzers` | Analyzers targeting `System.Linq` usage | [![NuGet](https://img.shields.io/nuget/v/Automaticks.Linq.Analyzers.svg)](https://www.nuget.org/packages/Automaticks.Linq.Analyzers/) [![Downloads](https://img.shields.io/nuget/dt/Automaticks.Linq.Analyzers.svg)](https://www.nuget.org/packages/Automaticks.Linq.Analyzers/) |
| `Automaticks.Reflection.Analyzers` | Analyzers targeting `System.Reflection` usage | [![NuGet](https://img.shields.io/nuget/v/Automaticks.Reflection.Analyzers.svg)](https://www.nuget.org/packages/Automaticks.Reflection.Analyzers/) [![Downloads](https://img.shields.io/nuget/dt/Automaticks.Reflection.Analyzers.svg)](https://www.nuget.org/packages/Automaticks.Reflection.Analyzers/) |
| `Automaticks.Extensions.Options.Analyzers` | Analyzers targeting `Microsoft.Extensions.Options` usage | [![NuGet](https://img.shields.io/nuget/v/Automaticks.Extensions.Options.Analyzers.svg)](https://www.nuget.org/packages/Automaticks.Extensions.Options.Analyzers/) [![Downloads](https://img.shields.io/nuget/dt/Automaticks.Extensions.Options.Analyzers.svg)](https://www.nuget.org/packages/Automaticks.Extensions.Options.Analyzers/) |
| `Automaticks.EntityFrameworkCore.Analyzers` | Analyzers and suppressors for Entity Framework Core | [![NuGet](https://img.shields.io/nuget/v/Automaticks.EntityFrameworkCore.Analyzers.svg)](https://www.nuget.org/packages/Automaticks.EntityFrameworkCore.Analyzers/) [![Downloads](https://img.shields.io/nuget/dt/Automaticks.EntityFrameworkCore.Analyzers.svg)](https://www.nuget.org/packages/Automaticks.EntityFrameworkCore.Analyzers/) |
| `Automaticks.CommunityToolkit.Mvvm.Analyzers` | Analyzers for CommunityToolkit.Mvvm | [![NuGet](https://img.shields.io/nuget/v/Automaticks.CommunityToolkit.Mvvm.Analyzers.svg)](https://www.nuget.org/packages/Automaticks.CommunityToolkit.Mvvm.Analyzers/) [![Downloads](https://img.shields.io/nuget/dt/Automaticks.CommunityToolkit.Mvvm.Analyzers.svg)](https://www.nuget.org/packages/Automaticks.CommunityToolkit.Mvvm.Analyzers/) |
| `Automaticks.Diagnostics.CodeAnalysis.Analyzers` | Analyzers targeting `System.Diagnostics.CodeAnalysis` — diagnostic suppression rules | [![NuGet](https://img.shields.io/nuget/v/Automaticks.Diagnostics.CodeAnalysis.Analyzers.svg)](https://www.nuget.org/packages/Automaticks.Diagnostics.CodeAnalysis.Analyzers/) [![Downloads](https://img.shields.io/nuget/dt/Automaticks.Diagnostics.CodeAnalysis.Analyzers.svg)](https://www.nuget.org/packages/Automaticks.Diagnostics.CodeAnalysis.Analyzers/) |
| `Automaticks.Testing.Analyzers` | Analyzers for test projects — naming conventions, forbidden APIs, and test structure | [![NuGet](https://img.shields.io/nuget/v/Automaticks.Testing.Analyzers.svg)](https://www.nuget.org/packages/Automaticks.Testing.Analyzers/) [![Downloads](https://img.shields.io/nuget/dt/Automaticks.Testing.Analyzers.svg)](https://www.nuget.org/packages/Automaticks.Testing.Analyzers/) |

## Rules

All 67 diagnostic rules and suppressors are catalogued in [docs/RULES.md](docs/RULES.md).

## Code fixes

33 of the catalogued rules ship an automated fix. Fixes appear on the IDE light bulb and support the
document, project, and solution Fix All scopes. To apply them in bulk:

```shell
dotnet format analyzers --diagnostics ATXCS048 --severity error
```

| Package | Rules with a fix |
|---|---|
| `Automaticks.CSharp.Analyzers` | 27 |
| `Automaticks.Diagnostics.CodeAnalysis.Analyzers` | 3 — every rule in the package |
| `Automaticks.CommunityToolkit.Mvvm.Analyzers` | 1 — every rule in the package |
| `Automaticks.Extensions.Options.Analyzers` | 1 — every rule in the package |
| `Automaticks.Threading.Tasks.Analyzers` | 1 of 2 |

Each package README lists its fixes and notes where a fix deliberately declines to act. A rule ships
no fix when resolving it needs a judgement a tool should not make — naming, decomposition, designing
a replacement type — or when the change would alter behaviour or a public signature rather than
satisfy the rule mechanically.

## Building

```shell
dotnet build -c Release
```

## Testing

```shell
dotnet test -c Release --no-build
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for information on contributing to this project.

This project has adopted the code of conduct defined by the [Contributor Covenant](https://www.contributor-covenant.org/)
to clarify expected behavior in our community.

## License

This project is licensed under the [MIT license](LICENSE).
