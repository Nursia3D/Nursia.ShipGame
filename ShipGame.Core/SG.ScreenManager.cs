using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nursia;
using Nursia.Rendering;
using Nursia.SceneGraph;
using Nursia.SceneGraph.Cameras;
using System;
using System.Collections.Generic;

namespace ShipGame
{
	partial class SG
	{
		public class ScreenManagerType : IDisposable
		{
			List<IScreen> screens;         // list of available screens
			IScreen current;               // currently active screen
			IScreen next;                  // next screen on a transition 
										  // (null for no transition)

			float fadeTime = 1.0f;        // total fade time when in a transition
			float fade = 0.0f;            // current fade time when in a transition
			Vector4 fadeColor = Vector4.One;  // color fading in and out

			RenderContext2D context2D;
			BlurManager blurManager;     // blur manager

			int frameRate;        // current game frame rate (in frames per sec)
			int frameRateCount;   // current frame count since last frame rate update
			float frameRateTime;  // elapsed time since last frame rate update

			Texture2D  textureBackground;  // the background texture used on menus
			float backgroundTime = 0.0f;  // time for background animation used on menus
			ForwardRenderer forwardRenderer;
			SpriteBatch spriteBatch;

			// constructor
			public ScreenManagerType()
			{
				screens = new List<IScreen>();

				// add all screens
				screens.Add(new ScreenIntro());
				screens.Add(new ScreenHelp());
				screens.Add(new ScreenPlayer());
				screens.Add(new ScreenLevel());
				screens.Add(new ScreenGame());
				screens.Add(new ScreenEnd());

				// fade in to intro screen
				SetNextScreen(ScreenType.ScreenIntro,
					GameOptions.FadeColor, GameOptions.FadeTime);
				fade = fadeTime * 0.5f;

				context2D = new RenderContext2D();
				forwardRenderer = new ForwardRenderer();
				spriteBatch = new SpriteBatch(Nrs.GraphicsDevice);
			}

			// process input
			public void ProcessInput(float elapsedTime)
			{
				InputManager.BeginInputProcessing(GameManager.GameMode == GameMode.SinglePlayer);

				// process input for currently active screen
				if (current != null && next == null)
					current.ProcessInput(elapsedTime);

				// toggle full screen with F5 key
				if (InputManager.IsKeyPressed(0, Keys.F5) ||
					InputManager.IsKeyPressed(1, Keys.F5))
					ShipGameGame.ToggleFullScreen();

				InputManager.EndInputProcessing();
			}

			// update for given elapsed time
			public void Update(float elapsedTime)
			{
				// if in a transition
				if (fade > 0)
				{
					// update transition time
					fade -= elapsedTime;

					// if time to switch to new screen (fade out finished)
					if (next != null && fade < 0.5f * fadeTime)
					{
						// tell new screen it is getting in focus
						next.Set();

						// tell the old screen it lost its focus
						if (current != null)
							current.Unset();

						// set new screen as current
						current = next;
						next = null;
					}
				}

				// if current screen available, update it
				if (current != null)
					current.Update(elapsedTime);

				// calulate frame rate
				frameRateTime += elapsedTime;
				if (frameRateTime > 0.5f)
				{
					frameRate = (int)((float)frameRateCount / frameRateTime);
					frameRateCount = 0;
					frameRateTime = 0;
				}

				// accumulate elapsed time for background animation
				backgroundTime += elapsedTime;
			}

			// draw the background animated image
			private void DrawBackground()
			{
				const float animationTime = 3.0f;
				const float animationLength = 0.4f;
				const int numberLayers = 2;
				const float layerDistance = 1.0f / numberLayers;

				// normalized time
				float normalizedTime = ((backgroundTime / animationTime) % 1.0f);

				// set render states
				var gd = Nrs.GraphicsDevice;
				DepthStencilState ds = gd.DepthStencilState;
				BlendState bs = gd.BlendState;
				gd.DepthStencilState = DepthStencilState.DepthRead;
				gd.BlendState = BlendState.AlphaBlend;

				float scale;
				Vector4 color;

				// render all background layers
				for (int i = 0; i < numberLayers; i++)
				{
					if (normalizedTime > 0.5f)
						scale = 2 - normalizedTime * 2;
					else
						scale = normalizedTime * 2;
					color = new Vector4(scale, scale, scale, 0);

					scale = 1 + normalizedTime * animationLength;

					blurManager.RenderScreenQuad(BlurTechnique.ColorTexture, textureBackground, color, scale);

					normalizedTime = (normalizedTime + layerDistance) % 1.0f;
				}

				// restore render states
				gd.DepthStencilState = ds;
				gd.BlendState = bs;
			}

