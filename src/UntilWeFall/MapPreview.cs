using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace UntilWeFall
{
	public sealed class MapPreview
	{
		private const int PreviewW = 64;
		private const int PreviewH = 64;

		private readonly int[,] _digits = new int[PreviewW, PreviewH];
		private readonly int[,] _glyphNoise = new int[PreviewW, PreviewH];

		private bool _seeded;
		private int _earthSeed;
		private int _skySeed;

		private string _previewLabel = "";
		private Point _previewStart = Point.Zero;
		private Point _spawnTile = Point.Zero;

		private Random _rng = new Random(1);

		// layout
		private int _cellW = 12;
		private int _cellH = 12;
		private Vector2 _origin;

		public bool Seeded => _seeded;
		public string PreviewLabel => _previewLabel;
		public Point PreviewStart => _previewStart;
		public Point SpawnTile => _spawnTile;

		public void SetPreview(
			Vector2 origin, 
			int? cellW = null, 
			int? cellH = null)
		{
			_origin = origin;
			
			if (cellW.HasValue)
			{
				_cellW = cellW.Value;
			}

			if (cellH.HasValue)
			{
				_cellH = cellH.Value;
			}
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
		) {
			_seeded = true;
			_earthSeed = earthSeed;
			_skySeed = skySeed;

			// IMPORTANT: reseed every regenerate
			_rng = new Random(HashSeeds(_earthSeed, _skySeed));

			for (int attempt = 0; attempt < maxAttempts; attempt++)
			{
				Point start = PickEdgeWindowStart(
					worldW, worldH, 
					PreviewW, PreviewH, 
					_rng, 
					out _previewLabel);

				float[,] raw = SimplexNoise.GenerateNoiseMap(
					PreviewW, PreviewH,
					earthSeed,
					noiseScale,
					octaves,
					persistence,
					lacunarity,
					start.X,
					start.Y);

				float[,] smooth = SimplexNoise.SmoothNoiseMap(
					raw, 
					PreviewW, PreviewH, 
					kernelSize: 3);

				for (int y = 0; y < PreviewH; y++) 
				{
					for (int x = 0; x < PreviewW; x++)
					{
						int wx = start.X + x;
						int wy = start.Y + y;

						float n01 = SimplexNoise.SmoothStep(
							0f, 
							1f, 
							smooth[x, y]);

						float mask = IslandMask(
							wx, 
							wy, 
							worldW, 
							worldH, 
							coast, 
							landBiasPow);

						n01 = MathHelper.Clamp(n01 * mask, 0f, 1f);

						int d = (int)(n01 * 9.999f);
						d = ClampInt(d, 0, 9);

						_digits[x, y] = d;
						_glyphNoise[x, y] = _rng.Next(4);	
					}
				}

				float landRatio = ComputeLandRatio(_digits, PreviewW, PreviewH);
				
				if (landRatio >= minLandRatio)
				{
					_previewStart = start;
					_spawnTile = PickSpawnInsideWindow(
						start, 
						_digits, 
						PreviewW, 
						PreviewH, 
						_rng);
					return;
				}
			}

			// fallback
			_previewStart = Point.Zero;
			_previewLabel = "somewhere totally random, lol";
			_spawnTile = new Point(worldW / 2, worldH / 2);
		}

		public void Draw(SpriteBatch sb, SpriteFont font)
		{
			sb.Begin(samplerState: SamplerState.PointClamp);

			// grid
			for (int y = 0; y < PreviewH; y++) {
				for (int x = 0; x < PreviewW; x++) {
					int digit = _digits[x, y];

					Color color;
					string glyph;

					if (_seeded) {
						if (digit <= 1) {
							color = Hex.convert("#14c5dd")* 0.5f; // sea

							//int hash = (x * 73856093) ^ (y * 19349663) ^ _earthSeed;
							
							//int n = Math.Abs(hash) % 4;
							int n = _glyphNoise[x, y];

							glyph = n switch {
								0 => "W",
								1 => "w",
								2 => "m",
								_ => "M"
							};
						}
						else if (digit == 2) {
							color = Hex.convert("#5af9de") * 0.75f; // reef

							//glyph = "%";
							
							int n = _glyphNoise[x, y];
							
							glyph = n switch {
								0 => "+",
								1 => "-",
								2 => "_",
								_ => "/"
							};
						}
						else if (digit == 3) {
							color = Hex.convert("#fdedaf") ; // beaches

							//glyph = "$";
							int n = _glyphNoise[x, y];
							
							glyph = n switch {
								0 => "#",
								1 => "/",
								2 => "+",
								_ => "%"
							};
						}
						else {
							color = Hex.convert("#b0e832") ; // land

							//glyph = "#";
							int n = _glyphNoise[x, y];
							
							glyph = n switch {
								0 => "'",
								1 => "\"",
								2 => ",",
								_ => "."
							};
						}
					}
					else {
						color = Color.DarkGray;
						glyph = "#";
					}

					float shade = MathHelper.Clamp(
						0.25f + (digit * 0.07f),
						0.25f,
						1f);

					sb.DrawString(
						font,
						glyph,
						_origin + new Vector2(x * _cellW, y * _cellH),
						color * shade);
				}
			}
				// spawn marker (draw once)
				if (_seeded)
				{
					int sx = _spawnTile.X - _previewStart.X;
					int sy = _spawnTile.Y - _previewStart.Y;

					if (sx >= 0 && sx < PreviewW && sy >= 0 && sy < PreviewH)
					{
						sb.DrawString(
							font,
							"@",
							_origin + new Vector2(
								sx * _cellW, 
								sy * _cellH),
							Color.Yellow
						);
					}
				}	
				// label (draw once)
				sb.DrawString(
					Fonts.Get("16"), 
					$"Landfall : {_previewLabel}", 
					_origin + new Vector2(
						32, 
						(PreviewH * _cellH) + 8), 
					Color.White);

			sb.End();
			
		}

		// ---------- helpers ----------

		private static int HashSeeds(int a, int b)
		{
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
			if (v < min) return min;
			if (v > max) return max;
			return v;
		}

		private static float IslandMask(int x, int y, int w, int h, float coast, float landBiasPow)
		{
			int distLeft = x;
			int distRight = (w - 1) - x;
			int distTop = y;
			int distBottom = (h - 1) - y;

			int distToEdge = Math.Min(
				Math.Min(distLeft, distRight), 
				Math.Min(distTop, distBottom));

			float t = MathHelper.Clamp(distToEdge / coast, 0f, 1f);
			t = t * t * (3f - 2f * t);
			t = MathF.Pow(t, landBiasPow);

			return t;
		}

		private static Point PickEdgeWindowStart(
			int worldW, 
			int worldH, 
			int previewW, 
			int previewH, 
			Random rng, 
			out string label)
		{
			int maxX = worldW - previewW;
			int maxY = worldH - previewH;

			int choice = rng.Next(8);

			switch (choice) {
				case 0: 
					label = "North West"; 
					return new Point(0, 0);
				case 1: 
					label = "North East"; 
					return new Point(maxX, 0);
				case 2: 
					label = "South West"; 
					return new Point(0, maxY);
				case 3: 
					label = "South East"; 
					return new Point(maxX, maxY);
				case 4: 
					label = "North";  
					return new Point(rng.Next(0, maxX + 1), 0);
				case 5: 
					label = "South";  
					return new Point(rng.Next(0, maxX + 1), maxY);
				case 6: 
					label = "West";  
					return new Point(0, rng.Next(0, maxY + 1));
				default: 
					label = "East"; 
					return new Point(maxX, rng.Next(0, maxY + 1));
			}
		}

		private static float ComputeLandRatio(int[,] digits, int w, int h)
		{
			int land = 0;
			int total = w * h;

			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++)
				{
					if (digits[x, y] >= 3) {
						land++;
					}
				}
			}

			return land / (float)total;
		}

		private static Point PickSpawnInsideWindow(
			Point start, 
			int[,] digits, 
			int w, 
			int h, 
			Random rng)
		{
			List<Point> inland = new();
			List<Point> coast = new();

			for (int y = 0; y < h; y++) {
				for (int x = 0; x < w; x++)
				{
					int d = digits[x, y];

					if (d >= 4) {
						inland.Add(new Point(
							start.X + x, 
							start.Y + y));
					}
					else if (d == 3) {
						coast.Add(new Point(
							start.X + x, 
							start.Y + y));
					}
				}
			}

			if (inland.Count > 0) {
				return inland[rng.Next(inland.Count)];
			}

			if (coast.Count > 0) {
				return coast[rng.Next(coast.Count)];
			}

			return new Point(start.X + w / 2, start.Y + h / 2);
		}
	}
}
