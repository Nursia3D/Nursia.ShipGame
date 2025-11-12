#region File Description
//-----------------------------------------------------------------------------
// ScreenHelp.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
#endregion

namespace ShipGame
{
	public class ScreenHelp : IScreen
	{
		Texture2D textureControls;    // controlls text texture
		Texture2D textureDisplay;     // controller texture
		Texture2D textureContinue;    // continue text texture

		public void Set()
		{
			// load all resources
			var content = SG.Assets;
			textureControls = content.LoadTexture2DDefault("screens/controls.tga");
			textureDisplay = content.LoadTexture2DDefault("screens/controls_display.tga");
			textureContinue = content.LoadTexture2DDefault("screens/continue.tga");
		}

		public void Unset()
		{
			// free all resources
			textureControls = null;
			textureDisplay = null;
			textureContinue = null;
		}

		// process input
		public void ProcessInput(float elapsedTime)
		{
			var input = SG.InputManager;
			for (int i = 0; i < 2; i++)
			{
				// Any key/button to go back
				if (input.IsButtonPressedA(i) ||
					input.IsButtonPressedB(i) ||
					input.IsButtonPressedX(i) ||
					input.IsButtonPressedY(i) ||
					input.IsButtonPressedLeftShoulder(i) ||
					input.IsButtonPressedRightShoulder(i) ||
					input.IsButtonPressedLeftStick(i) ||
					input.IsButtonPressedRightStick(i) ||
					input.IsButtonPressedBack(i) ||
					input.IsButtonPressedStart(i) ||
					input.IsKeyPressed(i, Keys.Enter) ||
					input.IsKeyPressed(i, Keys.Escape) ||
					input.IsKeyPressed(i, Keys.Space))
				{
					SG.ScreenManager.SetNextScreen(ScreenType.ScreenIntro);
					SG.GameManager.PlaySound("menu_cancel");
				}
			}
		}

		// update screen
		public void Update(float elapsedTime)
		{
		}

		// draw 3D scene
		public void Draw3D()
		{
			var gd = SG.GraphicsDevice;

			// clear background
			gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);

			// draw background animation
			SG.ScreenManager.DrawBackground();
		}

		// draw 2D gui
		public void Draw2D(RenderContext2D context)
		{
			Rectangle rect = new Rectangle(0, 0, 0, 0);

			var gd = SG.GraphicsDevice;
			int screenSizeX = gd.Viewport.Width;
			int screenSizeY = gd.Viewport.Height;

			// draw controlls text aligned to top of screen
			rect.Width = textureControls.Width;
			rect.Height = textureControls.Height;
			rect.X = screenSizeX / 2 - rect.Width / 2;
			rect.Y = 40;

			context.DrawTexture(textureControls, rect,
				Color.White, BlendState.AlphaBlend);

			// draw controller texture centered in screen
			rect.Width = textureDisplay.Width;
			rect.Height = textureDisplay.Height;
			rect.X = screenSizeX / 2 - rect.Width / 2;
			rect.Y = screenSizeY / 2 - rect.Height / 2 + 10;
			context.DrawTexture(textureDisplay, rect,
				Color.White, BlendState.AlphaBlend);

			// draw continue message aligned to bottom of screen
			rect.Width = textureContinue.Width;
			rect.Height = textureContinue.Height;
			rect.X = screenSizeX / 2 - rect.Width / 2;
			rect.Y = screenSizeY - rect.Height - 60;
			context.DrawTexture(textureContinue, rect,
				Color.White, BlendState.AlphaBlend);
		}
	}
}
