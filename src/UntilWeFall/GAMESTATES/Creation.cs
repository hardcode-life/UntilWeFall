using System;
using System.Collections.Generic;
using System.Linq;
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
		private string? prevSeed = null;
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

		private sealed class CreationPage
		{
			public string Id;
			public Button TabButton;          // the little vertical icon tab
			public Rectangle TabRect;         // optional, if you aren’t using Button bounds
			public Action<SpriteBatch, int> Draw;  // draw body + header
			public Action<MouseState> Update;        // optional
		}
		private readonly List<CreationPage> _pages = new();
		private int _activePageIndex = 0;

		private Rectangle _traitsTabRect, _traitsBodyRect;
		private Rectangle _censusTabRect, _censusBodyRect;
		private Rectangle _bloodTabRect, _bloodBodyRect;
		private CreationPage _traitsPage, _censusPage, _bloodPage;

		private void BringToFront(CreationPage page)
		{
			int i = _pages.IndexOf(page);
			if (i < 0) return;

			_pages.RemoveAt(i);
			_pages.Add(page); // now it will draw last (on top)
		}

		void DrawPages(SpriteBatch sb)
		{
			int count = _pages.Count;

			for (int i = 0; i < count; i++)
			{
				int depthFromTop = (count - 1) - i;
				_pages[i].Draw(sb, depthFromTop);
			}
		}

#region CENSUS tab
		private Button plus;
		private Rectangle plus_Bounds;
		private Button minus;
		private Rectangle minus_Bounds;
		private int humanPopulation = 50;
		private Vector2 humanPopulation_pos;
		private Rectangle _popInputBounds;
		private InputField _popInput;
		private const int PopMin = 1;
		private const int PopMax = 500;
#endregion

// =============================================================================
// ---------ANIMALS CROSSING AHEAD -----------------ANIMALS CROSSING AHEAD ------------------
// =============================================================================
// Creation.cs (fields)
private readonly List<CensusCounter> _animalCounters = new();
private readonly List<int> _animalTaxIDs = new(); // same index as _animalCounters


private CensusCounter _horse, _carabao;
private int horseMale = 12, horseFemale = 5;
private int carabaoMale = 2, carabaoFemale = 8;

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

			plus_Bounds  = new Rectangle(0, 0,
			(int)Fonts.Get("32").MeasureString("[+]").X,
			(int)Fonts.Get("32").MeasureString("[+]").Y);

			minus_Bounds = new Rectangle(0, 0,
			(int)Fonts.Get("32").MeasureString("[-]").X,
			(int)Fonts.Get("32").MeasureString("[-]").Y);

			plus  = new Button(Rectangle.Empty, Color.Orange) { 
				Text = "[+]",
				Font = Fonts.Get("16") };
			minus = new Button(Rectangle.Empty, Color.Orange) { 
				Text = "[-]",
				Font = Fonts.Get("16") };	

			#region POPULATION INPUT
			_popInput = new InputField(
				Rectangle.Empty,
				"",
				Fonts.Get("32"),
				() => { /* CHANGE THE HECKING VALUE AAAAAHHHHHHHHH */ },
				CTX.pixel,
				Color.White);

			_popInput.MaxLength = 3;
			_popInput.WithValue(humanPopulation.ToString());
			#endregion

