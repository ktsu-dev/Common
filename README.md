# ktsu Serialization Providers

A collection of serialization providers implementing `ktsu.Abstractions.ISerializationProvider` for popular .NET JSON libraries. This library provides a consistent, standardized interface for serialization across different JSON libraries.

## 📦 Available Providers

- **NewtonsoftJson** - Provider for Newtonsoft.Json
- **SystemTextJson** - Provider for System.Text.Json

## 🚀 Installation

### Newtonsoft.Json Provider

```bash
dotnet add package ktsu.SerializationProviders.NewtonsoftJson
```

### System.Text.Json Provider

```bash
dotnet add package ktsu.SerializationProviders.SystemTextJson
```

## 💡 Usage

### Basic Usage

```csharp
using ktsu.SerializationProvider;

// Using Newtonsoft.Json provider
var newtonsoftProvider = new NewtonsoftJson();

// Using System.Text.Json provider  
var systemTextJsonProvider = new SystemTextJson();

// Both implement ISerializationProvider
public void ProcessData(ISerializationProvider provider)
{
    // Serialize an object to TextWriter
    var data = new { Name = "John", Age = 30 };
    using var writer = new StringWriter();
    bool success = provider.TrySerialize(data, writer);
    
    if (success)
    {
        string json = writer.ToString();
        Console.WriteLine(json);
    }
    
    // Deserialize from byte span
    byte[] jsonBytes = Encoding.UTF8.GetBytes("{\"Name\":\"Jane\",\"Age\":25}");
    var result = provider.Deserialize<Person>(jsonBytes.AsSpan());
}
```

### Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using ktsu.Abstractions;
using ktsu.SerializationProvider;

var services = new ServiceCollection();

// Register your preferred provider
services.AddSingleton<ISerializationProvider, NewtonsoftJson>();
// or
services.AddSingleton<ISerializationProvider, SystemTextJson>();

var serviceProvider = services.BuildServiceProvider();
var serializer = serviceProvider.GetRequiredService<ISerializationProvider>();
```

## 🔧 API Reference

### ISerializationProvider Interface

Both providers implement the `ktsu.Abstractions.ISerializationProvider` interface:

```csharp
public interface ISerializationProvider
{
    T? Deserialize<T>(ReadOnlySpan<byte> data);
    bool TrySerialize(object obj, TextWriter writer);
}
```

#### Methods

- **`Deserialize<T>(ReadOnlySpan<byte> data)`**
  - Deserializes UTF-8 encoded JSON byte data into a specified type
  - Returns `default(T)` if data is empty or deserialization fails
  - Handles common exceptions gracefully

- **`TrySerialize(object obj, TextWriter writer)`**
  - Attempts to serialize an object to JSON and write to the specified TextWriter
  - Returns `true` if successful, `false` otherwise
  - Handles serialization exceptions gracefully

## 🎯 Features

- **Consistent API** - Same interface regardless of underlying JSON library
- **Error Handling** - Graceful handling of serialization/deserialization errors
- **Performance** - Optimized for common use cases
- **Multi-Target** - Supports .NET 9.0, 8.0, 7.0, 6.0, and .NET Standard 2.1
- **Dependency Injection Ready** - Easy integration with DI containers

## 🧪 Error Handling

Both providers handle errors gracefully:

- **Deserialization**: Returns `default(T)` on failure (empty data, invalid JSON, etc.)
- **Serialization**: Returns `false` on failure, with no exceptions thrown

```csharp
// Safe deserialization - won't throw
var result = provider.Deserialize<MyClass>(invalidJsonBytes);
if (result == null)
{
    // Handle deserialization failure
}

// Safe serialization - won't throw  
using var writer = new StringWriter();
if (!provider.TrySerialize(problematicObject, writer))
{
    // Handle serialization failure
}
```

## 🔄 Migration Between Providers

Since both providers implement the same interface, switching between them is seamless:

```csharp
// Easy to switch providers
ISerializationProvider provider = useNewtonsoft 
    ? new NewtonsoftJson() 
    : new SystemTextJson();
```

## 📋 Requirements

- .NET 9.0, 8.0, 7.0, 6.0, or .NET Standard 2.1
- ktsu.Abstractions package

## 📄 License

Licensed under the MIT License. See LICENSE.md for details.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.
