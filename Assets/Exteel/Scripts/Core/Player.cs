﻿namespace Exteel.Core.Player
{
	public class Player //ToDo: Rename to "User"?
	{
		//Inventory List<> Parts
		//Mechs List<> Builds
		//Mechs Array[SlotsAvailable] LoadOuts
		//int Credits
		//int RepairPoints
		//byte LoadoutSlots
		//Mech Mech //Active Mech
		//Pilot Pilot //Active Pilot
		//int[] Stats/Experience Points //Feed From Server, may not need it in class
		public Mechanaughts ActiveMech { get; set; }

		//ToDo: Move to Global Class
		//internal Mechanaughts LoadFromServer(System.Guid HashId)
		//{
		//	//GetPart(Id);
		//	//SetColor(int, datetime expire)
		//	//SetDurability(int)
		//	throw new System.Exception("Part ID doesnt exist in the database. Please check Arms constructor.");
		//}

		#region Nested Classes
		#endregion
	}
}

namespace Exteel.Game.Player
{
	public class Player : Exteel.Core.Player.Player
	{
		//Inventory List<> Parts
		//Mechs List<> Builds
		//int RepairPoints
		//Mech Mech //Active Mech
		//Pilot Pilot //Active Pilot
		//int CurrentHP
		//int CurrentEN
		//int CurrentSP
	}
}