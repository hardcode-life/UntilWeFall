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
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		Dictionary<string, SpriteFont> fonts = new Dictionary<string, SpriteFont>();
		private Texture2D mainLogo;
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
			#endregion CAMERA 2D

		#region SEED INPUT
			private string _seedInput = "";
			private int _earthSeed;
			private int _skySeed;
			private KeyboardState _kbPrev;
			#endregion SEED INPUT
		
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

			fonts["8"] = Content.Load<SpriteFont>("font/rs_12");
			mainLogo = Content.Load<Texture2D>("sprites/main_logo");

			_mapPreview.SetPreview(new Vector2(
				(GraphicsDevice.Viewport.Width / 2) + 
				((mainLogo.Width / 3) / 2) + 72,
				80
			));
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

			// Enter = commit seed
			if (kb.IsKeyDown(Keys.Enter) && !_kbPrev.IsKeyDown(Keys.Enter))
			{
				SeedGenerator.Derive(_seedInput, out _earthSeed, out _skySeed);
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
			}

			_kbPrev = kb;

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Hex("#070707"));

			// TODO: Add your drawing code here
			
			// Main logo
			_spriteBatch.Begin();

			_spriteBatch.Draw( // LOGO
				mainLogo,
				new Rectangle(
					(GraphicsDevice.Viewport.Width / 2) - ((mainLogo.Width / 3) / 2), 
					16, 
					mainLogo.Width / 3, 
					mainLogo.Height / 3), 
				Hex("#a0a0a0") * 0.5f);

		#region Draw UI
		// for drawing GUI
			_spriteBatch.DrawString(fonts["8"], $"Seed input: {_seedInput}", 
				new Vector2(
					(GraphicsDevice.Viewport.Width / 2) + ((mainLogo.Width / 3) / 2), 
					20), 
				Color.White);
			_spriteBatch.DrawString(fonts["8"], $"{_earthSeed}" + " + ", 
				new Vector2(
					(GraphicsDevice.Viewport.Width / 2) + ((mainLogo.Width / 3) / 2), 
					50), 
				Color.White * 0.25f);
			_spriteBatch.DrawString(fonts["8"], $"{_skySeed}", 
				new Vector2(
					(GraphicsDevice.Viewport.Width / 2) + ((mainLogo.Width / 3) / 2) + fonts["8"].MeasureString(_earthSeed.ToString() + " + ").X, 
					50), 
				Color.White * 0.25f);
		#endregion <-----DRAW UI---<<<-
			_spriteBatch.End();

			_spriteBatch.Begin(transformMatrix: _viewMatrix, samplerState: SamplerState.PointClamp);
		#region Draw WORLD
		// for drawing in-world elements
			int tileSize = 16; // or whatever
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
		#endregion <-----DRAW WORLD---<<<-
			_spriteBatch.End();

		#region MAP PREVIEW
			_mapPreview.Draw(_spriteBatch, fonts["8"]);
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
		*/
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


		private float IslandMask(int x, int y, int w, int h)
		{
			// distance to nearest edge in tiles
			int distLeft = x;
			int distRight = (w - 1) - x;
			int distTop = y;
			int distBottom = (h - 1) - y;

			int distToEdge = Math.Min(Math.Min(distLeft, distRight), Math.Min(distTop, distBottom));

			// how wide the coastal falloff zone is (in tiles)
			float coast = 30f;
			/*
			30f = thin coasts
			45f = balanced
			60 = bigger beachs/ lowlands
			*/

			float t = MathHelper.Clamp(distToEdge / coast, 0f, 1f);
						
			t = t * t * (3f - 2f * t); // smoothstep for nicer curve
			t = MathF.Pow(t, 0.7f); // bias toward land

			return t; // 0 at edges -> 1 deeper inland
		}

		private float ComputeLandRatio(int[,] digits, int w, int h)
		{
			int land = 0;
			int total = w * h;

			for (int y = 0; y < h; y++)
			for (int x = 0; x < w; x++)
			{
				// Match your PreviewMap logic!
				// You said <=2 is water-ish in your latest preview.
				// If you want "land" to include coast, treat digit >= 3 as land.
				if (digits[x, y] >= 3) land++;
			}

			return land / (float)total;
		}
	}
}