#region ANIMALS
			_animalCounters.Clear();
			_animalTaxIDs.Clear();
			foreach (var kv in CTX.AnimalRegistry.Animals.OrderBy(k => k.Value.TaxID))
			{
				int taxID = kv.Key;
				var def = kv.Value;

				var counter = new CensusCounter
				{
					Min = 0,
					Max = 99,
					Step = 1,
					Font = Fonts.Get("16"),
					Pixel = CTX.pixel,
					ArrowLeftTex = Textures.Get("arrow LEFT"),
					ArrowRightTex = Textures.Get("arrow RIGHT"),
					TextColor = Color.White,
				};

				counter.Initialize(def.AdultMale, def.AdultFemale, def.Name);

				counter.OnValueChanged_male += vM =>
				{
					def.AdultMale = vM;
				};

				counter.OnValueChanged_female += vF =>
				{
					def.AdultFemale = vF;
				};

				_animalCounters.Add(counter);
				_animalTaxIDs.Add(taxID);
			}

			/*_horse = new CensusCounter
			{
				Min = 0,
				Max = 99,
				Step = 1,
				Font = Fonts.Get("16"),
				Pixel = CTX.pixel,
				ArrowLeftTex = Textures.Get("arrow LEFT"),
				ArrowRightTex = Textures.Get("arrow RIGHT"),
				TextColor = Color.White,
			};
			_horse.Initialize(horseMale, horseFemale, "NORDIC WILD HORSE");
			_horse.OnValueChanged_male += vM => horseMale = vM;
			_horse.OnValueChanged_female += vF => horseFemale = vF;

			_carabao = new CensusCounter
			{
				Min = 0,
				Max = 99,
				Step = 1,
				Font = Fonts.Get("16"),
				Pixel = CTX.pixel,
				ArrowLeftTex = Textures.Get("arrow LEFT"),
				ArrowRightTex = Textures.Get("arrow RIGHT"),
				TextColor = Color.White,
			};
			_carabao.Initialize(carabaoMale, carabaoFemale, "CARABAO");
			_carabao.OnValueChanged_male += vM => carabaoMale = vM;
			_carabao.OnValueChanged_female += vF => carabaoFemale = vF;*/
#endregion
			
			ReflowLayout(w, h);

			_traitsPage = new CreationPage
			{
				Id = "traits",
				TabRect = _traitsTabRect,
				Draw = (sb, depth) => Traits(sb, depth),
				Update = (m) => { /* top-only later */ }
			};

			_censusPage = new CreationPage
			{
				Id = "census",
				TabRect = _censusTabRect,
				Draw = (sb, depth) => Census(sb, depth),
				Update = (m) => {  }
			};

			_bloodPage = new CreationPage
			{
				Id = "bloodline",
				TabRect = _bloodTabRect,
				Draw = (sb, depth) => Bloodline(sb, depth),
				Update = (m) => { /*later */}
			};

			_pages.Clear();
			_pages.Add(_bloodPage);
			_pages.Add(_traitsPage);
			_pages.Add(_censusPage); // last = on top initially (census on top)

			_focusedInput = seed_Input;
			seed_Input.Focus();
		}

#region REFLOW LAYOUT
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
				10,
				(int)Fonts.Get("ex").MeasureString("UNTIL\nWE\nFALL").Y + 16,
				Textures.Get("mainLogo").Width / 4,
				Textures.Get("mainLogo").Height / 4);
			logo_BTN.Bounds = logo_Bounds;

			int logoLaneW = (Textures.Get("mainLogo").Width / 4) + 16;
			//int leftGutter = LeftPad + logoLaneW - 24;
			int leftGutter = (int)(Fonts.Get("12").MeasureString("W").X * 12);

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
				_rightPanelRect.X -24,
				16,
				_rightPanelRect.Width + 24,
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
			seed_Input.SetBounds(seed_Input_bounds);
			seed_Input.SetPlaceholder("Enter seed . . .");
			worldName_Input.SetBounds(worldName_Input_bounds);
			worldName_Input.SetPlaceholder("What do you name this land?");
			tribeName_Input.SetBounds(tribeName_Input_bounds);
			tribeName_Input.SetPlaceholder("What do you name this people?");

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

			int rx = _rightPanelRect.X; // screen width / 2
			int rw = _rightPanelRect.Width;
#region PAGES RECTS
			_traitsTabRect  = new Rectangle(rx - 24, 88, 64, 240);
			_traitsBodyRect = new Rectangle(rx - 24, 275, rw, rw);

			_censusTabRect  = new Rectangle(rx + 170, 205, 330, 67);
			_censusBodyRect = new Rectangle(rx + 85, 256, rw - 100, rw);

			_bloodTabRect  = new Rectangle(rx + 837, 160, 144, 94);
			_bloodBodyRect = new Rectangle(rx + 630, 237, 580, 1124);
