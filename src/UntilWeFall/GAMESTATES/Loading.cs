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
		private float previewAlpha = 0f;
			
		Vector2 offset = Vector2.Zero;
		public Loading(GameContext ctx, Action<GameStateID> changeState)
			: base(ctx, changeState)
		{

		}

		public override void Enter()
		{
			_preview = CTX.LastPreview;
			displayProgress = 0f;

			_status.Progress = 0f;
			_status.Message = "Preparing world. . .";
			_status.Done = false;
			_status.Failed = false;

			_task = Task.Run(() =>
			{
				try
				{
					RunPipeline();
					_status.Done = true;
				}
				catch (Exception ex)
				{
					_status.Failed = true;
					_status.Error = ex.Message;
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
			//float speed = 8f; // ʕಠᴥಠʔ lower for snappier, higher for smoother

			float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
			const float loadSpeed = 1.2f;
			float lerpT = 1f - MathF.Exp(-loadSpeed * dt);

			float time = (float)gameTime.TotalGameTime.TotalSeconds;

			displayProgress = MathHelper.Lerp(
				displayProgress,
				_status.Progress,
				lerpT
				);

			float t = (float)gameTime.TotalGameTime.TotalSeconds;
			previewOffset = new Vector2(
				MathF.Sin(t * 0.15f),
				MathF.Cos(t * 0.12f)
			) * 2f;

			previewAlpha = MathHelper.Clamp(displayProgress / 0.1f, 0f, 1f);

			if (displayProgress > _status.Progress)
			{
				displayProgress = _status.Progress;
			}

			if (MathF.Abs(displayProgress - _status.Progress) < 0.001f) {
				displayProgress = _status.Progress;
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
				h - 100);

				sb.Draw(CTX.pixel, fullScreenRect, bgColor);

				if (_preview != null)
				{
					float previewAlpha = MathHelper.Clamp(displayProgress / 0.10f, 0f, 1f);
					Color tint = Color.Black * (0.80f * previewAlpha);

					_preview.Draw(
						sb,
						Fonts.Get("12"),
						hm: _preview.PreviewHeight,
						tint: tint,
						offset: previewOffset,
						drawSpawn: false
						);
				}

				//DrawLoadingUI(sb);
				sb.DrawString(font, bar, barPos, Color.White * 0.5f);
				sb.DrawString(font, _status.Message, barPos + new Vector2(0, 36), Color.White * 0.8f);
			sb.End();
		}
	}
}