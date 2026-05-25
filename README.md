# Automaticks Analyzers

Roslyn analyzers for C#, Entity Framework Core, CommunityToolkit.Mvvm, and more.

## Packages

| Package | Description |
|---|---|
| `Automaticks.CSharp.Analyzers` | Analyzers for C# language rules — naming, syntax, documentation, complexity, and language feature usage |
| `Automaticks.Threading.Tasks.Analyzers` | Analyzers targeting `System.Threading.Tasks` — async method conventions and task observability |
| `Automaticks.Linq.Analyzers` | Analyzers targeting `System.Linq` usage |
| `Automaticks.Reflection.Analyzers` | Analyzers targeting `System.Reflection` usage |
| `Automaticks.Extensions.Options.Analyzers` | Analyzers targeting `Microsoft.Extensions.Options` usage |
| `Automaticks.EntityFrameworkCore.Analyzers` | Analyzers and suppressors for Entity Framework Core |
| `Automaticks.CommunityToolkit.Mvvm.Analyzers` | Analyzers for CommunityToolkit.Mvvm |
| `Automaticks.Diagnostics.CodeAnalysis.Analyzers` | Analyzers targeting `System.Diagnostics.CodeAnalysis` — diagnostic suppression rules |
| `Automaticks.Testing.Analyzers` | Analyzers for test projects — naming conventions, forbidden APIs, and test structure |

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
