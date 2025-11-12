#region File Description
//-----------------------------------------------------------------------------
// Screen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

namespace ShipGame
{
	public enum ScreenType
	{
		ScreenIntro = 0,
		ScreenHelp,
		ScreenPlayer,
		ScreenLevel,
		ScreenGame,
		ScreenEnd
	};

	public interface IScreen
	{
		void Set();
		
		void Unset();

		void ProcessInput(float elapsedTime);

		// called to update state
		void Update(float elapsedTime);

		// called to draw the 3D world
		void Draw3D();

		// called to draw the 2D info text and hud
		void Draw2D(RenderContext2D context);
	}
}
