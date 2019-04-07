using UnityEngine;
using UnityEngine.UI;

namespace Exteel.Game
{
	public class OperatorStatsUI : MonoBehaviour
	{
		#region Variables
		#region New UI Fix
		private GameObject UI_HP;
		private GameObject UI_EN;
		private GameObject UI_SP;
		private GameObject UI_MPU;
		private GameObject UI_Size;
		private GameObject UI_Weight;
		private GameObject UI_MoveSpeed;
		private GameObject UI_DashSpeed;
		private GameObject UI_ENRecovery;
		private GameObject UI_MinENRequired;
		private GameObject UI_DashENDrain;
		private GameObject UI_JumpENDrain;
		private GameObject UI_DashAccel;
		private GameObject UI_DashDecel;
		private GameObject UI_MaxHeat;
		private GameObject UI_CooldownRate;
		private GameObject UI_ScanRange;
		private GameObject UI_Marksmanship;


		private Color32 BLUE = new Color32(39, 67, 253, 255), RED = new Color32(248, 84, 84, 255);
		private const int STAT_LABELS = 18; 
		#endregion
		[SerializeField] private Transform MechInfoStats;
		[SerializeField] private BuildMech Mech;
		[SerializeField] private MechPartManager MechPartManager;
		[SerializeField] private WeaponDataManager WeaponManager;
		[SerializeField] private Text playerName;

		private MechProperty curMechProperty;
		private Part[] MechParts = new Part[5];
		private WeaponData[] MechWeapons = new WeaponData[4];
		private Text[] stat_texts = null, stat_labels = null, stat_differences = null;
		//private string[] STAT_LABELS = new string[18]; 
		//{
		//   "HP","EN","SP","MPU","Size","Weight","Move Speed","Dash Speed","EN Recovery","Min. EN Required",
		//	"Dash EN Drain","Jump EN Drain","Dash Accel","Dash Decel","Max Heat","Cooldown Rate","Scan Range","Marksmanship"
		//};
		#endregion

		#region Unity
		private void Awake() {
			stat_texts = new Text[MechInfoStats.childCount];
			stat_labels = new Text[MechInfoStats.childCount];
			stat_differences = new Text[MechInfoStats.childCount];

			for (int i = 0; i < MechInfoStats.childCount; i++) {
				stat_texts[i] = MechInfoStats.GetChild(i).Find("Stats").GetComponent<Text>();
				stat_labels[i] = MechInfoStats.GetChild(i).Find("Label").GetComponent<Text>();

				//Init labels
				//stat_labels[i].text = STAT_LABELS[i]; //you can name them from here, but they're already set from Unity...
				stat_differences[i] = MechInfoStats.GetChild(i).Find("Change/Difference").GetComponent<Text>();
				stat_differences[i].enabled = false;
			}
		}

		private void OnEnable() {
			//playerName.text = PhotonNetwork.player.NickName;
		}
		#endregion

