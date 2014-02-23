using System.IO;
using GameFormatReader.Common;

// TODO: Finish this up.

namespace GameFormatReader.GCWii.Discs.Wii
{
	/// <summary>
	/// Represents a Wii <see cref="Disc"/>.
	/// </summary>
	public sealed class DiscWii : Disc
	{
		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="filepath">Path to the Wii <see cref="Disc"/>.</param>
		public DiscWii(string filepath) : base(filepath)
		{
			using (EndianBinaryReader reader = new EndianBinaryReader(File.OpenRead(filepath), Endian.BigEndian))
			{
				Header = new DiscHeaderWii(reader);
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Wii <see cref="DiscHeader"/>.
		/// </summary>
		public override DiscHeader Header
		{
			get;
			protected set;
		}

		#endregion
	}
}
