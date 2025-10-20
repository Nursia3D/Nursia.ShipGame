using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nursia;
using Nursia.Standard;
using System;
using System.IO;

#if MONOGAME
using MonoGame.Framework.Utilities;
#endif

namespace ShipGame
{
	internal static class AssetsExt
	{
		public static void UnloadAsset(this AssetManager content, string name)
		{
			if (name == null)
			{
				return;
			}

			object asset;
			if (!content.Cache.TryGetValue(name, out asset))
			{
				return;
			}

			var asdisp = asset as IDisposable;
			if (asdisp != null)
			{
				asdisp.Dispose();
			}

			content.Cache.Remove(name);
		}

		public static void Dispose(this AssetManager content)
		{
			foreach (var pair in content.Cache)
			{
				var asdisp = pair.Value as IDisposable;
				if (asdisp != null)
				{
					asdisp.Dispose();
				}
			}

			content.Cache.Clear();
		}

		public static Texture2D LoadTexture2DDefault(this AssetManager manager, GraphicsDevice gd, string assetName)
		{
			return manager.LoadTexture2D(gd, assetName, premultiplyAlpha: true, colorKey: new Color(255, 0, 255, 255));
		}

		public static NursiaModelNode LoadModel(this AssetManager assetManager, string assetName)
		{
			var scene = assetManager.LoadScene("Scenes/" + Path.ChangeExtension(assetName, "scene"));
			return (NursiaModelNode)scene.Root;
		}

		public static Effect LoadEffect2(this AssetManager manager, GraphicsDevice graphicsDevice, string assetName)
		{
			var file = Path.GetFileName(assetName);

			string path;
#if FNA
			path = "/shaders/FNA/" + file;
#else
			if (PlatformInfo.GraphicsBackend == GraphicsBackend.OpenGL)
			{
				path = "/shaders/MonoGameOGL/" + file;
			}
			else
			{
				path = "/shaders/MonoGameDX/" + file;
			}
#endif

			return manager.LoadEffect(graphicsDevice, path);
		}

	}
}
