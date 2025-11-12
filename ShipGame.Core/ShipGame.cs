#region File Description
//-----------------------------------------------------------------------------
// ShipGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using AssetManagementBase;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Reflection;

#endregion

namespace ShipGame
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class ShipGameGame : Game
	{
		static ShipGameGame instance;

		GraphicsDeviceManager graphics;
		ScreenManager screen;
		GameManager game;
		FontManager font;
		SoundManager soundManager;
		AssetManager assets;
		bool renderVsync = true;

		private static string ExecutingAssemblyDirectory
		{
			get
			{
				string codeBase = Assembly.GetExecutingAssembly().Location;
				UriBuilder uri = new UriBuilder(codeBase);
				string path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		public static AssetManager AssetManager => instance.assets;

		public ShipGameGame()
		{
			graphics = new GraphicsDeviceManager(this);
			Window.Title = "ShipGame";

			soundManager = new SoundManager();
			game = new GameManager(soundManager);

			graphics.PreferredBackBufferWidth = GameOptions.ScreenWidth;
			graphics.PreferredBackBufferHeight = GameOptions.ScreenHeight;

			IsFixedTimeStep = renderVsync;
			graphics.SynchronizeWithVerticalRetrace = renderVsync;
		}


		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to 
		/// run. This is where it can query for any required services and load any 
		/// non-graphic related content. Calling base.Initialize will enumerate through 
		/// any components and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			base.Initialize();
		}


		/// <summary>
		/// Load your graphics content.
		/// </summary>
		protected override void LoadContent()
		{
			font = new FontManager(graphics.GraphicsDevice);
			screen = new ScreenManager(this, font, game);

			var path = Path.Combine(ExecutingAssemblyDirectory, "Assets");
			assets = AssetManager.CreateFileAssetManager(path);
			soundManager.LoadContent(assets);
			font.LoadContent(assets);
			game.LoadContent(graphics.GraphicsDevice, assets);
			screen.LoadContent(graphics.GraphicsDevice, assets);
		}


		/// <summary>
		/// Unload your graphics content.
		/// </summary>
		protected override void UnloadContent()
		{
			soundManager.UnloadContent();
			screen.UnloadContent();
			game.UnloadContent();
			font.UnloadContent();

			screen = null;
			font = null;
		}


		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			float ElapsedTimeFloat = (float)gameTime.ElapsedGameTime.TotalSeconds;

			screen.ProcessInput(ElapsedTimeFloat);
			screen.Update(GraphicsDevice, ElapsedTimeFloat);

			base.Update(gameTime);
		}


		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			screen.Draw(graphics.GraphicsDevice);

			base.Draw(gameTime);
		}

		/// <summary>
		/// This is called to switch full screen mode.
		/// </summary>
		public void ToggleFullScreen()
		{
			graphics.ToggleFullScreen();
		}

		static public ShipGameGame GetInstance()
		{
			return instance;
		}

		static public void SetInstance(ShipGameGame game)
		{
			instance = game;
		}
	}
}