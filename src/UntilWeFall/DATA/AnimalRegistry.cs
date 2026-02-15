using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // <-- add this

namespace UntilWeFall
{
	public enum Sex { Male, Female }

	public enum SpeciesID
	{
		NordicWildHorse,
		Carabao
	}

	public readonly struct SexCount
	{
		public int Male { get; }
		public int Female { get; }

		public SexCount(int male, int female)
		{
			Male = male;
			Female = female;
		}

		public SexCount With(Sex sex, int value)
			=> sex == Sex.Male ? new SexCount(value, Female) : new SexCount(Male, value);
	}

	public sealed class AnimalDefinition
	{
		public int TaxID { get; init; }

		public int AdultFemale { get; set; }
		public int AdultMale { get; set; }
		public int JuvenileFemale { get; set; }
		public int JuvenileMale { get; set; }

		public string Name { get; set; } = "";
	}

	public sealed class SpeciesDef
	{
		public SpeciesID Id { get; }
		public string DisplayName { get; }
		public int Min { get; }
		public int Max { get; }

		public SpeciesDef(SpeciesID id, string displayName, int min, int max)
		{
			Id = id;
			DisplayName = displayName;
			Min = min;
			Max = max;
		}
	}

	public sealed class AnimalRegistry
	{
		private readonly Dictionary<SpeciesID, SpeciesDef> _defs = new();
		private readonly Dictionary<SpeciesID, SexCount> _counts = new();
		private readonly Dictionary<int, AnimalDefinition> _animals = new();

		public IReadOnlyDictionary<int, AnimalDefinition> Animals => _animals;

		public void Add(AnimalDefinition def)
		{
		_animals[def.TaxID] = def;
		}

		public AnimalDefinition? Get(int taxId)
		{
			_animals.TryGetValue(taxId, out var def);
			return def;
		}

		public static AnimalRegistryFile BuildFileFromRuntime(AnimalRegistry runtime)
		{
			var file = new AnimalRegistryFile();

			foreach (var kv in runtime.Animals.OrderBy(k => k.Key))
			{
				var def = kv.Value;
				file.Animals.Add(new AnimalEntry
				{
					TaxID = def.TaxID,
					Name = def.Name,
					AdultFemale = def.AdultFemale,
					AdultMale = def.AdultMale,
					JuvenileFemale = def.JuvenileFemale,
					JuvenileMale = def.JuvenileMale
				});
			}

			return file;
		}


		public bool TryGetByTaxId(int taxId, out AnimalDefinition def)
    			=> _animals.TryGetValue(taxId, out def!);


		public static void ApplyFileToRuntime(AnimalRegistry runtime, AnimalRegistryFile file)
		{
			foreach (var entry in file.Animals)
			{
				if (runtime.TryGetByTaxId(entry.TaxID, out var def))
				{
					def.AdultFemale = entry.AdultFemale;
					def.AdultMale = entry.AdultMale;
					def.JuvenileFemale = entry.JuvenileFemale;
					def.JuvenileMale = entry.JuvenileMale;
				}
				else
				{
					runtime.Add(new AnimalDefinition {
						TaxID = entry.TaxID,
						Name = entry.Name,
						AdultFemale = entry.AdultFemale,
						AdultMale = entry.AdultMale,
						JuvenileFemale = entry.JuvenileFemale,
						JuvenileMale = entry.JuvenileMale
					});
				}
			}
		}




		// Optional: UI / debug hooks.
		public event Action<SpeciesID, Sex, int>? OnValueChanged;

		public void Register(SpeciesDef def, SexCount initial)
		{
			_defs[def.Id] = def;

			int m = Math.Clamp(initial.Male, def.Min, def.Max);
			int f = Math.Clamp(initial.Female, def.Min, def.Max);

			_counts[def.Id] = new SexCount(m, f);
		}

		public bool IsRegistered(SpeciesID id) => _defs.ContainsKey(id);

		public SpeciesDef GetDef(SpeciesID id)
			=> _defs.TryGetValue(id, out var def)
			? def
			: throw new InvalidOperationException($"Species not registered: {id}");

		public SexCount GetCounts(SpeciesID id)
			=> _counts.TryGetValue(id, out var c) ? c : default;

		public int Get(SpeciesID id, Sex sex)
		{
			var c = GetCounts(id);
			return sex == Sex.Male ? c.Male : c.Female;
		}

		public void Set(SpeciesID id, Sex sex, int value)
		{
			var def = GetDef(id);
			value = Math.Clamp(value, def.Min, def.Max);

			var before = GetCounts(id);
			int current = sex == Sex.Male ? before.Male : before.Female;

			if (current == value) return;

			var after = before.With(sex, value);
			_counts[id] = after;

			OnValueChanged?.Invoke(id, sex, value);
		}

		public void ApplyDelta(SpeciesID id, Sex sex, int delta)
		{
			int cur = Get(id, sex);
			Set(id, sex, cur + delta);
		}
	}

	public static class AnimalRegistryLoader
	{
		public static AnimalRegistry Loader(AnimalRegistryFile file)
		{
			var r = new AnimalRegistry();

			foreach (var entry in file.Animals)
			{
				r.Add(new AnimalDefinition
				{
					TaxID = entry.TaxID,
					Name = entry.Name,
					AdultFemale = entry.AdultFemale,
					AdultMale = entry.AdultMale,
					JuvenileFemale = entry.JuvenileFemale,
					JuvenileMale = entry.JuvenileMale
				});
			}

			return r;
		}

	}
}
