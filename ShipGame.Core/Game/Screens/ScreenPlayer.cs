#region File Description
//-----------------------------------------------------------------------------
// ScreenPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using AssetManagementBase;
using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nursia;
using Nursia.Rendering;
using Nursia.Standard;
using System;
using System.IO;
#endregion

namespace ShipGame
{
	public class ScreenPlayer : Screen
	{
		ScreenManager screenManager;    // screen manager
		GameManager gameManager;         // game manager

		const int NumberShips = 2;    // number of available ships to choose from

		// name for each ship
		String[] ships = new String[NumberShips] { "ship2", "ship1" };

		// model for each ship
		Texture2D textureChangeShip;      // change ship texture
		Texture2D textureRotateShip;      // rotate ship texture
		Texture2D textureSelectBack;      // select and back texture
		Texture2D textureSelectCancel;    // select and cancel texture
		Texture2D textureInvertYCheck;    // checked invert y texture
		Texture2D textureInvertYUncheck;  // unchecked invert y texture

		static TextureCube reflectCube;

		// ship selection for each player
		int[] selection = new int[2] { 0, 1 };

		// confirmed status for each player
		bool[] confirmed = new bool[2] { false, false };

		// invert Y flags (bit flag for each player)
		uint invertY = 0;

		// rotation matrix for each player ship model
		float[] rotation = new float[2];

		// total elapsed time for ship model rotation
		float elapsedTime = 0.0f;

		SceneNode _sceneOnePlayer, _sceneTwoPlayers;
		readonly SceneNode[] _sceneShips = new SceneNode[2];
		ForwardRenderer _renderer = new ForwardRenderer();

		// constructor
		public ScreenPlayer(ScreenManager manager, GameManager game)
		{
			screenManager = manager;
			gameManager = game;
		}

		// called before screen shows
		public override void SetFocus(GraphicsDevice gd, AssetManager content, bool focus)
		{
			// if getting focus
			if (focus == true)
			{
				// load all resources
				confirmed[0] = false;
				confirmed[1] = (gameManager.GameMode == GameMode.SinglePlayer);

				rotation[0] = 0.0f;
				rotation[1] = 0.0f;

				_sceneOnePlayer = content.LoadScene("scenes/screenPlayer.scene").Root;
				_sceneTwoPlayers = content.LoadScene("scenes/screenPlayer2.scene").Root;
				_sceneShips[0] = content.LoadScene("scenes/ship1.scene").Root;
				_sceneShips[0].Id = "_ship";

				_sceneShips[1] = content.LoadScene("scenes/ship2.scene").Root;
				_sceneShips[1].Id = "_ship";

				textureChangeShip = content.LoadTexture2DDefault(gd, "screens/change_ship.tga");
				textureRotateShip = content.LoadTexture2DDefault(gd, "screens/rotate_ship.tga");
				textureSelectBack = content.LoadTexture2DDefault(gd, "screens/select_back.tga");
				textureSelectCancel = content.LoadTexture2DDefault(gd, "screens/select_cancel.tga");
				textureInvertYCheck = content.LoadTexture2DDefault(gd, "screens/inverty_check.tga");
				textureInvertYUncheck = content.LoadTexture2DDefault(gd, "screens/inverty_uncheck.tga");
			}
			else // loosing focus
			{
				// free all resources
				textureChangeShip = null;
				textureRotateShip = null;
				textureSelectBack = null;
				textureSelectCancel = null;
				textureInvertYCheck = null;
				textureInvertYUncheck = null;
			}
		}

