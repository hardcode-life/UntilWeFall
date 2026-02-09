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

		private Button logo_BTN;
		private Rectangle logo_Bounds;

#region SEED INPUT
		private Rectangle seed_Input_bounds;
		private InputField seed_Input;
		private string prevSeed = null;
		private int _earthSeed;
		private int _skySeed;
		private KeyboardState _kbPrev;
		//private bool _mapAccepted = false;
		//private bool _hasPreview = false;
#endregion <--SEED INPUT------<<<-

#region WORLD CUSTOMIZATION
		private Rectangle worldName_Input_bounds;
		private InputField worldName_Input;

		private Rectangle tribeName_Input_bounds;
		private InputField tribeName_Input;
#endregion <---WORLD CUSTOMIZATION---<<<-
		
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

		const int LeftPad = 24;
		const int TopPad = 80;
		const int RightPanelPad = 32;
		const int RightPanelWidthMin = 520;
		private int _lastW = -1;
		private int _lastH = -1;

		private Rectangle _rightPanelRect;
		private Vector2 _seedPos;
		private Vector2 _worldNamePos;
		const int PreviewTopPad = 0;
		const int PreviewBottomPad = 80; // label + breathing room

		private HeightMap? _previewHeight;

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

			logo_BTN = new  Button(Rectangle.Empty, Color.DarkGray)
			{
				Background = Textures.Get("mainLogo"),
				OnClick = () => ChangeState(GameStateID.StartMenu)
			};

			int w = CTX.GraphicsDevice.Viewport.Width;
			int h = CTX.GraphicsDevice.Viewport.Height;

			_lastW = w;
			_lastH = h;
			ReflowLayout(w, h);

			_focusedInput = seed_Input;
			seed_Input.Focus();
		}


		private void ReflowLayout(int w, int h)
		{
			string seedText = seed_Input?.Value ?? "";
			string worldText = worldName_Input?.Value ?? "";
			string tribeText = tribeName_Input?.Value ?? "";

#region FOCUS
			bool wasSeedFocused = _focusedInput == seed_Input;
			bool wasWorldFocused = _focusedInput == worldName_Input;
			bool wasTribeFocused = _focusedInput == tribeName_Input;
#endregion <----FOCUS---<<<-

			/*logo_Bounds = new Rectangle (
				0, 8,
				Textures.Get("mainLogo").Width / 4, 
				Textures.Get("mainLogo").Height / 4);*/
			logo_Bounds = new Rectangle(
				8,
				(int)Fonts.Get("ex").MeasureString("UNTIL\nWE\nFALL").Y + 16,
				Textures.Get("mainLogo").Width / 4,
				Textures.Get("mainLogo").Height / 4);
			logo_BTN.Bounds = logo_Bounds;

			int logoLaneW = (Textures.Get("mainLogo").Width / 4) + 16;
			int leftGutter = LeftPad + logoLaneW - 24;

			int rightPanelW = Math.Max(RightPanelWidthMin, w / 2);
			int previewAreaW = w - rightPanelW - leftGutter - RightPanelPad;

			int cellW = 12;
			int cellH = 12;

			int previewWcells = Math.Clamp(previewAreaW / cellW, 16, 256);

#region BOTTOM PADDING
			// label space should be based on font, not vibes 😤
			int labelH = Fonts.Get("16").LineSpacing + 12;
			//int previewAreaH = h - PreviewTopPad - labelH - 80;
			int previewAreaH = h
				- PreviewTopPad
				- labelH
				- PreviewBottomPad;
#endregion <-----BOTTOM PADDING----<<<-

#region SET MAP PREVIEW
			_mapPreview.SetPreview(
				origin: new Vector2(leftGutter, PreviewTopPad),
				areaWpx: previewWcells * cellW,
				areaHpx: previewAreaH,
				cellW: cellW,
				cellH: cellH
			);
#endregion <-------SET MAP PREVIEW-------<<<-
			
			int rightX = leftGutter  + _mapPreview.PixelWidth + RightPanelPad;
			_rightPanelRect = new Rectangle(rightX, 0, w - rightX, h);

#region INPUT FIELD BACKGROUND
			// Input bounds anchored to right panel
			seed_Input_bounds = new Rectangle(
				_rightPanelRect.X - 16,
				16,
				_rightPanelRect.Width + 16,
				32
			);

			worldName_Input_bounds = new Rectangle(
				_rightPanelRect.X + 32,
				112,
				450,
				32
			);
			
			tribeName_Input_bounds = new Rectangle(
				_rightPanelRect.X + 32,
				160,
				450,
				32
			);
#endregion <------INPUT FIELD BACKGROUND-------<<<-

			// Recreate or update InputFields (depends on your class design)
			seed_Input = new InputField(
				seed_Input_bounds,
				"Enter seed . . .",
				Fonts.Get("16"),
				() => seed_Input.Clear(),
				CTX.pixel,
				Color.White
			);

			worldName_Input = new InputField(
				worldName_Input_bounds,
				"What do you name this land?",
				Fonts.Get("16"),
				() => worldName_Input.Clear(),
				CTX.pixel,
				Color.White
			);

			tribeName_Input = new InputField(
				tribeName_Input_bounds,
				"What do you name this people?",
				Fonts.Get("16"),
				() => tribeName_Input.Clear(),
				CTX.pixel,
				Color.White
			);

			if (wasSeedFocused) 
			{ 
				_focusedInput = seed_Input; 
				seed_Input.Focus(); 
			}
			else if (wasWorldFocused) 
			{ 
				_focusedInput = worldName_Input; 
				worldName_Input.Focus(); 
			}
			else if (wasTribeFocused)
			{
				_focusedInput = tribeName_Input;
				tribeName_Input.Focus();
			}
			else { _focusedInput = null; }

			//seed_Input.Bounds = seed_Input_bounds;
			//worldName_Input.Bounds = worldName_Input_bounds;
			/*
			seed_Input.SetValue(seedText);
			seed_Input.SetPlaceholder("Enter seed . . .");
			
			worldName_Input.SetValue(worldText);
			worldName_Input.SetPlaceholder("What do you name this world?");

			tribeName_Input.SetValue(tribeText);
			tribeName_Input.SetPlaceholder("What do you name this people?");
			*/
			seed_Input.WithValue(seedText).WithPlaceholder("Enter seed . . .");
			worldName_Input.WithValue(worldText).WithPlaceholder("What do you name this world?");
			tribeName_Input.WithValue(tribeText).WithPlaceholder("What do you name this people?");
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
					tribeName_Input.Bounds.Contains(mouse.Position) ? tribeName_Input :
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
			logo_BTN.Update(mouse);
		
			worldName_Input.Update(mouse);
			seed_Input.Update(mouse);
			tribeName_Input.Update(mouse);

			if (_previewState == PreviewState.Accepted) {
        				_requestLoading = true;
			}

			if (_requestLoading) {
				ChangeState(GameStateID.Loading);
			}

			int w = CTX.GraphicsDevice.Viewport.Width;
			int h = CTX.GraphicsDevice.Viewport.Height;
			if (w != _lastW || h != _lastH)
			{
				_lastW = w;
				_lastH = h;
				ReflowLayout(w, h);
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
					seed_Input.Value) 
					? "default" 
					: seed_Input.Value;

				bool seedChanged = (_previewState == PreviewState.None) || (prevSeed != effectiveSeed);

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

					float[,] hRaw = SimplexNoise.GenerateNoiseMap(
						_mapPreview.PreviewWCells,
						_mapPreview.PreviewHCells,
						_earthSeed,
						200f,
						5,
						0.6f,
						2f,
						0,
						0);
					
					_previewHeight = new HeightMap(
						_mapPreview.PreviewWCells,
						_mapPreview.PreviewHCells,
						seaLevel: 0.32f,
						lakeLevel: 0.38f);

					for (int y = 0; y < _previewHeight.Height; y++)
					{
						for (int x = 0; x < _previewHeight.Width; x++)
						{
							_previewHeight.SetHeight(x, y, hRaw[x, y]);
						}
					}
					_previewHeight.ClassifyOceans();

					_previewState = PreviewState.Generated;

					prevSeed = effectiveSeed;
				} else {
					// ...else, ACCEPT MAP
					if (_previewState == PreviewState.Generated) {
						/*SimplexNoise.GenerateNoiseMap(
							512, 512,
							_earthSeed,
							200f,
							11,
							0.6f,
							2f,
							0,
							0
						);*/
						
						_previewState = PreviewState.Accepted;
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

#region Earth and Sky seeds
			_spriteBatch.DrawString(
				Fonts.Get("12"), 
				$"{_earthSeed}" + " + " + $"{_skySeed}", 
				new Vector2(
					_rightPanelRect.X + 32 , 
					64), 
				Color.White * 0.25f);
#endregion <-----DRAW SEED INPUT---<<<-

			logo_BTN.Draw(_spriteBatch);
			_spriteBatch.DrawString(
				Fonts.Get("ex"),
				"UNTIL\n    WE\nFALL",
				new Vector2(8, 8),
				Color.Orange
			);

#region Input
			seed_Input.Draw(_spriteBatch);
			worldName_Input.Draw(_spriteBatch);
			tribeName_Input.Draw(_spriteBatch);
#endregion  <------ INPUT ----<<<-

#region COMMIT AND LOAD
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
#endregion <-------COMMIT AND LOAD---<<<-

#region LANDFALL
			_spriteBatch.DrawString(
				Fonts.Get("24"),
				$"Landfall : {_mapPreview.PreviewLabel}",
				_mapPreview.Origin
				+ new Vector2(32, _mapPreview.PixelHeight + 8),
				Color.Orange * 0.5F);
			_spriteBatch.End();
#endregion <----LANDFALL--------<<<-

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