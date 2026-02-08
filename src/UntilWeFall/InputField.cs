using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using UntilWeFall;

public class InputField
{
	public Rectangle Bounds;

	public string Text;
	public SpriteFont Font;
	public Texture2D Background;
	public Color textColor;

	public string Placeholder = "";
	public string Value  = "";

	public bool _hovered { get; private set; }
	public Action OnClick;
	private bool _wasPressed;

	public bool Enabled = true;
	public bool IsFocused { get; private set; }

	public Action? OnFocus;
	public Action? OnBlur;

	public int MaxLength = 128;
	public void SetBounds(Rectangle r) => Bounds = r;

	public InputField(
		Rectangle bounds, 
		string _defaultString, 
		SpriteFont font, 
		Action onClick, 
		Texture2D background,
		Color color)
	{
		Bounds = bounds;
		Placeholder  = _defaultString;

		Font = font;
		Background = background;
		textColor = color;
		OnClick = onClick;
	}

	public void Focus()
	{
		if (!Enabled) {
			return;
		}

		if (IsFocused) {
			return;
		}

		IsFocused = true;
		OnFocus?.Invoke();
	}

	public void Blur()
	{
		if (!IsFocused) {
			return;
		}

		IsFocused = false;
		OnBlur?.Invoke();
	}

	public void Append(char c)
	{
		if (!Enabled || !IsFocused) return;
		if (Value.Length >= MaxLength) return;

		Value += c;
	}

	public void Update(MouseState mouse)
	{
		if (!Enabled)
		{
			_hovered = false;
			return;
		}

		_hovered = Bounds.Contains(mouse.X, mouse.Y);

		if (_hovered) {
			Mouse.SetCursor(MouseCursor.Hand);
		}
	}

	public void Draw(SpriteBatch sb)
	{
		Color tint =
			!Enabled ? Color.DarkGray :
			IsFocused ? Color.White :
			_hovered ? Color.Gray :
			Color.White;

#region INPUT BACKGROUND COLOR
		Color inputBG = Hex.convert("#1c242a");
		if (Background != null) {
			sb.Draw(Background, Bounds, inputBG);
		}
#endregion

		if (Font == null) return;

		bool showPlaceholder = string.IsNullOrEmpty(Value) && !IsFocused;
        		string toShow = showPlaceholder ? Placeholder : Value;

		// Placeholder should be dimmer
        		Color finalColor = showPlaceholder ? (textColor * 0.45f) : textColor;

#region PADDING
        		Vector2 pos = new Vector2(16, 5);
#endregion

		if (Font != null)
		{
			Vector2 textPos = new Vector2(Bounds.X,Bounds.Y);
			sb.DrawString(
				Font, 
				toShow, 
				textPos + pos, 
				finalColor);
		}

		if (IsFocused)
		{
			// Put caret at end of current text
			float w = Font.MeasureString(toShow).X;
			sb.DrawString(Font, "|", pos + new Vector2(Bounds.X + w + 2, Bounds.Y + 5), finalColor);
		}
	}

	public void Backspace()
	{
		if (Value.Length > 0) {
			Value = Value[..^1];
		}
	}

	public void Clear()
	{
		if (!Enabled) {
			return;
		}
		Value = "";
	}

	public void SetValue(string value)
	{
		Value = value ?? "";
		if (value.Length > MaxLength) {
			value = value.Substring(0, MaxLength);
		}

		Value = value;	
	}

	public InputField WithPlaceholder(string placeholder)
	{
		SetPlaceholder(placeholder);
		return this;
	}

	public void SetPlaceholder(string placeholder)
	{
		Placeholder = placeholder ?? "";
	}

	public InputField WithValue(string value)
	{
		SetValue(value);
		return this;
	}
}