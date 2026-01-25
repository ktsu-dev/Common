# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is the **Common** project within the ktsu.dev monorepo. It contains **provider implementations** for various abstractions defined in the `ktsu.Abstractions` package. All implementations follow the provider pattern with standardized interfaces.

**Provider Categories:**
- **SerializationProviders** - NewtonsoftJson, SystemTextJson
- **HashProviders** - MD5, SHA1, SHA256, SHA384, SHA512, FNV1_32, FNV1a_32, FNV1_64, FNV1a_64
- **CompressionProviders** - Gzip
- **EncryptionProviders** - Aes
- **FileSystemProviders** - Native
- **ObfuscationProviders** - Base64

**Key Characteristics:**
- Multi-targeted libraries supporting .NET 9.0, 8.0, 7.0, 6.0, and netstandard2.1
- Custom MSBuild SDK (`ktsu.Sdk`) for standardized build configuration
- Centralized dependency management via `Directory.Packages.props`
- Comprehensive test suite using MSTest with dynamic data-driven tests
- All providers implement interfaces from `ktsu.Abstractions`

## Build and Test Commands

### Building

```bash
# Build entire solution
dotnet build

# Build specific configuration
dotnet build --configuration Release

# Clean build (recommended after SDK changes)
dotnet build --no-incremental
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test by filter
dotnet test --filter "FullyQualifiedName~HashProviderTests.HashProviders_Produce_Correct_Length"

# Run tests for specific provider type
dotnet test --filter "FullyQualifiedName~HashProviderTests"
```

### Creating Packages

```bash
# Pack all projects
dotnet pack --configuration Release --output ./staging
```

## Architecture and Design Patterns

### Provider Pattern Implementation

All implementations follow a consistent provider pattern:

1. **Interface Definition** - Defined in `ktsu.Abstractions` (e.g., `IHashProvider`, `ISerializationProvider`)
2. **Concrete Implementation** - Each provider implements the interface
3. **Dependency Injection Ready** - All providers can be registered with DI containers
4. **Graceful Error Handling** - Methods return false or default values rather than throwing exceptions

Example provider structure:
```csharp
public class SHA256 : IHashProvider, IDisposable
{
    public int HashLengthBytes => 32;
    public bool TryHash(ReadOnlySpan<byte> data, Span<byte> destination) { ... }
    public bool TryHash(Stream data, Span<byte> destination) { ... }
}
```

### Multi-Targeting Configuration

Library projects (providers) target multiple frameworks:
```xml
<TargetFrameworks>net9.0;net8.0;net7.0;net6.0;netstandard2.1</TargetFrameworks>
```

Test projects target only the latest framework:
```xml
<TargetFramework>net9.0</TargetFramework>
```

### Custom SDK Integration

All projects reference `ktsu.Sdk` which provides:
- Common MSBuild properties and targets
- Metadata file integration (README.md, DESCRIPTION.md, etc.)
- Multi-targeting configuration
- NuGet package configuration

Project files are minimal:
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <Sdk Name="ktsu.Sdk" />
  <PropertyGroup>
    <TargetFrameworks>net9.0;net8.0;net7.0;net6.0;netstandard2.1</TargetFrameworks>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="ktsu.Abstractions" />
  </ItemGroup>
</Project>
```

## Testing Architecture

### Dynamic Data-Driven Testing

The test suite uses MSTest's `[DynamicData]` attribute to test all providers of a given type automatically:

```csharp
public static IEnumerable<object[]> HashProviders =>
    BuildProvider().EnumerateProviders<IHashProvider>();

