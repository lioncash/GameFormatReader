using System;
using System.IO;
using GameFormatReader.Common;

namespace GameFormatReader.GCWii.Compression
{
	/// <summary>
	/// Class for decoding the Yaz0 compression format.
	/// </summary>
	public static class Yaz0
	{
		// Position where the compressed data starts.
		private const int StartOfData = 0x10;

		/// <summary>
		/// Decodes a given Yaz0-compressed file.
		/// </summary>
		/// <param name="filepath">Path to the Yaz0 compressed file.</param>
		/// <returns>the decoded data.</returns>
		public static byte[] Decode(string filepath)
		{
			if (filepath == null)
				throw new ArgumentNullException("filepath", "filepath cannot be null.");

			if (!File.Exists(filepath))
				throw new IOException("filepath refers to a file which doesn't exist.");

			return Decode(File.ReadAllBytes(filepath));
		}

		/// <summary>
		/// Decodes the given source data.
		/// </summary>
		/// <remarks>Decoding alg borrowed from YAGCD.</remarks>.
		/// <param name="input">The source data to decode.</param>
		/// <returns>The decoded data.</returns>
		public static byte[] Decode(byte[] input)
		{
			using (EndianBinaryReader reader = new EndianBinaryReader(new MemoryStream(input), Endian.Big))
			{
				// Check if the data is even Yaz0 compressed.
				if (new string(reader.ReadChars(4)) != "Yaz0")
					throw new ArgumentException("input is not Yaz0 compressed data", "input");

				// These 4 bytes read tell us the decompressed data size.
				byte[] output = new byte[reader.ReadUInt32()];

				// Now decode the compressed data.
				long srcPos = StartOfData;
				long dstPos = 0;
				uint validBitCount = 0; // Number of valid bits left in the 'code' byte.
				byte currentCodeByte = 0;

				while (dstPos < output.Length)
				{
					if (validBitCount == 0)
					{
						currentCodeByte = input[srcPos++];
						validBitCount = 8;
					}

					if ((currentCodeByte & 0x80) != 0)
					{
						// Straight copy
						output[dstPos++] = input[srcPos++];
					}
					else
					{
						// RLE part.
						byte byte1 = input[srcPos++];
						byte byte2 = input[srcPos++];

						uint dist     = (uint) (((byte1 & 0x0F) << 8) | byte2);
						uint copySrc  = (uint) (dstPos - (dist + 1));
						uint numBytes = (uint) (byte1 >> 4);

						if (numBytes == 0)
						{
							numBytes = input[srcPos++] + 0x12U;
						}
						else
						{
							numBytes += 2;
						}

						// Copy run
						for (int i = 0; i < numBytes; i++)
						{
							output[dstPos++] = output[copySrc++];
						}
					}

					// Use next bit from the "code" byte.
					currentCodeByte <<= 1;
					validBitCount -= 1;
				}

				return output;
			}
		}

		/// <summary>
		/// Checkes if a given file is Yaz0-compressed.
		/// </summary>
		/// <param name="filePath">Path to the file to check.</param>
		/// <returns>true if the file is Yaz0-compressed; false otherwise.</returns>
		public static bool IsYaz0Compressed(string filePath)
		{
			if (filePath == null)
				throw new ArgumentNullException("filePath", "file cannot be null");

			if (!File.Exists(filePath))
				throw new ArgumentException("File specified by filePath does not exist", "filePath");

			using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
			{
				byte[] magic = new byte[4];
				fs.Read(magic, 0, 4);

				if (magic[0] != 'Y' &&
				    magic[1] != 'a' &&
				    magic[2] != 'z' &&
				    magic[3] != '0')
					return false;
			}

			return true;
		}
	}
}
