#region File Description
//-----------------------------------------------------------------------------
// ScreenPlayer.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using DigitalRiseModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nursia.SceneGraph;
using Nursia.SceneGraph.Cameras;
using System;
using Nursia;
#endregion

namespace ShipGame
{
	public class ScreenPlayer : IScreen
	{
		const int NumberShips = 2;    // number of available ships to choose from

		// name for each ship
		String[] ships = new String[NumberShips] { "ship2", "ship1" };

		// model for each ship
		DrModel[] shipModels = new DrModel[NumberShips];

		DrModel padModel;           // ship pad model
		DrModel padHaloModel;       // ship pad halo model
		DrModel padSelectModel;     // ship pad select model

		Texture2D textureChangeShip;      // change ship texture
		Texture2D textureRotateShip;      // rotate ship texture
		Texture2D textureSelectBack;      // select and back texture
		Texture2D textureSelectCancel;    // select and cancel texture
		Texture2D textureInvertYCheck;    // checked invert y texture
		Texture2D textureInvertYUncheck;  // unchecked invert y texture

		LightList lights;     // lights for scene

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
		float totalElapsedTime = 0.0f;

		public bool DrawBackground => true;

		public StoredScene Scene3D { get; private set; }

		public void Set()
		{
			// load all resources
			confirmed[0] = false;
			confirmed[1] = (SG.GameManager.GameMode == GameMode.SinglePlayer);

			rotation[0] = 0.0f;
			rotation[1] = 0.0f;

			var content = SG.Assets;
			lights = LightList.Load(content, "screens/player_lights.xml");

			for (int i = 0; i < NumberShips; i++)
			{
				shipModels[i] = content.LoadModel2($"ships/{ships[i]}");
				FixupShip(shipModels[i], "ships/" + ships[i]);
			}

			padModel = content.LoadModel2("ships/pad");
			padHaloModel = content.LoadModel2("ships/pad_halo");
			padSelectModel = content.LoadModel2("ships/pad_select");

			textureChangeShip = content.LoadTexture2DDefault("screens/change_ship.tga");
			textureRotateShip = content.LoadTexture2DDefault("screens/rotate_ship.tga");
			textureSelectBack = content.LoadTexture2DDefault("screens/select_back.tga");
			textureSelectCancel = content.LoadTexture2DDefault("screens/select_cancel.tga");
			textureInvertYCheck = content.LoadTexture2DDefault("screens/inverty_check.tga");
			textureInvertYUncheck = content.LoadTexture2DDefault("screens/inverty_uncheck.tga");

			var gameManager = SG.GameManager;
			if (gameManager.GameMode == GameMode.SinglePlayer)
			{
				Scene3D = content.LoadStoredScene("scenes/screenPlayer.scene");
			}
			else
			{
				Scene3D = content.LoadStoredScene("scenes/screenPlayer2.scene");
			}

			// Replace editor camera with camera node
			var camera = Scene3D.Root.QueryFirstByType<Camera>();
			camera.Parent.Children.Remove(camera);
			Scene3D.Camera = camera;
		}

		public void Unset()
		{
			// free all resources
			lights = null;

			for (int i = 0; i < NumberShips; i++)
				shipModels[i] = null;

			padModel = null;
			padHaloModel = null;
			padSelectModel = null;

			textureChangeShip = null;
			textureRotateShip = null;
			textureSelectBack = null;
			textureSelectCancel = null;
			textureInvertYCheck = null;
			textureInvertYUncheck = null;

			Scene3D = null;
		}

		private void UpdateShipModel(string subsceneId, int shipId)
		{
			var shipSubscene = (SubsceneNode)Scene3D.Root.QueryFirstById(subsceneId).QueryFirstById("_ship");
			if (shipId == 0)
			{
				shipSubscene.Node = SG.Assets.LoadSceneNode("scenes/ship1.scene");
			}
			else
			{
				shipSubscene.Node = SG.Assets.LoadSceneNode("scenes/ship2.scene");
			}
		}

		public void ProcessInput(float elapsedTime)
		{
			const float rotationVelocity = 3.0f;

			var input = SG.InputManager;
			var gameManager = SG.GameManager;
			var screenManager = SG.ScreenManager;

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

						// Update model
						if (SG.GameManager.GameMode == GameMode.SinglePlayer)
						{
							UpdateShipModel("_select", selection[i]);
						} else
						{
							if (i == 0)
							{
								UpdateShipModel("_select1", selection[i]);
							}
							else
							{
								UpdateShipModel("_select2", selection[i]);
							}
						}

						gameManager.PlaySound("menu_change");
					}

