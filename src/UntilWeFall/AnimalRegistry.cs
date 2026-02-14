using System;
using System.Collections.Generic;
using System.IO; // <-- add this

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

		public string Name { get; init; } = "";
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
		public static AnimalRegistry LoadFromFile(string path)
		{
			var registry = new AnimalRegistry();

			foreach (var line in File.ReadLines(path))
			{
				if (string.IsNullOrWhiteSpace(line))
				continue;

				if (line.StartsWith("#"))
				continue;

				var parts = line.Split('|');

				if (parts.Length < 6)
				continue; // or throw if you want strict mode

				int taxId = int.Parse(parts[0]);
				int adultF = int.Parse(parts[1]);
				int adultM = int.Parse(parts[2]);
				int juvF = int.Parse(parts[3]);
				int juvM = int.Parse(parts[4]);
				string name = parts[5];

				var def = new AnimalDefinition
				{
				TaxID = taxId,
				AdultFemale = adultF,
				AdultMale = adultM,
				JuvenileFemale = juvF,
				JuvenileMale = juvM,
				Name = name
				};

				registry.Add(def);
			}

			return registry;
		}
	}
}
