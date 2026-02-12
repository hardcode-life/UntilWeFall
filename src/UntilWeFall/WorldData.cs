using System;
using Microsoft.Xna.Framework;

namespace UntilWeFall
{
	public class WorldData
	{
		public float[,] world_HM {get; private set;}
		public float[,] TempVariance{get;private set;}
		public float[,] RuggednessMask{get;private set;}
		public float[,] MoistureBase{get;private set;}
		public float[,] MoistureDetail{get;private set;}
		public float[,] Moisture { get; private set; }
		public float[,] ForestDensity{get;private set;}

		public int worldWidth{get; }
		public int worldHeight {get; }

		public struct NoiseProfile
		{
			public readonly float Scale;
			public readonly int Octaves;
			public readonly float Persistence;
			public readonly float Lacunarity;
			public readonly float Amplitude;
			public readonly float Exponent;

			public NoiseProfile(
				float scale,
				int octaves,
				float persistence,
				float lacunarity,
				float amplitude = 1f,
				float exponent = 1f)
			{
				Scale = scale;
				Octaves = octaves;
				Persistence = persistence;
				Lacunarity = lacunarity;
				Amplitude = amplitude;
				Exponent = exponent;
			}
		}

		public WorldData(int width, int height)
		{
			worldWidth = width;
			worldHeight = height;
			world_HM = new float[width, height];
		}

		public static class HeightMapFactory
		{
			public static float[,] Generate(
			int width,
			int height,
			int seed,
			NoiseProfile profile)
			{
				return SimplexNoise.GenerateNoiseMap(
					width,
					height,
					seed,
					profile.Scale,
					profile.Octaves,
					profile.Persistence,
					profile.Lacunarity,
					profile.Amplitude,
					profile.Exponent,
					0,
					0
				);
			}
		}

		public void GenerateAll(int earthSeed, Action<float, string>? reportProgress = null)
		{
			reportProgress?.Invoke(0.15f, "Deriving seeds...");

			int seedHeight      = SeedGenerator.HashSeed(earthSeed, "HEIGHT");
			int seedRugged      = SeedGenerator.HashSeed(earthSeed, "RUGGED");
			int seedTempVar     = SeedGenerator.HashSeed(earthSeed, "TEMPVAR");
			int seedMoistBase   = SeedGenerator.HashSeed(earthSeed, "MOISTBASE");
			int seedMoistDetail = SeedGenerator.HashSeed(earthSeed, "MOISTDETAIL");
			int seedForestPatch = SeedGenerator.HashSeed(earthSeed, "FORESTPATCH");

			// GEOLOGY
			reportProgress?.Invoke(0.18f, "Generating terrain...");
			GenerateHeight(seedHeight);

			reportProgress?.Invoke(0.30f, "Shaping mountain belts...");
			GenerateRuggedness(seedRugged);

			// CLIMATE
			reportProgress?.Invoke(0.45f, "Calculating temperature...");
			GenerateTemperature(seedTempVar);

			reportProgress?.Invoke(0.60f, "Simulating moisture...");
			GenerateMoisture(seedMoistBase, seedMoistDetail);

			// ECOLOGY
			reportProgress?.Invoke(0.80f, "Growing forests...");
			GenerateForests(seedForestPatch);

			reportProgress?.Invoke(1.0f, "World complete.");
		}

		#region  GENERATE HEIGHT
		private void GenerateHeight(int seed)
		{
			var profile = new NoiseProfile(250f, 5, 0.50f, 2.0f, 1f, 1.25f);

			world_HM = HeightMapFactory.Generate(worldWidth, worldHeight, seed, profile);
		} 
		#endregion

		#region GENERATE RUGGEDNESS
		private void GenerateRuggedness(int seed)
		{
			var profile = new NoiseProfile(600f, 2, 0.50f, 2.0f, 1f, 1.2f);

			RuggednessMask = HeightMapFactory.Generate(worldWidth, worldHeight, seed, profile);

			// Apply ruggedness to height
			for (int y = 0; y < worldHeight; y++)
			{
				for (int x = 0; x < worldWidth; x++)
				{
					float rugged = RuggednessMask[x, y];
					world_HM[x, y] *= MathHelper.Lerp(0.8f, 1.4f, rugged);
				}
			}
		}
		#endregion

