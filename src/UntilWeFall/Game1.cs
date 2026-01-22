using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Globalization;
using System.Runtime.InteropServices;

using System;

namespace UntilWeFall
{	
	public class Game1 : Game
	{
		private static readonly Color window_bg_Color = new Color(7, 7, 7);
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		#region CAMERA 2D
			private Camera2D _camera;
			private MouseState _mousePrev;

			private float _panSpeed = 800f;
			private float _zoomStep = 0.10f;

			private Matrix _viewMatrix;
			private Vector2 mouseWorld;
		#endregion

		private Texture2D _pixel;

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

			base.Initialize();
		}

		protected override void LoadContent()
		{
			// TODO: use this.Content to load your game content here
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			_pixel = new Texture2D(GraphicsDevice, 1, 1);
			_pixel.SetData(new[] { Color.White });
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

			_camera.SetViewportSize(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

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

			_spriteBatch.End();
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

	}
}
