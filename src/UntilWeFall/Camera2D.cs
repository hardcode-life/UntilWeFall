using Microsoft.Xna.Framework;

namespace UntilWeFall
{
	public class Camera2D
	{
		// World position at the CENTER of the screen
		public Vector2 Position { get; private set; } = Vector2.Zero;

       	 	public float Zoom { get; private set; } = 1f;
        		public float Rotation { get; private set; } = 0f;

		public int ViewportWidth { get; private set; }
		public int ViewportHeight { get; private set; }

        		public float MinZoom { get; set; } = 0.25f;
        		public float MaxZoom { get; set; } = 4.0f;

        		public Camera2D(int viewportWidth, int viewportHeight)
        		{
            		ViewportWidth = viewportWidth;
            		ViewportHeight = viewportHeight;
        		}

        		public void SetViewportSize(int width, int height)
        		{
            		ViewportWidth = width;
            		ViewportHeight = height;
        		}

        		public Vector2 GetScreenCenter()
            		=> new Vector2(ViewportWidth * 0.5f, ViewportHeight * 0.5f);

        		public Matrix GetViewMatrix()
        		{
            		return
                			Matrix.CreateTranslation(new Vector3(-Position, 0f)) *
                			Matrix.CreateRotationZ(Rotation) *
                			Matrix.CreateScale(Zoom, Zoom, 1f) *
                			Matrix.CreateTranslation(new Vector3(GetScreenCenter(), 0f));
        		}

        		// Move camera by a delta in WORLD units
		public void Pan(Vector2 deltaWorld)
        		{
            		Position += deltaWorld;
        		}

        		// Convert screen pixel -> world position
        		public Vector2 ScreenToWorld(Vector2 screenPosition)
        		{
            		Matrix inverse = Matrix.Invert(GetViewMatrix());
            		return Vector2.Transform(screenPosition, inverse);
        		}

        		// Convert world position -> screen pixel
        		public Vector2 WorldToScreen(Vector2 worldPosition)
        		{
            		return Vector2.Transform(worldPosition, GetViewMatrix());
        		}

        		// Zoom while keeping the world point under the cursor fixed
        		public void ZoomAtScreenPoint(Vector2 screenPoint, float zoomFactor)
        		{
            		Vector2 before = ScreenToWorld(screenPoint);

            		Zoom = MathHelper.Clamp(Zoom * zoomFactor, MinZoom, MaxZoom);

            		Vector2 after = ScreenToWorld(screenPoint);

            		Position += (before - after);
        		}

        		// Optional helpers (use later if you want)
        		public void SetPosition(Vector2 worldCenter) => Position = worldCenter;

        		public void SetZoom(float zoom) => Zoom = MathHelper.Clamp(zoom, MinZoom, MaxZoom);

        		public void SetRotation(float radians) => Rotation = radians;
    	}
}
