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
		private static readonly Color window_bg_Color = new Color(7, 7, 7);
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		Dictionary<string, SpriteFont> fonts = new Dictionary<string, SpriteFont>();

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

		#region WORLD PREVIEW
			private const int PreviewW = 64; // width
			private const int PreviewH = 64; // height
			private Point _spawnTile = new Point(0, 0);
			private int[,] _previewDigits = new int[PreviewW, PreviewH];
			private string _previewCorner = "";

			#endregion

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
			_graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            		_graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
            		_graphics.SynchronizeWithVerticalRetrace = true;
			_graphics.IsFullScreen = true; // FALSE for windowed...
			_graphics.HardwareModeSwitch = true;

            		_graphics.ApplyChanges();

			_camera = new Camera2D(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

			Window.TextInput += OnTextInput;

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
			GraphicsDevice.Clear(window_bg_Color);

			// TODO: Add your drawing code here
		#region Draw WORLD
			// for drawing in-world elements
			_spriteBatch.Begin(transformMatrix: _viewMatrix, samplerState: SamplerState.PointClamp);

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

			_spriteBatch.End();
			#endregion

		#region Draw UI
			// for drawing GUI
			_spriteBatch.Begin(samplerState: SamplerState.PointClamp);

			_spriteBatch.DrawString(fonts["8"], $"Seed input: {_seedInput}", new Vector2(20, 20), Color.White);
			_spriteBatch.DrawString(fonts["8"], $"EarthSeed: {_earthSeed}", new Vector2(20, 50), Color.White);
			_spriteBatch.DrawString(fonts["8"], $"SkySeed: {_skySeed}", new Vector2(20, 80), Color.White);

			_spriteBatch.End();
			#endregion

		#region MAP PREVIEW
			PreviewMap(_spriteBatch);
			#endregion

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

		private void PreviewMap(SpriteBatch sb)
		{
			sb.Begin(samplerState: SamplerState.PointClamp);
			
			// space between characters
			int cellW = 14;
			int cellH = 18;
			
			// preview map position
			Vector2 origin = new Vector2(_graphics.PreferredBackBufferWidth/2, 18);

			for (int y = 0; y < PreviewH; y++)
			{
				for (int x = 0; x < PreviewW; x++)
				{
					int digit = _previewDigits[x, y];

					Color textColor;
					string _text = "";

					if (digit <= 1)
					{
						textColor = Color.Blue;
						_text = "#";
					}
					else if (digit == 2)
					{
						textColor = Color.SkyBlue;
						_text = "%";
					}
					else if (digit == 3)
					{
						textColor = Color.SandyBrown;
						_text = "$";
					}
					else
					{
						textColor = Color.DarkGreen;
						_text = "&";
					}

					sb.DrawString(
						fonts["8"],
						_text,
						origin + new Vector2(x * cellW, y * cellH),
						textColor * (.75f - ((_previewDigits[x, y] * 0.1f) - 0.1f))
					);

				}
			}
			
			sb.DrawString(fonts["8"], $"Corner: {_previewCorner}", new Vector2(20, 110), Color.White);

			sb.End();
		}

		private void RegenerateSpawnPreview()
		{
			int w = PreviewW;
			int h = PreviewH;
			int previewW = w;
			int previewH = h;

			// Finite world dimensions (even if you don't fully generate it yet)
			int worldW = 512;
			int worldH = 512;

			// Choose which corner you're previewing
    			string corner = "NE"; // later: randomize by seed, or let player pick

			int startX, startY;

			// Pick which corner to show
			switch (corner)
			{
				case "NW": 
					startX = 0; 
					startY = 0; 
					break;
				case "NE": 
					startX = Math.Max(0, worldW - previewW); 
					startY = 0; 
					break;
				case "SW": 
					startX = 0; 
					startY = Math.Max(0, worldH - previewH); 
					break;
				case "SE": 
					startX = Math.Max(0, worldW - previewW); 
					startY = Math.Max(0, worldH - previewH); 
					break;
				default:   
					startX = 0; 
					startY = 0; 
					break;
			}
			_previewCorner = corner;

			float scale = 200f;
			int octaves = 5;
			float persistence = 0.6f;
			float lacunarity = 2f;

			//SimplexNoise.view_offset_x = _spawnTile.X;
			//SimplexNoise.view_offset_y = _spawnTile.Y;

			float[,] raw = SimplexNoise.GenerateNoiseMap(
				previewW, previewH,
				_earthSeed,
				scale,
				octaves,
				persistence,
				lacunarity,
				startX,
				startY);

			float[,] smooth = SimplexNoise.SmoothNoiseMap(raw, previewW, previewH, kernelSize: 3);

			//BuildPreviewTexture(GraphicsDevice, raw, 16, 32, "TL");

			for (int y = 0; y < previewH; y++)
			{
				for (int x = 0; x < previewW; x++)
				{
					int wx = startX + x;
					int wy = startY + y;

					float n01 = smooth[x, y];
					n01 = SimplexNoise.SmoothStep(0f, 1f, n01);

					float mask = IslandMask(wx, wy, worldW, worldH); // 0..1
					n01 = MathHelper.Clamp(n01 * mask, 0f, 1f);
					//n01 = MathHelper.Clamp((n01 * mask) - (1f - mask) * 0.25f, 0f, 1f);

					int digit = (int)(n01 * 9.999f);
					if (digit < 0) digit = 0;
					if (digit > 9) digit = 9;

					_previewDigits[x, y] = digit;
				}
			}
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
	}
}
