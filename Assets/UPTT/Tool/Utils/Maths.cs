using System;
using System.Reflection;
using System.Xml.Schema;
using UnityEngine;

namespace UPTT.Tool.Utils
{
	public static class Maths
	{
		/// <summary>
		/// Gets a random float in given range x, y
		/// </summary>
		/// <param name="x">minimum value</param>
		/// <param name="y">maximum value</param>
		/// <returns></returns>
		public static float RandomRange(float x, float y)
		{
			return UnityEngine.Random.Range(x, y);
		}

		/// <summary>
		/// Get a random vector 3 within the given parameters
		/// </summary>
		/// <param name="xMax">Maximum x value</param>
		/// <param name="yMax">Maximum y value</param>
		/// <param name="zMax">Maximum z value</param>
		/// <param name="xMin">Minimum x value</param>
		/// <param name="yMin">Minimum y value</param>
		/// <param name="zMin">Minimum z value</param>
		/// <returns></returns>
		private static Vector3 RandomVector3(float xMax, float yMax, float zMax, float xMin = 0, float yMin = 0, float zMin = 0)
		{
			return new Vector3(RandomRange(xMin, xMax), RandomRange(yMin, yMax), RandomRange(zMin, zMax));
		}

		/// <summary>
		///  Get a random pair of coordinates within given width
		/// </summary>
		/// <param name="width"></param>
		/// <returns></returns>
		public static Vector2 RandomPositionInTerrain(int width)
		{
			var vec3 = RandomVector3(width, width, 0);

			return new Vector2(vec3.x, vec3.y);
		}

		/// <summary>
		/// Normalise the values in an array of type T
		/// </summary>
		/// <param name="array"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T[] NormaliseArray<T>(T[] array) where T: unmanaged, IComparable, IEquatable<T>
		{
			dynamic tracked = default;
			for (var i = 0; i < array.Length; i++)
			{
				if (i == 0) // tracked is still default value
				{
					tracked = array[i]; // Set to current value at i in the array
				}
				else
				{
					tracked += array[i]; // Add array[i] to the tracked total
				}
			}

			// Divide each element by the tracked total to get a value between 0 and 1
			for (var i = 0; i < array.Length; i++)
			{
				array[i] /= tracked;
			}

			// return the normalised array
			return array;
		}

		
		// Credit: https://prime31.github.io/simple-value-mapping/
		// Maps a value from some arbitrary range to the 0 to 1 range
		public static float Map01( float value, float min, float max )
		{
			return ( value - min ) * 1f / ( max - min );
		}

		/// <summary>
		/// // Clamp values of vector3 between min & max
		/// </summary>
		/// <param name="value">Vector to clamp</param>
		/// <param name="min">Minimum value</param>
		/// <param name="max">Maxmium value</param>
		/// <returns></returns>
		public static void ClampVector3(ref Vector3 value, float min, float max)
		{
			value.x = Mathf.Clamp(value.x, min, max);
			value.y = Mathf.Clamp(value.y, min, max);
			value.z = Mathf.Clamp(value.z, min, max);
		}
		
		/// <summary>
		/// Clamps float value between min & max
		/// </summary>
		/// <param name="value"></param>
		/// <param name="min"></param>
		/// <param name="max"></param>
		public static void Clamp(ref float value, float min, float max)
		{ 
			value = Mathf.Clamp(value, min, max);
		}
		
		/// <summary>
		/// // Clamp values of vector3 between min & max
		/// </summary>
		/// <param name="value">Vector to clamp</param>
		/// <param name="min">Minimum value</param>
		/// <param name="max">Maxmium value</param>
		/// <returns></returns>
		public static Vector3 ClampVector2(ref Vector2 value, float min, float max)
		{
			
			value.x = Mathf.Clamp(value.x, min, max);
			value.y = Mathf.Clamp(value.y, min, max);
			return value;
		}
	}
}