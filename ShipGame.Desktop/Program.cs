using AssetManagementBase;
using System;

namespace ShipGame
{
	class Program
	{
		static void Main(string[] args)
		{
			AMBConfiguration.Logger = Console.WriteLine;

			using (var game = new ShipGameGame())
			{
				ShipGameGame.SetInstance(game);
				game.Run();
			}
		}
	}
}
