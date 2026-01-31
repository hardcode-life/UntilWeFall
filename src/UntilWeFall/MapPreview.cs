using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace UntilWeFall
{
	public sealed class MapPreview
	{
		private const int PreviewW = 64; // width
		private const int PreviewH = 64; // height

		private int[,] _previewDigits = new int[PreviewW, PreviewH];
		private string _previewCorner = "";
		private Random _rng = new Random(1);

		private bool _seeded = false;	
		private int _earthSeed;
		private int _skySeed;

		private readonly int[,] _digits = new int[PreviewW, PreviewH];

		private string _previewLabel = "";
		private Point _previewStart = Point.Zero;
		private Point _spawnTile = Point.Zero;

		// preview window position
		private int _cellW = 14;
		private int _cellH = 18;
		private Vector2 _origin;

		public bool Seeded => _seeded;
		public string PreviewLabel => _previewLabel;
		public Point PreviewStart => _previewStart;
		public Point SpawnTile => _spawnTile;

		public void SetPreview(Vector2 origin, int cellW = 14, int cellH = 18)
		{
			_origin = origin;
			_cellW = cellW;
			_cellH = cellH;
		}

		public void Regenerate(
			int earthSeed,
			int skySeed,
			int worldW,
			int worldH,
			float minLandRatio = 0.45f,
			int maxAttempts = 12,
			float noiseScale = 200f,
			int octaves = 5,
			float persistence = 0.6f,
			float lacunarity = 2f,
			float coast = 30f,
			float landBiasPow = 0.7f
			)
		{
			_seeded = true;
			_earthSeed = earthSeed;
			_skySeed = skySeed;

			_rng ??= new Random(HashSeeds(_earthSeed, _skySeed));

			for (int attempt = 0; attempt < maxAttempts; attempt++)
			{
				Point start = PickEdgeWindowStart(worldW, worldH, PreviewW, PreviewH, _rng, out _previewLabel);

				float[,] raw = SimplexNoise.GenerateNoiseMap(
					PreviewW, PreviewH,
					earthSeed,
					noiseScale,
					octaves,
					persistence,
					lacunarity,
					start.X,
					start.Y);

				float[,] smooth = SimplexNoise.SmoothNoiseMap(raw, PreviewW, PreviewH, kernelSize: 3);

				for (int y = 0; y < PreviewH; y++)
				for (int x = 0; x < PreviewW; x++)
				{
					int wx = start.X + x;
					int wy = start.Y + y;

					float n01 = SimplexNoise.SmoothStep(0f, 1f, smooth[x, y]);

					float mask = IslandMask(wx, wy, worldW, worldH, coast, landBiasPow);
					n01 = MathHelper.Clamp(n01 * mask, 0f, 1f);

					int d = (int)(n01 * 9.999f);
					d = ClampInt(d, 0, 9);

					_digits[x, y] = d;
				}

				float landRatio = ComputeLandRatio(_digits, PreviewW, PreviewH);

				if (landRatio >= minLandRatio)
				{
					_previewStart = start;
					_spawnTile = PickSpawnInsideWindow(start, _digits, PreviewW, PreviewH, _rng);
					return;
				}
			}

			// If we failed every attempt, still set something sane:
			_previewStart = Point.Zero;
			_previewLabel = "fallback";
			_spawnTile = new Point(worldW / 2, worldH / 2);
		}

		public void Draw(SpriteBatch sb, SpriteFont font)
		{
			sb.Begin(samplerState: SamplerState.PointClamp);

			for (int y = 0; y < PreviewH; y++) {
				for (int x = 0; x < PreviewW; x++) {
					int digit = _digits[x, y];

					Color color;
					string glyph;

					if (_seeded) {
						if (digit <= 1) { // sea
							color = Color.Blue * 0.5f;

							int hash = (x * 73856093) ^ (y * 19349663) ^ _earthSeed;
							int n = Math.Abs(hash) % 4;

							glyph = n switch
							{
							0 => ".",
							1 => ",",
							2 => "'",
							_ => "+"
							};
						}
						else if (digit == 2) { // reef
							color = Color.SkyBlue * 0.75f;
							glyph = "%";
						}
						else if (digit == 3) { // coast						
							color = Color.SandyBrown;
							glyph = "$";
						}
						else { // inland
							color = Color.DarkGreen;
							glyph = "#";
						}
					}
					else {
						color = Color.DarkGray;
						glyph = "#";
					}

					float shade = MathHelper.Clamp(
						0.25f + (digit * 0.07f),
						0.25f,
						1f
					);

					sb.DrawString(
						font,
						glyph,
						_origin + new Vector2(x * _cellW, y * _cellH),
						color * shade
					);
				}

				// Spawn marker
				if (_seeded)
				{
					int sx = _spawnTile.X - _previewStart.X;
					int sy = _spawnTile.Y - _previewStart.Y;

					if (sx >= 0 && sx < PreviewW && sy >= 0 && sy < PreviewH)
					{
						sb.DrawString(
							font,
							"@",
							_origin + new Vector2(sx * _cellW, sy * _cellH),
							Color.Yellow
						);
					}
				}

				sb.DrawString(font, $"Preview: {_previewLabel}", _origin + new Vector2(-72, 0), Color.White);
			}	
			sb.End();	
		}

		private static int HashSeeds(int a, int b)
		{
			// takes two integers (a, b) and mixes them into a new integer that's nice and random.
			unchecked
			{
				int h = 17;
				h = h * 31 + a;
				h = h * 31 + b;
				return h;
			}
		}
		private static int ClampInt(int v, int min, int max)
		{
			if (v < min) {
				return min;
			}
			if (v > max) {
				return max;
			}

			return v;
		}

		private static float IslandMask(int x, int y, int w, int h, float coast, float landBiasPow)
		{
			// Distance to nearest edge
			int distLeft = x;
			int distRight = (w - 1) - x;
			int distTop = y;
			int distBottom = (h - 1) - y;

			int distToEdge = Math.Min(
				Math.Min(
					distLeft, 
					distRight), 
				Math.Min(
					distTop, 
					distBottom));

			float t = MathHelper.Clamp(distToEdge / coast, 0f, 1f);
			t = t * t * (3f - 2f * t);          // smoothstep
			t = MathF.Pow(t, landBiasPow);      // bias toward land

			return t;
		}

		private static Point PickEdgeWindowStart(
			int worldW, 
			int worldH,
			int previewW, 
			int previewH,
			Random rng,
			out string label
		) {
			int maxX = worldW - previewW;
			int maxY = worldH - previewH;

			int choice = rng.Next(8);

			switch (choice)
			{
				// Corners
				case 0: 
					label = "NW"; 
					return new Point(0, 0);
				case 1: 
					label = "NE"; 
					return new Point(maxX, 0);
				case 2: 
					label = "SW"; 
					return new Point(0, maxY);
				case 3: 
					label = "SE"; 
					return new Point(maxX, maxY);

				// Edges
				case 4: 
					label = "N";  
					return new Point(rng.Next(0, maxX + 1), 0);
				case 5: 
					label = "S";  
					return new Point(rng.Next(0, maxX + 1), maxY);
				case 6: 
					label = "W";  
					return new Point(0, rng.Next(0, maxY + 1));
				default: 
					label = "E"; 
					return new Point(maxX, rng.Next(0, maxY + 1));
			}
		}

		private static float ComputeLandRatio(int[,] digits, int w, int h)
		{
			int land = 0;
			int total = w * h;

			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++) {
					if (digits[x, y] >= 3) land++; // coast + inland
				}
			}

			return (land / (float)total);
		}

		private static Point PickSpawnInsideWindow(Point start, int[,] digits, int w, int h, Random rng)
		{
			List<Point> inland = new();
			List<Point> coast = new();

			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++)
				{
					int d = digits[x, y];
					if (d >= 4) {
						inland.Add(new Point(start.X + x, start.Y + y));
					}
					else if (d == 3) {
						coast.Add(new Point(start.X + x, start.Y + y));
					}
				}
			}

			if (inland.Count > 0) return inland[rng.Next(inland.Count)];
			if (coast.Count > 0) return coast[rng.Next(coast.Count)];

			return new Point(start.X + w / 2, start.Y + h / 2);
		}
	}
}