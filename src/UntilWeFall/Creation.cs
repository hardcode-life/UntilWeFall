using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace UntilWeFall
{
	public sealed class Creation : GameStateBase
	{	
	    	//private readonly GameContext _ctx;
    		//private readonly Action<GameStateID> _changeState; // callback to Game1
		private InputField? _focusedInput;
		private enum PreviewState
		{
			None,
			Generated,
			Accepted
		}
		PreviewState _previewState = PreviewState.None;

#region SEED INPUT
		private Rectangle seed_Input_bounds;
		private InputField seed_Input;
		private string prevSeed ="default";
		private int _earthSeed;
		private int _skySeed;
		private KeyboardState _kbPrev;
		//private bool _mapAccepted = false;
		//private bool _hasPreview = false;
#endregion <--SEED INPUT------<<<-

#region World Generation - customization
		private Rectangle worldName_Input_bounds;
		private InputField worldName_Input;
#endregion <---World Generation - customization---<<<-
		
		private MapPreview _mapPreview = new MapPreview();
		
#region CAMERA 2D
		private Camera2D _camera;
		private MouseState _mousePrev;
		//private float _panSpeed = 800f;
		//private float _zoomStep = 0.10f;
		private Matrix _viewMatrix;
		private Vector2 mouseWorld;
#endregion <--CAMERA 2D------<<<-
		
		private float keyRepeatTimer = 0f;
		private const float InitialDelay = 0.4f;
		private const float RepeatRate = 0.05f;

		private bool _requestLoading;

		public Creation(GameContext ctx, Action<GameStateID> changeState) : base(ctx, changeState)
		{
			/* spiders ahead
			 	/╲/\╭(ఠఠ益ఠఠ)╮/\╱\
			 */
		}

		public override void Enter()
		{
			CTX.Game.Window.TextInput += OnTextInput;

			_camera = new Camera2D(CTX.GraphicsDevice.Viewport.Width, CTX.GraphicsDevice.Viewport.Height);

#region MAP PREVIEW ORIGIN
			_mapPreview.SetPreview(new Vector2(
				CTX.GraphicsDevice.Viewport.Width - (12 * 64) - 12,
				80
			)); // set preview ORIGIN.
#endregion <---MAP PREVIEW ORIGIN--<<<-
			
#region INPUT FIELDS
			worldName_Input_bounds = new Rectangle(
				(CTX.GraphicsDevice.Viewport.Width / 2) + 32, 
				112, 
				450, 
				32);
			worldName_Input = new InputField(
				worldName_Input_bounds,
				"What do you name this land?",
				Fonts.Get("16"),
				() => worldName_Input.Clear(),
				CTX.pixel,
				Color.White);

			seed_Input_bounds = new Rectangle(
				(CTX.GraphicsDevice.Viewport.Width / 2) + 80, 
				24,
				1200,
				32);
			seed_Input = new InputField(
				seed_Input_bounds,
				"Enter seed . . .",
				Fonts.Get("16"),
				() => seed_Input.Clear(),
				CTX.pixel,
				Color.White);
#endregion <------INPUT FIELDS--<<<-
		}

		public override void Exit()
		{
			CTX.Game.Window.TextInput -= OnTextInput;
		}

		public override void Update(GameTime gameTime)
		{
			var kb = Keyboard.GetState();
			var mouse = Mouse.GetState();

			bool click = mouse.LeftButton == ButtonState.Pressed &&
				_mousePrev.LeftButton == ButtonState.Released;

			if (click)
			{
				InputField? next =
					seed_Input.Bounds.Contains(mouse.Position) ? seed_Input :
					worldName_Input.Bounds.Contains(mouse.Position) ? worldName_Input :
					null;

				if (next != _focusedInput)
				{
					_focusedInput?.Blur();
					_focusedInput = next;
					_focusedInput?.Focus();
				}
			}

			_mousePrev = mouse;

			_viewMatrix = _camera.GetViewMatrix();
			mouseWorld = _camera.ScreenToWorld(new Vector2(mouse.X, mouse.Y));

			// Backspace
			if (kb.IsKeyDown(Keys.Back) && !_kbPrev.IsKeyDown(Keys.Back))
			{
				_focusedInput?.Backspace();
				keyRepeatTimer = InitialDelay;
			}
			else if (kb.IsKeyDown(Keys.Back) && _kbPrev.IsKeyDown(Keys.Back))
			{
				keyRepeatTimer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
				if (keyRepeatTimer <= 0)
				{
					_focusedInput?.Backspace();
					keyRepeatTimer = RepeatRate;
				}
			}
			
			HandleSeedCommit(kb);
		
			worldName_Input.Update(mouse);
			seed_Input.Update(mouse);

			if (_previewState == PreviewState.Accepted) {
        				_requestLoading = true;
			}

			if (_requestLoading) {
				ChangeState(GameStateID.Loading);
			}
		}

		private void HandleSeedCommit(KeyboardState kb)
		{
#region Commit Seed
			bool seedFocused = _focusedInput == seed_Input;

			// Enter = commit seed / accept map
			if (seedFocused &&  kb.IsKeyDown(Keys.Enter) && !_kbPrev.IsKeyDown(Keys.Enter))
			{
				// Decide what "empty" means (pick one)
				string effectiveSeed = string.IsNullOrWhiteSpace(
					seed_Input.Value) ? "default"
					: seed_Input.Value;

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
					_previewState = PreviewState.Generated;
					//_hasPreview = true;
					//_mapAccepted = false;

					prevSeed = effectiveSeed;
				} else {
					// ...else, ACCEPT MAP
					if (_previewState == PreviewState.Generated) {
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
						_previewState = PreviewState.Accepted;
						//_mapAccepted = true;
					}

					// TODO: move world gen to loading state + Task.Run
				}
			}
			_kbPrev = kb;
#endregion <----Commit Seed------<<<-

		}

		public override void Draw(GameTime gameTime)
		{
			var _spriteBatch = CTX.SpriteBatch;
			_spriteBatch.Begin(samplerState: SamplerState.PointClamp);

#region Draw SEED INPUT
			_spriteBatch.DrawString(
				Fonts.Get("12"), 
				$"{_earthSeed}" + " + " + $"{_skySeed}", 
				new Vector2(
					(CTX.GraphicsDevice.Viewport.Width / 2) + 96, 
					64), 
				Color.White * 0.25f);
#endregion <-----DRAW SEED INPUT---<<<-

#region Input
			seed_Input.Draw(_spriteBatch);
			worldName_Input.Draw(_spriteBatch);
#endregion  <------ INPUT ----<<<-

			if (_previewState == PreviewState.Accepted)
			{
				_spriteBatch.DrawString(
					Fonts.Get("16"),
					"MAP ACCEPTED",
					new Vector2(
						(CTX.GraphicsDevice.Viewport.Width / 2) + 128,
						CTX.GraphicsDevice.Viewport.Height / 2),
					Color.White * .5f);
				//ChangeState(GameStateID.Loading);
			}
			_spriteBatch.End();

#region MAP PREVIEW
			_mapPreview.Draw(_spriteBatch, Fonts.Get("12"));
#endregion <-----MAP PREVIEW---<<<-

			_spriteBatch.Begin(
				transformMatrix: _viewMatrix, samplerState: SamplerState.PointClamp);
			
			DrawCursor(_spriteBatch);
	
			_spriteBatch.End();
		}

#region Draw CURSOR
		private void DrawCursor(SpriteBatch sb)
		{
			// snaps the cursor to tile position...
			int tileSize = 16; // ..or whatever

			Vector2 snapped = new Vector2(
				(int)(mouseWorld.X / tileSize) * tileSize,
				(int)(mouseWorld.Y / tileSize) * tileSize
			);

			sb.Draw(
				CTX.pixel, 
				new Rectangle((int)snapped.X, (int)snapped.Y, tileSize, tileSize), 
				Color.Yellow * 0.35f
			);

			DrawLine(sb, snapped, snapped + new Vector2(tileSize, 0), Color.Yellow, 2);
			
			DrawLine(sb, snapped, snapped + new Vector2(0, tileSize), Color.Yellow, 2);
		}
#endregion <-----Draw CURSOR---<<<-

		private void DrawLine(SpriteBatch sb, Vector2 start, Vector2 end, Color color, int thickness)
		{
			Vector2 edge = end - start;
			float angle = (float)System.Math.Atan2(edge.Y, edge.X);

			sb.Draw(
				CTX.pixel,
				new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), thickness),
				null,
				color,
				angle,
				Vector2.Zero,
				SpriteEffects.None,
				0
			);
		}

		private void OnTextInput(object sender, TextInputEventArgs e)
		{
			if (_focusedInput == null) {
				return;
			}

			char c = e.Character;
			if (char.IsControl(c)) {
				return;
			}

			if (char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_') {
				_focusedInput.Append(c);
			}
		}
	}
}