		public override void ProcessInput(float elapsedTime, InputManager input)
		{
			if (input == null)
			{
				throw new ArgumentNullException("input");
			}

			const float rotationVelocity = 3.0f;

			int i, j = (int)gameManager.GameMode;

			for (i = 0; i < j; i++)
				if (confirmed[i] == false)
				{
					// change invert Y selection 
					if (input.IsKeyPressed(i, Keys.Y) || input.IsButtonPressedY(i))
					{
						invertY ^= ((uint)1 << i);
						gameManager.PlaySound("menu_change");
					}

					// confirm selection
					if (input.IsKeyPressed(i, Keys.Enter) || input.IsButtonPressedA(i))
					{
						confirmed[i] = true;
						gameManager.PlaySound("menu_select");
					}

					// cancel and return to intro menu
					if (input.IsKeyPressed(i, Keys.Escape) || input.IsButtonPressedB(i))
					{
						gameManager.SetShips(null, null, 0);
						screenManager.SetNextScreen(ScreenType.ScreenIntro);
						gameManager.PlaySound("menu_cancel");
					}

					// rotate ship
					float RotX = rotationVelocity * input.LeftStick(i).X * elapsedTime;
					if (input.IsKeyDown(i, Keys.Left))
						RotX -= rotationVelocity * elapsedTime;
					if (input.IsKeyDown(i, Keys.Right))
						RotX += rotationVelocity * elapsedTime;
					if (Math.Abs(RotX) < 0.001f)
						RotX = -0.5f * elapsedTime;

					rotation[i] += RotX;

					// change ship (next)
					if (input.IsKeyPressed(i, Keys.Up) ||
						input.IsButtonPressedDPadUp(i) ||
						input.IsButtonPressedLeftStickUp(i))
					{
						selection[i] = (selection[i] + 1) % NumberShips;

						if (i == 0)
						{
							((SubsceneNode)_sceneOnePlayer.QueryFirstById("_select").QueryFirstById("_ship")).Node = _sceneShips[selection[i]];
							((SubsceneNode)_sceneTwoPlayers.QueryFirstById("_select1").QueryFirstById("_ship")).Node = _sceneShips[selection[i]];
						} else if(i == 1)
						{
							((SubsceneNode)_sceneTwoPlayers.QueryFirstById("_select2").QueryFirstById("_ship")).Node = _sceneShips[selection[i]];
						}

						gameManager.PlaySound("menu_change");
					}

					// change ship (previous)
					if (input.IsKeyPressed(i, Keys.Down) ||
						input.IsButtonPressedDPadDown(i) ||
						input.IsButtonPressedLeftStickDown(i))
					{
						if (selection[i] == 0)
						{
							selection[i] = NumberShips - 1;
						}
						else
						{
							selection[i] = selection[i] - 1;
						}

						if (i == 0)
						{
							((SubsceneNode)_sceneOnePlayer.QueryFirstById("_select").QueryFirstById("_ship")).Node = _sceneShips[selection[i]].Clone();
							((SubsceneNode)_sceneTwoPlayers.QueryFirstById("_select1").QueryFirstById("_ship")).Node = _sceneShips[selection[i]].Clone();
						}
						else if (i == 1)
						{
							((SubsceneNode)_sceneTwoPlayers.QueryFirstById("_select2").QueryFirstById("_ship")).Node = _sceneShips[selection[i]].Clone();
						}

						gameManager.PlaySound("menu_change");
					}
				}
				else
				{
					// cancel selection
					if (input.IsKeyPressed(i, Keys.Escape) || input.IsButtonPressedB(i))
					{
						confirmed[i] = false;
						gameManager.PlaySound("menu_cancel");
					}
				}

			// if both ships confirmed, go to game screen
			if (confirmed[0] && confirmed[1])
			{
				if (gameManager.GameMode == GameMode.SinglePlayer)
				{
					gameManager.SetShips(ships[selection[0]], null, invertY);
				}
				else
				{
					gameManager.SetShips(ships[selection[0]], ships[selection[1]], invertY);
				}

				screenManager.SetNextScreen(ScreenType.ScreenLevel);
			}
		}

		public override void Update(float elapsedTime)
		{
			// accumulate elapsed time
			this.elapsedTime += elapsedTime;
		}

