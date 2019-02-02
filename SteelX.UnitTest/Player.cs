using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Exteel;

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
			//Weapon sets also contain 4 skills (so switching weapons enables/disables skills)
			//Active mech color (mech is purchased, and color is applied to it afterwards, so dont need a separate class for it -- paint color is on a rent timer)
		}
		[TestMethod]
		public void Player_Get_Experience()
		{
		}
		[TestMethod]
		public void Player_Get_Skills_CanUse()
		{
			//returns a yes or no on if skill can be activated 
			//depends on active weapons, and amount of skill energy collected
			//also some skills have a range-lock, so it can only be used if within a certain distance, and if locked-on
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

		#region Operateor Stats
		public void CompareTwoMechForOperatorStatsDifference()
		{
			//Whatever player is viewing on screen, create it, and calculate data
			Exteel.Core.Mechanaughts shopMech = new Exteel.Core.Mechanaughts(new Exteel.Mech());
			Exteel.Core.Mechanaughts active = new Exteel.Core.Mechanaughts(new Exteel.Mech());//new Player.Player().ActiveMech;
			string[] stat_differences = new string[shopMech.OperatorStats.Length];

			//int[] displayMech = TransformMechPropertiesToArray();

			//foreach (var item in TransformMechPropertiesToArray())
			for (int i = 0; i < shopMech.OperatorStats.Length; i++)
			{
				//Return the data for the model player is viewing
				//stat_differences.text = currentModel;

				//if there is a difference in stats from model player is viewing, and the one player has equiped
				int diff = shopMech.OperatorStats[i] - active.OperatorStats[i];
				if (System.Math.Abs(diff) > 0) //If difference is positive or negative
				{
					//stat_differences[j].text = (newMechPropertiesArray[j] - curMechPropertiesArray[j] > 0 ? "▲" : "▼") + (Mathf.Abs(newMechPropertiesArray[j] - curMechPropertiesArray[j])).ToString();
					//stat_differences[j].color = newMechPropertiesArray[j] - curMechPropertiesArray[j] > 0 ? RED : BLUE;
					//stat_differences.text = diff;
					//stat_differences.color = diff > 0 ? RED : BLUE;
					stat_differences[i] = string.Format("{0} {1}", diff > 0 ? "▲" : "▼", Math.Abs(diff).ToString());
					//stat_differences[i].color = diff > 0 ? RED : BLUE;
				}
			}
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
