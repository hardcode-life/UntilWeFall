using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace UntilWeFall
{
	public static class AnimalRegistryJsonIO
	{
		public static AnimalRegistryFile LoadFile(string jsonPath)
		{
			if (!File.Exists(jsonPath))
				return new AnimalRegistryFile();

			try
			{
				string json = File.ReadAllText(jsonPath);
				var file = JsonSerializer.Deserialize<AnimalRegistryFile>(json, JsonUtil.Options);
				return file ?? new AnimalRegistryFile();
			}
			catch
			{
				// Fallback: if JSON is corrupted, try .bak
				string bak = jsonPath + ".bak";
				if (File.Exists(bak))
				{
					string json = File.ReadAllText(bak);
					var file = JsonSerializer.Deserialize<AnimalRegistryFile>(json, JsonUtil.Options);
					return file ?? new AnimalRegistryFile();
				}

				// If both fail, return empty (or throw if you prefer strict)
				return new AnimalRegistryFile();
			}
		}

		public static void SaveFileAtomic(string jsonPath, AnimalRegistryFile data)
		{
			string? dir = Path.GetDirectoryName(jsonPath);
			if (!string.IsNullOrEmpty(dir))
				Directory.CreateDirectory(dir);

			string tmp = jsonPath + ".tmp";
			string bak = jsonPath + ".bak";

			string json = JsonSerializer.Serialize(data, JsonUtil.Options);
			File.WriteAllText(tmp, json);

			// If existing file, back it up first
			if (File.Exists(jsonPath))
			{
				try
				{
					// On Windows: atomic replace with backup
					File.Replace(tmp, jsonPath, bak, ignoreMetadataErrors: true);
					return;
				}
				catch
				{
					// Cross-platform fallback
					try
					{
						File.Copy(jsonPath, bak, overwrite: true);
					}
					catch { /* ignore */ }

					File.Copy(tmp, jsonPath, overwrite: true);
					File.Delete(tmp);
					return;
				}
			}

			// No existing file
			File.Copy(tmp, jsonPath, overwrite: true);
			File.Delete(tmp);
		}
	}

	public static class AnimalRegistrySanitizer
	{
		public static AnimalRegistryFile Sanitize(AnimalRegistryFile file)
		{
			if (file.Animals == null)
				file.Animals = new();

			var seen = new HashSet<int>();
			var cleaned = new List<AnimalEntry>();

			foreach (var a in file.Animals)
			{
				if (a == null) continue;

				// Basic required fields
				if (a.TaxID <= 0) continue; // skip invalid IDs
				if (string.IsNullOrWhiteSpace(a.Name))
				a.Name = "Unnamed";

				// Clamp ranges
				if (a.Min < 0) a.Min = 0;
				if (a.Max < a.Min) a.Max = a.Min;

				a.AdultFemale    = Clamp(a.AdultFemale, a.Min, a.Max);
				a.AdultMale      = Clamp(a.AdultMale, a.Min, a.Max);
				a.JuvenileFemale = Clamp(a.JuvenileFemale, a.Min, a.Max);
				a.JuvenileMale   = Clamp(a.JuvenileMale, a.Min, a.Max);

				// Handle duplicates: last one wins (replace previous)
				if (seen.Contains(a.TaxID))
				{
				for (int i = cleaned.Count - 1; i >= 0; i--)
				{
				if (cleaned[i].TaxID == a.TaxID)
				{
					cleaned[i] = a;
					break;
				}
				}
				continue;
				}

				seen.Add(a.TaxID);
				cleaned.Add(a);
			}

			file.Animals = cleaned;
			return file;

			static int Clamp(int v, int min, int max)
				=> v < min ? min : (v > max ? max : v);
		}
	}
}
