using System.Collections.Generic;

namespace UntilWeFall
{
	public sealed class AnimalRegistryFile
	{
		public int Version { get; set; } = 1;

		// Key = TaxID
    		public List<AnimalEntry> Animals { get; set; } = new();
	}

	public sealed class AnimalEntry
	{
		public int TaxID { get; set; }
		public string Name { get; set; } = "";

		public int AdultFemale { get; set; }
		public int AdultMale { get; set; }
		public int JuvenileFemale { get; set; }
		public int JuvenileMale { get; set; }

		// optional, for future expansion
		public int Min { get; set; } = 0;
		public int Max { get; set; } = 999999;
	}
}