		#region Methods
		public void RefreshDisplay()
		{
			//Whatever player is viewing on screen, create it, and calculate data
			Exteel.Core.Mechanaughts shopMech = new Exteel.Core.Mechanaughts(new Exteel.Mech());
			Exteel.Core.Mechanaughts active = new Player.Player().ActiveMech;

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
					stat_differences[i].text = string.Format("{0} {1}", diff > 0 ? "▲" : "▼", System.Math.Abs(diff).ToString());
					stat_differences[i].color = diff > 0 ? RED : BLUE;
				}
			}
		}

		//private int[] TransformMechPropertiesToArray() {//MechProperty mechProperty, int partWeight, int weaponWeight) {
		//	Exteel.Core.Mechanaughts mechProperty = new Player.Player().ActiveMech;
		//	int[] PropertiesArray = new int[STAT_LABELS];
		//	PropertiesArray[0] = mechProperty.HP;
		//	PropertiesArray[1] = mechProperty.EN;
		//	PropertiesArray[2] = mechProperty.SP;
		//	PropertiesArray[3] = mechProperty.MPU;
		//	PropertiesArray[4] = mechProperty.Size;
		//	PropertiesArray[5] = mechProperty.Weight;
		//	//PropertiesArray[6] = (int)mechProperty.GetMoveSpeed(partWeight, weaponWeight);
		//	//PropertiesArray[7] = (int)mechProperty.GetDashSpeed(partWeight + weaponWeight);
		//	PropertiesArray[8] = mechProperty.ENOutputRate;
		//	PropertiesArray[9] = mechProperty.MinEN_Required;//MinENRequired;
		//	PropertiesArray[10] = mechProperty.DashENDrain;
		//	PropertiesArray[11] = mechProperty.JumpENDrain;//GetJumpENDrain(partWeight + weaponWeight);
		//	//PropertiesArray[12] = mechProperty.GetDashAcceleration(partWeight + weaponWeight);
		//	//PropertiesArray[13] = mechProperty.GetDashDecelleration(partWeight + weaponWeight);
		//	PropertiesArray[14] = mechProperty.MaxHeat;
		//	PropertiesArray[15] = mechProperty.CooldownRate;
		//	PropertiesArray[16] = mechProperty.ScanRange;
		//	PropertiesArray[17] = mechProperty.Marksmanship;
		//
		//	return PropertiesArray;
		//}

		//public void DisplayMechProperties() {
		//	curMechProperty = Mech.MechProperty;
		//	MechParts = Mech.curMechParts;
		//
		//	int[] MechPropertiesArray = TransformMechPropertiesToArray(curMechProperty, CalculatePartsWeight(MechParts), CalculateWeaponWeight(Mech.WeaponDatas));
		//	for (int i = 0; i < MechPropertiesArray.Length; i++) {
		//		stat_texts[i].text = MechPropertiesArray[i].ToString();
		//	}
		//	ClearAllDiff();
		//}

		//public void PreviewMechProperty(string part, bool isWeapon) {
		//	Part[] tmpParts = (Part[])MechParts.Clone();
		//
		//	if (isWeapon) {
		//		//WeaponData newWeap = WeaponManager.FindData(part);
		//	} else {
		//		//Part newPart = MechPartManager.FindData(part);
		//
		//		System.Type[] partTypes = new System.Type[5];// { typeof(Head), typeof(Core), typeof(Arm), typeof(Leg), typeof(Booster) };
		//
		//		for (int i = 0; i < 5; i++) {
		//			if (MechPartManager.GetPartType(part) == partTypes[i]) {//which type is this part
		//				for (int j = 0; j < 5; j++) {
		//					if (MechPartManager.GetPartType(MechParts[j].name) == partTypes[i]) {//find the corresponding part in MechParts
		//						tmpParts[j] = MechPartManager.FindData(part);//switch
		//						break;
		//					}
		//				}
		//				break;
		//			}
		//		}
		//		MechProperty newMechProperty = new MechProperty();
		//		//Load all property info
		//		for (int i = 0; i < 5; i++) {
		//			if (tmpParts[i] != null) {
		//				tmpParts[i].LoadPartInfo(ref newMechProperty);
		//			}
		//		}
		//
		//		//show diff
		//		int[] curMechPropertiesArray = TransformMechPropertiesToArray(curMechProperty, CalculatePartsWeight(MechParts), CalculateWeaponWeight(Mech.WeaponDatas));
		//		int[] newMechPropertiesArray = TransformMechPropertiesToArray(newMechProperty, CalculatePartsWeight(tmpParts), CalculateWeaponWeight(Mech.WeaponDatas));
		//
		//		for (int j = 0; j < 18; j++) {
		//			if (j == 4 || j == 5 || j == 9 || j == 10 || j == 11) {
		//				stat_differences[j].text = (newMechPropertiesArray[j] - curMechPropertiesArray[j] > 0 ? "▲" : "▼") + (Mathf.Abs(newMechPropertiesArray[j] - curMechPropertiesArray[j])).ToString();
		//				stat_differences[j].color = newMechPropertiesArray[j] - curMechPropertiesArray[j] > 0 ? RED : BLUE;
		//			} else {
		//				stat_differences[j].text = (newMechPropertiesArray[j] - curMechPropertiesArray[j] > 0 ? "▲" : "▼") + (Mathf.Abs(newMechPropertiesArray[j] - curMechPropertiesArray[j])).ToString();
		//				stat_differences[j].color = newMechPropertiesArray[j] - curMechPropertiesArray[j] > 0 ? BLUE : RED;
		//			}
		//			stat_differences[j].enabled = (newMechPropertiesArray[j] - curMechPropertiesArray[j] != 0);
		//		}
		//	}
		//}

		//private int CalculatePartsWeight(Part[] parts) {
		//	int partsWeight = 0;
		//	for (int i = 0; i < parts.Length; i++) {
		//		partsWeight += parts[i].Weight;
		//	}
		//	return partsWeight;
		//}

		//private int CalculateWeaponWeight(WeaponData[] weapons) {
		//	int weight_1 = (weapons[Mech.GetWeaponOffset()] == null) ? 0 : weapons[Mech.GetWeaponOffset()].weight,
		//		weight_2 = (weapons[Mech.GetWeaponOffset() + 1] == null) ? 0 : weapons[Mech.GetWeaponOffset() + 1].weight;
		//	return weight_1 + weight_2;
		//}

		//private void ClearAllDiff() {
		//	for (int i = 0; i < 18; i++) {
		//		stat_differences[i].enabled = false;
		//	}
		//}
		#endregion
	}
}