#region File Description
//-----------------------------------------------------------------------------
// ScreenIntro.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nursia;
using Nursia.SceneGraph;
using Nursia.SceneGraph.Cameras;
using System;
#endregion

namespace ShipGame
{
	public class ScreenIntro : IScreen
	{
		int menuSelection;              // current menu selection
		float menuTime;                 // menu time for animation

		Texture2D textureLogo;          // logo texture
		Texture2D textureLens;          // lens texture

		Texture2D textureCursorAnim;    // cursor textures
		Texture2D textureCursorBullet;
		Texture2D textureCursorArrow;

		// menu itens
		const int NumberMenuItems = 4;
		String[] menuNames = new String[NumberMenuItems]
				 { "menu_sp", "menu_mp", "menu_hp", "menu_qg" };

		// menu textures without hover
		Texture2D[] textureMenu = new Texture2D[NumberMenuItems];
		// menu textures with hover
		Texture2D[] textureMenuHover = new Texture2D[NumberMenuItems];

		public bool DrawBackground => true;

		public StoredScene Scene3D => null;

		public void Set()
		{
			// load all resources
			SG.GameManager.GameMode = GameMode.SinglePlayer;

			var content = SG.Assets;
			textureLogo = content.LoadTexture2DDefault("screens/intro_logo.tga");
			textureLens = content.LoadTexture2DDefault("screens/intro_lens.tga");

			textureCursorAnim = content.LoadTexture2DDefault("screens/cursor_anim.tga");
			textureCursorArrow = content.LoadTexture2DDefault("screens/cursor_arrow.tga");
			textureCursorBullet = content.LoadTexture2DDefault("screens/cursor_bullet.tga");

			for (int i = 0; i < NumberMenuItems; i++)
			{
				textureMenu[i] = content.LoadTexture2DDefault($"screens/{menuNames[i]}.tga");
				textureMenuHover[i] = content.LoadTexture2DDefault($"screens/{menuNames[i]}_hover.tga");
			}
		}

		public void Unset()
		{
			// free all resources
			textureLogo = null;
			textureLens = null;
			textureCursorAnim = null;
			textureCursorArrow = null;
			textureCursorBullet = null;

			for (int i = 0; i < NumberMenuItems; i++)
			{
				textureMenu[i] = null;
				textureMenuHover[i] = null;
			}
		}

		// process input
		public void ProcessInput(float elapsedTime)
		{
			var input = SG.InputManager;
			for (int i = 0; i < 2; i++)
			{
				var gameManager = SG.GameManager;
				// A button or enter to select menu option
				if (input.IsButtonPressedA(i) ||
					input.IsButtonPressedStart(i) ||
					input.IsKeyPressed(i, Keys.Enter) ||
					input.IsKeyPressed(i, Keys.Space))
				{
					var screenManager = SG.ScreenManager;
					switch (menuSelection)
					{
						case 0:
							// single player
							gameManager.GameMode = GameMode.SinglePlayer;
							screenManager.SetNextScreen(ScreenType.ScreenPlayer);
							break;
						case 1:
							// multi player
							gameManager.GameMode = GameMode.MultiPlayer;
							screenManager.SetNextScreen(ScreenType.ScreenPlayer);
							break;
						case 2:
							// help
							screenManager.SetNextScreen(ScreenType.ScreenHelp);
							break;
						case 3:
							// exit game
							screenManager.Exit();
							break;
					}
					gameManager.PlaySound("menu_select");
				}

				// up/down keys change menu sel
				if (input.IsKeyPressed(i, Keys.Up) ||
					input.IsButtonPressedDPadUp(i) ||
					input.IsButtonPressedLeftStickUp(i))
				{
					menuSelection =
						(menuSelection == 0 ? NumberMenuItems - 1 : menuSelection - 1);
					gameManager.PlaySound("menu_change");
				}
				if (input.IsKeyPressed(i, Keys.Down) ||
					input.IsButtonPressedDPadDown(i) ||
					input.IsButtonPressedLeftStickDown(i))
				{
					menuSelection = (menuSelection + 1) % NumberMenuItems;
					gameManager.PlaySound("menu_change");
				}
			}
		}

		// update screen
		public void Update(float elapsedTime)
		{
			// accumulate elapsed time
			menuTime += elapsedTime;
		}

		// draw 3D scene
		public void Draw3D()
		{
		}

		// draw the animated cursor
		void DrawCursor(RenderContext2D context, int x, int y)
		{
			Rectangle rect = new Rectangle(0, 0, 0, 0);

			float rotation = menuTime * 2;

			// draw animated cursor texture
			rect.X = x - textureCursorAnim.Width / 2;
			rect.Y = y - textureCursorAnim.Height / 2;
			rect.Width = textureCursorAnim.Width;
			rect.Height = textureCursorAnim.Height;
			context.DrawTexture(textureCursorAnim, rect, rotation,
				Color.White, BlendState.AlphaBlend);

			// draw bullet cursor texture
			rect.X = x - textureCursorBullet.Width / 2;
			rect.Y = y - textureCursorBullet.Height / 2;
			rect.Width = textureCursorBullet.Width;
			rect.Height = textureCursorBullet.Height;
			context.DrawTexture(textureCursorBullet, rect,
				Color.White, BlendState.AlphaBlend);

			// draw arrow cursor texture
			rect.X = x - textureCursorArrow.Width / 2 + 32;
			rect.Y = y - textureCursorArrow.Height / 2;
			rect.Width = textureCursorArrow.Width;
			rect.Height = textureCursorArrow.Height;
			context.DrawTexture(textureCursorArrow, rect,
				Color.White, BlendState.AlphaBlend);
		}

		// draw 2D gui
		public void Draw2D(RenderContext2D context)
		{
			// screen rect
			var gd = Nrs.GraphicsDevice;
			Rectangle rect = new Rectangle(gd.Viewport.X, gd.Viewport.Y,
							gd.Viewport.Width, gd.Viewport.Height);

			// draw lens flare texture
			context.DrawTexture(textureLens, rect,
				Color.White, BlendState.Additive);

			// draw logo texture
			context.DrawTexture(textureLogo, rect,
				Color.White, BlendState.AlphaBlend);

			// draw menu itens
			int Y = rect.Height - 200;
			for (int i = 0; i < NumberMenuItems; i++)
			{
				// if item selected
				if (i == menuSelection)
				{
					rect.X = 540;
					rect.Y = Y;
					rect.Width = textureMenuHover[i].Width;
					rect.Height = textureMenuHover[i].Height;
					context.DrawTexture(textureMenuHover[i], rect,
						Color.White, BlendState.AlphaBlend);

					// draw cursor left of selected item
					DrawCursor(context, rect.X - 60, rect.Y + 19);

					Y += 50;
				}
				else // item not selected
				{
					rect.X = 540;
					rect.Y = Y;
					rect.Width = textureMenu[i].Width;
					rect.Height = textureMenu[i].Height;

					context.DrawTexture(textureMenu[i], rect,
						Color.White, BlendState.AlphaBlend);

					Y += 40;
				}
			}
		}
	}
}