		public override void Draw3D(GraphicsDevice gd)
		{
			if (gd == null)
			{
				throw new ArgumentNullException("gd");
			}

			// clear backgournd
			gd.Clear(Color.Black);

			// draw background animation
			screenManager.DrawBackground(gd);

			SceneNode scene;
			if (gameManager.GameMode == GameMode.SinglePlayer)
			{
				scene = _sceneOnePlayer;

				var ship = scene.QueryFirstById("_select").QueryFirstById("_ship");
				var rotation = ship.Rotation;
				rotation.Y = MathHelper.ToDegrees(this.rotation[0]);
				ship.Rotation = rotation;

				// if not confirmed, draw animated selection circle
				if (!confirmed[0])
				{
					var padSelect = scene.QueryFirstById("_select").QueryFirstById("_padSelect");
					rotation = padSelect.Rotation;
					rotation.Y = MathHelper.ToDegrees(elapsedTime);
					padSelect.Rotation = rotation;

					float scale = 1.0f + 0.03f * (float)Math.Cos(elapsedTime * 7);
					padSelect.Scale = new Vector3(scale, scale, scale);
				}
			}
			else
			{
				scene = _sceneTwoPlayers;

				var ship = scene.QueryFirstById("_select1").QueryFirstById("_ship");
				var rotation = ship.Rotation;
				rotation.Y = MathHelper.ToDegrees(this.rotation[0]);
				ship.Rotation = rotation;

				// if not confirmed, draw animated selection circle for player 1
				if (!confirmed[0])
				{
					var padSelect = scene.QueryFirstById("_select1").QueryFirstById("_padSelect");
					rotation = padSelect.Rotation;
					rotation.Y = MathHelper.ToDegrees(elapsedTime);
					padSelect.Rotation = rotation;

					float scale = 1.0f + 0.03f * (float)Math.Cos(elapsedTime * 7);
					padSelect.Scale = new Vector3(scale, scale, scale);
				}

				ship = scene.QueryFirstById("_select2").QueryFirstById("_ship");
				rotation = ship.Rotation;
				rotation.Y = MathHelper.ToDegrees(this.rotation[1]);
				ship.Rotation = rotation;

				// if not confirmed, draw animated selection circle for player 2
				if (!confirmed[1])
				{
					var padSelect = scene.QueryFirstById("_select2").QueryFirstById("_padSelect");
					rotation = padSelect.Rotation;
					rotation.Y = MathHelper.ToDegrees(elapsedTime);
					padSelect.Rotation = rotation;

					float scale = 1.0f + 0.03f * (float)Math.Cos(elapsedTime * 7);
					padSelect.Scale = new Vector3(scale, scale, scale);
				}
			}

			var camera = scene.QueryFirstByType<Camera>();
			_renderer.Render(scene, camera);
		}

