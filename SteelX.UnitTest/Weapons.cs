using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SteelX.UnitTest
{
	[TestClass]
	public class Weapons
	{
		#region Unit Database Values 
		//A Weapon Unit has additional default values from base `Weapon` Class
		//So UnitTesting any Weapon each one should be able to answer any of these additional questions

		[TestMethod]
		public void Weapon_Get_WeaponType()
		{
		}
		[TestMethod]
		public void Weapon_Get_NumberOfHands()
		{
		}
		[TestMethod]
		public void Weapon_Get_HasAimingReticle()
		{
			//I think this is the same thing as "Is Auto Aim"
		}
		[TestMethod]
		public void Weapon_Get_CanLockOn()
		{
		}
		#endregion

		#region Weapon Database Values 
		//Each Weapon should inherit `Weapon` Class
		//So UnitTesting any Individual Piece will acknowledge base class

		[TestMethod]
		public void Weapon_Get_Name()
		{
		}
		//[TestMethod]
		//public void Weapon_Get_WeightSeries()
		//{
		//}
		[TestMethod]
		public void Weapon_Get_Price_Credits()
		{
		}
		[TestMethod]
		public void Weapon_Get_Price_Currency2()
		{
		}
		[TestMethod]
		public void Weapon_Get_RankRequired()
		{
		}
		[TestMethod]
		//ToDo: Get MaxDurability, CurrentDurability, if any part has expired
		public void Weapon_Get_Durability()
		{
		}
		[TestMethod]
		public void Weapon_Get_Weight()
		{
		}
		[TestMethod]
		public void Weapon_Get_Size()
		{
		}
		[TestMethod]
		public void Weapon_Get_Range()
		{
		}
		[TestMethod]
		public void Weapon_Get_Damage()
		{
		}
		[TestMethod]
		public void Weapon_Get_Accuracy()
		{
		}
		[TestMethod]
		public void Weapon_Get_Reload()
		{
		}
		[TestMethod]
		public void Weapon_Get_OverHeat()
		{
		}
		[TestMethod]
		public void Weapon_Get_Description()
		{
		}
		#endregion

		#region Arms Database Values
		//Each Part should already inherit `Part` Class
		//So UnitTesting Specific stats for this Part Type

		[TestMethod]
		public void Weapon_Arms_Get_MaxHeat()
		{
		}
		[TestMethod]
		public void Weapon_Arms_Get_CooldownRate()
		{
		}
		[TestMethod]
		public void Weapon_Arms_Get_Marksmanship()
		{
		}
		#endregion

		#region Leg Database Values
		//Each Part should already inherit `Part` Class
		//So UnitTesting Specific stats for this Part Type

		[TestMethod]
		public void Weapon_Legs_Get_BasicSpeed()
		{
		}
		[TestMethod]
		public void Weapon_Legs_Get_Capacity()
		{
		}
		[TestMethod]
		public void Weapon_Legs_Get_Deceleration()
		{
		}
		#endregion

		#region Core Database Values
		//Each Part should already inherit `Part` Class
		//So UnitTesting Specific stats for this Part Type

		/// <summary>
		/// EN sets the minimum EN of the Mechanaught
		/// </summary>
		[TestMethod]
		public void Weapon_Core_Get_En()
		{
		}
		[TestMethod]
		public void Weapon_Core_Get_MinEN_Required()
		{
		}
		/// <summary>
		/// EN Output Rate sets the rate of EN regeneration of a mechanaught
		/// </summary>
		[TestMethod]
		public void Weapon_Core_Get_EN_OutputRate()
		{
		}
		#endregion

		#region Head Database Values
		//Each Part should already inherit `Part` Class
		//So UnitTesting Specific stats for this Part Type

		/// <summary>
		/// SP sets the minimum Skill Points of the Mechanaught
		/// </summary>
		[TestMethod]
		public void Weapon_Head_Get_SP()
		{
		}
		/// <summary>
		///MPU sets how many <see cref="Skills"/> can be equipped on the mechanaught
		/// </summary>
		[TestMethod]
		public void Weapon_Head_Get_MPU()
		{
		}
		/// <summary>
		/// Scan Range sets the minimum scan range of the mechanaught.
		/// </summary>
		[TestMethod]
		public void Weapon_Head_Get_ScanRange()
		{
		}
		#endregion

		#region Booster Database Values
		//Each Part should already inherit `Part` Class
		//So UnitTesting Specific stats for this Part Type
		//some Boosters are Required to use certain `Skill`

		[TestMethod]
		public void Weapon_Booster_Get_DashOutput()
		{
		}
		[TestMethod]
		public void Weapon_Booster_Get_Dash_EN_Drain()
		{
		}
		[TestMethod]
		public void Weapon_Booster_Get_Jump_EN_Drain()
		{
		}
		#endregion
	}
}