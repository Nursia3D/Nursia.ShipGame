#region File Description
//-----------------------------------------------------------------------------
// ScreenGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace ShipGame
{
	public class ScreenGame : IScreen
	{
		Texture2D hudCrosshair;       // hud crosshair texture
		Texture2D hudEnergy;          // hud energy/shield/boost texture
		Texture2D hudMissile;         // hud missile texture
		Texture2D hudScore;           // hud score texture
		Texture2D hudBars;            // hud energy/shield/boost bars texture

		Texture2D damageTexture;      // damage indication texture

		public void Set()
		{
			var gameManager = SG.GameManager;

			// load all resources
			gameManager.LoadFiles();

			// load hud textures
			var content = SG.Assets;
			if (gameManager.GameMode == GameMode.SinglePlayer)
			{
				hudCrosshair = content.LoadTexture2DDefault("screens/hud_sp_crosshair.tga");
				hudEnergy = content.LoadTexture2DDefault("screens/hud_sp_energy.tga");
				hudMissile = content.LoadTexture2DDefault("screens/hud_sp_missile.tga");
				hudScore = content.LoadTexture2DDefault("screens/hud_sp_score.tga");
				hudBars = content.LoadTexture2DDefault("screens/hud_sp_bars.tga");
			}
			else
			{
				hudCrosshair = content.LoadTexture2DDefault("screens/hud_mp_crosshair.tga");
				hudEnergy = content.LoadTexture2DDefault("screens/hud_mp_energy.tga");
				hudMissile = content.LoadTexture2DDefault("screens/hud_mp_missile.tga");
				hudScore = content.LoadTexture2DDefault("screens/hud_mp_score.tga");
				hudBars = content.LoadTexture2DDefault("screens/hud_mp_bars.tga");
			}

			// load damage indicator texture
			damageTexture = content.LoadTexture2DDefault("screens/damage.tga");
		}

		public void Unset()
		{
			var gameManager = SG.GameManager;

			// free all resources
			gameManager.UnloadFiles();

			// unload hud
			hudCrosshair = null;
			hudEnergy = null;
			hudMissile = null;
			hudScore = null;
			hudBars = null;

			// unload damage texture
			damageTexture = null;
		}

		// process input
		public void ProcessInput(float elapsedTime)
		{
			var gameManager = SG.GameManager;
			gameManager.ProcessInput(elapsedTime);

			int i, j = (int)gameManager.GameMode;
			var input = SG.InputManager;
			for (i = 0; i < j; i++)
			{
				if (input.IsKeyPressed(i, Keys.Escape) || input.IsButtonPressedBack(i))
				{
					gameManager.GetPlayer(i).Score = -1;
					SG.ScreenManager.SetNextScreen(ScreenType.ScreenEnd);
					gameManager.PlaySound("menu_cancel");
				}
			}
		}

		// update screen
		public void Update(float elapsedTime)
		{
			// update game
			var gameManager = SG.GameManager;
			gameManager.Update(elapsedTime);

			// check if any player have reached the score limit
			// if so, changes to the end screen
			int i, j = (int)gameManager.GameMode;
			for (i = 0; i < j; i++)
			{
				if (gameManager.GetPlayer(i).Score == GameOptions.MaxPoints)
				{
					SG.ScreenManager.SetNextScreen(ScreenType.ScreenEnd,
						GameOptions.FadeColor, GameOptions.FadeTime);
				}
			}
		}

		// draw 3D scene
		public void Draw3D()
		{
			// draw the 3d game scene
			SG.GameManager.Draw3D();
		}

		/// <summary>
		/// Draw the HUD interface
		/// </summary>
		void DrawHud(RenderContext2D context, Rectangle rect, Vector3 bars, int barsLeft, int barsWidth, bool crosshair)
		{
			Rectangle r = new Rectangle(0, 0, 0, 0);

			// if crosshair enabled
			if (crosshair)
			{
				// draw crosshair hud texture
				r.X = rect.X + (rect.Width - hudCrosshair.Width) / 2;
				r.Y = rect.Y + (rect.Height - hudCrosshair.Height) / 2;
				r.Width = hudCrosshair.Width;
				r.Height = hudCrosshair.Height;
				context.DrawTexture(hudCrosshair, r,
					Color.White, BlendState.AlphaBlend);
			}

			// draw score hud texture
			r.X = rect.X + (rect.Width - hudScore.Width) / 2;
			r.Y = rect.Y;
			r.Width = hudScore.Width;
			r.Height = hudScore.Height;
			context.DrawTexture(hudScore, r, Color.White, BlendState.AlphaBlend);

			// draw missile hud texture
			r.X = rect.X + rect.Width - hudMissile.Width;
			r.Y = rect.Y + rect.Height - hudMissile.Height;
			r.Width = hudMissile.Width;
			r.Height = hudMissile.Height;
			context.DrawTexture(hudMissile, r, Color.White, BlendState.AlphaBlend);

			// draw energy hud texture
			r.X = rect.X;
			r.Y = rect.Y + rect.Height - hudEnergy.Height;
			r.Width = hudEnergy.Width;
			r.Height = hudEnergy.Height;
			context.DrawTexture(hudEnergy, r, Color.White, BlendState.AlphaBlend);

			// get hud bars
			Rectangle s = new Rectangle(0, 0, hudBars.Width, hudBars.Height);

			// draw the energy bar
			r.Width = s.Width = barsLeft + (int)(barsWidth * bars.X);
			context.DrawTexture(hudBars, r, s, Color.Red, BlendState.Additive);

			// draw the shield bar
			r.Width = s.Width = barsLeft + (int)(barsWidth * bars.Y);
			context.DrawTexture(hudBars, r, s, Color.Green, BlendState.Additive);

			// draw the boost bar
			r.Width = s.Width = barsLeft + (int)(barsWidth * bars.Z);
			context.DrawTexture(hudBars, r, s, Color.Blue, BlendState.Additive);
		}

		// draw 2D gui
		public void Draw2D(RenderContext2D context)
		{
			// draw 2D game gui
			Rectangle rect = context.ScreenRectangle;

			// if in single player mode
			var gm = SG.GameManager;
			var gameMode = gm.GameMode;
			if (gameMode == GameMode.SinglePlayer)
			{
				var player = gm.GetPlayer(0);
				if (player.IsAlive)
				{
					// draw hud 
					DrawHud(context, rect, player.Bars, 70, 120,
						player.Camera3rdPerson == false);

					// draw missile count
					context.DrawText(FontType.MediumFont,
						player.MissileCount.ToString(),
						new Vector2(rect.Right - 138, rect.Bottom - 120),
						Color.LightCyan);
				}

				// draw damage indicator
				Color DamageColor = player.DamageColor;
				if (DamageColor.A > 0)
					context.DrawTexture(damageTexture, rect,
						DamageColor, BlendState.AlphaBlend);
			}
			else
			{
				// multiplayer half horizontal screen
				rect.Width /= 2;

				// if player is alive
				var player1 = gm.GetPlayer(0);
				if (player1.IsAlive)
				{
					// draw hud 
					DrawHud(context, rect, player1.Bars, 80, 100,
						player1.Camera3rdPerson == false);

					// draw missile count
					context.DrawText(FontType.MediumFont,
						player1.MissileCount.ToString(),
						new Vector2(rect.Right - 138, rect.Bottom - 125),
						Color.LightCyan);
				}

				// draw damage indicator
				Color damageColor = player1.DamageColor;
				if (damageColor.A > 0)
					context.DrawTexture(damageTexture, rect,
						damageColor, BlendState.AlphaBlend);

				// second player on second horizontal half
				rect.X += rect.Width;

				// if player is alive
				var player2 = gm.GetPlayer(1);
				if (player2.IsAlive)
				{
					// draw hud
					DrawHud(context, rect, player2.Bars, 80, 100,
						player2.Camera3rdPerson == false);

					// draw missile count
					context.DrawText(FontType.MediumFont,
						player2.MissileCount.ToString(),
						new Vector2(rect.Right - 138, rect.Bottom - 125),
						Color.LightCyan);
				}

				// draw damage indicator
				damageColor = player2.DamageColor;
				if (damageColor.A > 0)
					context.DrawTexture(damageTexture, rect,
						damageColor, BlendState.AlphaBlend);

				// draw score
				context.DrawText(FontType.LargeFont,
					player1.Score.ToString(),
					new Vector2(rect.Width / 2 - 20, 20),
					Color.LightCyan);
				context.DrawText(FontType.LargeFont,
					player2.Score.ToString(),
					new Vector2(rect.Width * 3 / 2 - 20, 20),
					Color.LightCyan);
			}
		}
	}
}
