// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Common.Tests;

using System.Collections.Generic;
using System.Text;
using ktsu.Abstractions;
using ktsu.CompressionProviders;
using ktsu.EncryptionProviders;
using ktsu.FileSystemProviders;
using ktsu.HashProviders;
using ktsu.ObfuscationProviders;
using ktsu.SerializationProvider;
using ktsu.SerializationProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class DiTests
{
	private static ServiceProvider BuildProvider()
	{
		ServiceCollection services = new();

		// Register concrete providers
		services.AddSingleton<ICompressionProvider, Gzip>();
		services.AddSingleton<IEncryptionProvider, Aes>();
		services.AddSingleton<IFileSystemProvider, Native>();

		// Hash providers: register all three; resolve by IEnumerable<IHashProvider>
		services.AddSingleton<IHashProvider, MD5>();
		services.AddSingleton<IHashProvider, SHA1>();
		services.AddSingleton<IHashProvider, SHA256>();

		services.AddSingleton<IObfuscationProvider, Base64>();

		// Two different implementations for ISerializationProvider; resolve by IEnumerable
		services.AddSingleton<ISerializationProvider, NewtonsoftJson>();
		services.AddSingleton<ISerializationProvider, SystemTextJson>();

		return services.BuildServiceProvider();
	}

	[TestMethod]
	public void Compression_Gzip_Roundtrip_Bytes()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		ICompressionProvider compressor = serviceProvider.GetRequiredService<ICompressionProvider>();
		byte[] original = Encoding.UTF8.GetBytes("hello world");

		byte[] compressed = compressor.Compress(original);
		Assert.IsGreaterThan(0, compressed.Length);

		byte[] decompressed = compressor.Decompress(compressed);
		CollectionAssert.AreEqual(original, decompressed);
	}

	[TestMethod]
	public void Compression_Gzip_TryCompress_Span_Buffer_Size_Behavior()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		ICompressionProvider compressor = serviceProvider.GetRequiredService<ICompressionProvider>();
		byte[] original = Encoding.UTF8.GetBytes("some data that will compress");
		Span<byte> smallDestination = stackalloc byte[4];
		bool smallResult = compressor.TryCompress(original, smallDestination);
		Assert.IsFalse(smallResult);

		byte[] largeBuffer = new byte[original.Length * 10];
		bool largeResult = compressor.TryCompress(original, largeBuffer);
		Assert.IsTrue(largeResult);
	}

	[TestMethod]
	public void Compression_Gzip_TryDecompress_Span_From_Bytes()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		ICompressionProvider compressor = serviceProvider.GetRequiredService<ICompressionProvider>();
		byte[] original = Encoding.UTF8.GetBytes("payload to compress and then decompress");
		byte[] compressed = compressor.Compress(original);
		byte[] destination = new byte[original.Length];
		bool ok = compressor.TryDecompress(compressed, destination);
		Assert.IsTrue(ok);
		CollectionAssert.AreEqual(original, destination);
	}

	[TestMethod]
	public void Compression_Gzip_Stream_To_Stream_Roundtrip()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		ICompressionProvider compressor = serviceProvider.GetRequiredService<ICompressionProvider>();
		byte[] original = Encoding.UTF8.GetBytes("stream roundtrip");
		using MemoryStream input = new(original);
		using MemoryStream compressed = new();
		bool compressedOk = compressor.TryCompress(input, compressed);
		Assert.IsTrue(compressedOk);
		compressed.Position = 0;
		using MemoryStream decompressed = new();
		bool decompressedOk = compressor.TryDecompress(compressed, decompressed);
		Assert.IsTrue(decompressedOk);
		byte[] result = decompressed.ToArray();
		CollectionAssert.AreEqual(original, result);
	}

	[TestMethod]
	public void Compression_Gzip_Async_Roundtrip()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		ICompressionProvider compressor = serviceProvider.GetRequiredService<ICompressionProvider>();
		byte[] original = Encoding.UTF8.GetBytes("async compression");
		byte[] compressed = compressor.CompressAsync(original, TestContext.CancellationTokenSource.Token).Result;
		Assert.IsGreaterThan(0, compressed.Length);
		byte[] decompressed = compressor.DecompressAsync(compressed, TestContext.CancellationTokenSource.Token).Result;
		CollectionAssert.AreEqual(original, decompressed);
	}

	[TestMethod]
	public void Encryption_Aes_Roundtrip_Bytes()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		IEncryptionProvider aes = serviceProvider.GetRequiredService<IEncryptionProvider>();
		byte[] data = Encoding.UTF8.GetBytes("secret");

		byte[] key = aes.GenerateKey();
		byte[] iv = aes.GenerateIV();

		byte[] encrypted = aes.Encrypt(data, key, iv);
		Assert.IsGreaterThan(0, encrypted.Length);

		byte[] decrypted = aes.Decrypt(encrypted, key, iv);
		CollectionAssert.AreEqual(data, decrypted);
	}

	[TestMethod]
	public void Encryption_Aes_TryEncrypt_And_TryDecrypt_Span()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		IEncryptionProvider aes = serviceProvider.GetRequiredService<IEncryptionProvider>();
		byte[] data = Encoding.UTF8.GetBytes("span encrypt");
		byte[] key = aes.GenerateKey();
		byte[] iv = aes.GenerateIV();

		// small buffer should fail
		Span<byte> smallDest = stackalloc byte[4];
		bool small = aes.TryEncrypt(data, key, iv, smallDest);
		Assert.IsFalse(small);

		// reasonable buffer should succeed
		byte[] large = new byte[data.Length + 64];
		bool ok = aes.TryEncrypt(data, key, iv, large);
		Assert.IsTrue(ok);

		byte[] encrypted = aes.Encrypt(data, key, iv);
		byte[] decryptedDest = new byte[data.Length + 16];
		bool decOk = aes.TryDecrypt(encrypted, key, iv, decryptedDest);
		Assert.IsTrue(decOk);
		CollectionAssert.AreEqual(data, decryptedDest[..data.Length]);
	}

	[TestMethod]
	public void Encryption_Aes_Stream_To_Stream_Roundtrip()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		IEncryptionProvider aes = serviceProvider.GetRequiredService<IEncryptionProvider>();
		byte[] data = Encoding.UTF8.GetBytes("stream encrypt");
		byte[] key = aes.GenerateKey();
		byte[] iv = aes.GenerateIV();

		using MemoryStream plaintext = new(data);
		using MemoryStream ciphertext = new();
		bool encOk = aes.TryEncrypt(plaintext, key, iv, ciphertext);
		Assert.IsTrue(encOk);
		ciphertext.Position = 0;
		using MemoryStream decrypted = new();
		bool decOk = aes.TryDecrypt(ciphertext, key, iv, decrypted);
		Assert.IsTrue(decOk);
		byte[] result = decrypted.ToArray();
		CollectionAssert.AreEqual(data, result);
	}

	[TestMethod]
	public void Encryption_Aes_Async_Roundtrip()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		IEncryptionProvider aes = serviceProvider.GetRequiredService<IEncryptionProvider>();
		byte[] data = Encoding.UTF8.GetBytes("async encrypt");
		byte[] key = aes.GenerateKey();
		byte[] iv = aes.GenerateIV();

		byte[] encrypted = aes.EncryptAsync(data, key, iv, TestContext.CancellationTokenSource.Token).Result;
		byte[] decrypted = aes.DecryptAsync(encrypted, key, iv, TestContext.CancellationTokenSource.Token).Result;
		CollectionAssert.AreEqual(data, decrypted);
	}

	[TestMethod]
	public void FileSystem_Native_Can_Create_And_Read_File()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		IFileSystemProvider fileSystem = serviceProvider.GetRequiredService<IFileSystemProvider>();
		string tempDir = fileSystem.Path.Combine(fileSystem.Path.GetTempPath(), "ktsu-tests");
		fileSystem.Directory.CreateDirectory(tempDir);
		string path = fileSystem.Path.Combine(tempDir, Guid.NewGuid() + ".txt");

		const string content = "data";
		fileSystem.File.WriteAllText(path, content);
		string read = fileSystem.File.ReadAllText(path);
		Assert.AreEqual(content, read);

		fileSystem.File.Delete(path);
	}

	[TestMethod]
	public void HashProviders_MD5_SHA1_SHA256_Produce_Correct_Lengths()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		IEnumerable<IHashProvider> hashes = serviceProvider.GetServices<IHashProvider>();
		byte[] data = Encoding.UTF8.GetBytes("hash me");

		foreach (IHashProvider provider in hashes)
		{
			byte[] result = provider.Hash(data);
			Assert.AreEqual(provider.HashLengthBytes, result.Length);
		}
	}

	[TestMethod]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2014:Do not use stackalloc in loops", Justification = "Test code")]
	public void HashProviders_TryHash_Span_And_Stream()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		IEnumerable<IHashProvider> hashes = serviceProvider.GetServices<IHashProvider>();
		byte[] data = Encoding.UTF8.GetBytes("hash inputs");

		foreach (IHashProvider provider in hashes)
		{
			Span<byte> tooSmall = stackalloc byte[Math.Max(1, provider.HashLengthBytes - 1)];
			bool smallOk = provider.TryHash(data, tooSmall);
			Assert.IsFalse(smallOk);

			byte[] dest = new byte[provider.HashLengthBytes];
			bool ok = provider.TryHash(data, dest);
			Assert.IsTrue(ok);

			using MemoryStream stream = new(data);
			Array.Fill(dest, (byte)0);
			bool streamOk = provider.TryHash(stream, dest);
			Assert.IsTrue(streamOk);
		}
	}

	[TestMethod]
	public void HashProviders_Async_Variants()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		IEnumerable<IHashProvider> hashes = serviceProvider.GetServices<IHashProvider>();
		byte[] data = Encoding.UTF8.GetBytes("async hash");

		foreach (IHashProvider provider in hashes)
		{
			byte[] result = provider.HashAsync(data, TestContext.CancellationTokenSource.Token).Result;
			Assert.AreEqual(provider.HashLengthBytes, result.Length);

			byte[] dest = new byte[provider.HashLengthBytes];
			bool tryOk = provider.TryHashAsync(data, dest, TestContext.CancellationTokenSource.Token).Result;
			Assert.IsTrue(tryOk);

			using MemoryStream stream = new(data);
			Array.Fill(dest, (byte)0);
			bool tryStreamOk = provider.TryHashAsync(stream, dest, TestContext.CancellationTokenSource.Token).Result;
			Assert.IsTrue(tryStreamOk);
		}
	}

	[TestMethod]
	public void Obfuscation_Base64_Roundtrip()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		IObfuscationProvider base64 = serviceProvider.GetRequiredService<IObfuscationProvider>();
		byte[] original = Encoding.UTF8.GetBytes("roundtrip");

		byte[] obfuscated = base64.Obfuscate(original);
		Assert.IsGreaterThan(0, obfuscated.Length);

		byte[] deobfuscated = base64.Deobfuscate(obfuscated);
		CollectionAssert.AreEqual(original, deobfuscated);
	}

	[TestMethod]
	public void Obfuscation_Base64_TryObfuscate_And_TryDeobfuscate_Span()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		IObfuscationProvider base64 = serviceProvider.GetRequiredService<IObfuscationProvider>();
		byte[] original = Encoding.UTF8.GetBytes("span-obfuscation");
		string expectedBase64 = Convert.ToBase64String(original);

		Span<byte> small = stackalloc byte[4];
		bool smallOk = base64.TryObfuscate(original, small);
		Assert.IsFalse(smallOk);

		byte[] dest = new byte[expectedBase64.Length];
		bool ok = base64.TryObfuscate(original, dest);
		Assert.IsTrue(ok);
		string actual = Encoding.UTF8.GetString(dest);
		Assert.AreEqual(expectedBase64, actual);

		byte[] decodedDest = new byte[original.Length];
		bool decOk = base64.TryDeobfuscate(dest, decodedDest);
		Assert.IsTrue(decOk);
		CollectionAssert.AreEqual(original, decodedDest);
	}

	[TestMethod]
	public void Obfuscation_Base64_Stream_To_Stream_Roundtrip()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		IObfuscationProvider base64 = serviceProvider.GetRequiredService<IObfuscationProvider>();
		byte[] original = Encoding.UTF8.GetBytes("stream-obfuscation");
		using MemoryStream input = new(original);
		using MemoryStream obfuscated = new();
		bool obOk = base64.TryObfuscate(input, obfuscated);
		Assert.IsTrue(obOk);
		obfuscated.Position = 0;
		using MemoryStream deobfuscated = new();
		bool deobOk = base64.TryDeobfuscate(obfuscated, deobfuscated);
		Assert.IsTrue(deobOk);
		byte[] result = deobfuscated.ToArray();
		CollectionAssert.AreEqual(original, result);
	}

	[TestMethod]
	public void Obfuscation_Base64_Async_Variants()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		IObfuscationProvider base64 = serviceProvider.GetRequiredService<IObfuscationProvider>();
		byte[] original = Encoding.UTF8.GetBytes("async-obfuscation");

		byte[] obfuscated = base64.ObfuscateAsync(original, TestContext.CancellationTokenSource.Token).Result;
		byte[] deobfuscated = base64.DeobfuscateAsync(obfuscated, TestContext.CancellationTokenSource.Token).Result;
		CollectionAssert.AreEqual(original, deobfuscated);

		using MemoryStream reader = new(original);
		using MemoryStream writer = new();
		bool ok = base64.TryObfuscateAsync(reader, writer, TestContext.CancellationTokenSource.Token).Result;
		Assert.IsTrue(ok);
		writer.Position = 0;
		using MemoryStream round = new();
		bool deok = base64.TryDeobfuscateAsync(writer, round, TestContext.CancellationTokenSource.Token).Result;
		Assert.IsTrue(deok);
		CollectionAssert.AreEqual(original, round.ToArray());
	}

	[TestMethod]
	public void Serialization_Newtonsoft_And_SystemText_Can_Serialize_And_Deserialize()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		IEnumerable<ISerializationProvider> serializers = serviceProvider.GetServices<ISerializationProvider>();

		TestPoco obj = new() { Name = "Alice", Age = 30 };
		foreach (ISerializationProvider serializer in serializers)
		{
			string json = serializer.Serialize(obj);
			Assert.IsFalse(string.IsNullOrWhiteSpace(json));

			byte[] bytes = Encoding.UTF8.GetBytes(json);
			TestPoco? round = serializer.Deserialize<TestPoco>(bytes);
			Assert.IsNotNull(round);
			Assert.AreEqual(obj.Name, round!.Name);
			Assert.AreEqual(obj.Age, round.Age);
		}
	}

	[TestMethod]
	public void Serialization_Newtonsoft_And_SystemText_Async_Variants()
	{
		using ServiceProvider serviceProvider = BuildProvider();
		IEnumerable<ISerializationProvider> serializers = serviceProvider.GetServices<ISerializationProvider>();
		TestPoco obj = new() { Name = "Bob", Age = 42 };

		foreach (ISerializationProvider serializer in serializers)
		{
			using StringWriter writer = new();
			bool ok = serializer.TrySerializeAsync(obj, writer, TestContext.CancellationTokenSource.Token).Result;
			Assert.IsTrue(ok);
			string json = writer.ToString();
			byte[] bytes = Encoding.UTF8.GetBytes(json);
			TestPoco? round = serializer.DeserializeAsync<TestPoco>(bytes, TestContext.CancellationTokenSource.Token).Result;
			Assert.IsNotNull(round);

			using StringReader reader = new(json);
			TestPoco? round2 = serializer.DeserializeAsync<TestPoco>(reader, TestContext.CancellationTokenSource.Token).Result;
			Assert.IsNotNull(round2);
		}
	}

	private sealed class TestPoco
	{
		public string Name { get; set; } = string.Empty;
		public int Age { get; set; }
	}

	public TestContext TestContext { get; set; }
}
