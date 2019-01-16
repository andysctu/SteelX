namespace Exteel.Core
{
	/// <summary>
	/// 
	/// </summary>
	/// ToDo: In-Game weapons should use [2][2] (to track ammo, and other misc data)
	/// Use bool to check for Weapon[n][IsEquipped && IsTwoHanded]
	public class Weapons
	{
		#region Variables
		public Weapon WeaponId { get; private set; }
		public WeaponType Class { get { return GetWeapType(WeaponId); } }


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
		public string ImagePath { get { return string.Format("{0}", PartName); } }
		/// <summary>
		/// The name of the sprite asset in UnityEngine
		/// </summary>
		/// Either this or ImagePath would be the best way to handle loading Icon
		public string ImageSpriteAsset { get; private set; }

		public int Durability	{ get; private set; }
		public int Weight		{ get; private set; }
		public int Size			{ get; private set; }
		public float Range		{ get; private set; }
		public float Damage		{ get; private set; }
		public float Accuracy	{ get; private set; }
		public float Reload		{ get; private set; }
		public float Overheat	{ get; private set; }
		/// <summary>
		/// </summary>
		/// ToDo: When in hands of player, price is buy multiplied by (durability divided by max durability)
		/// An Array of this Unit's Price
		/// If value is null, then option does not exist
		/// Index[1] : Credits
		/// Index[2] : Coins
		public virtual int? SellPrice	{ get { return 0; } }
		public int BuyPrice	{ get; private set; }
		/// <summary>
		/// Rank required to purchase or equip part
		/// 
		/// </summary>
		public byte RankRequired { get; set; }
		/// <summary>
		/// 
		/// </summary>
		public int Description { get; set; }
		#endregion

		#region Methods
		public static WeaponType GetWeapType(Weapon weap)
		{
			//if (WeaponId == Weapon.NONE)
			//	return WeaponType.NONE;
			//else 
			switch (weap)
			{
				case Weapon.NONE:
				default:
					return WeaponType.NONE;
			}
		}
		#endregion

		#region Database
		private static readonly Weapons[] Database;
		static Weapons()
		{
			Database = new Weapons[] {
			};
		}
		#endregion
	}
}