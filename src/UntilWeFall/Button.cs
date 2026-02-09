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

	public Color _color;

	public Button(Rectangle bounds, Color color)
	{
		Bounds = bounds;
		_color = color;
	}

	public void Update(MouseState mouse)
	{
		if (!Enabled) { 
			return;
		}

		Point mousePoint = new Point(mouse.X, mouse.Y);
		_hovered = Bounds.Contains(mousePoint);

		if (_hovered) {
			Mouse.SetCursor(MouseCursor.Hand);
		}

		bool isPressed = mouse.LeftButton == ButtonState.Pressed;

		if (_hovered && isPressed && !_wasPressed) {
			OnClick?.Invoke();
		}

		_wasPressed = isPressed;
	}


	public void Draw(SpriteBatch sb)
	{
		var tint = !Enabled ? Color.DarkGray :
		_hovered ? Color.Gray : 
		_color;

		if (Background != null) {
			sb.Draw(
				Background,
				Bounds,
				_hovered ? Color.Gray : _color);
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
				_color);
		}
	}
}