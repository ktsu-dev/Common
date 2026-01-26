# ktsu Common Providers

A comprehensive collection of provider implementations for the ktsu ecosystem. This library provides standardized implementations of interfaces defined in `ktsu.Abstractions`, enabling consistent patterns across serialization, hashing, compression, encryption, file system access, and obfuscation.

## Available Providers

### Serialization Providers

Implementations of `ISerializationProvider` for popular JSON libraries:

| Package | Description |
|---------|-------------|
| `ktsu.SerializationProviders.NewtonsoftJson` | Newtonsoft.Json (Json.NET) provider |
| `ktsu.SerializationProviders.SystemTextJson` | System.Text.Json provider |

### Hash Providers

Implementations of `IHashProvider` for cryptographic and non-cryptographic hash algorithms:

| Package | Hash Length | Description |
|---------|-------------|-------------|
| `ktsu.HashProviders.MD5` | 16 bytes | MD5 hash algorithm |
| `ktsu.HashProviders.SHA1` | 20 bytes | SHA-1 hash algorithm |
| `ktsu.HashProviders.SHA256` | 32 bytes | SHA-256 hash algorithm |
| `ktsu.HashProviders.SHA384` | 48 bytes | SHA-384 hash algorithm |
| `ktsu.HashProviders.SHA512` | 64 bytes | SHA-512 hash algorithm |
| `ktsu.HashProviders.FNV1_32` | 4 bytes | FNV-1 32-bit non-cryptographic hash |
| `ktsu.HashProviders.FNV1a_32` | 4 bytes | FNV-1a 32-bit non-cryptographic hash |
| `ktsu.HashProviders.FNV1_64` | 8 bytes | FNV-1 64-bit non-cryptographic hash |
| `ktsu.HashProviders.FNV1a_64` | 8 bytes | FNV-1a 64-bit non-cryptographic hash |

### Compression Providers

Implementations of `ICompressionProvider`:

| Package | Description |
|---------|-------------|
| `ktsu.CompressionProviders.Gzip` | Gzip compression/decompression |

### Encryption Providers

Implementations of `IEncryptionProvider`:

| Package | Description |
|---------|-------------|
| `ktsu.EncryptionProviders.Aes` | AES symmetric encryption |

### File System Providers

Implementations of `IFileSystemProvider`:

| Package | Description |
|---------|-------------|
| `ktsu.FileSystemProviders.Native` | Native file system operations |

### Obfuscation Providers

Implementations of `IObfuscationProvider`:

| Package | Description |
|---------|-------------|
| `ktsu.ObfuscationProviders.Base64` | Base64 encoding/decoding |

## Installation

Install the specific provider packages you need:

```bash
# Serialization
dotnet add package ktsu.SerializationProviders.NewtonsoftJson
dotnet add package ktsu.SerializationProviders.SystemTextJson

# Hashing
dotnet add package ktsu.HashProviders.SHA256
dotnet add package ktsu.HashProviders.MD5
# ... other hash providers

# Compression
dotnet add package ktsu.CompressionProviders.Gzip

# Encryption
dotnet add package ktsu.EncryptionProviders.Aes

# File System
dotnet add package ktsu.FileSystemProviders.Native

# Obfuscation
dotnet add package ktsu.ObfuscationProviders.Base64
```

## Usage

### Serialization

```csharp
using ktsu.Abstractions;
using ktsu.SerializationProviders.SystemTextJson;

ISerializationProvider serializer = new SystemTextJson();

// Serialize
using var writer = new StringWriter();
if (serializer.TrySerialize(myObject, writer))
{
    string json = writer.ToString();
}

// Deserialize
byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonString);
var result = serializer.Deserialize<MyClass>(jsonBytes.AsSpan());
```

### Hashing

```csharp
using ktsu.Abstractions;
using ktsu.HashProviders.SHA256;

using IHashProvider hasher = new SHA256();

byte[] data = Encoding.UTF8.GetBytes("Hello, World!");
Span<byte> hash = stackalloc byte[hasher.HashLengthBytes];

if (hasher.TryHash(data, hash))
{
    string hashHex = Convert.ToHexString(hash);
}

// Hash from stream
using var stream = File.OpenRead("file.txt");
if (hasher.TryHash(stream, hash))
{
    // hash contains the file hash
}
```

### Compression

```csharp
using ktsu.Abstractions;
using ktsu.CompressionProviders.Gzip;

ICompressionProvider compressor = new Gzip();

byte[] original = Encoding.UTF8.GetBytes("Data to compress");

// Compress
if (compressor.TryCompress(original, out byte[] compressed))
{
    // Use compressed data
}

// Decompress
if (compressor.TryDecompress(compressed, out byte[] decompressed))
{
    string text = Encoding.UTF8.GetString(decompressed);
}
```

### Encryption

```csharp
using ktsu.Abstractions;
using ktsu.EncryptionProviders.Aes;

using IEncryptionProvider encryptor = new Aes();

byte[] key = new byte[32]; // 256-bit key
byte[] iv = new byte[16];  // 128-bit IV
RandomNumberGenerator.Fill(key);
RandomNumberGenerator.Fill(iv);

byte[] plaintext = Encoding.UTF8.GetBytes("Secret message");

// Encrypt
if (encryptor.TryEncrypt(plaintext, key, iv, out byte[] ciphertext))
{
    // Store or transmit ciphertext
}

// Decrypt
if (encryptor.TryDecrypt(ciphertext, key, iv, out byte[] decrypted))
{
    string message = Encoding.UTF8.GetString(decrypted);
}
```

### Dependency Injection

All providers integrate seamlessly with Microsoft.Extensions.DependencyInjection:

```csharp
using Microsoft.Extensions.DependencyInjection;
using ktsu.Abstractions;

var services = new ServiceCollection();

// Register specific providers
services.AddSingleton<ISerializationProvider, SystemTextJson>();
services.AddSingleton<IHashProvider, SHA256>();
services.AddSingleton<ICompressionProvider, Gzip>();
services.AddSingleton<IEncryptionProvider, Aes>();
services.AddSingleton<IFileSystemProvider, Native>();
services.AddSingleton<IObfuscationProvider, Base64>();

var provider = services.BuildServiceProvider();

// Resolve and use
var serializer = provider.GetRequiredService<ISerializationProvider>();
var hasher = provider.GetRequiredService<IHashProvider>();
```

## Features

- **Consistent APIs** - All providers implement standardized interfaces from `ktsu.Abstractions`
- **Graceful Error Handling** - Methods return success/failure indicators rather than throwing exceptions
- **Multi-Target Support** - .NET 9.0, 8.0, 7.0, 6.0, and .NET Standard 2.1
- **Dependency Injection Ready** - Easy integration with DI containers
- **Provider Pattern** - Swap implementations without changing consumer code
- **Disposable Resources** - Providers that hold resources implement `IDisposable`

## Error Handling

All providers follow consistent error handling patterns:

```csharp
// Hash providers return false on failure
if (!hasher.TryHash(data, destination))
{
    // Handle failure (buffer too small, disposed, etc.)
}

// Serialization returns false/default on failure
if (!serializer.TrySerialize(obj, writer))
{
    // Handle serialization failure
}

var result = serializer.Deserialize<MyClass>(invalidData);
if (result == null)
{
    // Handle deserialization failure
}

// Compression/Encryption use out parameters
if (!compressor.TryCompress(data, out byte[] compressed))
{
    // Handle compression failure
}
```

## Requirements

- .NET 9.0, 8.0, 7.0, 6.0, or .NET Standard 2.1
- ktsu.Abstractions package

## License

Licensed under the MIT License. See LICENSE.md for details.

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.
