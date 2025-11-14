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
		private static ShipGameGame instance;

		GraphicsDeviceManager graphics;
		bool renderVsync = true;

		public ShipGameGame()
		{
			instance = this;

			graphics = new GraphicsDeviceManager(this);
			Window.Title = "ShipGame";

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
			SG.Initialize(this);
		}


		/// <summary>
		/// Unload your graphics content.
		/// </summary>
		protected override void UnloadContent()
		{
			SG.Uninitialize();
		}


		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			SG.Update(gameTime);
		}


		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			SG.Draw(gameTime);

			base.Draw(gameTime);
		}

		/// <summary>
		/// This is called to switch full screen mode.
		/// </summary>
		public static void ToggleFullScreen()
		{
			instance.graphics.ToggleFullScreen();
		}

		public static void DoExit()
		{
			instance.Exit();
		}
	}
}