#endregion
			int y = _censusTabRect.Y + 10;
			int xPlus = _censusTabRect.Right - plus_Bounds.Width - 12;
			int xMinus = xPlus - minus_Bounds.Width - 8;

			plus.Bounds  = new Rectangle(xPlus, y, plus_Bounds.Width, plus_Bounds.Height);
			minus.Bounds = new Rectangle(xMinus, y, minus_Bounds.Width, minus_Bounds.Height);

			if (_traitsPage != null) 
			{
				_traitsPage.TabRect = _traitsTabRect;
			}

			if (_censusPage != null) 
			{
				_censusPage.TabRect = _censusTabRect;
			}

			if (_bloodPage != null)
			{
				_bloodPage.TabRect = _bloodTabRect;
			}

#region Census tab BUTTONS
//new Vector2((CTX.GraphicsDevice.Viewport.Width / 2) + 200, 220)
/* reference based on position of "Human Population" text
_censusTabRect.X + 24, 
_censusTabRect.Y + 8),
*/
			int popW = (int)Fonts.Get("32").MeasureString("000").X + 16; // + padding...
			int popH = (int)Fonts.Get("32").MeasureString("0").Y + 8;

			humanPopulation_pos = new Vector2(
				_censusTabRect.X + (
					_censusTabRect.Width 
					- (int)(Fonts.Get("32").MeasureString("000").X 
						* 2f)
				), 
				_censusTabRect.Y + 8
			);

			_popInputBounds = new Rectangle(
				(int)humanPopulation_pos.X - 14,
				(int)humanPopulation_pos.Y - 6,
				popW,
				popH
			);

			_popInput.SetBounds(_popInputBounds);
			if (_popInput.Value != humanPopulation.ToString()) {
    				_popInput.WithValue(humanPopulation.ToString());
			}

			minus_Bounds = new Rectangle(
				(int)humanPopulation_pos.X - 32,
				(int)humanPopulation_pos.Y, 
				(int)Fonts.Get("24").MeasureString("-").X, 
				(int)Fonts.Get("24").MeasureString("-").Y + 4); 

			minus = new Button(minus_Bounds, Color.White)
			{
				Font = Fonts.Get("24"),
				Text = "-",
				Bounds = minus_Bounds
			};

			plus_Bounds = new Rectangle(
				(int)humanPopulation_pos.X + 64,
				(int)humanPopulation_pos.Y, 
				(int)Fonts.Get("24").MeasureString("+").X, 
				(int)Fonts.Get("24").MeasureString("+").Y + 4);

			plus = new Button(plus_Bounds, Color.White)
			{
				Font = Fonts.Get("24"),
				Text = "+",
				Bounds = plus_Bounds
			};

			plus.OnClick = () => AddPop(+1);
			minus.OnClick = () => AddPop(-1);
#endregion

#region ANIMALS
			/*Counter(sb, new Vector2(_censusBodyRect.X + 240, _censusBodyRect.Y + 80), 8, 24);
			Counter(sb, new Vector2(_censusBodyRect.X + 366, _censusBodyRect.Y + 80), 8, 24);

			sb.DrawString(Fonts.Get("16"), "Horse", new Vector2(_censusBodyRect.X + 12, _censusBodyRect.Y + 80), Color.White);*/
			/*sb.Draw(Textures.Get("male"), new Rectangle(_censusBodyRect.X + 240, _censusBodyRect.Y + 32, 24, 24), Color.White);
			sb.Draw(Textures.Get("female"), new Rectangle(_censusBodyRect.X + 360, _censusBodyRect.Y + 32, 24, 24), Color.White);*/

			int startY = _censusBodyRect.Y + 80;
			int rowH = (int)(Fonts.Get("16").LineSpacing * 1.6f);

			for (int i = 0; i < _animalCounters.Count; i++)
			{
				var c = _animalCounters[i];

				c.maleBounds = new Rectangle(
					_censusBodyRect.X + 240,
					startY + (rowH * i),
					(int)(Fonts.Get("16").MeasureString("99").X * 1.5f),
					24
				);

				c.Reflow();
			}

			/*int row = (int)(Fonts.Get("16").MeasureString("99").Y * 1.5f);
			_horse.maleBounds = new Rectangle(
				_censusBodyRect.X + 240,
				_censusBodyRect.Y + 80, // _censusBodyRect.Y + 80 + (row * index)
				(int)(Fonts.Get("16").MeasureString("99").X * 1.5f),
				24);
			_horse.Reflow();

			_carabao.maleBounds  = new Rectangle(
				_censusBodyRect.X + 240,
				_censusBodyRect.Y + 80 + row,
				(int)(Fonts.Get("16").MeasureString("99").X * 1.5f),
				24);
			_carabao.Reflow();*/
