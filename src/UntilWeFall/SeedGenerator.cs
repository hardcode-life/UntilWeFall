using System;

namespace UntilWeFall
{
	public static class SeedGenerator
	{
		public static void Derive(string input, out int earthSeed, out int skySeed)
		{
			int combined = HashToInt32(input);
			SplitSeed(combined, out earthSeed, out skySeed);
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

		private static void SplitSeed(int combined, out int earth, out int sky)
		{
			unchecked
			{
				uint u = (uint)combined;
				earth = (int)((u >> 16) & 0xffff);
				sky = (int)(u & 0xffff);
			}
		}
	}
}