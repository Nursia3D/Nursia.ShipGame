using AssetManagementBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;

namespace ShipGame
{
	public static partial class SG
	{
		public static GraphicsDevice GraphicsDevice { get; private set; }
		public static AssetManager Assets { get; private set; }


		public static GameManagerType GameManager { get; private set; }

		public static ScreenManagerType ScreenManager { get; private set; }
		public static InputManagerType InputManager { get; private set; }


		public static void Initialize(GraphicsDevice device)
		{
			GraphicsDevice = device ?? throw new ArgumentNullException(nameof(device));

			var path = Path.Combine(Utility.ExecutingAssemblyDirectory, "Assets");
			Assets = AssetManager.CreateFileAssetManager(path);

			GameManager = new GameManagerType();
			GameManager.LoadContent();

			ScreenManager = new ScreenManagerType();
			ScreenManager.LoadContent();

			InputManager = new InputManagerType();
		}

		public static void Uninitialize()
		{
			GameManager.UnloadContent();
			GameManager.Dispose();
			GameManager = null;

			ScreenManager.UnloadContent();
			ScreenManager.Dispose();
			ScreenManager = null;

			InputManager = null;
		}

		public static void Update(GameTime gameTime)
		{
			float elapsedTimeFloat = (float)gameTime.ElapsedGameTime.TotalSeconds;

			ScreenManager.ProcessInput(elapsedTimeFloat);
			ScreenManager.Update(elapsedTimeFloat);
		}

		public static void Draw(GameTime gameTime)
		{
			ScreenManager.Draw();
		}
	}
}
