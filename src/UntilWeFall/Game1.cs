using System.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System;

namespace UntilWeFall
{
	#region CAMERA 2D
	public class Camera2D
	{
		public Vector2 position {get; private set; } = Vector2.Zero;
		public float Zoom {get; private set;} = 1f;
		public float Rotation {get; private set;} = 0f;
		public int ViewportWidth {get; private set;}
		public int ViewportHeight {get; private set;}
		public float minZoom {get; set;} = 0.25f;
		public float maxZoom {get; set;} = 4.0f;

		public Camera2D(int viewportWidth, int viewportHeight)
		{
			ViewportHeight = viewportHeight;
			ViewportWidth = viewportWidth;
		}

		public void SetViewportSize(int width, int height)
		{
			ViewportWidth = width;
			ViewportHeight = height;
		}

		public void Move(Vector2 amount)
		{
			position += amount;
		}
		
		public void AddZoom (float amount)
		{
			Zoom += amount;
			Zoom = MathHelper.Clamp(Zoom, minZoom, maxZoom);
		}

		public Vector2 GetScreenCenter()  // CENTER OF SCREEN (pixels)
			=> new Vector2(ViewportWidth * 0.5f, ViewportHeight * 0.5f);

		public Matrix GetViewMatrix() // basically the player's POV, lol
		{
			return
				Matrix.CreateTranslation(new Vector3(-position, 0f)) * // "move around"
				Matrix.CreateRotationZ(Rotation) * // spin view
				Matrix.CreateScale(Zoom, Zoom, 1f) *  // zoom "in and out"
				Matrix.CreateTranslation(new Vector3(GetScreenCenter(), 0f)); //centering
		}

		public Vector2 ScreenToWorld (Vector2 screenPosition) // when I need to converted a screen coordinate into world coordinates
		{
			Matrix inverse = Matrix.Invert(GetViewMatrix());
			return Vector2.Transform(screenPosition, inverse);
		}
	}
    	#endregion
	public class Game1 : Game
	{
		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;

		private Camera2D _camera;
		private float _cameraSpeed = 800f;
		private MouseState _prevMouse;
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
			_camera = new Camera2D(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			// TODO: use this.Content to load your game content here
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			_pixel = new Texture2D(GraphicsDevice, 1, 1);
			_pixel.SetData(new[] { Color.White });
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
			Exit();

			// TODO: Add your update logic here
			float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

			var keyboard = Keyboard.GetState();

			Vector2 moveDir = Vector2.Zero;

			if (keyboard.IsKeyDown(Keys.W) || keyboard.IsKeyDown(Keys.Up))
				moveDir.Y -= 1;

			if (keyboard.IsKeyDown(Keys.S) || keyboard.IsKeyDown(Keys.Down))
				moveDir.Y += 1;

			if (keyboard.IsKeyDown(Keys.A) || keyboard.IsKeyDown(Keys.Left))
				moveDir.X -= 1;

			if (keyboard.IsKeyDown(Keys.D) || keyboard.IsKeyDown(Keys.Right))
				moveDir.X += 1;
			
			if (moveDir != Vector2.Zero)
    				moveDir.Normalize();

			float zoomAdjustedSpeed = _cameraSpeed / _camera.Zoom;

			_camera.Move(moveDir * zoomAdjustedSpeed * dt);

			var mouse = Mouse.GetState();

			int scrollDelta = mouse.ScrollWheelValue - _prevMouse.ScrollWheelValue;

			if (scrollDelta != 0)
			{
				float zoomStep = 0.1f;
				float direction = Math.Sign(scrollDelta);

				_camera.AddZoom(direction * zoomStep);
			}

			_prevMouse = mouse;

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.CornflowerBlue);

			// TODO: Add your drawing code here
			_spriteBatch.Begin(transformMatrix: _camera.GetViewMatrix(), samplerState: SamplerState.PointClamp);

			// Draw a simple cross at world origin so you can see movement
			DrawLine(new Vector2(-200, 0), new Vector2(200, 0), Color.Red, 3);
			DrawLine(new Vector2(0, -200), new Vector2(0, 200), Color.Red, 3);

			// Draw a rectangle "tile" at world (100, 100)
			_spriteBatch.Draw(_pixel, new Rectangle(100, 100, 64, 64), Color.ForestGreen);

			_spriteBatch.End();

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

	}
}
