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
		private enum GameState
		{
			StartMenu,
			Creation,
			Loading,
			Playing
		}
		GameState currentGameState = GameState.Creation;
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

			private Matrix _viewMatrix;
			private Vector2 mouseWorld;
		#endregion <--CAMERA 2D------<<<-

		private string _string = "";

		#region SEED INPUT
			private string _seedInput = "";
			private string prevSeed ="default";
			private int _earthSeed;
			private int _skySeed;
			private KeyboardState _kbPrev;
			private bool _mapAccepted = false;
			private bool _hasPreview = false;
		#endregion <--SEED INPUT------<<<-

		#region World Generation - customization
			private string _worldName = "world name";
			private Rectangle worldName_Input_bounds;
			private InputField worldName_Input;
		#endregion
		
		private MapPreview _mapPreview = new MapPreview();
		private Texture2D _pixel; // temporary

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

			Window.TextInput += OnTextInput;
			Window.Position = Point.Zero; // FORCE TOP-LEFT

			base.Initialize();
		}

		private void OnTextInput(object sender, TextInputEventArgs e)
		{
			char c = e.Character;

			// ignore control characters
			if (char.IsControl(c)) { return; }

			if (char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_')
			{
				_seedInput += c;
			}
		}

		protected override void LoadContent()
		{
			// TODO: use this.Content to load your game content here
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			_pixel = new Texture2D(GraphicsDevice, 1, 1);
			_pixel.SetData(new[] { Color.White });

			Fonts.Load(Content); // initialize FONT class
			Textures.Load(Content); // initialize TEXTURES class

			mainLogo = Textures.Get("mainLogo");
			testBackground =Content.Load<Texture2D>("sprites/2560x1440 test");
			inputBG = Content.Load<Texture2D>("sprites/stoneBlock");

			#region MAP PREVIEW ORIGIN
				_mapPreview.SetPreview(new Vector2(
					GraphicsDevice.Viewport.Width - (12 * 64) - 12,
					80
				)); // set preview ORIGIN.
			#endregion
			
			worldName_Input_bounds = new Rectangle(0, 0, 500, 500);
			worldName_Input = new InputField(
				worldName_Input_bounds,
				"this is a test",
				Fonts.Get("16"),
				() => worldName_Input.inputString = "hello",
				inputBG);
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

			if(currentGameState == GameState.Playing) {
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
			}

			_mousePrev = mouse;

			_viewMatrix = _camera.GetViewMatrix();
			mouseWorld = _camera.ScreenToWorld(new Vector2(mouse.X, mouse.Y));

			// Backspace
			if (kb.IsKeyDown(Keys.Back) && !_kbPrev.IsKeyDown(Keys.Back))
			{
				if (_seedInput.Length > 0) {
					_seedInput = _seedInput[..^1];
				}
			}

			#region Commit Seed
			// Enter = commit seed / accept map
			if (kb.IsKeyDown(Keys.Enter) && !_kbPrev.IsKeyDown(Keys.Enter))
			{
				// Decide what "empty" means (pick one)
				string effectiveSeed = string.IsNullOrWhiteSpace(_seedInput) ? "default" : _seedInput;
				bool seedChanged = prevSeed != effectiveSeed;

				if (seedChanged) {
					// if seed is empty or is not the same as the last seed....
					SeedGenerator.Derive(effectiveSeed, out _earthSeed, out _skySeed);

					_mapPreview.Regenerate(
						earthSeed: _earthSeed,
						skySeed: _skySeed,
						worldW: 512,
						worldH: 512,
						minLandRatio: 0.45f,
						maxAttempts: 12,
						coast: 30f,
						landBiasPow: 0.7f
					);
					_hasPreview = true;
					_mapAccepted = false;

					prevSeed = effectiveSeed;
				} else {
					// ...else, ACCEPT MAP
					if (_hasPreview) {
						SimplexNoise.GenerateNoiseMap(
							512, 512,
							_earthSeed,
							200f,
							11,
							0.6f,
							2f,
							0,
							0
						);
						_mapAccepted = true;
					}

					// TODO: move world gen to loading state + Task.Run
				}
			}
			_kbPrev = kb;
			#endregion
			
			worldName_Input.Update(mouse);

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

				_spriteBatch.Draw( // SEED INPUT
					inputBG,
					new Rectangle(
						GraphicsDevice.Viewport.Width - (1212), 
						20, 
						1200, 
						24), 
					Hex.convert("#ffffff"));

			#region Draw SEED INPUT
				// for drawing GUI
				_spriteBatch.DrawString(
					Fonts.Get("12"), 
					$"Seed : {_seedInput}", 
					new Vector2(
						(GraphicsDevice.Viewport.Width / 2) + 80, 
						24), 
					Hex.convert("#222222"));

				_spriteBatch.DrawString(
					Fonts.Get("12"), 
					$"{_earthSeed}" + " + " + $"{_skySeed}", 
					new Vector2(
						(GraphicsDevice.Viewport.Width / 2) + 128, 
						56), 
					Color.White * 0.25f);

					/*
				_spriteBatch.DrawString(Fonts.Get("12"), $"{_skySeed}", 
					new Vector2(
						(GraphicsDevice.Viewport.Width / 2) + ((mainLogo.Width / 3) / 2) + Fonts.Get("12").MeasureString(_earthSeed.ToString() + " + ").X + 80, 
						56), 
					Color.White * 0.25f);
					*/
			#endregion <-----DRAW SEED INPUT---<<<-

			#region Map Type
			worldName_Input.Draw(_spriteBatch);

			if (_mapAccepted)
			{
				_spriteBatch.DrawString(
					Fonts.Get("16"),
					"MAP ACCEPTED",
					new Vector2(
						(GraphicsDevice.Viewport.Width / 2) + 128,
						GraphicsDevice.Viewport.Height / 2),
					Color.White * .5f);
			}
			#endregion
			_spriteBatch.End();
			
			#region Draw CURSOR
			_spriteBatch.Begin(transformMatrix: _viewMatrix, samplerState: SamplerState.PointClamp);
				// snaps the cursor to tile position...
				int tileSize = 16; // ..or whatever
				Vector2 snapped = new Vector2(
					(int)(mouseWorld.X / tileSize) * tileSize,
					(int)(mouseWorld.Y / tileSize) * tileSize
				);

				_spriteBatch.Draw(
					_pixel, 
					new Rectangle((int)snapped.X, (int)snapped.Y, tileSize, tileSize), 
					Color.Yellow * 0.35f
				);

				DrawLine(snapped, snapped + new Vector2(tileSize, 0), Color.Yellow, 2);
				DrawLine(snapped, snapped + new Vector2(0, tileSize), Color.Yellow, 2);
			_spriteBatch.End();
			#endregion <-----Draw CURSOR---<<<-

			#region MAP PREVIEW
			_mapPreview.Draw(_spriteBatch, Fonts.Get("12"));
			#endregion <-----MAP PREVIEW---<<<-

			base.Draw(gameTime);
		}

		private void DrawLine(Vector2 start, Vector2 end, Color color, int thickness)
		{
			Vector2 edge = end - start;
			float angle = (float)System.Math.Atan2(edge.Y, edge.X);

			_spriteBatch.Draw(
				_pixel,
				new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), thickness),
				null,
				color,
				angle,
				Vector2.Zero,
				SpriteEffects.None,
				0
			);
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
