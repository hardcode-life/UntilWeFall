using System.IO;

namespace UntilWeFall
{
	public static class AnimalRegistryMigration
	{
		public static AnimalRegistry LoadOrCreate(string jsonPath, string txtPath)
		{
			// 1) JSON exists => load JSON => sanitize => build runtime
			if (File.Exists(jsonPath))
			{
				var file = AnimalRegistryJsonIO.LoadFile(jsonPath);
				file = AnimalRegistrySanitizer.Sanitize(file);

				// Optional: write back repaired JSON so the player sees normalized results
				AnimalRegistryJsonIO.SaveFileAtomic(jsonPath, file);

				return AnimalRegistryLoader.Loader(file);
			}

			// 2) Legacy TXT exists => load TXT => convert => save JSON
			if (File.Exists(txtPath))
			{
				var runtime = AnimalRegistryTxtIO.LoadTxt(txtPath);

				var outFile = AnimalRegistry.BuildFileFromRuntime(runtime);
				AnimalRegistryJsonIO.SaveFileAtomic(jsonPath, outFile);

				// Optional: archive old txt
				try { File.Move(txtPath, txtPath + ".bak", overwrite: true); } catch { }

				return runtime;
			}

			// 3) Nothing exists => create a starter file
			var starter = AnimalRegistryDefaults.CreateStarterFile();
			AnimalRegistryJsonIO.SaveFileAtomic(jsonPath, starter);
			return AnimalRegistryLoader.Loader(starter);
		}
	}
}
