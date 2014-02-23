using System;

namespace GameFormatReader.GCWii.Discs
{
	/// <summary>
	/// Represents a GameCube or Wii disc.
	/// </summary>
	public abstract class Disc
	{
		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="filepath">Path to a disc.</param>
		protected Disc(string filepath)
		{
			if (filepath == null)
				throw new ArgumentException("filepath cannot be null", "filepath");
		}

		#endregion

		#region Properties

		/// <summary>
		/// The <see cref="DiscHeader"/> for this disc.
		/// </summary>
		public abstract DiscHeader Header
		{
			get;
			protected set;
		}

		#endregion
	}
}
