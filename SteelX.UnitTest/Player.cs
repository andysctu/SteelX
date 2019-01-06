using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SteelX.UnitTest
{
	[TestClass]
	public class Player
	{
		#region Player Unit Database Values 
		//A Single Mechanaught Unit should be a collection of its `Part` Class
		//So UnitTesting any Individual Mechnaught set or player build will acknowledge group of pieces

		[TestMethod]
		public void Player_Get_Mech()
		{
			//a player's single mech unit consisted of
			//a pilot (each mech is assigned to a pilot, but muliple mechs can have the same pilot --i think) 
			//(i could be wrong, and it's probably one pilot in player profile, and you switch between them before and after games)
			//a full mech (1 of each part: arm, leg, core, head, booster)
			//4 weapons (or any combination and set that totals up to a LH/RH twice -- to switch between)
			//Weapon sets also contain 4 skills (so switching weapons changes skills)
			//Active mech color (mech is purchased, and color is applied to it afterwards, so dont need a separate class for it -- paint color is on a rent timer)
		}
		[TestMethod]
		public void Player_Get_Experience()
		{
		}
		[TestMethod]
		public void Player_Get_Rank()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_MechName()
		{
			//players got to create mechs, and name them
		}
		[TestMethod]
		//ToDo: Get MaxDurability, CurrentDurability, if any part has expired
		public void Mechanaughts_Get_CurrentDurability()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_MaxDurability()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_AnyPartDurabilityExpired()
		{
			//bool
		}
		[TestMethod]
		public void Mechanaughts_Get_CurrentWeight()
		{
			//Weight from map's gravity + combination of mech parts?
		}
		[TestMethod]
		public void Mechanaughts_Get_Size()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_CurrentHP()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_MaxHP()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_CurrentEN()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_MaxEN()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_CurrentSP()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_MaxSP()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_MPU()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_CurrentMoveSpeed()
		{
			//Weight from map's gravity + combination of mech parts?
		}
		[TestMethod]
		public void Mechanaughts_Get_EN_Recovery()
		{
			//Player Pilot Profile and Mastery Rank changes value here
		}
		[TestMethod]
		public void Mechanaughts_Get_MinEN_Required()
		{
		}
		#endregion

		#region Player Pilot
		#endregion

		#region Weapon Database Values
		//Player Weapons should consist of a Weapons[,] or Weapons[][], 2 sets x 2 hands

		[TestMethod]
		public void Player_Mechanaught_Get_WeaponSet()
		{
			//Set One, Set Two. 
			//Switching weapons cycles between sets
		}
		[TestMethod]
		public void Player_Mechanaught_Get_WeaponSlot()
		{
			//Left Hand, Right Hand, Both Hands
		}
		[TestMethod]
		public void Player_Mechanaught_Get_WeaponSlot1_AmmoClipCount()
		{
		}
		//Each weapon type is given experience points based on KDR when using it
		#endregion

		#region Player Inventory
		//mech suits, and misc items purchased
		//each mech part could only be used once 
		//(so multiple variations of a mech required the same part multiple times)
		//Player Durability Points was on a Currency System. (it accumulated, and was spent to replenish equipments)
		//When items durability expired, they didnt go away. Only items on a rent-timer disappeared
		#endregion
	}
}