			// draws the currently active screen
			public void Draw()
			{
				frameRateCount++;

				// if a valid current screen is set
				var gd = Nrs.GraphicsDevice;
				if (current != null)
				{
					RenderTarget2D screenBuffer = null;
					
					// draw the screen 3D scene to target
					var scene = current.Scene3D;
					if (scene != null)
					{
						scene.Camera.SetViewport(gd.Viewport.Width, gd.Viewport.Height);

						screenBuffer = forwardRenderer.RenderToTarget(scene.Root, scene.Camera, scene.RenderEnvironment);
					}

					gd.SetRenderTarget(null);

					// clear background
					gd.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1, 0);

					if (current.DrawBackground)
					{
						DrawBackground();
					}

					// Draw the 3d
					if (screenBuffer != null)
					{
						spriteBatch.Begin();

						spriteBatch.Draw(screenBuffer, Vector2.Zero, Color.White);

						spriteBatch.End();
					}

					// begin text mode
					context2D.BeginText();

					// draw the 2D scene 
					current.Draw2D(context2D);

					// draw fps
					//fontManager.DrawText(
					//    FontType.SmallFont,
					//    "FPS: " + frameRate,
					//    new Vector2(gd.Viewport.Width - 80, 0), Color.White);

					// end text mode
					context2D.EndText();
				}

				// if in a transition
				if (fade > 0)
				{
					// compute transtition fade intensity
					float size = fadeTime * 0.5f;
					fadeColor.W = 1.25f * (1.0f - Math.Abs(fade - size) / size);

					// set alpha blend and no depth test or write
					gd.DepthStencilState = DepthStencilState.None;
					gd.BlendState = BlendState.AlphaBlend;

					// draw transition fade color
					blurManager.RenderScreenQuad(BlurTechnique.Color, null, fadeColor);

					// restore render states
					gd.DepthStencilState = DepthStencilState.Default;
					gd.BlendState = BlendState.Opaque;
				}
			}

			// load all content
			public void LoadContent()
			{
				var content = Assets;
				textureBackground = content.LoadTexture2DDefault("screens/intro_bg.tga");
				// create blur manager
				blurManager = new BlurManager(content.LoadEffect2("Blur.efb"),
					GameOptions.GlowResolution, GameOptions.GlowResolution);

				var gd = Nrs.GraphicsDevice;
				int width = gd.Viewport.Width;
				int height = gd.Viewport.Height;

				context2D.LoadContent();
			}

			// unload all content
			public void UnloadContent()
			{
				textureBackground = null;
				if (blurManager != null)
				{
					blurManager.Dispose();
					blurManager = null;
				}

				context2D.UnloadContent();
			}

			// starts a transition to a new screen
			// using a 1 sec fade time to custom color
			public bool SetNextScreen(ScreenType screenType, Vector4 fadeColor,
				float fadeTime)
			{
				// if no transition already happening
				if (next == null)
				{
					// set next screen and transition options
					next = screens[(int)screenType];
					this.fadeTime = fadeTime;
					this.fadeColor = fadeColor;
					this.fade = this.fadeTime;
					return true;
				}
				return false;
			}

			// starts a transition to a new screen
			// using a 1 sec fade time to custom color
			public bool SetNextScreen(ScreenType screenType, Vector4 fadeColor)
			{
				return SetNextScreen(screenType, fadeColor, 1.0f);
			}

			// starts a transition to a new screen
			// using a 1 sec fade time to black
			public bool SetNextScreen(ScreenType screenType)
			{
				return SetNextScreen(screenType, Vector4.Zero, 1.0f);
			}

			// get screen with given type
			public IScreen GetScreen(ScreenType screenType)
			{
				return screens[(int)screenType];
			}

			// get intro screen
			public ScreenIntro ScreenIntro
			{ get { return (ScreenIntro)screens[(int)ScreenType.ScreenIntro]; } }

			// get help screen
			public ScreenIntro ScreenHelp
			{ get { return (ScreenIntro)screens[(int)ScreenType.ScreenHelp]; } }

			// get player screen
			public ScreenPlayer ScreenPlayer
			{ get { return (ScreenPlayer)screens[(int)ScreenType.ScreenPlayer]; } }

			// get level screen
			public ScreenLevel ScreenLevel
			{ get { return (ScreenLevel)screens[(int)ScreenType.ScreenLevel]; } }

			// get game screen
			public ScreenGame ScreenGame
			{ get { return (ScreenGame)screens[(int)ScreenType.ScreenGame]; } }

			// get end screen
			public ScreenEnd ScreenEnd
			{ get { return (ScreenEnd)screens[(int)ScreenType.ScreenEnd]; } }

			// exit game
			public void Exit() { ShipGameGame.DoExit(); }

			#region IDisposable Members

			bool isDisposed = false;
			public bool IsDisposed
			{
				get { return isDisposed; }
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			void Dispose(bool disposing)
			{
				if (disposing && !isDisposed)
				{
					UnloadContent();

					context2D.Dispose();
				}
			}
			#endregion
		}
	}
}
