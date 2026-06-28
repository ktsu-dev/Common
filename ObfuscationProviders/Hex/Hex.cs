// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.ObfuscationProviders;

using System;
using System.IO;
using System.Text;
using ktsu.Abstractions;

/// <summary>
/// An obfuscation provider that uses hexadecimal encoding for data obfuscation and deobfuscation.
/// </summary>
public class Hex : IObfuscationProvider
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
			string hexString = Convert.ToHexString(data);
			byte[] obfuscatedData = Encoding.UTF8.GetBytes(hexString);

			if (obfuscatedData.Length > destination.Length)
			{
				return false;
			}

			obfuscatedData.CopyTo(destination);
			// Clear the rest of the destination buffer to ensure only obfuscated data is present
			destination[obfuscatedData.Length..].Clear();
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
	/// <param name="data">The data to obfuscate.</param>
	/// <param name="destination">The destination to write the obfuscated data to.</param>
	/// <returns>True if the obfuscation was successful, false otherwise.</returns>
	public bool TryObfuscate(Stream data, Stream destination)
	{
		if (data is null || destination is null)
		{
			return false;
		}

		try
		{
			using MemoryStream inputBuffer = new();
			data.CopyTo(inputBuffer);
			byte[] inputData = inputBuffer.ToArray();

			string hexString = Convert.ToHexString(inputData);
			byte[] obfuscatedData = Encoding.UTF8.GetBytes(hexString);

			destination.Write(obfuscatedData, 0, obfuscatedData.Length);
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
	/// <param name="obfuscatedData">The obfuscated data to deobfuscate.</param>
	/// <param name="destination">The destination to write the deobfuscated data to.</param>
	/// <returns>True if the deobfuscation was successful, false otherwise.</returns>
	public bool TryDeobfuscate(ReadOnlySpan<byte> obfuscatedData, Span<byte> destination)
	{
		try
		{
			// Find the actual length of obfuscated data (excluding trailing zeros)
			ReadOnlySpan<byte> actualData = obfuscatedData;
			int lastNonZero = obfuscatedData.Length - 1;
			while (lastNonZero >= 0 && obfuscatedData[lastNonZero] == 0)
			{
				lastNonZero--;
			}

			if (lastNonZero >= 0)
			{
				actualData = obfuscatedData[..(lastNonZero + 1)];
			}
			else
			{
				return false; // All zeros is not valid obfuscated data
			}

			string hexString = Encoding.UTF8.GetString(actualData);
			byte[] deobfuscatedData = Convert.FromHexString(hexString);

			if (deobfuscatedData.Length > destination.Length)
			{
				return false;
			}

			deobfuscatedData.CopyTo(destination);
			// Clear the rest of the destination buffer
			destination[deobfuscatedData.Length..].Clear();
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
	/// <param name="obfuscatedData">The obfuscated data to deobfuscate.</param>
	/// <param name="destination">The destination to write the deobfuscated data to.</param>
	/// <returns>True if the deobfuscation was successful, false otherwise.</returns>
	public bool TryDeobfuscate(Stream obfuscatedData, Stream destination)
	{
		if (obfuscatedData is null || destination is null)
		{
			return false;
		}

		try
		{
			using MemoryStream inputBuffer = new();
			obfuscatedData.CopyTo(inputBuffer);
			string hexString = Encoding.UTF8.GetString(inputBuffer.ToArray());

			byte[] deobfuscatedData = Convert.FromHexString(hexString);
			destination.Write(deobfuscatedData, 0, deobfuscatedData.Length);
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
