using System;
using System.IO;
using System.Text.Json;

using UntilWeFall;

public static class RegistryIO
{
	public static AnimalRegistryFile Load(string path)
	{
		if (!File.Exists(path))
		{
			return new AnimalRegistryFile();
		}

		string json = File.ReadAllText(path);
		var data = JsonSerializer.Deserialize<AnimalRegistryFile>(
			json, 
			JsonUtil.Options);

		return data ?? new AnimalRegistryFile();
	}

	public static void Save(string path, AnimalRegistryFile data)
	{
		Directory.CreateDirectory(Path.GetDirectoryName(path)!);

		string tmp = path + ".tmp";
		string json = JsonSerializer.Serialize(data, JsonUtil.Options);

		File.WriteAllText(tmp, json);

		if (File.Exists(path))
		{
			try
			{
				File.Replace(
					tmp, 
					path, 
					path + ".bak", 
					ignoreMetadataErrors: true);

				return;
			}
			catch
			{
				// fallback
			}
		}

		File.Copy(tmp, path, overwrite: true);
		File.Delete(tmp);
	}
}