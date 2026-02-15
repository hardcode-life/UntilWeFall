using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace UntilWeFall
{
	public sealed class Loading : GameStateBase
	{
		//private readonly GameContext _ctx;
    		private readonly Action<GameStateID> _changeState;

		private sealed class Status
		{
			public volatile float Progress = 0f;
			public volatile string Message = "Loading. .. . .";
			public volatile bool Done = false;
			public volatile bool Failed = false;
			public volatile string Error = "";
		}
		private float displayProgress = 0f;
		private readonly Status _status = new();
		private Task? _task;
		private MapPreview? _preview;
		private Vector2 previewOffset = Vector2.Zero;
		//private float previewAlpha = 0f;
			
		//Vector2 offset = Vector2.Zero;
		private string _displayMessage = "";
		private float _displayTargetProgress = 0f;
		private int _generatedEarthSeed;
		private int _generatedSkySeed;
		private WorldData? _generatedWorld;
		WorldData wd;
		public Loading(GameContext ctx, Action<GameStateID> changeState)
			: base(ctx, changeState)
		{

		}

		public override void Enter()
		{
			displayProgress = 0f;

			_status.Progress = 0f;
			_status.Message = "Preparing world. . .";
			_status.Done = false;
			_status.Failed = false;
			_status.Error = "";

			int w = CTX.GraphicsDevice.Viewport.Width;
			int h = CTX.GraphicsDevice.Viewport.Height;

			_preview = new MapPreview();
			_preview.SetPreview(Vector2.Zero, w, h, cellW: 12, cellH: 12);
			_preview.Regenerate(CTX.EarthSeed, CTX.SkySeed, CTX.WorldWidth, CTX.WorldHeight);

			

			_task = Task.Run(() =>
			{
				try
				{
					//RunPipeline();
					GenerateWorld();
					_status.Done = true;
				}
				catch (Exception ex)
				{
					_status.Failed = true;
					_status.Error = ex.ToString(); // better than Message only
				}
			});
		}

		private void RunPipeline()
		{
			Stage(0.10f, "Sowing seeds.. . . ..", () => //computing seed
			{
				Thread.Sleep(50);
			});

			Stage(0.35f, "Raising the land. .. ... .", () => //creating the height maop
			{
				Thread.Sleep(200);
			});

			Stage(0.30f, "Opening flood gates... . ..", ()=> //filling bodies of water (lakes and rivers)
			{
				Thread.Sleep(150);
			});

			Stage(0.20f, "Populating the land.. .. ...", () => //spawning units and animals
			{
				Thread.Sleep(100);
			});

			Stage(0.0f, "Finalizing. ... . ..", ()=>
			{
				Thread.Sleep(50);
			});

			_status.Progress = 1f;
			_status.Message = "Done.";
		}

		private void GenerateWorld(){
			_status.Message = "Sowing seeds.. . . ..";
			_status.Progress = 0.05f;
			
			SeedGenerator.Derive_fromLoading(out int earthSeed, out int skySeed);

			var newWorld = new WorldData(CTX.WorldWidth, CTX.WorldHeight);

			_generatedEarthSeed = earthSeed;
			_generatedSkySeed = skySeed;
			_generatedWorld = newWorld;

			_status.Message = "Allocating world data...";
			_status.Progress = 0.10f;

			wd = new WorldData(CTX.WorldWidth, CTX.WorldHeight);   // or whatever size
			CTX.worldData = wd;                // store in GameContext

			//_status.Message = "Generating terrain...";
			//_status.Progress = 0.15f;

			newWorld.GenerateAll(earthSeed, (p, m) => { _status.Progress = p; _status.Message = m; });

			wd.GenerateAll(earthSeed, (progress, message) => 
			{
				_status.Progress = progress;
				_status.Message = message;
			});

			_status.Message = "Finalizing...";
			_status.Progress = 0.95f;

			// Optional smoothing or biome calculation here

			_status.Progress = 1f;
			_status.Message = "Done.";
		}

		private void Stage(float weight, string msg, Action work)
		{
			_status.Message = msg;
			float start = _status.Progress;

			work();

			_status.Progress = Math.Clamp(start + weight, 0f, 1f);
		}

		public override void Exit()
		{
			// ヽ(￣(ｴ)￣)ﾉ
		}

		public override void Update(GameTime gameTime) 
		{	
			_displayTargetProgress = _status.Progress;
    			_displayMessage = _status.Message;
		
			//float speed = 8f; // ʕಠᴥಠʔ lower for snappier, higher for smoother

			float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
			const float loadSpeed = 1.2f;
			float lerpT = 1f - MathF.Exp(-loadSpeed * dt);

			float time = (float)gameTime.TotalGameTime.TotalSeconds;

			displayProgress = MathHelper.Lerp(
				displayProgress,
				_displayTargetProgress,
				lerpT);

			float t = (float)gameTime.TotalGameTime.TotalSeconds;
			previewOffset = new Vector2(
				MathF.Sin(t * 0.15f),
				MathF.Cos(t * 0.12f)
			) * 2f;

			//previewAlpha = MathHelper.Clamp(displayProgress / 0.1f, 0f, 1f);

			if (displayProgress > _displayTargetProgress)
			{
				displayProgress = _displayTargetProgress;
			}

			if (MathF.Abs(displayProgress - _displayTargetProgress) < 0.001f) {
				displayProgress = _displayTargetProgress;
			}

			if (_status.Failed)
			{
			
				if (Keyboard.GetState().IsKeyDown(Keys.Escape))
				{
					ChangeState(GameStateID.Creation);
				}
				return;
			}

			if (_status.Done && displayProgress >= 0.999f)
			{
				ChangeState(GameStateID.Playing);
			}

			if (_status.Done && !_status.Failed && _generatedWorld != null)
			{
				CTX.EarthSeed = _generatedEarthSeed;
				CTX.SkySeed = _generatedSkySeed;
				CTX.worldData = _generatedWorld;
			}
		}

		private static string AsciiBar(
			float progress, 
			SpriteFont font,
			int screenWidth,
			float widthRatio = 0.85f) // 85% of screen wdith
		{
			progress = Math.Clamp(progress, 0f, 1f);

			float charWidth = font.MeasureString("X").X; // measure character width

			int totalChars = Math.Max( // how many chars can fit in sc reen
				4,
				(int)((screenWidth * widthRatio) / charWidth) - 2
				);

			int filled = (int)MathF.Round(progress * totalChars);

			return "[" 
				+ new string('X', filled)
				+ new string('-', totalChars - filled)
				+ "]";
		}

		public override void Draw(GameTime gameTime)
		{
			var sb = CTX.SpriteBatch;

			sb.Begin(samplerState: SamplerState.PointClamp);
				var font = Fonts.Get("32");

				int w = CTX.GraphicsDevice.Viewport.Width;
				int h = CTX.GraphicsDevice.Viewport.Height;

				var fullScreenRect = new Rectangle(0, 0, w, h);
				var bgColor = Hex.convert("#1c242a"); // or whatever your base UI bg is

				string bar = AsciiBar(displayProgress, font, w);

				Vector2 barPos = new Vector2(
				(w - font.MeasureString(bar).X) / 2f,
				h);

				sb.Draw(CTX.pixel, fullScreenRect, bgColor);

				if (_preview != null)
				{
					float previewAlpha = MathHelper.Clamp(displayProgress / 0.10f, 0f, 1f);
					Color tint = Color.White * (0.80f * previewAlpha);

					/*_preview.Draw(
						sb,
						Fonts.Get("12"),
						hm: _preview.PreviewHeight,
						tint: tint,
						offset: previewOffset,
						drawSpawn: false
						);*/
					_preview.Draw(
						sb,
						Fonts.Get("12"),
						hm: _preview.PreviewHeight,
						Color.White * .25f,
						offset: new Vector2(0, 0),
						drawSpawn: false);
				}
				var msgSize = font.MeasureString(_displayMessage);
				//var msgPos = new Vector2((w - msgSize.X) / 2f, barPos.Y + 36);
				var msgPos = new Vector2(8, h - (Fonts.Get("16").MeasureString("X").Y * 10));

				DrawLoadingUI(sb, font, msgPos, bar);
				//sb.DrawString(font, bar, barPos, Color.White * 0.5f);
				//sb.DrawString(font, _status.Message, barPos + new Vector2(0, 36), Color.White * 0.8f);
			sb.End();
		}

		private void DrawLoadingUI(SpriteBatch sb, SpriteFont font, Vector2 barPos, string bar)
		{
			sb.DrawString(font, bar, barPos, Color.White * 0.5f);
			sb.DrawString(Fonts.Get("16"), _displayMessage, barPos + new Vector2(64, 40), Color.Orange * 0.8f);
		}

	}
}