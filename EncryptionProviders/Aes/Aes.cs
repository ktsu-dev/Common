// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

namespace ktsu.EncryptionProviders;

using System;
using System.IO;
using System.Security.Cryptography;
using ktsu.Abstractions;

/// <summary>
/// An encryption provider that uses AES for data encryption and decryption.
/// </summary>
public class Aes : IEncryptionProvider, IDisposable
{
	private const int KeySize = 32; // 256 bits
	private const int IVSize = 16; // 128 bits
	private bool disposedValue;

	private readonly Lazy<System.Security.Cryptography.Aes> _aes;
	private readonly Lazy<System.Security.Cryptography.Aes> _generator;
	private readonly Lazy<ICryptoTransform> _encryptor;
	private readonly Lazy<ICryptoTransform> _decryptor;

	/// <summary>
	/// Creates a new instance of the <see cref="Aes"/> class.
	/// </summary>
	public Aes()
	{
		_aes = new(System.Security.Cryptography.Aes.Create);
		_generator = new(System.Security.Cryptography.Aes.Create);
		_encryptor = new(_aes.Value.CreateEncryptor);
		_decryptor = new(_aes.Value.CreateDecryptor);
	}

	/// <summary>
	/// Generates a new encryption key.
	/// </summary>
	/// <returns>A new encryption key.</returns>
	public byte[] GenerateKey()
	{
		_generator.Value.GenerateKey();
		return _generator.Value.Key;
	}

	/// <summary>
	/// Generates a new initialization vector.
	/// </summary>
	/// <returns>A new initialization vector.</returns>
	public byte[] GenerateIV()
	{
		_generator.Value.GenerateIV();
		return _generator.Value.IV;
	}

	/// <summary>
	/// Tries to encrypt the data from the span and write the result to the destination.
	/// </summary>
	/// <param name="data">The data to encrypt.</param>
	/// <param name="key">The key to use for encryption.</param>
	/// <param name="iv">The initialization vector to use for encryption.</param>
	/// <param name="destination">The destination to write the encrypted data to.</param>
	/// <returns>True if the encryption was successful, false otherwise.</returns>
	public bool TryEncrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, Span<byte> destination)
	{
		if (key.Length != KeySize || iv.Length != IVSize)
		{
			return false;
		}

		try
		{
			_aes.Value.Key = key.ToArray();
			_aes.Value.IV = iv.ToArray();

			byte[] encryptedData = _encryptor.Value.TransformFinalBlock(data.ToArray(), 0, data.Length);

			if (encryptedData.Length > destination.Length)
			{
				return false;
			}

			encryptedData.CopyTo(destination);
			return true;
		}
		catch (ArgumentException)
		{
			return false;
		}
		catch (CryptographicException)
		{
			return false;
		}
		catch (ObjectDisposedException)
		{
			return false;
		}
	}

	/// <summary>
	/// Tries to encrypt the data from the stream and write the result to the destination.
	/// </summary>
	/// <param name="data">The data to encrypt.</param>
	/// <param name="key">The key to use for encryption.</param>
	/// <param name="iv">The initialization vector to use for encryption.</param>
	/// <param name="destination">The destination to write the encrypted data to.</param>
	/// <returns>True if the encryption was successful, false otherwise.</returns>
	public bool TryEncrypt(Stream data, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, Stream destination)
	{
		if (data is null || destination is null || key.Length != KeySize || iv.Length != IVSize)
		{
			return false;
		}

		try
		{
			_aes.Value.Key = key.ToArray();
			_aes.Value.IV = iv.ToArray();

			using CryptoStream cryptoStream = new(destination, _encryptor.Value, CryptoStreamMode.Write, leaveOpen: true);
			data.CopyTo(cryptoStream);
			return true;
		}
		catch (ArgumentException)
		{
			return false;
		}
		catch (CryptographicException)
		{
			return false;
		}
		catch (IOException)
		{
			return false;
		}
		catch (ObjectDisposedException)
		{
			return false;
		}
	}

	/// <summary>
	/// Tries to decrypt the data from the span and write the result to the destination.
	/// </summary>
	/// <param name="data">The data to decrypt.</param>
	/// <param name="key">The key to use for decryption.</param>
	/// <param name="iv">The initialization vector to use for decryption.</param>
	/// <param name="destination">The destination to write the decrypted data to.</param>
	/// <returns>True if the decryption was successful, false otherwise.</returns>
	public bool TryDecrypt(ReadOnlySpan<byte> data, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, Span<byte> destination)
	{
		if (key.Length != KeySize || iv.Length != IVSize)
		{
			return false;
		}

		try
		{
			_aes.Value.Key = key.ToArray();
			_aes.Value.IV = iv.ToArray();

			byte[] decryptedData = _decryptor.Value.TransformFinalBlock(data.ToArray(), 0, data.Length);

			if (decryptedData.Length > destination.Length)
			{
				return false;
			}

			decryptedData.CopyTo(destination);
			return true;
		}
		catch (ArgumentException)
		{
			return false;
		}
		catch (CryptographicException)
		{
			return false;
		}
		catch (ObjectDisposedException)
		{
			return false;
		}
	}

	/// <summary>
	/// Tries to decrypt the data from the stream and write the result to the destination.
	/// </summary>
	/// <param name="data">The data to decrypt.</param>
	/// <param name="key">The key to use for decryption.</param>
	/// <param name="iv">The initialization vector to use for decryption.</param>
	/// <param name="destination">The destination to write the decrypted data to.</param>
	/// <returns>True if the decryption was successful, false otherwise.</returns>
	public bool TryDecrypt(Stream data, ReadOnlySpan<byte> key, ReadOnlySpan<byte> iv, Stream destination)
	{
		if (data is null || destination is null || key.Length != KeySize || iv.Length != IVSize)
		{
			return false;
		}

		try
		{
			_aes.Value.Key = key.ToArray();
			_aes.Value.IV = iv.ToArray();

			using CryptoStream cryptoStream = new(data, _decryptor.Value, CryptoStreamMode.Read, leaveOpen: true);
			cryptoStream.CopyTo(destination);
			return true;
		}
		catch (ArgumentException)
		{
			return false;
		}
		catch (CryptographicException)
		{
			return false;
		}
		catch (IOException)
		{
			return false;
		}
		catch (ObjectDisposedException)
		{
			return false;
		}
	}

	/// <summary>
	/// Disposes the resources used by the Aes instance.
	/// </summary>
	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				if (_encryptor.IsValueCreated)
				{
					_encryptor.Value.Dispose();
				}

				if (_decryptor.IsValueCreated)
				{
					_decryptor.Value.Dispose();
				}

				if (_aes.IsValueCreated)
				{
					_aes.Value.Dispose();
				}

				if (_generator.IsValueCreated)
				{
					_generator.Value.Dispose();
				}
			}

			disposedValue = true;
		}
	}

	/// <summary>
	/// Disposes the Aes instance and releases all resources.
	/// </summary>
	public void Dispose()
	{
		// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
