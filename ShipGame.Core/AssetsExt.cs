using AssetManagementBase;
using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

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

		private static AssetLoader<DrModel> _modelLoader = (manager, assetName, settings, tag) =>
		{
			// Load gltf
			var device = (GraphicsDevice)tag;

			var model = DigitalRiseModelAssetsExt.LoadModel(manager, device, Path.ChangeExtension(assetName, "glb"), ModelLoadFlags.EnsureUVs);

			var materialName = Path.ChangeExtension(assetName, "material");
			if (manager.Exists(materialName))
			{
				var json = manager.ReadAsString(materialName);
				var materialInfo = JsonSerializer.Deserialize<Dictionary<string, Dictionary<int, Dictionary<string, string>>>>(json);

				foreach (var mesh in model.Meshes)
				{
					Dictionary<int, Dictionary<string, string>> meshMaterials;
					if (mesh.Name == null || !materialInfo.TryGetValue(mesh.Name, out meshMaterials))
					{
						continue;
					}

					for(var partIndex = 0; partIndex < mesh.MeshParts.Count; ++partIndex)
					{
						var part = mesh.MeshParts[partIndex];

						Dictionary<string, string> meshPartMaterials;
						if (!meshMaterials.TryGetValue(partIndex, out meshPartMaterials))
						{
							continue;
						}

						var effect = manager.LoadEffect2(device, "NormalMapping.efb").Clone();
						foreach(var pair in meshPartMaterials)
						{
							effect.Parameters[pair.Key].SetValue(manager.LoadTexture2D(device, pair.Value));
						}

						part.Tag = effect;
					}
				}

			}

			return model;
		};

		public static DrModel LoadModel(this AssetManager assetManager, GraphicsDevice graphicsDevice, string assetName)
		{
			return assetManager.UseLoader(_modelLoader, assetName, tag: graphicsDevice);
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
