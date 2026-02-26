# General

- Prefer simple solutions
- Ask if unsure
- Keep answers concise
- Break solutions into small incremental steps
- Do one step at a time

# Tech stack

- .NET (netstandard2.0 for library, net6.0 for tests)
- F# (primary language)
- C# (sample project)
- xUnit for testing
- Unquote for test assertions

# Build and test

- Build: `dotnet build`
- Test: `dotnet test`
- Clean: `dotnet clean`

# Structure

This is a .NET F# library for generating deterministic GUIDs (UUID v3/v5):

- `src/unique/` - Main F# library source (`Library.fs`)
- `test/unique.Tests.Unit/` - xUnit test project
- `samples/UniqueIdMaker/` - C# sample project demonstrating usage
