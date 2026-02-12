using System;

namespace UntilWeFall
{
	public static class SeedGenerator
	{
		public static int worldSEED;
		public static void Derive(string input, out int earthSeed, out int skySeed)
		{
			int combined = HashToInt32(input);
			worldSEED = combined;

			earthSeed = HashSeed(combined, "EARTH");
			skySeed   = HashSeed(combined, "SKY");
			//SplitSeed(combined, out earthSeed, out skySeed);
		}

		public static void Derive_fromLoading(out int earthSeed, out int skySeed)
		{
			earthSeed = HashSeed(worldSEED, "EARTH");
			skySeed   = HashSeed(worldSEED, "SKY");
		}

		private static int HashToInt32(string s)
		{
			unchecked 
			/*
			this basically serves as the plug when math overflows the size of the type. hahaha
			*/
			{
				// 32bit Decimal parameter for offset basis
				const uint fnvOffset = 2166136261; 

				// 32bit Decimal parameter for prime
				const uint fnvPrime = 16777619; 

				uint hash = fnvOffset;

				// treat null as empty
				if (s == null) 
				{
					s = ""; 
				}

				for (int i = 0; i < s.Length; i++)
				{
					hash ^= s[i];
					hash *= fnvPrime;
				}

				return (int)hash;
			}
		}

		public static int HashSeed(int baseSeed, string label)
		{
			unchecked
			{
				int hash = 17;
				hash = hash * 31 + baseSeed;
				for (int i = 0; i < label.Length; i++) {
					hash = hash * 31 + label[i];
				}	
				return hash;
			}
		}

		/*private static void SplitSeed(int combined, out int earth, out int sky)
		{
			unchecked
			{
				uint u = (uint)combined;
				earth = (int)((u >> 32) & 0xffff);
				sky = (int)(u & 0xffff);
			}
		}*/
	}
}