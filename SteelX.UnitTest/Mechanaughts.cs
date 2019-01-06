using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SteelX.UnitTest
{
	[TestClass]
	public class Mechanaughts
	{
		#region Unit Database Values 
		//A Single Mechanaught Unit should be a collection of its `Part` Class
		//So UnitTesting any Individual Mechnaught set or player build will acknowledge group of pieces

		[TestMethod]
		public void Mechanaughts_Get_Durability()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_Weight()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_Size()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_HP()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_EN()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_SP()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_MPU()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_MoveSpeed()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_EN_Recovery()
		{
		}
		[TestMethod]
		public void Mechanaughts_Get_MinEN_Required()
		{
		}
		#endregion

		#region Part Database Values 
		//Each Part should inherit `Part` Class
		//So UnitTesting any Individual Piece will acknowledge base class

		[TestMethod]
		public void Mechanaughts_Part_Get_SetName()
		{
		}
		[TestMethod]
		public void Mechanaughts_Part_Get_PartName()
		{
		}
		[TestMethod]
		public void Mechanaughts_Part_Get_WeightSeries()
		{
		}
		[TestMethod]
		public void Mechanaughts_Part_Get_Price_Credits()
		{
		}
		[TestMethod]
		public void Mechanaughts_Part_Get_Price_Currency2()
		{
		}
		[TestMethod]
		public void Mechanaughts_Part_Get_RankReqired()
		{
		}
		[TestMethod]
		public void Mechanaughts_Part_Get_Durability()
		{
		}
		[TestMethod]
		public void Mechanaughts_Part_Get_Weight()
		{
		}
		[TestMethod]
		public void Mechanaughts_Part_Get_Size()
		{
		}
		[TestMethod]
		public void Mechanaughts_Part_Get_HP()
		{
		}
		[TestMethod]
		public void Mechanaughts_Part_Get_EN()
		{
		}
		[TestMethod]
		public void Mechanaughts_Part_Get_SP()
		{
		}
		[TestMethod]
		public void Mechanaughts_Part_Get_MPU()
		{
		}
		[TestMethod]
		public void Mechanaughts_Part_Get_EnergyDrain()
		{
		}
		[TestMethod]
		public void Mechanaughts_Part_Get_MoveSpeed()
		{
		}
		[TestMethod]
		public void Mechanaughts_Part_Get_EN_Recovery()
		{
		}
		[TestMethod]
		public void Mechanaughts_Part_Get_Description()
		{
		}
		#endregion

		#region Arms Database Values
		//Each Part should already inherit `Part` Class
		//So UnitTesting Specific stats for this Part Type

		[TestMethod]
		public void Mechanaughts_Arms_Get_MaxHeat()
		{
		}
		[TestMethod]
		public void Mechanaughts_Arms_Get_CooldownRate()
		{
		}
		[TestMethod]
		public void Mechanaughts_Arms_Get_Marksmanship()
		{
		}
		#endregion

		#region Leg Database Values
		//Each Part should already inherit `Part` Class
		//So UnitTesting Specific stats for this Part Type

		[TestMethod]
		public void Mechanaughts_Legs_Get_BasicSpeed()
		{
		}
		[TestMethod]
		public void Mechanaughts_Legs_Get_Capacity()
		{
		}
		[TestMethod]
		public void Mechanaughts_Legs_Get_Deceleration()
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
		public void Mechanaughts_Core_Get_En()
		{
		}
		[TestMethod]
		public void Mechanaughts_Core_Get_MinEN_Required()
		{
		}
		/// <summary>
		/// EN Output Rate sets the rate of EN regeneration of a mechanaught
		/// </summary>
		[TestMethod]
		public void Mechanaughts_Core_Get_EN_OutputRate()
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
		public void Mechanaughts_Head_Get_SP()
		{
		}
		/// <summary>
		///MPU sets how many <see cref="Skills"/> can be equipped on the mechanaught
		/// </summary>
		[TestMethod]
		public void Mechanaughts_Head_Get_MPU()
		{
		}
		/// <summary>
		/// Scan Range sets the minimum scan range of the mechanaught.
		/// </summary>
		[TestMethod]
		public void Mechanaughts_Head_Get_ScanRange()
		{
		}
		#endregion

		#region Booster Database Values
		//Each Part should already inherit `Part` Class
		//So UnitTesting Specific stats for this Part Type
		//some Boosters are Required to use certain `Skill`

		[TestMethod]
		public void Mechanaughts_Booster_Get_DashOutput()
		{
		}
		[TestMethod]
		public void Mechanaughts_Booster_Get_Dash_EN_Drain()
		{
		}
		[TestMethod]
		public void Mechanaughts_Booster_Get_Jump_EN_Drain()
		{
		}
		#endregion
	}
}
