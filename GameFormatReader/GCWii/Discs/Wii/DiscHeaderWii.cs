using GameFormatReader.Common;

namespace GameFormatReader.GCWii.Discs.Wii
{
	/// <summary>
	/// Represents the <see cref="DiscHeader"/> of a Wii disc.
	/// </summary>
	public sealed class DiscHeaderWii : DiscHeader
	{
		#region Constructor

		internal DiscHeaderWii(EndianBinaryReader reader)
		{
			ReadHeader(reader);
		}

		#endregion

		#region Protected Methods

		// Reads the common portion of the header between the Gamecube and Wii discs.
		// Disc classes for GC and Wii must implement the rest of the reading.
		protected override void ReadHeader(EndianBinaryReader reader)
		{
			Type = DetermineDiscType(reader.ReadChar());
			GameCode = new string(reader.ReadChars(2));
			RegionCode = DetermineRegion(reader.ReadChar());
			MakerCode = new string(reader.ReadChars(2));
			DiscNumber = reader.ReadByte();
			AudioStreaming = reader.ReadBoolean();
			StreamingBufferSize = reader.ReadByte();

			// Skip unused bytes
			reader.BaseStream.Position += 14;

			// Now we read the magic word to determine which system.
			MagicWord = reader.ReadInt32();

			// Skip the other 4 bytes, since this is a Wii header, not a GameCube one.
			reader.BaseStream.Position += 4;

			GameTitle = new string(reader.ReadChars(64));

			// This is all the relevant information, so we can skip to the end of the header.
			// (or, the beginning of the Partition information).
			reader.BaseStream.Position = 0x40000;
		}

		#endregion
	}
}
