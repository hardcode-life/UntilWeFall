using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class Button
{
	public Rectangle Bounds;

	//can be optional
		public string Text;
		public SpriteFont Font;
		public Texture2D Background;

	public bool _hovered { get; private set; }
	public Action OnClick;
	private bool _wasPressed;

	public bool Enabled = true;

	public Button(Rectangle bounds)
	{
		Bounds = bounds;
	}

	public void Update(MouseState mouse)
	{
		if (!Enabled)
		{
			return;
		}
		else {
			if (_hovered)
			{
				Mouse.SetCursor(MouseCursor.Hand);
			}
			
			Point mousePoint = new Point(mouse.X, mouse.Y);
			_hovered = Bounds.Contains(mousePoint);

			bool _isPressed = mouse.LeftButton == ButtonState.Pressed;

			if (_hovered && _isPressed && !_wasPressed)
			{
				// if Left BTN is pressed, Invoke action
				OnClick?.Invoke();
			}
			_wasPressed = _isPressed;
		}
	}

	public void Draw(SpriteBatch sb)
	{
		var tint = !Enabled ? Color.DarkGray :
		_hovered ? Color.Gray : 
		Color.White;

		if (Background != null) {
			sb.Draw(
				Background,
				Bounds,
				_hovered ? Color.Gray : Color.White);
		}
		
		if (!string.IsNullOrEmpty(Text) && Font != null) {	
			Vector2 textSize = Font.MeasureString(Text);
			Vector2 textPos = new Vector2(
				Bounds.X + (Bounds.Width - textSize.X) / 2,
				Bounds.Y + (Bounds.Height - textSize.Y) / 2
				);
			sb.DrawString(
				Font,
				Text,
				textPos,
				Color.White);
		}
	}
}