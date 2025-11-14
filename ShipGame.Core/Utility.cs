using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Reflection;

namespace ShipGame
{
	internal static class Utility
	{
		public static string ExecutingAssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().Location;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		public static Vector3 ToDegrees(this Vector3 rads) =>
			new Vector3(MathHelper.ToDegrees(rads.X),
				MathHelper.ToDegrees(rads.Y),
				MathHelper.ToDegrees(rads.Z));
	}
}
