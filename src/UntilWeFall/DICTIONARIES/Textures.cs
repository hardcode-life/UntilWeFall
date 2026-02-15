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
			textures["bg"] = Content.Load<Texture2D>("sprites/bg_64x64");

			textures["traits_tab"] = Content.Load<Texture2D>("sprites/tab1"); // TRAITS
			textures["traits_tab_img1"] = Content.Load<Texture2D>("sprites/TRAIT_tab_img1_40x49");
			textures["traits_tab_img2"] = Content.Load<Texture2D>("sprites/TRAIT_tab_img2_19x29");

			textures["census_tab"] = Content.Load<Texture2D>("sprites/tab2");  // POPULAYIONS
			textures["commit_to_AnimalRegistry"] = Content.Load<Texture2D>("sprites/commit to Animal Registry BTN 415x55");

			textures["bloodline_tab"] = Content.Load<Texture2D>("sprites/tab3"); // BLOODLINE

			textures["female"] = Content.Load<Texture2D>("sprites/female"); // 32x32
			textures["male"] = Content.Load<Texture2D>("sprites/male"); // 32x32

			textures["arrow LEFT"] = Content.Load<Texture2D>("sprites/arrow_LEFT");
			textures["arrow RIGHT"] = Content.Load<Texture2D>("sprites/arrow_RIGHT");
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