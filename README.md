# unique

[![Tests](https://github.com/dburriss/unique/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/dburriss/unique/actions/workflows/build-and-test.yml)
[![Nuget](https://img.shields.io/nuget/v/unique)](https://www.nuget.org/packages/unique/)

Unique allows for the creation of a **"Deterministic Guid"** which will be unique in a SpaceId over time with a high probability. The same `Guid` will be generated for a given **Namespace ID** and **name**.
 
## Usage

In C# `using Unique.CSharp` you can use `NamedGuid.NewGuid` to generate a new Guid. You can check the version of the generated `Guid` by passing a `Guid` to `Version`.

```csharp
// C# example
using Unique.CSharp;
//...
// Use the pre-defined DNS namespace
Guid uniqueIdForDns = NamedGuid.NewGuid(SpaceId.DNS, "example.com");
Console.WriteLine($"DNS example.com ID is {uniqueIdForDns}");
// OUTPUT: DNS example.com ID is cfbff0d1-9375-5685-968c-48ce8b15ae17

// Use a custom namespace Guid
Guid customGuid = Guid.Parse("AA0F4712-691F-4C72-B5EC-19730324EAFD");
Guid uniqueIdForCustomSpace = NamedGuid.NewGuid(customGuid, "bob@builder.com");
Console.WriteLine($"Custom bob@builder.com ID is {uniqueIdForCustomSpace}");
// OUTPUT: Custom bob@builder.com ID is dbead1ff-3f86-5d73-b577-cb00ee3fccaf

// Get the version of any Guid
int version = NamedGuid.Version(uniqueIdForCustomSpace);
Console.WriteLine($"Version is {version}");
// OUTPUT: Version is 5
// Since the default hash algorithm is SHA-1 the version is always 5
// Use Unique.NamedGuid.newGuid to define the hash algorithm.

version = NamedGuid.Version(Guid.NewGuid());
Console.WriteLine($"Version is {version}");
// OUTPUT: Version is 4
```

If you need to generate using a `byte[]` or change the algorithm, use the functions found on `Unique.NamedGuid`, `Unique.NS` and `Unigue.Algorithm`.

```fsharp
// F# example
let guid = NamedGuid.newGuid Algorithm.MD5 NS.DNS "www.example.com"
// val: 5df41881-3aed-3515-88a7-2f4a814cf09e
```