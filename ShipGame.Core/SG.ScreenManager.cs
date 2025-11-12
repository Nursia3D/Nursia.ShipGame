using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

			RenderTarget2D colorRT;   // render target for main color buffer
			RenderTarget2D glowRT1;   // render target for glow horizontal blur
			RenderTarget2D glowRT2;   // render target for glow vertical blur

			RenderContext2D context2D;
			BlurManager blurManager;     // blur manager

			int frameRate;        // current game frame rate (in frames per sec)
			int frameRateCount;   // current frame count since last frame rate update
			float frameRateTime;  // elapsed time since last frame rate update

			Texture2D textureBackground;  // the background texture used on menus
			float backgroundTime = 0.0f;  // time for background animation used on menus

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

			// blur the color render target using the alpha channel and blur intensity
			void BlurGlowRenterTarget()
			{
				//DepthStencilState ds = new DepthStencilState() { DepthBufferEnable = true, DepthBufferWriteEnable = true };
				//gd.DepthStencilState = ds;

				var gd = GraphicsDevice;
				gd.DepthStencilState = DepthStencilState.None;
				//gd.BlendState = BlendState.Opaque;


				// if in game screen and split screen mode
				if (current == ScreenGame &&
					GameManager.GameMode == GameMode.MultiPlayer)
				{
					// blur horizontal with split horizontal blur shader
					gd.SetRenderTarget(glowRT1);
					blurManager.RenderScreenQuad(BlurTechnique.BlurHorizontalSplit, colorRT, Vector4.One);
				}
				else
				{
					// blur horizontal with regular horizontal blur shader
					gd.SetRenderTarget(glowRT1);
					blurManager.RenderScreenQuad(BlurTechnique.BlurHorizontal, colorRT, Vector4.One);
				}

				// blur vertical with regular vertical blur shader
				gd.SetRenderTarget(glowRT2);
				blurManager.RenderScreenQuad(BlurTechnique.BlurVertical, glowRT1, Vector4.One);

				//ds = new DepthStencilState() { DepthBufferEnable = false, DepthBufferWriteEnable = false };
				//gd.DepthStencilState = ds;
				gd.DepthStencilState = DepthStencilState.Default;

				gd.SetRenderTarget(null);
			}

			// draw render target as fullscreen texture with given intensity and blend mode
			void DrawRenderTargetTexture(
				RenderTarget2D renderTarget,
				float intensity,
				bool additiveBlend)
			{
				// set up render state and blend mode
				//BlendState bs = gd.BlendState;
				//gd.DepthStencilState = DepthStencilState.Default;
				//if (additiveBlend)
				//{
				//    gd.BlendState = BlendState.Additive;
				//}

				var gd = GraphicsDevice;
				gd.DepthStencilState = DepthStencilState.None;
				if (additiveBlend)
				{
					gd.BlendState = BlendState.Additive;
				}

				// draw render tareget as fullscreen texture
				blurManager.RenderScreenQuad(BlurTechnique.ColorTexture,
					renderTarget, new Vector4(intensity));

				// restore render state and blend mode
				//gd.BlendState = bs;
				//gd.DepthStencilState = DepthStencilState.Default;
				//gd.BlendState = BlendState.Opaque;
				gd.DepthStencilState = DepthStencilState.Default;

			}

			// draw the background animated image
			public void DrawBackground()
			{
				const float animationTime = 3.0f;
				const float animationLength = 0.4f;
				const int numberLayers = 2;
				const float layerDistance = 1.0f / numberLayers;

				// normalized time
				float normalizedTime = ((backgroundTime / animationTime) % 1.0f);

				// set render states
				var gd = GraphicsDevice;
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
				var gd = GraphicsDevice;
				if (current != null)
				{
					// set the color render target
					gd.SetRenderTarget(colorRT);

					// draw the screen 3D scene
					current.Draw3D();

					// resolve the color render target
					gd.SetRenderTarget(null);

					// blur the glow render target
					BlurGlowRenterTarget();

					// draw the 3D scene texture
					DrawRenderTargetTexture(colorRT, 1.0f, false);

					// draw the glow texture with additive blending
					DrawRenderTargetTexture(glowRT2, 2.0f, true);

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

				var gd = GraphicsDevice;
				int width = gd.Viewport.Width;
				int height = gd.Viewport.Height;

				// create render targets
				colorRT = new RenderTarget2D(gd, width, height,
					true, SurfaceFormat.Color, DepthFormat.Depth24);
				glowRT1 = new RenderTarget2D(gd, GameOptions.GlowResolution, GameOptions.GlowResolution,
					true, SurfaceFormat.Color, DepthFormat.Depth24);
				glowRT2 = new RenderTarget2D(gd, GameOptions.GlowResolution, GameOptions.GlowResolution,
					true, SurfaceFormat.Color, DepthFormat.Depth24);

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

				if (colorRT != null)
				{
					colorRT.Dispose();
					colorRT = null;
				}
				if (glowRT1 != null)
				{
					glowRT1.Dispose();
					glowRT1 = null;
				}
				if (glowRT2 != null)
				{
					glowRT2.Dispose();
					glowRT2 = null;
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