		#region GENERATE TEMPERATURE
		private void GenerateTemperature(int seed)
		{
			var profile = new NoiseProfile(500f, 3, 0.55f, 2.1f);

			TempVariance = HeightMapFactory.Generate(worldWidth, worldHeight, seed, profile);

			for (int y = 0; y < worldHeight; y++)
			{
				float latitude = MathF.Abs((y / (float)worldHeight) - 0.5f) * 2f;
				float latitudeTemp = 1f - latitude;

				for (int x = 0; x < worldWidth; x++)
				{
					float altitudeCooling = world_HM[x, y] * 0.5f; // max(0, height - seaLevel)

					TempVariance[x, y] = MathHelper.Clamp(
						(latitudeTemp * 0.7f)
						+ (TempVariance[x, y] * 0.2f)
						- altitudeCooling,
						0f,
						1f);
				}
			}
		}
		#endregion

		#region GENERATE MOISTURE
		private void GenerateMoisture(int seedBase, int seedDetail)
		{
			var baseP  = new NoiseProfile(350f, 5, 0.62f, 2.2f);
			var detail = new NoiseProfile(80f, 3, 0.52f, 2.4f);

			Moisture = new float[worldWidth, worldHeight];

			MoistureBase   = HeightMapFactory.Generate(worldWidth, worldHeight, seedBase, baseP);
			MoistureDetail = HeightMapFactory.Generate(worldWidth, worldHeight, seedDetail, detail);

			for (int y = 0; y < worldHeight; y++)
			{
				for (int x = 0; x < worldWidth; x++)
				{
					float moisture =
					MoistureBase[x, y] * 0.7f +
					MoistureDetail[x, y] * 0.3f;

					float altitudeDrying = world_HM[x, y] * 0.4f;

					Moisture[x, y] = MathHelper.Clamp(moisture - altitudeDrying, 0f, 1f);
				}
			}
		}
		#endregion

		#region GENERATE FORESTS
		private void GenerateForests(int seedPatch)
		{
			var patchProfile = new NoiseProfile(60f, 2, 0.50f, 2.2f, 1f, 1.6f);
			var patch = HeightMapFactory.Generate(worldWidth, worldHeight, seedPatch, patchProfile);

			float[,] slope = ComputeSlope();
			ForestDensity = new float[worldWidth, worldHeight];

			// Decide style ONCE per world
			int styleSeed = SeedGenerator.HashSeed(seedPatch, "CLIFFSTYLE");
			var styleProfile = new NoiseProfile(200f, 2, 0.5f, 2.0f);
			var styleMask = HeightMapFactory.Generate(worldWidth, worldHeight, styleSeed, styleProfile);

			for (int y = 0; y < worldHeight; y++)
			{
				for (int x = 0; x < worldWidth; x++)
				{
					float temp = TempVariance[x, y];
					float moisture = Moisture[x, y];
					float suitability = MathHelper.Clamp(temp * moisture, 0f, 1f);

					float s = slope[x, y];
					float linear = 1f - s;
					float gentle = 1f - (s * s);
					float harsh  = 1f - MathF.Sqrt(s);

					float t = styleMask[x, y]; // 0..1

					float cliffPenalty = 
					(t < 0.33f) ? linear : 
					(t < 0.66f ? gentle : harsh);


					ForestDensity[x, y] = suitability * patch[x, y] * cliffPenalty;
				}
			}
		}
		#endregion

//=================================================================
//-------------------HELPERS------------------------------------------------
//=================================================================

		private float[,] ComputeSlope()
		{
			var slope = new float[worldWidth, worldHeight];
			float maxSlope = 0f;

			// Pass 1: compute raw slope + find max
			for (int y = 1; y < worldHeight - 1; y++)
			{
				for (int x = 1; x < worldWidth - 1; x++)
				{
					float dx = world_HM[x + 1, y] - world_HM[x - 1, y];
					float dy = world_HM[x, y + 1] - world_HM[x, y - 1];

					float s = MathF.Sqrt(dx * dx + dy * dy);
					slope[x, y] = s;

					if (s > maxSlope) {
						maxSlope = s;
					}
				}
			}

			// Pass 2: normalize to 0..1 (guard against flat maps)
			if (maxSlope > 0f)
			{
				float inv = 1f / maxSlope;
				for (int y = 1; y < worldHeight - 1; y++)
				{
					for (int x = 1; x < worldWidth - 1; x++)
					{
						slope[x, y] *= inv;
					}
				}
			}

			// Optional: fill edges so they aren't always 0 slope
			// (simple copy from nearest interior)
			for (int x = 0; x < worldWidth; x++)
			{
				slope[x, 0] = slope[x, 1];
				slope[x, worldHeight - 1] = slope[x, worldHeight - 2];
			}

			for (int y = 0; y < worldHeight; y++)
			{
				slope[0, y] = slope[1, y];
				slope[worldWidth - 1, y] = slope[worldWidth - 2, y];
			}

			return slope;
		}


	}
}