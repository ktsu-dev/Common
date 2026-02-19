---
name: add-provider
description: Scaffold a new provider implementation following the ktsu.Common pattern
---

# Add Provider Skill

Scaffold a new provider implementation in the ktsu.Common project. This handles all 7 steps needed to add a provider correctly.

## Arguments

The user should provide:
- **category**: One of `Hash`, `Compression`, `Encryption`, `Serialization`, `FileSystem`, `Obfuscation`
- **name**: The provider name (e.g., `SHA3`, `LZ4`, `ChaCha20`)

If arguments are not provided, ask the user for them.

## Steps

### 1. Create the provider directory

Create the directory at `<Category>Providers/<Name>/`.

### 2. Create the .csproj file

Create `<Category>Providers/<Name>/<Name>.csproj` based on this template:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <Sdk Name="ktsu.Sdk" />

  <PropertyGroup>
    <TargetFrameworks>net10.0;net9.0;net8.0;net7.0;net6.0;netstandard2.1</TargetFrameworks>
    <SuppressTfmSupportBuildWarnings>true</SuppressTfmSupportBuildWarnings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="ktsu.Abstractions" />
    <PackageReference Include="Microsoft.SourceLink.GitHub" PrivateAssets="All" />
    <PackageReference Include="Microsoft.SourceLink.AzureRepos.Git" PrivateAssets="All" />
    <PackageReference Include="Polyfill" PrivateAssets="All" />
  </ItemGroup>

  <ItemGroup>
    <InternalsVisibleTo Include="ktsu.Common.Tests" />
  </ItemGroup>

</Project>
```

Add any additional `<PackageReference>` entries needed for the specific implementation (e.g., `System.IO.Hashing` for hash providers that use it).

### 3. Create the implementation class

Create `<Category>Providers/<Name>/<Name>.cs` implementing the correct interface:

| Category | Namespace | Interface |
|----------|-----------|-----------|
| Hash | `ktsu.HashProviders` | `IHashProvider` |
| Compression | `ktsu.CompressionProviders` | `ICompressionProvider` |
| Encryption | `ktsu.EncryptionProviders` | `IEncryptionProvider` |
| Serialization | `ktsu.SerializationProviders` | `ISerializationProvider` |
| FileSystem | `ktsu.FileSystemProviders` | `IFileSystemProvider` |
| Obfuscation | `ktsu.ObfuscationProviders` | `IObfuscationProvider` |

Follow the existing code style:
- Copyright header: `// Copyright (c) ktsu.dev` / `// All rights reserved.` / `// Licensed under the MIT license.`
- File-scoped namespace
- XML documentation on the class and all public members
- Error handling pattern: return `false` or default values rather than throwing exceptions

### 4. Add the project to Common.sln

Use `dotnet sln Common.sln add <Category>Providers/<Name>/<Name>.csproj --solution-folder <Category>Providers` to add the project under the correct solution folder.

### 5. Add ProjectReference to Common.Tests

Add a `<ProjectReference>` to `Common.Tests/Common.Tests.csproj`:
```xml
<ProjectReference Include="..\<Category>Providers\<Name>\<Name>.csproj" />
```

### 6. Register in DI

Update `Common.Tests/ServiceCollectionExtensions.cs`:
- Add a `using` directive for the provider namespace if not already present
- Add `services.AddSingleton<I<Category>Provider, <Name>>();` to the appropriate `Add<Category>Providers` method

### 7. Build and test

Run `dotnet build` to verify compilation, then `dotnet test` to verify the provider passes all dynamic data-driven tests.

## Important Notes

- If the provider needs additional NuGet packages, add the version to `Directory.Packages.props` first
- Look at existing providers in the same category for implementation reference
- Hash providers must correctly implement both `TryHash(ReadOnlySpan<byte>, Span<byte>)` and `TryHash(Stream, Span<byte>)` overloads
- Compression/Encryption providers that use streams should properly dispose resources
