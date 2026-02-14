using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace UntilWeFall
{
	public sealed class CensusCounter
	{
		public Rectangle maleBounds, femBounds;

		public int Min = 0;
		public int Max = 999;
		public int Step = 1;

		public string Name = "";

		public int MaleValue
		{
			get => _maleValue;

			set
			{
				int clamped = Math.Clamp(value, Min, Max);
				if (clamped == _maleValue) return;
				_maleValue = clamped;
				OnValueChanged_male?.Invoke(_maleValue);
			}
		}

		public int FemaleValue
		{
			get => _femaleValue;

			set
			{
				int clamped = Math.Clamp(value, Min, Max);
				if (clamped == _femaleValue) return;
				_femaleValue = clamped;
				OnValueChanged_female?.Invoke(_femaleValue);
			}
		}

		public event Action<int>? OnValueChanged_male;
		public event Action<int>? OnValueChanged_female;

		private int _maleValue, _femaleValue;

		// Visuals
		public SpriteFont Font = null!;
		public Texture2D Background = null!;
		public Texture2D ArrowLeftTex = null!;
		public Texture2D ArrowRightTex = null!;
		public Texture2D Pixel = null!; // your 1x1

		public Color TextColor = Color.White;
		public Color BgNormal = Hex.convert("#1c242a");
		public Color BgHover = Hex.convert("#2a353d");
		public Color BgDisabled = Hex.convert("#141b1f");

		public bool Enabled = true;

		// Buttons
		private Button male_btnLeft = null!;
		private Button male_btnRight = null!;
		private Button fem_btnLeft = null!;
		private Button fem_btnRight = null!;
		private Rectangle male_leftRect, male_rightRect;
		private Rectangle fem_leftRect, fem_rightRect;
		private Vector2 _namePos;
		private bool male_hovered, fem_hovered;

		// Layout knobs
		public int ArrowW = 8;
		public int ArrowH = 24;
		public int Gap = 8;
		public int ValuePadX = 10;
		public int ValuePadY = 6;

		public void Initialize(int start_maleValue, int start_femValue, string name)
		{
			_maleValue = Math.Clamp(start_maleValue, Min, Max);

			male_btnLeft = new Button(Rectangle.Empty, Color.Transparent)
			{
				Background = null, // we'll draw arrow texture ourselves
				OnClick = () => { if (Enabled) MaleValue -= Step; }
			};

			male_btnRight = new Button(Rectangle.Empty, Color.Transparent)
			{
				Background = null,
				OnClick = () => { if (Enabled) MaleValue += Step; }
			};

			_femaleValue = Math.Clamp(start_femValue, Min, Max);

			fem_btnLeft = new Button(Rectangle.Empty, Color.Transparent)
			{
				Background = null, // we'll draw arrow texture ourselves
				OnClick = () => { if (Enabled) FemaleValue -= Step; }
			};

			fem_btnRight = new Button(Rectangle.Empty, Color.Transparent)
			{
				Background = null,
				OnClick = () => { if (Enabled) FemaleValue += Step; }
			};

			Name = name;

			Reflow();
		}

		public void Reflow()
		{
			femBounds = new Rectangle(
				maleBounds.X + 80, // 80 = distance between male and female icons
				maleBounds.Y,
				maleBounds.Width,
				maleBounds.Height);

			// Left arrow
			male_leftRect = new Rectangle(
			maleBounds.X - (int)(Font.MeasureString("99").X *0.5f),
			maleBounds.Y,
			ArrowW,
			ArrowH
			);

			// Right arrow
			male_rightRect = new Rectangle(
			maleBounds.X + (int)(Font.MeasureString("99").X * 1.5f),
			maleBounds.Y,
			ArrowW,
			ArrowH
			);

			// Left arrow
			fem_leftRect = new Rectangle(
			femBounds.X - (int)(Font.MeasureString("99").X *0.5f),
			femBounds.Y,
			ArrowW,
			ArrowH
			);

			// Right arrow
			fem_rightRect = new Rectangle(
			femBounds.X + (int)(Font.MeasureString("99").X * 1.5f),
			femBounds.Y,
			ArrowW,
			ArrowH
			);

			// Value box in the middle
			_namePos = new Vector2(
				maleBounds.X - ((int)Font.MeasureString(Name).X + 32),
				maleBounds.Y + 2
			);

			male_btnLeft.Bounds = male_leftRect;
			male_btnRight.Bounds = male_rightRect;

			fem_btnLeft.Bounds = fem_leftRect;
			fem_btnRight.Bounds = fem_rightRect;
		}

		public void Update(MouseState mouse)
		{
			if (!Enabled)
			{
				male_hovered = false;
				fem_hovered = false;
				return;
			}

			male_hovered = maleBounds.Contains(mouse.Position);
			fem_hovered = femBounds.Contains(mouse.Position);

			// Only buttons need click logic
			male_btnLeft.Update(mouse);
			male_btnRight.Update(mouse);
			fem_btnLeft.Update(mouse);
			fem_btnRight.Update(mouse);
		}

		public void Draw(SpriteBatch sb)
		{
			if (Font == null || Pixel == null) {
				return;
			}

			Color bgM = 
				!Enabled ? BgDisabled 
				: (male_hovered ? BgHover : BgNormal);
			Color bgF = 
				!Enabled ? BgDisabled 
				: (fem_hovered  ? BgHover : BgNormal);
			
			sb.Draw(Pixel, maleBounds, bgM);
			sb.Draw(Pixel, femBounds, bgF);

			// Value text (centered)
			string sM = _maleValue.ToString();
			string sF = _femaleValue.ToString();
			Vector2 size = Font.MeasureString("0");
			Vector2 male_textPos = new Vector2(
				maleBounds.X + (size.X / 2),
				maleBounds.Y
				);
			Vector2 fem_textPos = new Vector2(
				femBounds.X + (size.X / 2),
				femBounds.Y
				);

			sb.DrawString(Font, sM, male_textPos, TextColor);
			sb.DrawString(Font, sF, fem_textPos, TextColor);

			sb.DrawString(Font, Name, _namePos, TextColor);

			// Arrows
			if (ArrowLeftTex != null) {
				sb.Draw(ArrowLeftTex, male_leftRect, Enabled ? Color.White : Color.White * 0.35f);
			}

			if (ArrowRightTex != null) {
				sb.Draw(ArrowRightTex, male_rightRect, Enabled ? Color.White : Color.White * 0.35f);
			}

			if (ArrowLeftTex != null) {
				sb.Draw(ArrowLeftTex,fem_leftRect, Enabled ? Color.White : Color.White * 0.35f);
			}

			if (ArrowRightTex != null) {
				sb.Draw(ArrowRightTex, fem_rightRect, Enabled ? Color.White : Color.White * 0.35f);
			}
		}
	}
}
