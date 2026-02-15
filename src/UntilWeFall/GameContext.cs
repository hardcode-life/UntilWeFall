using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using UntilWeFall;
using System;
using System.IO;


public sealed class GameContext
{
	public Game Game { get; }
	public GraphicsDevice GraphicsDevice { get; }
	public ContentManager Content { get; }
	public SpriteBatch SpriteBatch { get; }
	public Texture2D pixel { get; }
	public MapPreview? LastPreview { get; set; }

	public int EarthSeed { get; set; }
	public int SkySeed { get; set; }

	public int WorldWidth { get; set; }
	public int WorldHeight { get; set; }

	public WorldData worldData { get; set; }

	public string SaveDirectory { get; }
	public string AnimalRegistryJsonPath { get; }
	public string AnimalRegistryTxtPath { get; }

	public AnimalRegistry AnimalRegistry { get; private set; } = new AnimalRegistry();

	public void SetAnimalRegistry(AnimalRegistry registry)
	=> AnimalRegistry = registry ?? throw new ArgumentNullException(nameof(registry));

	public GameContext(Game game, GraphicsDevice gd, ContentManager content, SpriteBatch sb)
	{
		Game = game;
		GraphicsDevice = gd;
		Content = content;
		SpriteBatch = sb;

		pixel = new Texture2D(GraphicsDevice, 1, 1);
		pixel.SetData(new[] { Color.White });

		SaveDirectory = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"UntilWeFall"
		);
		Directory.CreateDirectory(SaveDirectory);

		AnimalRegistryJsonPath = Path.Combine(SaveDirectory, "AnimalRegistry.json");
		AnimalRegistryTxtPath  = Path.Combine(SaveDirectory, "AnimalRegistry.txt");
	}
}
