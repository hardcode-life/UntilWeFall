using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace UntilWeFall
{
	public sealed class HeightMap
	{
		public int Width { get; }
		public int Height { get; }

		private readonly float[,] _height;
		private readonly bool[,] _isWater;
		private readonly bool[,] _isOcean;

		public float SeaLevel { get; }
		public float LakeLevel { get; } // unused for now, reserved for future fancy stuff

		public HeightMap(int width, int height, float seaLevel, float lakeLevel)
		{
			Width = width;
			Height = height;
			SeaLevel = seaLevel;
			LakeLevel = lakeLevel;

			_height = new float[width, height];
			_isWater = new bool[width, height];
			_isOcean = new bool[width, height];
		}

		public float GetHeight(int x, int y) => _height[x, y];
		public bool IsWater(int x, int y) => _isWater[x, y];
		public bool IsOcean(int x, int y) => _isWater[x, y] && _isOcean[x, y];
		public bool IsLake(int x, int y) => _isWater[x, y] && !_isOcean[x, y];
		public bool IsLand(int x, int y) => !_isWater[x, y];

		public void SetHeight(int x, int y, float h01)
		{
			float v = MathHelper.Clamp(h01, 0f, 1f);
			_height[x, y] = v;

			_isWater[x, y] = v < SeaLevel;
			_isOcean[x, y] = false;
		}

		public void ClearOceanFlags()
		{
			Array.Clear(_isOcean, 0, _isOcean.Length);
		}

		public void ClassifyOceans()
		{
			ClearOceanFlags();

			var q = new Queue<Point>();

			void TryEnqueue(int x, int y)
			{
				if ((uint)x >= (uint)Width || (uint)y >= (uint)Height) return;
				if (!_isWater[x, y]) return;
				if (_isOcean[x, y]) return;

				_isOcean[x, y] = true;
				q.Enqueue(new Point(x, y));
			}

			// Seed edges
			for (int x = 0; x < Width; x++)
			{
				TryEnqueue(x, 0);
				TryEnqueue(x, Height - 1);
			}
			for (int y = 0; y < Height; y++)
			{
				TryEnqueue(0, y);
				TryEnqueue(Width - 1, y);
			}

			// Flood-fill ocean
			while (q.Count > 0)
			{
				var p = q.Dequeue();
				TryEnqueue(p.X + 1, p.Y);
				TryEnqueue(p.X - 1, p.Y);
				TryEnqueue(p.X, p.Y + 1);
				TryEnqueue(p.X, p.Y - 1);
			}
		}

		public float GetSlope4Way(int x, int y)
		{
			float h = _height[x, y];
			float sum = 0f;
			int count = 0;

			void Add(int nx, int ny)
			{
				if ((uint)nx >= (uint)Width || (uint)ny >= (uint)Height) return;
				sum += MathF.Abs(h - _height[nx, ny]);
				count++;
			}

			Add(x + 1, y);
			Add(x - 1, y);
			Add(x, y + 1);
			Add(x, y - 1);

			return count > 0 ? sum / count : 0f;
		}
	}
}
