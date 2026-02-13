// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Common.Tests;

using System.Collections.Generic;
using ktsu.Abstractions;
using ktsu.EncryptionProviders;
using ktsu.FileSystemProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DiTests
{
	private static ServiceProvider BuildProvider()
	{
		ServiceCollection services = new();

		services.AddCommon();
		return services.BuildServiceProvider();
	}

	[TestMethod]
	public void DI_Can_Resolve_All_Singleton_Providers()
	{
		using ServiceProvider serviceProvider = BuildProvider();

		// Test single-implementation providers
		IEncryptionProvider encryption = serviceProvider.GetRequiredService<IEncryptionProvider>();
		Assert.IsNotNull(encryption);
		Assert.IsInstanceOfType<Aes>(encryption);

		IFileSystemProvider fileSystem = serviceProvider.GetRequiredService<IFileSystemProvider>();
		Assert.IsNotNull(fileSystem);
		Assert.IsInstanceOfType<Native>(fileSystem);
	}

	[TestMethod]
	public void DI_Can_Resolve_Multiple_Compression_Providers()
	{
		using ServiceProvider serviceProvider = BuildProvider();

		IEnumerable<ICompressionProvider> compressionProviders = serviceProvider.GetServices<ICompressionProvider>();
		ICompressionProvider[] providers = [.. compressionProviders];

		Assert.HasCount(4, providers, "Should resolve all 4 compression providers");

		string[] expectedTypes = ["Brotli", "Deflate", "Gzip", "ZLib"];
		string[] actualTypes = [.. providers.Select(p => p.GetType().Name).OrderBy(n => n)];
		CollectionAssert.AreEquivalent(expectedTypes, actualTypes);
	}

	[TestMethod]
	public void DI_Can_Resolve_Multiple_Obfuscation_Providers()
	{
		using ServiceProvider serviceProvider = BuildProvider();

		IEnumerable<IObfuscationProvider> obfuscationProviders = serviceProvider.GetServices<IObfuscationProvider>();
		IObfuscationProvider[] providers = [.. obfuscationProviders];

		Assert.HasCount(2, providers, "Should resolve both obfuscation providers");

		string[] expectedTypes = ["Base64", "Hex"];
		string[] actualTypes = [.. providers.Select(p => p.GetType().Name).OrderBy(n => n)];
		CollectionAssert.AreEquivalent(expectedTypes, actualTypes);
	}

	[TestMethod]
	public void DI_Can_Resolve_Multiple_Hash_Providers()
	{
		using ServiceProvider serviceProvider = BuildProvider();

		IEnumerable<IHashProvider> hashProviders = serviceProvider.GetServices<IHashProvider>();
		IHashProvider[] providers = [.. hashProviders];

		Assert.HasCount(15, providers, "Should resolve all 15 hash providers");

		// Verify all expected types are present
		string[] expectedTypes = ["MD5", "SHA1", "SHA256", "SHA384", "SHA512", "FNV1_32", "FNV1a_32", "FNV1_64", "FNV1a_64", "CRC32", "CRC64", "XxHash32", "XxHash64", "XxHash3", "XxHash128"];
		string[] actualTypes = [.. providers.Select(p => p.GetType().Name).OrderBy(n => n)];
		CollectionAssert.AreEquivalent(expectedTypes, actualTypes);
	}

	[TestMethod]
	public void DI_Can_Resolve_Multiple_Serialization_Providers()
	{
		using ServiceProvider serviceProvider = BuildProvider();

		IEnumerable<ISerializationProvider> serializationProviders = serviceProvider.GetServices<ISerializationProvider>();
		ISerializationProvider[] providers = [.. serializationProviders];

		Assert.HasCount(2, providers, "Should resolve both serialization providers");

		// Verify all expected types are present
		string[] expectedTypes = ["NewtonsoftJson", "SystemTextJson"];
		string[] actualTypes = [.. providers.Select(p => p.GetType().Name).OrderBy(n => n)];
		CollectionAssert.AreEquivalent(expectedTypes, actualTypes);
	}

	[TestMethod]
	public void DI_Providers_Are_Singletons()
	{
		using ServiceProvider serviceProvider = BuildProvider();

		// Test that singleton providers return the same instance
		ICompressionProvider compression1 = serviceProvider.GetRequiredService<ICompressionProvider>();
		ICompressionProvider compression2 = serviceProvider.GetRequiredService<ICompressionProvider>();
		Assert.AreSame(compression1, compression2, "Compression provider should be singleton");

		IEncryptionProvider encryption1 = serviceProvider.GetRequiredService<IEncryptionProvider>();
		IEncryptionProvider encryption2 = serviceProvider.GetRequiredService<IEncryptionProvider>();
		Assert.AreSame(encryption1, encryption2, "Encryption provider should be singleton");

		// Test hash providers are also singletons
		IEnumerable<IHashProvider> hashProviders1 = serviceProvider.GetServices<IHashProvider>();
		IEnumerable<IHashProvider> hashProviders2 = serviceProvider.GetServices<IHashProvider>();

		IHashProvider[] providers1 = [.. hashProviders1];
		IHashProvider[] providers2 = [.. hashProviders2];

		for (int i = 0; i < providers1.Length; i++)
		{
			Assert.AreSame(providers1[i], providers2[i], $"Hash provider {providers1[i].GetType().Name} should be singleton");
		}
	}

	public TestContext TestContext { get; set; } = null!;
}
