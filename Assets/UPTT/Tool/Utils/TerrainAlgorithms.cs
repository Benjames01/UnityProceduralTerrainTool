using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;


namespace UPTT.Tool.Utils
{
	public static class TerrainAlgorithms 
	{
		public static float FractalBrownianMotion( float x, float y, int octaves, float persistence=0.5f, float lacunarity = 2.0f)
		{
			/* Frequency is how close the waves are together
			 *
			 * Value is total height calculated for given co-ordinate
			 * Max value Keeps count of each addition of amplitude for each octave
			 */
			// 
			float frequency = 1.0f, amplitude = 1.0f, value = 0, maxValue = 0;

			// Loop through each octave
			for (var i = 0; i < octaves; i++)
			{
				value += amplitude * Mathf.PerlinNoise(frequency * x, frequency * y);
				frequency *= lacunarity;
				amplitude *= persistence;
				maxValue += amplitude;
			}

			// Get an average of the height at x,y ( Will end up with huge curves with more octaves otherwise)
			return value / maxValue;
		}

		private static float GetHeightMapValueAtVec2(float[,]  heightMap, Vector2 coord)
		{
			return heightMap[(int)coord.x, (int)coord.y];
		}

		private static float GetAverage(IReadOnlyCollection<float> toAverage)
		{
			return toAverage.Sum() / toAverage.Count;
		}

		private static float AddAndAverage(float[,] heightMap, IEnumerable<Vector2> toAverage)
		{
			var floatsToAverage = toAverage.Select(coord => GetHeightMapValueAtVec2(heightMap, coord)).ToList();

			return GetAverage(floatsToAverage);
		}

		private static bool CheckIfIn2DArrayBounds(this float[,] array, int x, int y)
		{
			return x >= array.GetLowerBound(0) && y >= array.GetLowerBound(1) && x <= array.GetUpperBound(0) && y <= array.GetUpperBound(1);
		}
		
		private static IEnumerable<Vector2> CheckInBoundsAddToList(this float[,] array, IEnumerable<Vector2> toCheck)
		{
			return toCheck.Where(vec => CheckIfIn2DArrayBounds(array, (int) vec.x, (int) vec.y)).ToList();
		}

		private static IEnumerable<Vector2> GetPotentialCoords(int x, int y)
		{
			var centreLeft = new Vector2(x - 1, y);
			var centreMiddle = new Vector2(x, y);
			var centreRight = new Vector2(x + 1, y);
					
			var topLeft = new Vector2(x - 1, y + 1);
			var topMiddle = new Vector2(x, y + 1);
			var topRight = new Vector2(x + 1, y + 1);
					
			var bottomLeft = new Vector2(x - 1, y - 1);
			var bottomMiddle = new Vector2(x, y - 1);
			var bottomRight = new Vector2(x + 1, y - 1);


			var list = new List<Vector2>()
			{
				centreLeft, centreMiddle, centreRight,
				topLeft, topMiddle, topRight,
				bottomLeft, bottomMiddle, bottomRight
			};

			return list;
		}
		
		public static float[,] BlurAlgorithm(float[,] heightMap, int size)
		{
			for (var i = 0; i < size; i++)
			{
				for (var j = 0; j < size; j++)
				{
					var toCheck = GetPotentialCoords(j, i);
					var inBounds = CheckInBoundsAddToList(heightMap, toCheck);
					var average = AddAndAverage(heightMap, inBounds);

					heightMap[j, i] = average;
				}
			}
			return heightMap;
		}
		
		
	}
}

