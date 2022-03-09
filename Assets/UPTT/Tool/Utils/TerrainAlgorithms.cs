using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UPTT.Tool.Utils
{
	public static class TerrainAlgorithms 
	{

		public static float FractalBrownianMotion( float x, float y, int octaves, float persistence=0.5f, float lacunarity = 2.0f)
		{
			// Frequency is how close the waves are together
			float frequency = 1.0f;
			//
			float amplitude = 1.0f;

			// Total height calculate for given co-ordinate
			float value = 0;
			
			// Keeps count of each addition of amplitude for each octave;
			float maxValue = 0;
			
			// Loop through each octave
			for (int i = 0; i < octaves; i++)
			{
				value += amplitude * Mathf.PerlinNoise(frequency * x, frequency * y);
				frequency *= lacunarity;
				amplitude *= persistence;
				maxValue += amplitude;
			}

			// Get an average of the height at x,y ( Will end up with huge curves with more octaves otherwise)
			return value / maxValue;
		}
		
		
		
		
	}
}