					// change ship (previous)
					if (input.IsKeyPressed(i, Keys.Down) ||
						input.IsButtonPressedDPadDown(i) ||
						input.IsButtonPressedLeftStickDown(i))
					{
						if (selection[i] == 0)
							selection[i] = NumberShips - 1;
						else
							selection[i] = selection[i] - 1;
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
					gameManager.SetShips(ships[selection[0]], null, invertY);
				else
					gameManager.SetShips(ships[selection[0]],
								ships[selection[1]], invertY);
				screenManager.SetNextScreen(ScreenType.ScreenLevel);
			}
		}

		public void Update(float elapsedTime)
		{
			// accumulate elapsed time
			totalElapsedTime += elapsedTime;

			// Update the scene
			// if single player mode
			var gameManager = SG.GameManager;
			var root = Scene3D.Root;
			if (gameManager.GameMode == GameMode.SinglePlayer)
			{
				// Set ship rotation
				var shipSubscene = (SubsceneNode)root.QueryFirstById("_select").QueryFirstById("_ship");
				var rotation = shipSubscene.Rotation;
				rotation.Y = MathHelper.ToDegrees(this.rotation[0]);
				shipSubscene.Rotation = rotation;

				// if not confirmed, draw animated selection circle
				if (confirmed[0] == false)
				{
					var padSelect = root.QueryFirstById("_select").QueryFirstById("_padSelect");

					var transform = Matrix.CreateRotationY(totalElapsedTime);
					float scale = 1.0f + 0.03f * (float)Math.Cos(totalElapsedTime * 7);
					transform = transform * Matrix.CreateScale(scale);
					transform.M42 = -10;

					padSelect.LocalTransform = transform;
				}
			}
			else // if multi player mode
			{
				// Left ship rotation
				var ship = root.QueryFirstById("_select1").QueryFirstById("_ship");
				var rotation = ship.Rotation;
				rotation.Y = MathHelper.ToDegrees(this.rotation[0]);
				ship.Rotation = rotation;

				// if not confirmed, draw animated selection circle for player 1
				if (!confirmed[0])
				{
					var padSelect = root.QueryFirstById("_select1").QueryFirstById("_padSelect");

					var transform = Matrix.CreateRotationY(totalElapsedTime);
					float scale = 0.9f + 0.03f * (float)Math.Cos(totalElapsedTime * 7);
					transform = transform * Matrix.CreateScale(scale);
					transform.M41 = 90;
					transform.M42 = -10;

					padSelect.LocalTransform = transform;
				}

				// Right ship rotation
				ship = root.QueryFirstById("_select2").QueryFirstById("_ship");
				rotation = ship.Rotation;
				rotation.Y = MathHelper.ToDegrees(this.rotation[1]);
				ship.Rotation = rotation;

				// if not confirmed, draw animated selection circle for player 2
				if (confirmed[1] == false)
				{
					var padSelect = root.QueryFirstById("_select2").QueryFirstById("_padSelect");

					var transform = Matrix.CreateRotationY(totalElapsedTime);
					float scale = 0.9f + 0.03f * (float)Math.Cos(totalElapsedTime * 7);
					transform = transform * Matrix.CreateScale(scale);
					transform.M41 = -90;
					transform.M42 = -10;

					padSelect.LocalTransform = transform;
				}
			}

			// camera position
			Vector3 cameraPosition = new Vector3(0, 240, -800);

			// view and projection matrices
			Matrix view = Matrix.CreateLookAt(cameraPosition, Vector3.Zero, Vector3.Up);
			Scene3D.Camera.View = view;
		}

