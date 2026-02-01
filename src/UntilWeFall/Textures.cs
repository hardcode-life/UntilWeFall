using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace UntilWeFall
{
	public static class Textures
	{
		private static readonly Dictionary<string, Texture2D> textures = new();

		public static void Load(ContentManager Content)
		{
			textures["mainLogo"] = Content.Load<Texture2D>("sprites/main_logo");
		}

		public static Texture2D Get (string key)
		{
			if(!textures.TryGetValue(key, out var tex))
			{
				throw new KeyNotFoundException($"Texture '{key}' not found!");
			}
			return tex;
		}
	}
}