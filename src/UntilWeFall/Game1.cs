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
			private bool _seeded = false;	
			#endregion SEED INPUT

		#region WORLD PREVIEW
			private const int PreviewW = 64; // width
			private const int PreviewH = 64; // height
			private Point _spawnTile = new Point(0, 0);
			private int[,] _previewDigits = new int[PreviewW, PreviewH];
			private string _previewCorner = "";
			private Point _previewStart = Point.Zero;
			private Random _rng;
			#endregion WORLD PREVIEW

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
				RegenerateSpawnPreview();
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
			PreviewMap(_spriteBatch);
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

		private static int HashSeeds(int a, int b)
		{
			// takes two integers (a, b) and mixes them into a new integer that's nice and random.
			unchecked
			{
				int h = 17;
				h = h * 31 + a;
				h = h * 31 + b;
				return h;
			}
		}


		private void PreviewMap(SpriteBatch sb)
		{
			sb.Begin(samplerState: SamplerState.PointClamp);
			
			// space between characters
			int cellW = 14;
			int cellH = 18;
			
			// preview map position
			Vector2 origin = new Vector2(
				(GraphicsDevice.Viewport.Width / 2) + ((mainLogo.Width / 3) / 2) + 72, 
				80);

			for (int y = 0; y < PreviewH; y++)
			{
				for (int x = 0; x < PreviewW; x++)
				{
					int digit = _previewDigits[x, y];

					Color textColor;
					string _text;
					if (_seeded) {
						if (digit <= 1) // sea
						{
							textColor = Color.Blue * 0.75f;
							
							int hash = (x * 73856093) ^ (y * 19349663) ^ _earthSeed; 
								//73856093 and 19349663 are just large prime numbers
							int n = Math.Abs(hash) % 4;

							_text = n switch
							{
								0 => ".",
								1 => ",",
								2 => "'",
								_ => "+"
							};
						}
						else if (digit == 2) // reef
						{
							textColor = Color.SkyBlue * 0.75f;
							_text = "%";
						}
						else if (digit == 3) // coastline
						{
							textColor = Color.SandyBrown;
							_text = "$";
						}
						else // inland
						{
							textColor = Color.DarkGreen;
							_text = "#";
						}
					}
					else
					{
						textColor = Color.DarkGray;
						_text = "#";
					}

					float shade = MathHelper.Clamp(
						0.25f + (_previewDigits[x, y] * 0.07f),
						0.25f,
						1f
					);
					sb.DrawString(
						fonts["8"],
						_text,
						origin + new Vector2(x * cellW, y * cellH),
						textColor * shade
					);
				}
			}

			if (_seeded)
			{
				int sx = _spawnTile.X - _previewStart.X;
				int sy = _spawnTile.Y - _previewStart.Y;

				if (sx >= 0 && sx < PreviewW && sy >= 0 && sy < PreviewH)
				{
					sb.DrawString(
						fonts["8"],
						"@",
						origin + new Vector2(sx * cellW, sy * cellH),
						Color.Yellow
					);
				}
			}


		
			sb.DrawString(fonts["8"], $"Preview: {_previewCorner}", 
				new Vector2(
					(GraphicsDevice.Viewport.Width / 2) + ((mainLogo.Width / 3) / 2), 
					80), 
				Color.White);

			sb.End();
		}

		private void RegenerateSpawnPreview()
		{
			_seeded = true;

			int w = PreviewW;
			int h = PreviewH;

			int previewW = w;
			int previewH = h;

			// Finite world dimensions (even if you don't fully generate it yet)
			int worldW = 512;
			int worldH = 512;

			_rng ??= new Random(HashSeeds(_earthSeed, _skySeed)); // checks null with "??=" . So that when _rng turns null, it'll just assign the HashSeed anyway

			const float minLand =0.45f; // 45%
			const int maxAttempts = 12;

			for (int attempt = 0; attempt < maxAttempts; attempt++)
			{
				Point start = PickEdgeWindowStart(worldW, worldH, previewW, previewH);

				float[,] raw = SimplexNoise.GenerateNoiseMap(
					previewW, previewH,
					_earthSeed,
					200f, 5, 0.6f, 2f,
					start.X, start.Y);

				float[,] smooth = SimplexNoise.SmoothNoiseMap(raw, previewW, previewH, kernelSize: 3);

				// Fill _previewDigits from this candidate
				for (int y = 0; y < previewH; y++)
				for (int x = 0; x < previewW; x++)
				{
					int wx = start.X + x;
					int wy = start.Y + y;

					float n01 = SimplexNoise.SmoothStep(0f, 1f, smooth[x, y]);
					float mask = IslandMask(wx, wy, worldW, worldH);
					n01 = MathHelper.Clamp(n01 * mask, 0f, 1f);

					int digit = (int)(n01 * 9.999f);
					digit = Math.Clamp(digit, 0, 9);
					_previewDigits[x, y] = digit;
				}

				float landRatio = ComputeLandRatio(_previewDigits, previewW, previewH);

				if (landRatio >= minLand)
				{
					_previewStart = start;

					//spawn on land rather than in the middle of the ocean, lol
					_spawnTile = PickSpawnInsideWindow(start, _previewDigits, previewW, previewH);

					return;
				}
			}
		}

		private Point PickSpawnInsideWindow(Point start, int[,] digits, int w, int h)
		{
			// Prefer inland first, then coast if needed
			List<Point> inland = new();
			List<Point> coast = new();

			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					int d = digits[x, y];

					if (d >= 4) {
						inland.Add(new Point(start.X + x, start.Y + y));
					}
					else if (d == 3) {
						coast.Add(new Point(start.X + x, start.Y + y));
					}
				}
			}

			if (inland.Count > 0) {
				return inland[_rng.Next(inland.Count)];
			}
			if (coast.Count > 0) {
				return coast[_rng.Next(coast.Count)];
			}

			// Worst-case fallback: center of window
			return new Point(start.X + w / 2, start.Y + h / 2);
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

		private Point PickEdgeWindowStart(int worldW, int worldH, int previewW, int previewH)
		{
			int maxX = worldW - previewW;
			int maxY = worldH - previewH;

			int choice = _rng.Next(8); // 0..7

			switch (choice)
			{
				// Corners
				case 0: 
					_previewCorner = "NW"; 
					return new Point(0, 0);
				case 1: 
					_previewCorner = "NE"; 
					return new Point(maxX, 0);
				case 2: 
					_previewCorner = "SW"; 
					return new Point(0, maxY);
				case 3: 
					_previewCorner = "SE"; 
					return new Point(maxX, maxY);

				// Edges (random slide)
				case 4: 
					_previewCorner = "N";  
					return new Point(_rng.Next(0, maxX + 1), 0);
				case 5: 
					_previewCorner = "S";  
					return new Point(_rng.Next(0, maxX + 1), maxY);
				case 6: 
					_previewCorner = "W";  
					return new Point(0, _rng.Next(0, maxY + 1));
				default:
					_previewCorner = "E";  
					return new Point(maxX, _rng.Next(0, maxY + 1));
			}
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