[TestMethod]
[DynamicData(nameof(HashProviders))]
public void HashProviders_Produce_Correct_Length(IHashProvider provider, string providerName)
{
    // Test runs once for each registered hash provider
}
```

This ensures:
- All providers are tested with the same test suite
- Adding new providers automatically includes them in tests
- Tests verify interface contract compliance

### Dependency Injection in Tests

Tests use Microsoft.Extensions.DependencyInjection to register and enumerate providers:

```csharp
private static ServiceProvider BuildProvider()
{
    ServiceCollection services = new();
    services.AddHashProviders(); // Extension method registers all hash providers
    return services.BuildServiceProvider();
}
```

### Known Vector Testing

Each provider has specific tests verifying correct implementation against known test vectors:
- Hash providers verify against official test vectors (e.g., SHA256 of "abc")
- These tests ensure cryptographic correctness

## Project Structure

```
Common/
├── SerializationProviders/
│   ├── NewtonsoftJson/
│   │   ├── NewtonsoftJson.csproj
│   │   └── NewtonsoftJson.cs
│   └── SystemTextJson/
│       ├── SystemTextJson.csproj
│       └── SystemTextJson.cs
├── HashProviders/
│   ├── MD5/
│   ├── SHA1/
│   ├── SHA256/
│   ├── SHA384/
│   ├── SHA512/
│   ├── FNV1_32/
│   ├── FNV1a_32/
│   ├── FNV1_64/
│   └── FNV1a_64/
├── CompressionProviders/
│   └── Gzip/
├── EncryptionProviders/
│   └── Aes/
├── FileSystemProviders/
│   └── Native/
├── ObfuscationProviders/
│   └── Base64/
├── Common.Tests/
│   ├── DiTests.cs
│   ├── HashProviderTests.cs
│   ├── FileSystemProviderTests.cs
│   ├── RoundTripTests.cs
│   └── ServiceCollectionExtensions.cs
├── Common.sln
├── Directory.Packages.props
└── global.json
```

## Dependency Management

Dependencies are managed centrally via `Directory.Packages.props` using Central Package Management (CPM):
- Package versions are defined once in `Directory.Packages.props`
- Projects reference packages without specifying versions
- Example: `<PackageReference Include="ktsu.Abstractions" />` (no Version attribute)

To add a new dependency:
1. Add version to `Directory.Packages.props`: `<PackageVersion Include="PackageName" Version="X.Y.Z" />`
2. Reference in project: `<PackageReference Include="PackageName" />`

## Adding a New Provider

To add a new provider implementation:

1. **Create provider directory** under the appropriate category (e.g., `HashProviders/SHA3/`)
2. **Create .csproj file** using `ktsu.Sdk`:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <Sdk Name="ktsu.Sdk" />
     <PropertyGroup>
       <TargetFrameworks>net9.0;net8.0;net7.0;net6.0;netstandard2.1</TargetFrameworks>
     </PropertyGroup>
     <ItemGroup>
       <PackageReference Include="ktsu.Abstractions" />
     </ItemGroup>
   </Project>
   ```
3. **Implement the interface** from `ktsu.Abstractions` (e.g., `IHashProvider`)
4. **Add to solution**: Edit `Common.sln` to include the new project
5. **Add to tests**: Add `<ProjectReference>` in `Common.Tests/Common.Tests.csproj`
6. **Register in DI**: Update `ServiceCollectionExtensions.cs` to register the provider
7. **Run tests** to verify the implementation using existing dynamic tests

## CI/CD Pipeline

The project uses GitHub Actions with a custom PowerShell build module (`PSBuild.psm1`):

**Workflow triggers:**
- Push to `main` or `develop` branches
- Pull requests
- Daily scheduled builds
- Manual workflow dispatch

**Pipeline stages:**
1. Build all projects
2. Run tests with code coverage
3. SonarQube analysis (if configured)
4. Pack NuGet packages
5. Publish to NuGet and GitHub Packages (on main branch)
6. Create GitHub release
7. Update Winget manifests

**Key features:**
- Full git history fetch for versioning
- Multi-framework builds and tests
- Code coverage collection
- Automated dependency scanning

## Error Handling Conventions

All providers follow consistent error handling patterns:

**Hash Providers:**
- `TryHash()` returns `false` on error (insufficient buffer, disposed object, etc.)
- Never throws exceptions in normal operation

**Serialization Providers:**
- `TrySerialize()` returns `false` on serialization failure
- `Deserialize<T>()` returns `default(T)` on deserialization failure
- Gracefully handle null inputs and invalid JSON

**Example:**
```csharp
// Provider implementation
public bool TryHash(ReadOnlySpan<byte> data, Span<byte> destination)
{
    if (destination.Length < HashLengthBytes)
        return false;

    try
    {
        return _sha256.Value.TryComputeHash(data, destination, out int bytesWritten)
            && bytesWritten == HashLengthBytes;
    }
    catch (ArgumentException) { return false; }
    catch (ObjectDisposedException) { return false; }
}
```

## SDK Version

The project uses .NET SDK version specified in `global.json`:
```json
{
  "sdk": {
    "version": "9.0.301",
    "rollForward": "latestFeature"
  }
}
```

The custom `ktsu.Sdk` versions are also pinned in `global.json` under `msbuild-sdks`.
