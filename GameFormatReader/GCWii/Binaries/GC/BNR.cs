using System;
using System.IO;
using System.Text;
using GameFormatReader.Common;

// TODO: BNR2 multi-language reading.

namespace GameFormatReader.GCWii.Binaries.GC
{
	/// <summary>
	/// Banner file format
	/// </summary>
	public sealed class BNR
	{
		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="filepath">Path to the BNR file.</param>
		public BNR(string filepath)
		{
			if (filepath == null)
				throw new ArgumentException("filepath cannot be null", "filepath");

			ReadBNR(filepath);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="data">Data that contains the BNR file.</param>
		/// <param name="offset">Offset in the data to begin reading at.</param>
		public BNR(byte[] data, int offset)
		{
			if (data == null)
				throw new ArgumentException("data cannot be null", "data");

			if (offset < 0)
				throw new ArgumentException("offset cannot be a negative number", "offset");

			ReadBNR(data, offset);
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Magic word.
		/// <para>BNR1 for (US/JP)</para>
		/// <para>BNR2 for EU.</para>
		/// </summary>
		public string MagicWord
		{
			get;
			private set;
		}

		/// <summary>
		/// Graphical data stored in the banner.
		/// Pixel format is RGB5A1.
		/// </summary>
		public byte[] Data
		{
			get;
			private set;
		}

		/// <summary>
		/// Basic game title.
		/// </summary>
		public string GameTitle
		{
			get;
			private set;
		}

		/// <summary>
		/// Name of the company/developer of the game.
		/// </summary>
		public string DeveloperName
		{
			get;
			private set;
		}

		/// <summary>
		/// Full title of the game.
		/// </summary>
		public string FullGameTitle
		{
			get;
			private set;
		}

		/// <summary>
		/// Full name of the company/developer or description.
		/// </summary>
		public string FullDeveloperName
		{
			get;
			private set;
		}

		/// <summary>
		/// Description of the game this banner is for.
		/// </summary>
		public string GameDescription
		{
			get;
			private set;
		}

		#endregion

		#region Private Methods

		// File-based reading
		private void ReadBNR(string filepath)
		{
			using (EndianBinaryReader reader = new EndianBinaryReader(File.OpenRead(filepath), Endian.BigEndian))
			{
				MagicWord = new string(reader.ReadChars(4));

				// Skip padding bytes
				reader.BaseStream.Position = 0x20;

				Data = reader.ReadBytes(0x1800);

				// Name related data
				Encoding shiftJis = Encoding.GetEncoding("shift_jis");
				GameTitle = shiftJis.GetString(reader.ReadBytes(0x20));
				DeveloperName = shiftJis.GetString(reader.ReadBytes(0x20));
				FullGameTitle = shiftJis.GetString(reader.ReadBytes(0x40));
				FullDeveloperName = shiftJis.GetString(reader.ReadBytes(0x40));
				GameDescription = shiftJis.GetString(reader.ReadBytes(0x80));
			}
		}

		// Buffer-based reading
		private void ReadBNR(byte[] data, int offset)
		{
			MemoryStream ms = new MemoryStream(data, offset, data.Length);

			using (EndianBinaryReader reader = new EndianBinaryReader(ms, Endian.BigEndian))
			{
				MagicWord = new string(reader.ReadChars(4));

				// Skip padding bytes
				reader.BaseStream.Position = 0x20;

				Data = reader.ReadBytes(0x1800);

				// Name related data
				Encoding shiftJis = Encoding.GetEncoding("shift_jis");
				GameTitle = shiftJis.GetString(reader.ReadBytes(0x20));
				DeveloperName = shiftJis.GetString(reader.ReadBytes(0x20));
				FullGameTitle = shiftJis.GetString(reader.ReadBytes(0x40));
				FullDeveloperName = shiftJis.GetString(reader.ReadBytes(0x40));
				GameDescription = shiftJis.GetString(reader.ReadBytes(0x80));
			}
		}

		#endregion
	}
}