#endregion
		}
#endregion

		public override void Exit()
		{
			CTX.Game.Window.TextInput -= OnTextInput;
		}

		public override void Update(GameTime gameTime)
		{
			bool clickedPage = false;
			var kb = Keyboard.GetState();
			var mouse = Mouse.GetState();

			bool click = mouse.LeftButton == ButtonState.Pressed &&
				_mousePrev.LeftButton == ButtonState.Released;

			if (click)
			{	
				#region  INCLUDE YOUR INPUTS HERE
				InputField? next =
					seed_Input.Bounds.Contains(mouse.Position) ? seed_Input :
					worldName_Input.Bounds.Contains(mouse.Position) ? worldName_Input :
					tribeName_Input.Bounds.Contains(mouse.Position) ? tribeName_Input :
					_popInput.Bounds.Contains(mouse.Position) ? _popInput :
					null;
				#endregion

				if (next != null)
				{
					// Make sure the right page is on top if needed
					if (next == _popInput) {
						BringToFront(_censusPage);
					}

					if (next != _focusedInput) {
						if (_focusedInput == _popInput)
						CommitPopulationFromInput(live: false);

						_focusedInput?.Blur();
						_focusedInput = next;
						_focusedInput?.Focus();
					}

					_mousePrev = mouse;
					//return;
				}

				for (int i = _pages.Count - 1; i >= 0; i--)
				{
					var p = _pages[i];

					// click tab
					if (p.TabRect.Contains(mouse.Position))
					{
						BringToFront(p);
						clickedPage = true;
						break;
					}

					// OPTIONAL: click body brings page front too
					if (p.Id == "traits" && _traitsBodyRect.Contains(mouse.Position))
					{
						BringToFront(p);
						if (next == null) {
							clickedPage = true;
						}
						break;
					}

					if (p.Id == "census" && _censusBodyRect.Contains(mouse.Position))
					{
						BringToFront(p);
						if (next == null) {
							clickedPage = true;
						}
						break;
					}
				}

				if (!clickedPage) {
					if (next != _focusedInput)
					{
						if (_focusedInput == _popInput) {
							CommitPopulationFromInput(live: false);
						}
						_focusedInput?.Blur();
						_focusedInput = next;
						_focusedInput?.Focus();
					}
				}
			}
			#region ANIMALS
			if (_pages.Count > 0 && _pages[^1].Id == "census")
			{
				/*_horse.Update(Mouse.GetState());
				_carabao.Update(Mouse.GetState());*/
				var ms = Mouse.GetState();
				for (int i = 0; i < _animalCounters.Count; i++)
				{
					_animalCounters[i].Update(ms);
				}
			}
			#endregion
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

			CTX.EarthSeed = _earthSeed;
			CTX.SkySeed = _skySeed;
			CTX.WorldWidth = CTX.GraphicsDevice.Viewport.Width;
			CTX.WorldHeight = CTX.GraphicsDevice.Viewport.Height;
			CTX.LastPreview = _mapPreview;

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

#region tab BUTTONS update
			//plus.Update(mouse);
			//minus.Update(mouse);

			if (_pages.Count > 0)
			{
				var top = _pages[^1];
				top.Update?.Invoke(mouse);

				// Example: only update plus/minus if census page is on top
				if (top.Id == "census")
				{
					plus.Update(mouse);
					minus.Update(mouse);
				}
			}
			_popInput.Update(mouse);
#endregion
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
						1f,
						1f,
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

			if (_focusedInput == _popInput && kb.IsKeyDown(Keys.Enter) && !_kbPrev.IsKeyDown(Keys.Enter))
			{
				CommitPopulationFromInput(live: false);
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
				new Vector2(12, 8),
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
#endregion <----LANDFALL--------<<<-

#region MAP PREVIEW
			_mapPreview.Draw(_spriteBatch, Fonts.Get("12"), hm: _mapPreview.PreviewHeight);
#endregion <-----MAP PREVIEW---<<<-
			
			//Census(_spriteBatch);
			//Traits(_spriteBatch);
			DrawPages(_spriteBatch);
			//Bloodline(_spriteBatch, true);

			DrawCursor(_spriteBatch);

			#region DEBUG
			//_spriteBatch.DrawString(Fonts.Get("32"),"+", new Vector2(plus.Bounds.X, plus.Bounds.Y), Color.Red * 0.3f);
			//_spriteBatch.Draw(CTX.pixel, minus.Bounds, Color.Lime * 0.3f);
			#endregion


			_spriteBatch.End();
		}

#region TRAITS tab
		private void Traits(SpriteBatch sb, int depthFromTop)
		{
			Color topTint = Hex.convert("#1c242a");          // font
			Color midTint = Hex.convert("#3e4951"); // mid
			Color backTint = Hex.convert("#2c373e"); // back

			Color iconTint =
				depthFromTop == 0 ? Color.White :
				depthFromTop == 1 ? Color.White * 0.8f :
				Color.White * 0.6f;

			Color tint =
				depthFromTop == 0 ? topTint :
				depthFromTop == 1 ? midTint :
				backTint;

			sb.Draw(
				Textures.Get("traits_tab"), 
				/*new Rectangle(
					(CTX.GraphicsDevice.Viewport.Width / 2) - 24, 
					88, 
					64, 
					240),*/
				_traitsTabRect,
				tint);

			
			if (depthFromTop == 0)
			{
				var shadow = _traitsBodyRect;
				shadow.X += 4;
				shadow.Y += 4;
				sb.Draw(Textures.Get("bg"), shadow, Color.Black * 0.15f);
			}
			sb.Draw(
				Textures.Get("bg"), 
				/*new Rectangle(
					(CTX.GraphicsDevice.Viewport.Width / 2) - 24, 
					275, 
					CTX.GraphicsDevice.Viewport.Width / 2, 
					CTX.GraphicsDevice.Viewport.Width / 2), */
				_traitsBodyRect,
				tint);

			sb.Draw(
				Textures.Get("traits_tab_img1"), 
				new Rectangle(
					(CTX.GraphicsDevice.Viewport.Width / 2) - 22, 
					95, 
					40, 
					49), 
				iconTint);

			sb.Draw(
				Textures.Get("traits_tab_img2"), 
				new Rectangle(
					(CTX.GraphicsDevice.Viewport.Width / 2) - 11, 
					162, 
					19, 
					29), 
				iconTint);

			/*if (depthFromTop == 0)
			{
				plus.Draw(sb);
				minus.Draw(sb);
			}   */
		}
#endregion

#region CENSUS tab
		private void Census(SpriteBatch sb, int depthFromTop)
		{
			Color topTint = Hex.convert("#1c242a"); 
			Color midTint = Hex.convert("#3e4951");
			Color backTint = Hex.convert("#2c373e"); 

			Color tint =
				depthFromTop == 0 ? topTint :
				depthFromTop == 1 ? midTint :
				backTint;

			sb.Draw(
				Textures.Get("census_tab"), 
				/*new Rectangle(
					(CTX.GraphicsDevice.Viewport.Width / 2) + 170, 
					205, 
					330, 
					67),*/
				_censusTabRect,
				tint);

			if (depthFromTop == 0)
			{
				var shadow = _censusBodyRect;
				shadow.X += 4;
				shadow.Y += 4;
				sb.Draw(Textures.Get("bg"), shadow, Color.Black * 0.15f);
			}
			sb.Draw(
				Textures.Get("bg"), 
				/*new Rectangle(
					(CTX.GraphicsDevice.Viewport.Width / 2) + 85, 
					256, 
					(CTX.GraphicsDevice.Viewport.Width / 2) - 100, 
					CTX.GraphicsDevice.Viewport.Width / 2), */
				_censusBodyRect,
				tint);

			sb.DrawString(
				Fonts.Get("16"),
				"Human Population",
				new Vector2(
					_censusTabRect.X + 32, 
					_censusTabRect.Y + 8),
				Color.White);

			/*sb.DrawString(
				Fonts.Get("32"),
				humanPopulation.ToString(),
				humanPopulation_pos,
				Color.White);*/
			_popInput.Draw(sb);

			if (depthFromTop == 0)
			{
				_popInput.Draw(sb);

				Rectangle minus_rec = new Rectangle(
					minus.Bounds.X - 2,
					minus.Bounds.Y,
					minus.Bounds.Width +2,
					minus.Bounds.Height);
				sb.Draw(CTX.pixel, minus_rec, Color.Black * 0.25f);
				minus.Draw(sb);

				Rectangle plus_rec = new Rectangle(
					plus.Bounds.X - 2,
					plus.Bounds.Y,
					plus.Bounds.Width +2,
					plus.Bounds.Height);
				sb.Draw(CTX.pixel, plus_rec, Color.Black * 0.25f);
				plus.Draw(sb);
			}  

			sb.Draw(Textures.Get("male"), new Rectangle(_censusBodyRect.X + 240, _censusBodyRect.Y + 32, 24, 24), Color.White);
			sb.Draw(Textures.Get("female"), new Rectangle(_censusBodyRect.X + 320, _censusBodyRect.Y + 32, 24, 24), Color.White);

			/*sb.DrawString(Fonts.Get("16"), "Horse", new Vector2(_censusBodyRect.X + 12, _censusBodyRect.Y + 80), Color.White);*/
			#region ANIMALS
			/*_horse.Draw(sb);
			_carabao.Draw(sb);*/
			for (int i =0; i < _animalCounters.Count; i++)
			{
				_animalCounters[i].Draw(sb);
			}
			#endregion
		}

		void Counter(SpriteBatch sb, Vector2 pos, int width, int height)
		{
			sb.Draw(Textures.Get("arrow LEFT"), new Rectangle((int)pos.X - 20, (int)pos.Y, width, height), Color.White);
			sb.DrawString(Fonts.Get("16"), "12", pos, Color.White);
			sb.Draw(Textures.Get("arrow RIGHT"), new Rectangle((int)pos.X + 28, (int)pos.Y, width, height), Color.White);
		}

		void AddPop(int delta)
		{
			humanPopulation = Math.Clamp(humanPopulation + delta, PopMin, PopMax);
			_popInput.WithValue(humanPopulation.ToString());
		}
#endregion

#region  BLOODLINE tab
		private void Bloodline(SpriteBatch sb, int depthFromTop)
		{
			Color topTint = Hex.convert("#1c242a"); 
			Color midTint = Hex.convert("#3e4951");
			Color backTint = Hex.convert("#2c373e"); 

			Color tint =
				depthFromTop == 0 ? topTint :
				depthFromTop == 1 ? midTint :
				backTint;

			sb.Draw(Textures.Get("bloodline_tab"), 
				_bloodTabRect,
				tint);

			if (depthFromTop == 0)
			{
				var shadow = _bloodBodyRect;
				shadow.X += 4;
				shadow.Y += 4;
				sb.Draw(Textures.Get("bg"), shadow, Color.Black * 0.15f);
			}
			sb.Draw(Textures.Get("bg"), 
				_bloodBodyRect,
				tint);

			//connection				
			sb.Draw(Textures.Get("bg"), 
				//new Rectangle(_rightPanelRect.X + 837, 160, 32, 4),
				new Rectangle(_bloodTabRect.X, _bloodTabRect.Y + 64, 32, 4),
				Color.White);
		}
#endregion

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

			// make sure population input is DIGITS ONLY
			if (_focusedInput == _popInput)
			{
				if (!char.IsDigit(c)) { 
					return; 
				}

				_focusedInput.Append(c);
				CommitPopulationFromInput(live: true);
				return; // important: don't fall through to the general rule
			}


			if (char.IsLetterOrDigit(c) || c == ' ' || c == '-' || c == '_') {
				_focusedInput.Append(c);
			}
		}

		private void CommitPopulationFromInput(bool live = false)
		{
			if (_popInput == null) { return; }

			if(string.IsNullOrWhiteSpace(_popInput.Value))
			{
				if (!live)
				{
					humanPopulation = PopMin;
					_popInput.WithValue(humanPopulation.ToString());
				}
				return;
			}

			if (!int.TryParse(_popInput.Value, out int v)) {
				v = humanPopulation;
			}

			v = Math.Clamp(v, PopMin, PopMax);
			humanPopulation = v;

			_popInput.WithValue(humanPopulation.ToString());
		}
	}
}