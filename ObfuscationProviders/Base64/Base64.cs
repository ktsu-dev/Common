// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.ObfuscationProviders;

using System;
using System.IO;
using System.Text;
using ktsu.Abstractions;

/// <summary>
/// An obfuscation provider that uses Base64 encoding for data obfuscation and deobfuscation.
/// </summary>
public class Base64 : IObfuscationProvider
{
	/// <summary>
	/// Tries to obfuscate the data from the span and write the result to the destination.
	/// </summary>
	/// <param name="data">The data to obfuscate.</param>
	/// <param name="destination">The destination to write the obfuscated data to.</param>
	/// <returns>True if the obfuscation was successful, false otherwise.</returns>
	public bool TryObfuscate(ReadOnlySpan<byte> data, Span<byte> destination)
	{
		try
		{
			string base64String = Convert.ToBase64String(data);
			byte[] obfuscatedData = Encoding.UTF8.GetBytes(base64String);

			if (obfuscatedData.Length > destination.Length)
			{
				return false;
			}

			obfuscatedData.CopyTo(destination);
			return true;
		}
		catch (ArgumentException)
		{
			return false;
		}
		catch (FormatException)
		{
			return false;
		}
	}

	/// <summary>
	/// Tries to obfuscate the data from the reader and write the result to the writer.
	/// </summary>
	/// <param name="reader">The reader to read the data from.</param>
	/// <param name="writer">The writer to write the obfuscated data to.</param>
	/// <returns>True if the obfuscation was successful, false otherwise.</returns>
	public bool TryObfuscate(Stream reader, Stream writer)
	{
		if (reader is null || writer is null)
		{
			return false;
		}

		try
		{
			using MemoryStream inputBuffer = new();
			reader.CopyTo(inputBuffer);
			byte[] inputData = inputBuffer.ToArray();

			string base64String = Convert.ToBase64String(inputData);
			byte[] obfuscatedData = Encoding.UTF8.GetBytes(base64String);

			writer.Write(obfuscatedData, 0, obfuscatedData.Length);
			return true;
		}
		catch (ArgumentException)
		{
			return false;
		}
		catch (IOException)
		{
			return false;
		}
		catch (FormatException)
		{
			return false;
		}
		catch (ObjectDisposedException)
		{
			return false;
		}
	}

	/// <summary>
	/// Tries to deobfuscate the data from the span and write the result to the destination.
	/// </summary>
	/// <param name="data">The data to deobfuscate.</param>
	/// <param name="destination">The destination to write the deobfuscated data to.</param>
	/// <returns>True if the deobfuscation was successful, false otherwise.</returns>
	public bool TryDeobfuscate(ReadOnlySpan<byte> data, Span<byte> destination)
	{
		try
		{
			string base64String = Encoding.UTF8.GetString(data);
			byte[] deobfuscatedData = Convert.FromBase64String(base64String);

			if (deobfuscatedData.Length > destination.Length)
			{
				return false;
			}

			deobfuscatedData.CopyTo(destination);
			return true;
		}
		catch (ArgumentException)
		{
			return false;
		}
		catch (FormatException)
		{
			return false;
		}
	}

	/// <summary>
	/// Tries to deobfuscate the data from the reader and write the result to the writer.
	/// </summary>
	/// <param name="reader">The reader to read the data from.</param>
	/// <param name="writer">The writer to write the deobfuscated data to.</param>
	/// <returns>True if the deobfuscation was successful, false otherwise.</returns>
	public bool TryDeobfuscate(Stream reader, Stream writer)
	{
		if (reader is null || writer is null)
		{
			return false;
		}

		try
		{
			using MemoryStream inputBuffer = new();
			reader.CopyTo(inputBuffer);
			string base64String = Encoding.UTF8.GetString(inputBuffer.ToArray());

			byte[] deobfuscatedData = Convert.FromBase64String(base64String);
			writer.Write(deobfuscatedData, 0, deobfuscatedData.Length);
			return true;
		}
		catch (ArgumentException)
		{
			return false;
		}
		catch (IOException)
		{
			return false;
		}
		catch (FormatException)
		{
			return false;
		}
		catch (ObjectDisposedException)
		{
			return false;
		}
	}
}
