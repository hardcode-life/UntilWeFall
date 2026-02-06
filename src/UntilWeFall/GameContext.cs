using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;


public sealed class GameContext
{
	public Game Game { get; }
	public GraphicsDevice GraphicsDevice { get; }
	public ContentManager Content { get; }
	public SpriteBatch SpriteBatch { get; }
	public Texture2D pixel { get; }

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
