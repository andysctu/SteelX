using Exteel.Core.Player;
using System.Linq;
using System.Collections.Generic;

namespace Exteel
{
	public class Server
	{
		#region Variables
		/// <summary>
		/// All online players currently logged on active server
		/// </summary>
		public static List<Player> PlayerList { get; set; }
		private int PlayerId { get; set; }
		public Player ActivePlayer { get { return PlayerList.FirstOrDefault(x => x.GetHashCode() == PlayerId); } }
		#endregion

		#region Constructor
		//ToDo: Either use as constructor or method that returns Server
		public Server(int playerId)
		{

		}
		public Server(string username, string pass)
		{

		}
		private Server()
		{
			PlayerList = new List<Player>();
		}
		#endregion
	}
}