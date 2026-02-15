using System.IO;

namespace UntilWeFall
{
	public static class AnimalRegistryTxtIO
	{
		// Format: taxId|adultF|adultM|juvF|juvM|name
		public static AnimalRegistry LoadTxt(string path)
		{
			var registry = new AnimalRegistry();

			foreach (var line in File.ReadLines(path))
			{
				if (string.IsNullOrWhiteSpace(line)) continue;
				if (line.StartsWith("#")) continue;

				var parts = line.Split('|');
				if (parts.Length < 6) continue;

				int taxId = int.Parse(parts[0]);
				int adultF = int.Parse(parts[1]);
				int adultM = int.Parse(parts[2]);
				int juvF = int.Parse(parts[3]);
				int juvM = int.Parse(parts[4]);
				string name = parts[5];

				registry.Add(new AnimalDefinition
				{
					TaxID = taxId,
					Name = name,
					AdultFemale = adultF,
					AdultMale = adultM,
					JuvenileFemale = juvF,
					JuvenileMale = juvM
				});
			}

			return registry;
		}
	}
}
