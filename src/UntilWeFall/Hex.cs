using Microsoft.Xna.Framework;
using System;

namespace UntilWeFall
{
	public static class Hex
	{
		public static Color convert(string hex) {
			if (hex.StartsWith("#")) {
				hex = hex[1..];
			}

			byte r = Convert.ToByte(hex.Substring(0, 2), 16);
			byte g = Convert.ToByte(hex.Substring(2, 2), 16);
			byte b = Convert.ToByte(hex.Substring(4, 2), 16);

			byte a = hex.Length >= 8
				? Convert.ToByte(hex.Substring(6, 2), 16)
				: (byte)255;
			
			return new Color(r, g, b, a);
		}
	}
}