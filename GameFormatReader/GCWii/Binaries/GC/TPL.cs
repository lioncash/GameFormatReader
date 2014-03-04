using System;
using System.IO;
using GameFormatReader.Common;

namespace GameFormatReader.GCWii.Binaries.GC
{
	/// <summary>
	/// Represents a custom format that stores
	/// texture and pallet information.
	/// </summary>
	public sealed class TPL
	{
		#region Private Fields

		private const int CorrectHeaderSize = 0x0C;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="filepath">Path to the TPL file.</param>
		public TPL(string filepath)
		{
			if (filepath == null)
				throw new ArgumentNullException("filepath", "filepath cannot be null");

			if (!File.Exists(filepath))
				throw new ArgumentException("The file indicated by filepath does not exist", "filepath");

			using (EndianBinaryReader reader = new EndianBinaryReader(File.OpenRead(filepath), Endian.BigEndian))
			{
				ReadHeader(reader);
				ReadTextures(reader);
			}
		}

		#endregion

		#region Enums

		/// <summary>
		/// Defines the possible texture formats
		/// for an embedded texture.
		/// </summary>
		public enum TextureFormat
		{
			/// <summary>4-bit intensity, 8x8 tiles</summary>
			I4     = 0,
			/// <summary>8-bit intensity, 8x4 tiles</summary>
			I8     = 1,
			/// <summary>4-bit intensity with 4-bit alpha, 8x4 tiles</summary>
			IA4    = 2,
			/// <summary>8-bit intensity with 8-bit alpha, 8x8 tiles</summary>
			IA8    = 3,
			/// <summary>4x4 tiles</summary>
			RGB565 = 4,
			/// <summary>4x4 tiles - is RGB5 if color value is negative and RGB4A3 otherwise.</summary>
			RGB5A3 = 5,
			/// <summary>4x4 tiles in two cache lines - first is AR and second is GB</summary>
			RGBA8  = 6,
			/// <summary>4-bit color index, 8x8 tiles</summary>
			CI4    = 8,
			/// <summary>8-bit color index, 8x4 tiles</summary>
			CI8    = 9,
			/// <summary>14-bit color index, 4x4 tiles</summary>
			CI14X2 = 10,
			/// <summary>S3TC compressed, 2x2 blocks of 4x4 tiles</summary>
			CMP    = 14,
		}

		/// <summary>
		/// Defines the possible palette formats
		/// for an embedded palette.
		/// </summary>
		public enum PalFormat
		{
			/// <summary>8-bit intensity with 8-bit alpha, 8x8 tiles</summary>
			IA8    = 0,
			/// <summary>4x4 tiles</summary>
			RGB565 = 1,
			/// <summary>4x4 tiles - is RGB5 if color value is negative and RGB4A3 otherwise.</summary>
			RGB5A3 = 2,
		}

		/// <summary>
		/// The type of wrapping being applied to the
		/// horizontal and vertical axes of the texture
		/// </summary>
		public enum WrapMode
		{
			/// <summary>The last pixel of the texture image stretches outwards indefinitely.</summary>
			Clamp  = 0,
			/// <summary>The texture image repeats indefinitely.</summary>
			Repeat = 1,
			/// <summary>The texture image is mirrored indefinitely</summary>
			Mirror = 2,
		}

		#endregion

		#region Structs

		/// <summary>
		/// Represents an embedded texture
		/// </summary>
		public struct Texture
		{
			/// <summary>Height of this texture in pixels.</summary>
			public int Height { get; internal set; }
			/// <summary>Width of this texture in pixels.</summary>
			public int Width  { get; internal set; }
			/// <summary>Texture format</summary>
			public TextureFormat TexFormat { get; internal set; }
			/// <summary>Texture data</summary>
			public byte[] TextureData { get; internal set; }
			/// <summary>Horizontal wrapping mode</summary>
			public WrapMode WrapS { get; internal set; }
			/// <summary>Vertical wrapping mode</summary>
			public WrapMode WrapT { get; internal set; }
			/// <summary>Minimization filter (TODO: Does this correspond to specific values (if it does, it indicates enum vals))</summary>
			public int MinFilter { get; internal set; }
			/// <summary>Magnification filter (TODO: Does this correspond to specific values (if it does, it indicates enum vals))</summary>
			public int MagFilter { get; internal set; }
			/// <summary>LOD (Level of Detail) bias.</summary>
			public float LODBias { get; internal set; }
			/// <summary>Edge level of detail</summary>
			public byte EdgeLOD { get; internal set; }
			/// <summary>Minimum level of detail</summary>
			public byte MinLOD { get; internal set; }
			/// <summary>Maximum level of detail</summary>
			public byte MaxLOD { get; internal set; }
			/// <summary>Whether or not data is unpacked</summary>
			public byte IsUnpacked { get; internal set; }
			/// <summary>This texture's palette (optional).</summary>
			public Palette Palette { get; internal set; }
		}

