using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
//using System.Globalization;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

namespace UntilWeFall
{
	public class Game1 : Game
	{
		private IGameState _state;
		private GameContext _ctx;
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		private Texture2D mainLogo, testBackground, inputBG;
		private int screenWidth;
		private int screenHeight;

/// --------------------------------------------
///-----------///     WARNING! HEATHENS AHEAD!!!     ///
/// --------------------------------------------

#region CAMERA 2D
		private Camera2D _camera;
		private MouseState _mousePrev;
		private float _panSpeed = 800f;
		private float _zoomStep = 0.10f;

		//public Matrix _viewMatrix;
		//private Vector2 mouseWorld;
#endregion <--CAMERA 2D------<<<-

		public Game1()
		{
			_graphics = new GraphicsDeviceManager(this);

			Content.RootDirectory = "Content";
			IsMouseVisible = true;
		}

		protected override void Initialize()
		{
			// TODO: Add your initialization logic here
			_graphics.IsFullScreen = false; // FALSE for windowed...
			_graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            		_graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            		_graphics.SynchronizeWithVerticalRetrace = true;

			screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
			screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

			_graphics.HardwareModeSwitch = false;

            		_graphics.ApplyChanges();
			
			Window.IsBorderless = true;

			_camera = new Camera2D(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

			//Window.TextInput += OnTextInput;
			Window.Position = Point.Zero; // FORCE TOP-LEFT

			base.Initialize();
		}

		protected override void LoadContent()
		{
			// TODO: use this.Content to load your game content here
			_spriteBatch = new SpriteBatch(GraphicsDevice);
			
			_ctx = new GameContext(this, GraphicsDevice, Content, _spriteBatch);

			Fonts.Load(Content); // initialize FONT class
			Textures.Load(Content); // initialize TEXTURES class

			mainLogo = Textures.Get("mainLogo");
			testBackground =Content.Load<Texture2D>("sprites/2560x1440 test");
			inputBG = Content.Load<Texture2D>("sprites/stoneBlock");

			ChangeState(GameStateID.Creation);
		}

		
		private void ChangeState(GameStateID id)
		{
			_state?.Exit();

			_state = id switch
			{
				GameStateID.Creation => new Creation(_ctx, ChangeState),
				GameStateID.StartMenu => new StartMenu(_ctx, ChangeState),
				GameStateID.Loading => new Loading(_ctx, ChangeState),
				GameStateID.Playing => new Playing(_ctx, ChangeState),
				_ => throw new Exception("Unknown state")
			};

			_state.Enter();
		}

		protected override void Update(GameTime gameTime)
		{
			if (Keyboard.GetState().IsKeyDown(Keys.Escape)) {
				Exit();
			}

			// TODO: Add your update logic here
			float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

			var kb = Keyboard.GetState();
			var mouse = Mouse.GetState();

				int w = GraphicsDevice.Viewport.Width;
				int h = GraphicsDevice.Viewport.Height;
				if (w != _camera.ViewportWidth || h != _camera.ViewportHeight)
				{
					_camera.SetViewportSize(w, h);
				}

				Vector2 move = Vector2.Zero;
				// camera PAN
				if (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up))
					move.Y -= 1;

				if (kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down))
					move.Y += 1;

				if (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left))
					move.X -= 1;

				if (kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right))
					move.X += 1;
				
				if (move != Vector2.Zero) {
					move.Normalize();
					float speed = _panSpeed / _camera.Zoom; // camera movement slows when zoomed in, relative to how close you're zoomed in.
					_camera.Pan( move * speed * dt);
				}

				int wheelDelta = mouse.ScrollWheelValue - _mousePrev.ScrollWheelValue;
				if (wheelDelta != 0)
				{
					int notches = wheelDelta / 120; // typically ±1
					float zoomFactor = 1f;

					if (notches > 0) {
						for (int i = 0; i < notches; i++) {
							zoomFactor *= (1f + _zoomStep);
						}
					}
					else {
						for (int i = 0; i < -notches; i++) {
							zoomFactor *= (1f - _zoomStep);
						}
					}

					_camera.ZoomAtScreenPoint(
						new Vector2(mouse.X, mouse.Y), 
						zoomFactor);
				}
				
			_state.Update(gameTime);
			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Hex.convert("#242f36")); // 242f36

			// TODO: Add your drawing code here

				_spriteBatch.Begin();
					/*_spriteBatch.Draw( // test background
						testBackground,
						new Rectangle(0, 0, 2560, 1440), 
						Color.White * 0.1f);*/

					_spriteBatch.Draw( // LOGO
						mainLogo,
						new Rectangle(
							(GraphicsDevice.Viewport.Width / 2) - (mainLogo.Width / 2), 
							64, 
							mainLogo.Width / 2, 
							mainLogo.Height / 2), 
						Hex.convert("#ffffff") * 0.08f);
					_spriteBatch.DrawString(
						Fonts.Get("24"), 
						"UNTIL\nWE\nFALL",
						new Vector2(
							(GraphicsDevice.Viewport.Width / 2) - (Fonts.Get("24").MeasureString("UNTIL\nWE\nFALL").X * 2f) - 24,
							128),
						Color.Orange * .25f);
				_spriteBatch.End();

			_state.Draw(gameTime);
			base.Draw(gameTime);
		}

		
		
/// ----------------------------------------------------
///-----------///     HERE BE THE BONES OF THE FORGOTTEN     ///
/// ----------------------------------------------------
		/*
		Color convertToRGBA(string hexString) {
			// change HEX color to RGBA
			if (hexString == null) {
				return Color.Black;
			}

			// replace # occurences
			if (hexString.IndexOf('#') != -1) {
				hexString = hexString.Replace("#", "");
			}

			int r, g, b;

			r = int.Parse(hexString.Substring(0, 2), NumberStyles.AllowHexSpecifier);
			g = int.Parse(hexString.Substring(2, 2), NumberStyles.AllowHexSpecifier);
			b = int.Parse(hexString.Substring(4, 2), NumberStyles.AllowHexSpecifier);

			return new Color(r, g, b);
		}
		static Color Hex(string hex)
		{
			if (hex.StartsWith("#")) {
				hex = hex[1..];
			}

			byte r = Convert.ToByte(hex.Substring(0, 2), 16);
			byte g = Convert.ToByte(hex.Substring(2, 2), 16);
			byte b = Convert.ToByte(hex.Substring(4, 2), 16);

			byte a = hex.Length >= 8
				? Convert.ToByte(hex.Substring(6, 2), 16)
				: (byte)255;
			
			return new Color(r, g, b, a);
		}
		*/
	}
}