		public override void Draw2D(GraphicsDevice gd, FontManager font)
		{
			if (gd == null)
			{
				throw new ArgumentNullException("gd");
			}

			Rectangle rect = new Rectangle(0, 0, 0, 0);

			int screenSizeX = gd.Viewport.Width;
			int screenSizeY = gd.Viewport.Height;

			// if single player mode
			if (gameManager.GameMode == GameMode.SinglePlayer)
			{
				rect.Width = textureSelectBack.Width;
				rect.Height = textureSelectBack.Height;
				rect.X = screenSizeX / 2 - rect.Width / 2;
				rect.Y = 50;
				if (confirmed[0])
				{
					rect.Width = textureSelectCancel.Width;
					rect.Height = textureSelectCancel.Height;
					screenManager.DrawTexture(textureSelectCancel, rect,
						Color.White, BlendState.AlphaBlend);
				}
				else
					screenManager.DrawTexture(textureSelectBack, rect,
						Color.White, BlendState.AlphaBlend);

				rect.Width = textureInvertYCheck.Width;
				rect.Height = textureInvertYCheck.Height;
				rect.Y = screenSizeY - rect.Height - 30;
				rect.X = screenSizeX / 2 - rect.Width / 2;
				if ((invertY & 1) == 0)
					screenManager.DrawTexture(textureInvertYUncheck, rect,
						Color.White, BlendState.AlphaBlend);
				else
					screenManager.DrawTexture(textureInvertYCheck, rect,
						Color.White, BlendState.AlphaBlend);

				rect.Width = textureChangeShip.Width;
				rect.Height = textureChangeShip.Height;
				rect.X = screenSizeX / 5 - rect.Width / 2;
				rect.Y = 60;
				screenManager.DrawTexture(textureChangeShip, rect,
					Color.White, BlendState.AlphaBlend);

				rect.Width = textureRotateShip.Width;
				rect.Height = textureRotateShip.Height;
				rect.X = screenSizeX * 4 / 5 - rect.Width / 2;
				rect.Y = 60;
				screenManager.DrawTexture(textureRotateShip, rect,
					Color.White, BlendState.AlphaBlend);
			}
			else // if multi player mode
			{
				rect.Width = textureChangeShip.Width;
				rect.Height = textureChangeShip.Height;
				rect.X = (screenSizeX - rect.Width) / 2;
				rect.Y = 40;
				screenManager.DrawTexture(textureChangeShip, rect,
					Color.White, BlendState.AlphaBlend);

				rect.Width = textureRotateShip.Width;
				rect.Height = textureRotateShip.Height;
				rect.X = (screenSizeX - rect.Width) / 2;
				rect.Y = 40 + textureChangeShip.Height;
				screenManager.DrawTexture(textureRotateShip, rect,
					Color.White, BlendState.AlphaBlend);

				rect.Width = textureInvertYCheck.Width;
				rect.Height = textureInvertYCheck.Height;
				rect.Y = screenSizeY - rect.Height - 30;
				rect.X = screenSizeX / 4 - rect.Width / 2;
				if ((invertY & 1) == 0)
					screenManager.DrawTexture(textureInvertYUncheck, rect,
						Color.White, BlendState.AlphaBlend);
				else
					screenManager.DrawTexture(textureInvertYCheck, rect,
						Color.White, BlendState.AlphaBlend);
				rect.X = screenSizeX * 3 / 4 - rect.Width / 2;
				if ((invertY & 2) == 0)
					screenManager.DrawTexture(textureInvertYUncheck, rect,
						Color.White, BlendState.AlphaBlend);
				else
					screenManager.DrawTexture(textureInvertYCheck, rect,
						Color.White, BlendState.AlphaBlend);

				rect.Width = textureSelectBack.Width;
				rect.Height = textureSelectBack.Height;
				rect.X = screenSizeX / 8 - rect.Width / 2;
				rect.Y = 40;
				if (confirmed[0])
				{
					rect.Width = textureSelectCancel.Width;
					rect.Height = textureSelectCancel.Height;
					screenManager.DrawTexture(textureSelectCancel, rect,
						Color.White, BlendState.AlphaBlend);
				}
				else
					screenManager.DrawTexture(textureSelectBack, rect,
						Color.White, BlendState.AlphaBlend);
				rect.Width = textureSelectBack.Width;
				rect.Height = textureSelectBack.Height;
				rect.X = screenSizeX * 7 / 8 - rect.Width / 2;
				rect.Y = 40;
				if (confirmed[1])
				{
					rect.Width = textureSelectCancel.Width;
					rect.Height = textureSelectCancel.Height;
					screenManager.DrawTexture(textureSelectCancel, rect,
						Color.White, BlendState.AlphaBlend);
				}
				else
					screenManager.DrawTexture(textureSelectBack, rect,
						Color.White, BlendState.AlphaBlend);
			}
		}
		/// <summary>
		/// Performs effect initialization, which is required in XNA 4.0
		/// </summary>
		/// <param name="model"></param>
		private void FixupShip(NursiaModelNode model, string path)
		{
			ShipGameGame game = ShipGameGame.GetInstance();

			foreach (var mesh in model.Model.Meshes)
			{
				// for each mesh part
				/*				foreach (Effect effect in mesh.Effects)
								{
									effect.Parameters["Reflect"].SetValue(GetReflectCube());
								}*/
			}
		}

		/// <summary>
		/// Creates a reflection textureCube
		/// </summary>
		static TextureCube GetReflectCube()
		{
			if (reflectCube != null)
				return reflectCube;

			Color[] cc = new Color[]
			{
				new Color(1,0,0), new Color(0.9f,0,0.1f),
				new Color(0.8f,0,0.2f), new Color(0.7f,0,0.3f),
				new Color(0.6f,0,0.4f), new Color(0.5f,0,0.5f),
				new Color(0.4f,0,0.6f), new Color(0.3f,0,0.7f),
				new Color(0.2f,0,0.8f), new Color(0.1f,0,0.9f),
				new Color(0.1f,0,0.9f), new Color(0.0f,0,1.0f),
			};

			reflectCube = new TextureCube(ShipGameGame.GetInstance().GraphicsDevice,
				8, true, SurfaceFormat.Color);

			Random rand = new Random();

			for (int s = 0; s < 6; s++)
			{
				Color[] sideData = new Color[reflectCube.Size * reflectCube.Size];
				for (int i = 0; i < sideData.Length; i++)
				{
					sideData[i] = cc[rand.Next(cc.Length)];
				}
				reflectCube.SetData((CubeMapFace)s, sideData);
			}

			return reflectCube;
		}



	}
}
