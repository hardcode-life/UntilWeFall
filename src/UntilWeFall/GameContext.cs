using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using UntilWeFall;


public sealed class GameContext
{
	public Game Game { get; }
	public GraphicsDevice GraphicsDevice { get; }
	public ContentManager Content { get; }
	public SpriteBatch SpriteBatch { get; }
	public Texture2D pixel { get; }
	public MapPreview? LastPreview { get; set;}

	public int EarthSeed {get; set;}
	public int SkySeed {get; set; }

	public int WorldWidth { get; set;}
	public int WorldHeight {get; set;}

	public WorldData worldData {get; set;}

	public GameContext(Game game, GraphicsDevice gd, ContentManager content, SpriteBatch sb)
	{	
		Game = game;
		GraphicsDevice = gd;
		Content = content;
		SpriteBatch = sb;
		
		pixel = new Texture2D(GraphicsDevice, 1, 1);
		pixel.SetData(new[] { Color.White });
	}
}
