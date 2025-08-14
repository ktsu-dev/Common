// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.Common.Tests;

using ktsu.Abstractions;
using ktsu.CompressionProviders;
using ktsu.EncryptionProviders;
using ktsu.FileSystemProviders;
using ktsu.HashProviders;
using ktsu.ObfuscationProviders;
using ktsu.SerializationProviders;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
	public static ServiceCollection AddCommon(this ServiceCollection services)
	{
		services.AddCompressionProviders();
		services.AddEncryptionProviders();
		services.AddFileSystemProviders();
		services.AddHashProviders();
		services.AddObfuscationProviders();
		services.AddSerializationProviders();
		return services;
	}

	public static ServiceCollection AddCompressionProviders(this ServiceCollection services)
	{
		services.AddSingleton<ICompressionProvider, Gzip>();
		return services;
	}

	public static ServiceCollection AddEncryptionProviders(this ServiceCollection services)
	{
		services.AddSingleton<IEncryptionProvider, Aes>();
		return services;
	}

	public static ServiceCollection AddFileSystemProviders(this ServiceCollection services)
	{
		services.AddSingleton<IFileSystemProvider, Native>();
		return services;
	}

	public static ServiceCollection AddHashProviders(this ServiceCollection services)
	{
		services.AddSingleton<IHashProvider, SHA1>();
		services.AddSingleton<IHashProvider, SHA256>();
		services.AddSingleton<IHashProvider, SHA384>();
		services.AddSingleton<IHashProvider, SHA512>();
		services.AddSingleton<IHashProvider, MD5>();
		services.AddSingleton<IHashProvider, FNV1_32>();
		services.AddSingleton<IHashProvider, FNV1_64>();
		services.AddSingleton<IHashProvider, FNV1a_32>();
		services.AddSingleton<IHashProvider, FNV1a_64>();
		return services;
	}

	public static ServiceCollection AddObfuscationProviders(this ServiceCollection services)
	{
		services.AddSingleton<IObfuscationProvider, Base64>();
		return services;
	}

	public static ServiceCollection AddSerializationProviders(this ServiceCollection services)
	{
		services.AddSingleton<ISerializationProvider, SystemTextJson>();
		services.AddSingleton<ISerializationProvider, NewtonsoftJson>();
		return services;
	}
}
