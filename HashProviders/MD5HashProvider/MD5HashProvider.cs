// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.HashProvider;

using System;
using System.IO;
using System.Security.Cryptography;
using ktsu.Abstractions;

/// <summary>
/// A hash provider that uses MD5 for hashing data.
/// </summary>
public class MD5HashProvider : IHashProvider
{
	/// <summary>
	/// The length of the MD5 hash in bytes (16 bytes / 128 bits).
	/// </summary>
	public int HashLengthBytes => 16;

	/// <summary>
	/// Tries to hash the specified data into the provided hash buffer using MD5.
	/// </summary>
	/// <param name="data">The data to hash.</param>
	/// <param name="destination">The hash buffer to write the result to.</param>
	/// <returns>True if the hash operation was successful, false otherwise.</returns>
	public bool TryHash(ReadOnlySpan<byte> data, Span<byte> destination)
	{
		if (destination.Length < HashLengthBytes)
		{
			return false;
		}

		try
		{
#if NET7_0_OR_GREATER
			return MD5.TryHashData(data, destination, out int bytesWritten) && bytesWritten == HashLengthBytes;
#else
			using MD5 md5 = MD5.Create();
			return md5.TryComputeHash(data, destination, out int bytesWritten) && bytesWritten == HashLengthBytes;
#endif
		}
		catch (ArgumentException)
		{
			return false;
		}
		catch (ObjectDisposedException)
		{
			return false;
		}
		catch (NotSupportedException)
		{
			return false;
		}
	}

	/// <summary>
	/// Tries to hash the specified data from a stream into the provided hash buffer using MD5.
	/// </summary>
	/// <param name="data">The stream containing data to hash.</param>
	/// <param name="destination">The hash buffer to write the result to.</param>
	/// <returns>True if the hash operation was successful, false otherwise.</returns>
	public bool TryHash(Stream data, Span<byte> destination)
	{
		if (destination.Length < HashLengthBytes)
		{
			return false;
		}

		if (data is null)
		{
			return false;
		}

		try
		{
			using MD5 md5 = MD5.Create();
			byte[] hash = md5.ComputeHash(data);

			if (hash.Length != HashLengthBytes)
			{
				return false;
			}

			hash.CopyTo(destination);
			return true;
		}
		catch (ArgumentException)
		{
			return false;
		}
		catch (ObjectDisposedException)
		{
			return false;
		}
		catch (NotSupportedException)
		{
			return false;
		}
		catch (IOException)
		{
			return false;
		}
	}
}