		public void Draw2D(RenderContext2D context)
		{
			Rectangle rect = new Rectangle(0, 0, 0, 0);

			var gd = Nrs.GraphicsDevice;
			int screenSizeX = gd.Viewport.Width;
			int screenSizeY = gd.Viewport.Height;

			// if single player mode
			if (SG.GameManager.GameMode == GameMode.SinglePlayer)
			{
				rect.Width = textureSelectBack.Width;
				rect.Height = textureSelectBack.Height;
				rect.X = screenSizeX / 2 - rect.Width / 2;
				rect.Y = 50;
				if (confirmed[0])
				{
					rect.Width = textureSelectCancel.Width;
					rect.Height = textureSelectCancel.Height;
					context.DrawTexture(textureSelectCancel, rect,
						Color.White, BlendState.AlphaBlend);
				}
				else
					context.DrawTexture(textureSelectBack, rect,
						Color.White, BlendState.AlphaBlend);

				rect.Width = textureInvertYCheck.Width;
				rect.Height = textureInvertYCheck.Height;
				rect.Y = screenSizeY - rect.Height - 30;
				rect.X = screenSizeX / 2 - rect.Width / 2;
				if ((invertY & 1) == 0)
					context.DrawTexture(textureInvertYUncheck, rect,
						Color.White, BlendState.AlphaBlend);
				else
					context.DrawTexture(textureInvertYCheck, rect,
						Color.White, BlendState.AlphaBlend);

				rect.Width = textureChangeShip.Width;
				rect.Height = textureChangeShip.Height;
				rect.X = screenSizeX / 5 - rect.Width / 2;
				rect.Y = 60;
				context.DrawTexture(textureChangeShip, rect,
					Color.White, BlendState.AlphaBlend);

				rect.Width = textureRotateShip.Width;
				rect.Height = textureRotateShip.Height;
				rect.X = screenSizeX * 4 / 5 - rect.Width / 2;
				rect.Y = 60;
				context.DrawTexture(textureRotateShip, rect,
					Color.White, BlendState.AlphaBlend);
			}
			else // if multi player mode
			{
				rect.Width = textureChangeShip.Width;
				rect.Height = textureChangeShip.Height;
				rect.X = (screenSizeX - rect.Width) / 2;
				rect.Y = 40;
				context.DrawTexture(textureChangeShip, rect,
					Color.White, BlendState.AlphaBlend);

				rect.Width = textureRotateShip.Width;
				rect.Height = textureRotateShip.Height;
				rect.X = (screenSizeX - rect.Width) / 2;
				rect.Y = 40 + textureChangeShip.Height;
				context.DrawTexture(textureRotateShip, rect,
					Color.White, BlendState.AlphaBlend);

				rect.Width = textureInvertYCheck.Width;
				rect.Height = textureInvertYCheck.Height;
				rect.Y = screenSizeY - rect.Height - 30;
				rect.X = screenSizeX / 4 - rect.Width / 2;
				if ((invertY & 1) == 0)
					context.DrawTexture(textureInvertYUncheck, rect,
						Color.White, BlendState.AlphaBlend);
				else
					context.DrawTexture(textureInvertYCheck, rect,
						Color.White, BlendState.AlphaBlend);
				rect.X = screenSizeX * 3 / 4 - rect.Width / 2;
				if ((invertY & 2) == 0)
					context.DrawTexture(textureInvertYUncheck, rect,
						Color.White, BlendState.AlphaBlend);
				else
					context.DrawTexture(textureInvertYCheck, rect,
						Color.White, BlendState.AlphaBlend);

				rect.Width = textureSelectBack.Width;
				rect.Height = textureSelectBack.Height;
				rect.X = screenSizeX / 8 - rect.Width / 2;
				rect.Y = 40;
				if (confirmed[0])
				{
					rect.Width = textureSelectCancel.Width;
					rect.Height = textureSelectCancel.Height;
					context.DrawTexture(textureSelectCancel, rect,
						Color.White, BlendState.AlphaBlend);
				}
				else
					context.DrawTexture(textureSelectBack, rect,
						Color.White, BlendState.AlphaBlend);
				rect.Width = textureSelectBack.Width;
				rect.Height = textureSelectBack.Height;
				rect.X = screenSizeX * 7 / 8 - rect.Width / 2;
				rect.Y = 40;
				if (confirmed[1])
				{
					rect.Width = textureSelectCancel.Width;
					rect.Height = textureSelectCancel.Height;
					context.DrawTexture(textureSelectCancel, rect,
						Color.White, BlendState.AlphaBlend);
				}
				else
					context.DrawTexture(textureSelectBack, rect,
						Color.White, BlendState.AlphaBlend);
			}
		}
		/// <summary>
		/// Performs effect initialization, which is required in XNA 4.0
		/// </summary>
		/// <param name="model"></param>
		private void FixupShip(DrModel model, string path)
		{
			foreach (var mesh in model.Meshes)
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

			reflectCube = new TextureCube(Nrs.GraphicsDevice, 8, true, SurfaceFormat.Color);

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
