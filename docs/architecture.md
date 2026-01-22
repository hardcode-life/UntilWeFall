## Camera System

The camera is implemented as a 2D transform using a view matrix.  
All world objects are drawn in world-space, and the camera matrix is applied via `SpriteBatch.Begin(transformMatrix: ...)`.

Features:
- Position (Vector2)
- Zoom (float)
- Screen-to-world conversion (for future tile picking.. which I'll 100% need and is absolutely essential to gameplay)
