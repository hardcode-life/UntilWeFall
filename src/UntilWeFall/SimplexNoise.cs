using System;
using Microsoft.Xna.Framework;

 namespace UntilWeFall {
  public static class SimplexNoise
  {
    static float _scale; // 200f
    static int _octaves; // 11, 5
    static float _persistance; // 0.6f
    static float _lacunarity; // 2f

    public static float view_offset_x = 80f;
    public static float view_offset_y = 88f;

    public static float[,] SquareGradient;

    static float[,] noise;

    public static float[,] GenerateNoiseMap(
            int width,
            int height,
            int seed,
            float scale,
            int octaves,
            float persistence,
            float lacunarity)
        {
            var noiseMap = new float[width, height];
            var prng = new Random(seed);
            var octaveOffsets = new Vector2[octaves];

            // Create random offsets for each octave
            for (int i = 0; i < octaves; i++) {
                float offsetX = prng.Next(-100000, 100000);
                float offsetY = prng.Next(-100000, 100000);
                octaveOffsets[i] = new Vector2(offsetX, offsetY);
            }

            if (scale <= 0) scale = 0.0001f;

            float maxNoise = float.MinValue;
            float minNoise = float.MaxValue;

            // Generate raw noise heights
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    float amplitude = 1f;
                    float frequency = 1f;
                    float noiseHeight = 0f;

                    for (int i = 0; i < octaves; i++) {
                        float sampleX = (x / scale * frequency) + octaveOffsets[i].X;
                        float sampleY = (y / scale * frequency) + octaveOffsets[i].Y;
                        float simplexValue = simplex(sampleX, sampleY);
                        // OpenSimplex typically yields ~[-1,1]
                        noiseHeight += simplexValue * amplitude;

                        amplitude *= persistence;
                        frequency *= lacunarity;
                    }

                    if (noiseHeight > maxNoise) maxNoise = noiseHeight;
                    if (noiseHeight < minNoise) minNoise = noiseHeight;
                    noiseMap[x, y] = noiseHeight;
                }
            }

            // Normalize to [0,1]
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    noiseMap[x, y] = (noiseMap[x, y] - minNoise) / (maxNoise - minNoise);
                }
            }

            return noiseMap;
        }    
        
        public static float[,] SmoothNoiseMap(float[,] noise, int width, int height, int kernelSize = 3)
      {
          float[,] smoothedNoise = new float[width, height];
          int halfKernel = kernelSize / 2;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float sum = 0f;
                int count = 0;

                // Loop through neighboring pixels
                for (int ky = -halfKernel; ky <= halfKernel; ky++)
                {
                    for (int kx = -halfKernel; kx <= halfKernel; kx++)
                    {
                        int nx = x + kx;
                        int ny = y + ky;

                        if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                        {
                            sum += noise[nx, ny];
                            count++;
                        }
                    }
                }

                smoothedNoise[x, y] = (sum / count) * 100f; // Average the values
            }
        }

      return smoothedNoise;
  }


    public static float SmoothStep(float edge0, float edge1, float x)
  {
      // Clamp x between 0 and 1
      x = Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
      return x * x * (3 - 2 * x); // SmoothStep function
  }

  public static float Clamp(float value, float min, float max)
  {
      return MathF.Max(min, MathF.Min(max, value));
  }

  static void Square_Gradient(
        int WORLD_WIDTH,
        int WORLD_HEIGHT,
        float[,] NOISE)
      {
        SquareGradient = new float[WORLD_WIDTH, WORLD_HEIGHT];

        for (int y = 0; y < WORLD_HEIGHT; y++)
        {
          for (int x = 0; x < WORLD_WIDTH; x++)
          {
            float d = MathF.Sqrt(50 * 50 + 50 * 50) / MathF.Sqrt(0.5f);
            SquareGradient[x, y] = 50 + (1 + NOISE[x, y] - d) / 2;
          }
        }
      }

      private static int fastfloor(float v) => (v>0? (int)v : (int)v -1);

      private static int[][] grad3 = new[] {
            new[] {1,1,0}, new[]{-1,1,0}, new[]{1,-1,0}, new[]{-1,-1,0},
            new[]{1,0,1}, new[]{-1,0,1}, new[]{1,0,-1}, new[]{-1,0,-1},
            new[]{0,1,1}, new[]{0,-1,1}, new[]{0,1,-1}, new[]{0,-1,-1}
        };

      private static float dot(int[] g, float x, float y) => g[0]*x + g[1]*y;

      private static float dot(int[] g, float x, float y, float z)
      {
        return g[0] * x + g[1] * y + g[2] * z;
      }

      private static float dot(int[] g, float x, float y, float z, float w)
      {
        return g[0] * x + g[1] * y + g[2] * z + g[3] * w;
      }

      private static int[] permutation =
      {
        151, 160, 137, 91, 90, 15, 131, 13, 201, 95, 96, 53, 194, 233, 7, 225, 140, 36, 103,
        30, 69, 142, 8, 99, 37, 240, 21, 10, 23, 190, 6, 148, 247, 120, 234, 75, 0, 26, 197,
        62, 94, 252, 219, 203, 117, 35, 11, 32, 57, 177, 33, 88, 237, 149, 56, 87, 174, 20, 125,
        136, 171, 168, 68, 175, 74, 165, 71, 134, 139, 48, 27, 166, 77, 146, 158, 231, 83,
        111, 229, 122, 60, 211, 133, 230, 220, 105, 92, 41, 55, 46, 245, 40, 244, 102, 143, 54,
        65, 25, 63, 161, 1, 216, 80, 73, 209, 76, 132, 187, 208, 89, 18, 169, 200, 196, 135,
        130, 116, 188, 159, 86, 164, 100, 109, 198, 173, 186, 3, 64, 52, 217, 226, 250, 124,
        123, 5, 202, 38, 147, 118, 126, 255, 82, 85, 212, 207, 206, 59, 227, 47, 16, 58, 17, 182,
        189, 28, 42, 223, 183, 170, 213, 119, 248, 152, 2, 44, 154, 163, 70, 221, 153, 101,
        155, 167, 43, 172, 9, 129, 22, 39, 253, 19, 98, 108, 110, 79, 113, 224, 232, 178,
        185, 112, 104, 218, 246, 97, 228, 251, 34, 242, 193, 238, 210, 144, 12, 191, 179, 162,
        241, 81, 51, 145, 235, 249, 14, 239, 107, 49, 192, 214, 31, 181, 199, 106, 157, 184, 84,
        204, 176, 115, 121, 50, 45, 127, 4, 150, 254, 138, 236, 205, 93, 222, 114, 67, 29, 24, 72,
        243, 141, 128, 195, 78, 66, 215, 61, 156, 180
      };

      private static int[] p = new int[512];

      private static void set_permutation()
      {
        for (int i = 0; i < 512; i++)
        {
          p[i] = permutation[i & 255];
        }
      }

      public static float simplex(float xin, float yin)
      {
        set_permutation();

        float F2 = 0.5f * (MathF.Sqrt(3f)-1f);
            float s = (xin+yin)*F2;
            int i = fastfloor(xin+s), j = fastfloor(yin+s);
            float G2 = (3f-MathF.Sqrt(3f))/6f;
            float t = (i+j)*G2;
            float X0=i-t, Y0=j-t;
            float x0=xin-X0, y0=yin-Y0;
            int i1 = x0>y0?1:0, j1 = x0>y0?0:1;
            float x1=x0-i1+G2, y1=y0-j1+G2;
            float x2=x0-1f+2f*G2, y2=y0-1f+2f*G2;
            int ii=i&255, jj=j&255;
            int gi0=p[ii+p[jj]]%12, gi1=p[ii+i1+p[jj+j1]]%12, gi2=p[ii+1+p[jj+1]]%12;
            float n0 = MathF.Max(0.5f - x0*x0 - y0*y0, 0f);
            float n1 = MathF.Max(0.5f - x1*x1 - y1*y1, 0f);
            float n2 = MathF.Max(0.5f - x2*x2 - y2*y2, 0f);
            n0 = n0*n0*n0*n0* dot(grad3[gi0], x0, y0);
            n1 = n1*n1*n1*n1* dot(grad3[gi1], x1, y1);
            n2 = n2*n2*n2*n2* dot(grad3[gi2], x2, y2);
            return 70f*(n0+n1+n2);
      }

      public static float inverseLerp(float a, float b, float c) {
        return (c - a) / (b - a);
      } 

      public static float[,] GenerateGradientMap(int width, int height)
        {
            var gradient = new float[width, height];
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    float dx = (x - width * 0.5f) / (width * 0.5f);
                    float dy = (y - height * 0.5f) / (height * 0.5f);
                    float dist = MathF.Sqrt(dx * dx + dy * dy);
                    // perturb by small simplex noise
                    float n = simplex(x * 0.02f, y * 0.02f) * 0.1f;
                    float falloff = MathHelper.Clamp(1f - (dist + n), 0f, 1f);
                    gradient[x, y] = falloff;
                }
            }
            return gradient;
        }

  }
}