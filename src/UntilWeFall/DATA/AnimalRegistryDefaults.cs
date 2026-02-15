namespace UntilWeFall
{
	public static class AnimalRegistryDefaults
	{
		public static AnimalRegistryFile CreateStarterFile()
		{
			return new AnimalRegistryFile
			{
				Version = 1,
				Animals =
				{
					new AnimalEntry { TaxID = 9760, Name="Nordic Wild Horse", AdultFemale=2, AdultMale=1, JuvenileFemale=1, JuvenileMale=0 },
					new AnimalEntry { TaxID = 9902, Name="Boats", AdultFemale=0, AdultMale=0, JuvenileFemale=0, JuvenileMale=7 },
				}
			};
		}
	}
}
