This is my email : chengchonz@yahoo.com.tw  </br> 	
If you have any bug , ideas , feel free to report.</br>  	
Joining us (Andy & me) is also welcome. </br> 	

  	
To implement List : </br>  	  
 </br>
main :</br>  	
 
	RCL034 : bulletTrace</br>
	RCL034 : hitsound </br>
RCL034 : crosshair</br>
RCL034 : bulletTrace start pos
	RCL034 : make RCL034B detects multiple objects(maybe later)</br>
</br>
Adjust APS403B&BI</br>
Fix disable player & re-enable player </br>
Remake SHL animation & hit detect</br>

Add the heat bar</br>
Game modes : deathmatch , CTF(PhotonNetwork.player.SetCustomProperty("TeamRed"))</br>  
Generalize a standard way to change weapon ( different weapon has its own damage , animation , ... ) </br>  	

Make sure Shield detect well</br>
</br>
Next weapons : rifle -> shotgun   
Tidy up codes & files </br>	
Make one map</br>
Save User data & account</br>    		
Remake all animations (skills not included) to high quality</br>  	
Make skills</br>  	
  </br>	
others : </br>  	  	
  	
Optimize jump boost(vertical boost not good )</br> 	
Put all weapons & robot parts on </br> 	
Add "locked" message on UI  	</br>	
Set mutiple "HIT" on UI  </br>	
Put the health & energy numbers on UI  </br>	
Sound effects  </br>
Switch weapon animation  	</br>
Add crosshair shaking  	</br>
Add the Tab function  </br>
LobbyChat (can't use photon InRoomChat, maybe use www ?)</br>
Build maps  </br>
Improve efficiency
 

Unsolved bug :
Penetrating the ground ( rare , changing the ground to box collidor didn't fix )  (it even happened when vertical boosting)