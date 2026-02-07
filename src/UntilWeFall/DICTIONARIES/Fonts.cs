using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace UntilWeFall
{
	public static class Fonts
	{
		private static readonly Dictionary<string, SpriteFont> fonts = new();

		public static void Load(ContentManager Content)
		{
			fonts["12"] = Content.Load<SpriteFont>("font/rs_12");
			fonts["16"] = Content.Load<SpriteFont>("font/rs_16");
			fonts["24"] = Content.Load<SpriteFont>("font/rs_24");
			fonts["32"] = Content.Load<SpriteFont>("font/rs_32");
			
			fonts["ex"] = Content.Load<SpriteFont>("font/rs_ex");
		}

		public static SpriteFont Get (string key)
		{
			if(!fonts.TryGetValue(key, out var font))
			{
				throw new KeyNotFoundException($"Font '{key}' not found!");
			}
			return font;
		}
	}
}