		/// <summary>
		/// Represents an embedded palette
		/// </summary>
		public sealed class Palette
		{
			/// <summary>Number of palettes</summary>
			public short NumItems { get; internal set; }
			/// <summary>Whether or not data is unpacked.</summary>
			public byte IsUnpacked { get; internal set; }
			/// <summary>Indicates padding? Truthfully, I don't know</summary>
			public byte Padding { get; internal set; }
			/// <summary>Palette format</summary>
			public PalFormat PaletteFormat { get; internal set; }
			/// <summary>Palette data</summary>
			public byte[] Data { get; internal set; }
		}

		#endregion

		#region Properties

		/// <summary>
		/// Textures within this file.
		/// </summary>
		public Texture[] Textures
		{
			get;
			internal set;
		}

		#endregion

		#region Private Methods

		private void ReadHeader(EndianBinaryReader reader)
		{
			byte[] magic = reader.ReadBytes(4);
			if (magic[0] != 0x00 || magic[1] != 0x20 ||
			    magic[2] != 0xAF || magic[3] != 0x30)
			{
				throw new IOException("Not a valid TPL file - Incorrect file magic");
			}

			int numTextures = reader.ReadInt32();
			Textures = new Texture[numTextures];

			// Note that the header size must be 0x0C to be valid.
			int headerSize = reader.ReadInt32();

			if (headerSize != CorrectHeaderSize)
				throw new IOException(string.Format("Incorrect TPL header size. Should be {0}, was {1}", CorrectHeaderSize, headerSize));
		}

		private void ReadTextures(EndianBinaryReader reader)
		{
			for (int i = 0; i < Textures.Length; i++)
			{
				int textureOffset = reader.ReadInt32();
				int paletteOffset = reader.ReadInt32();

				

				Textures[i]           = new Texture();
				Textures[i].Height    = reader.ReadInt16At(textureOffset + 0x00);
				Textures[i].Width     = reader.ReadInt16At(textureOffset + 0x02);
				Textures[i].TexFormat = (TextureFormat) reader.ReadInt32At(textureOffset + 0x04);

				int texDataOffset = reader.ReadInt32At(textureOffset + 0x08);
				Textures[i].TextureData = reader.ReadBytesAt(texDataOffset, GetTextureDataSize(i));

				Textures[i].WrapS = (WrapMode) reader.ReadInt32At(textureOffset + 0x0C);
				Textures[i].WrapT = (WrapMode) reader.ReadInt32At(textureOffset + 0x10);

				Textures[i].MinFilter  = reader.ReadInt32At(textureOffset  + 0x14);
				Textures[i].MagFilter  = reader.ReadInt32At(textureOffset  + 0x18);
				Textures[i].LODBias    = reader.ReadSingleAt(textureOffset + 0x1C);
				Textures[i].EdgeLOD    = reader.ReadByteAt(textureOffset   + 0x20);
				Textures[i].MinLOD     = reader.ReadByteAt(textureOffset   + 0x21);
				Textures[i].MaxLOD     = reader.ReadByteAt(textureOffset   + 0x22);
				Textures[i].IsUnpacked = reader.ReadByteAt(textureOffset   + 0x23);

				if (paletteOffset != 0)
				{
					Textures[i].Palette = new Palette();
					Textures[i].Palette.NumItems      = reader.ReadInt16At(paletteOffset + 0x00);
					Textures[i].Palette.IsUnpacked    = reader.ReadByteAt(paletteOffset  + 0x02);
					Textures[i].Palette.Padding       = reader.ReadByteAt(paletteOffset  + 0x03);
					Textures[i].Palette.PaletteFormat = (PalFormat) reader.ReadInt32At(paletteOffset + 0x04);

					int dataOffset = reader.ReadInt32At(paletteOffset + 0x08);
					Textures[i].Palette.Data = reader.ReadBytesAt(dataOffset, Textures[i].Palette.NumItems);
				}
				else
				{
					Textures[i].Palette = null;
				}
			}
		}

		private int GetTextureDataSize(int i)
		{
			if (i < 0)
				throw new ArgumentException("i cannot be less than zero", "i");

			if (i >= Textures.Length)
				throw new ArgumentException("i cannot be larger than the total number of textures", "i");

			int size = 0;

			int width = Textures[i].Width;
			int height = Textures[i].Height;

			switch (Textures[i].TexFormat)
			{
				case TextureFormat.I4:
				case TextureFormat.CI4:
				case TextureFormat.CMP:
					size = ((width + 7) >> 3)*((height + 7) >> 3)*32;
					break;

				case TextureFormat.I8:
				case TextureFormat.IA4:
				case TextureFormat.CI8:
					size = ((width + 7) >> 3)*((height + 7) >> 2)*32;
					break;

				case TextureFormat.IA8:
				case TextureFormat.CI14X2:
				case TextureFormat.RGB565:
				case TextureFormat.RGB5A3:
					size = ((width + 3) >> 2)*((height + 3) >> 2)*32;
					break;

				case TextureFormat.RGBA8:
					size = ((width + 3) >> 2)*((height + 3) >> 2)*64;
					break;
			}

			return size;
		}

		#endregion
	}
}
