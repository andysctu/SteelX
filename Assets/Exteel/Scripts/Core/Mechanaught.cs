namespace Exteel.Core
{
	public class Mechanaughts
	{
		#region Variables
		public Arms Arm			{ get; private set; }
		public Legs Leg			{ get; private set; }
		public Cores Core		{ get; private set; }
		public Heads Head		{ get; private set; }
		public Boosters Booster { get; private set; }
		public int Durability	{ get; private set; }
		public int Weight		{ get; private set; }
		public int Size			{ get; private set; }
		//public int CurrentHP	{ get; private set; }
		//public int MaxHP		{ get; private set; }
		public int HP			{ get; private set; }
		public int EN			{ get; private set; }
		public int SP			{ get; private set; }
		public int MPU			{ get; private set; }
		public int MoveSpeed	{ get; private set; }
		public int EN_Recovery	{ get; private set; }
		public int MinEN_Required { get; private set; }
		#endregion

		#region Nested Classes
		public class Arms : Part
		{
			/// <summary>
			/// 
			/// </summary>
			public int MaxHeat { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public int CooldownRate { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public int Marksmanship { get; set; }

			public Arms()
			{
				//this.
			}
		}
		public class Legs : Part
		{
			/// <summary>
			/// 
			/// </summary>
			public int BasicSpeed { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public int Capacity { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public int Deceleration { get; set; }

			public Legs()
			{

			}
		}
		public class Cores : Part
		{
			/// <summary>
			/// EN sets the minimum EN of the Mechanaught
			/// </summary>
			public int EN { get; set; }
			/// <summary>
			/// Minimum EN Required
			/// </summary>
			public int MinEN { get; set; }
			/// <summary>
			/// EN Output Rate sets the rate of EN regeneration of a mechanaught
			/// </summary>
			public int OutputRate { get; set; }

			public Cores()
			{

			}
		}
		public class Heads : Part
		{
			/// <summary>
			/// SP sets the minimum Skill Points of the Mechanaught
			/// </summary>
			public int SP { get; set; }
			/// <summary>
			///MPU sets how many <see cref="Skills"/> can be equipped on the mechanaught
			/// </summary>
			public int MPU { get; set; }
			/// <summary>
			/// Scan Range sets the minimum scan range of the mechanaught.
			/// </summary>
			public int ScanRange { get; set; }

			public Heads()
			{

			}
		}
		public class Boosters : Part
		{
			/// <summary>
			/// 
			/// </summary>
			public int DashOutput { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public int DashDrainEN { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public int JumpDrainEN { get; set; }

			public Boosters()
			{

			}
		}
		public abstract class Part
		{
			/// <summary>
			/// Name of the set, this part belongs to
			/// </summary>
			public string SetName { get; set; }
			/// <summary>
			/// Internal name of this individual part
			/// </summary>
			public string PartName { get; set; }
			/// <summary>
			/// Name of this this part when viewed in-game from shop or inventory
			/// </summary>
			public string DisplayName { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public byte WeightSeries { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public int Weight { get; set; }
			/// <summary>
			/// An Array of this Unit's Price
			/// If value is null, then option does not exist
			/// Index[1] : Credits
			/// Index[2] : Coins
			/// </summary>
			public int?[] Price { get; set; }
			/// <summary>
			/// Rank required to purchase or equip part
			/// 
			/// </summary>
			public byte RankRequired { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public int Durability { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public int Size { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public int HP { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public int EN { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public int SP { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public int EnergyDrain { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public int MoveSpeed { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public int RecoveryEN { get; set; }
			/// <summary>
			/// 
			/// </summary>
			public int Description { get; set; }
		}
		#endregion
	}
}