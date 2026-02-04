using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class InputField
{
	public Rectangle Bounds;

	public string Text;
	public SpriteFont Font;
	public Texture2D Background;

	public string defaultString = "";
	public string inputString = "";

	public bool _hovered { get; private set; }
	public Action OnClick;
	private bool _wasPressed;

	public bool Enabled = true;
	public bool Selected = false;
	public InputField(Rectangle bounds, string _defaultString, SpriteFont font, Action onClick, Texture2D background)
	{
		Bounds = bounds;
		defaultString = _defaultString;

		Font = font;
		Background = background;
		OnClick = onClick;
	}

	public void Update(MouseState mouse)
	{
		if (!Enabled)
		{
			return;
		}
		else {	
			Point mousePoint = new Point(mouse.X, mouse.Y);
			_hovered = Bounds.Contains(mousePoint);

			if (_hovered)
			{
				Mouse.SetCursor(MouseCursor.Hand);
			}

			bool _isPressed = mouse.LeftButton == ButtonState.Pressed;

			if (_hovered && _isPressed && !_wasPressed)
			{
				// if Left BTN is pressed, Invoke action
				OnClick?.Invoke();
				Text = "hello";
			}
			_wasPressed = _isPressed;
		}
	}

	public void Draw(SpriteBatch sb)
	{
		var tint = !Enabled ? Color.DarkGray :
		_hovered ? Color.Gray :
		Color.White;

		if (Background != null)
		sb.Draw(Background, Bounds, tint);

		if (Font == null) return;

		string shown = string.IsNullOrEmpty(inputString) ? defaultString : inputString;

		if (Font != null)
		{
			Vector2 textPos = new Vector2(Bounds.X,Bounds.Y);

			sb.DrawString(Font, shown, textPos, Color.White);
		}
	}
}