//all terminalblocks
List<IMyTerminalBlock> allTBlocks = new List<IMyTerminalBlock>();
//all armor blocks - be defined

string statusChannelTag = "RDOStatusChannel";
string commandChannelTag = "RDOCommandChannel";

bool setupcomplete = false;
bool checkDestroyedBlocks = true;
bool lockedState = false;
double damagedBlockRatio = 0;
double destroyedAmount = 0;

List<IMyRadioAntenna> antenna;
List<MyDetectedEntityInfo> targets = new List<MyDetectedEntityInfo>();
Dictionary<string, int> ammoDefaultAmount = new Dictionary<string, int>(); //subtype_id, ammount
Dictionary<string, int> itemsDefaultAmount = new Dictionary<string, int>(); //subtype_id, ammount - not ammo

Int32 energyTreshold = 25; //% of max capacity, default - 25%
Int32 gasTreshold = 25; //% of max capacity, default - 25%
Int32 uraniumTreshold = 0; //kg
Int32 damageTreshold = 20; //% of terminal blocks, default - 20%

IMyBroadcastListener commandListener;
IMyBroadcastListener statusListener;

public Program(){
	// Set the script to run every 100 ticks, so no timer needed.
	Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

public void fixGridState(){
	GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(allTBlocks, x => x.CubeGrid == Me.CubeGrid);
	//todo save armor block state
	
}

public string getMyCoords(){
	string result = "";
	result = Me.GetPosition().ToString();
	return result;
}

public bool isAttacked()
{
	//is turrets locked and target is grid
	List<IMyLargeTurretBase> turrets = new List<IMyLargeTurretBase>();
	GridTerminalSystem.GetBlocksOfType<IMyLargeTurretBase>(turrets);
	bool isTarget = false;
	foreach(IMyLargeTurretBase t in turrets)
	{
		if (t.HasTarget){
			MyDetectedEntityInfo trg = t.GetTargetedEntity();
			if (trg.Type != MyDetectedEntityType.None || trg.Type != MyDetectedEntityType.FloatingObject || trg.Type != MyDetectedEntityType.Meteor || trg.Type != MyDetectedEntityType.Planet){
				targets.Add(trg);
				isTarget = true;
			}
		}
	}
	bool isLocked = false;
	if (lockedState){        //lockedState initiate by cockpit, run PB with argument "locked"
		isLocked = true;
	}
	
	bool isLargeDamage = false;
	//isLargeDamage = (damagedBlockRatio > (damageTreshold/100));
	
	bool isDestroyedBlocks = false;
	if (checkDestroyedBlocks){
		List<IMyTerminalBlock> currentState = new List<IMyTerminalBlock>();
		GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(currentState, x => x.CubeGrid == Me.CubeGrid);
		isDestroyedBlocks = currentState.Count() != allTBlocks.Count();
		destroyedAmount = allTBlocks.Count() - currentState.Count();
	}
	
	return (isTarget || isLocked || isLargeDamage || isDestroyedBlocks);
}

public string getDamagedBlocks(){
	List<string> result = new List<string>();
	List<IMySlimBlock> slim = new List<IMySlimBlock>();
	List<IMyTerminalBlock> grid = new List<IMyTerminalBlock>();
	
	double damagedCounter = 0;
	
	GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(grid, x => x.CubeGrid == Me.CubeGrid);

	foreach(IMyTerminalBlock terminalBlock in grid){
		IMySlimBlock slimBlock = terminalBlock.CubeGrid.GetCubeBlock(terminalBlock.Position);
		slim.Add(slimBlock);
		if(slimBlock.CurrentDamage > 0){
			damagedCounter = damagedCounter + 1;
			result.Add("\"" + terminalBlock.DisplayNameText + "\"");
		}
	}
	damagedBlockRatio = damagedCounter/allTBlocks.Count();
	return String.Join(",", result);
}

public bool isDamaged(){
	return getDamagedBlocks() != "";
}

public bool isLowAmmo(){
	bool result = false;
	//todo
	return result;
}

public double getGridBatteryCharge(){ 
    List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>(); 
    GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries, x => x.CubeGrid == Me.CubeGrid); 
    float current_capacity = 0; 
    float max_capacity = 0; 
    foreach(IMyBatteryBlock b in batteries){ 
       		current_capacity = current_capacity + b.CurrentStoredPower; 
       		max_capacity = max_capacity + b.MaxStoredPower; 
    } 
    return Math.Round((current_capacity / max_capacity) * 100); 
} 

public double getCurrentGasAmount(string gasType){ 
	//gasType {"Oxygen","Hydrogen"}                                            
    List<IMyGasTank> tanks = new List<IMyGasTank>(); 
    GridTerminalSystem.GetBlocksOfType<IMyGasTank>(tanks, x => x.CubeGrid == Me.CubeGrid); 
    double current_amount = 0; 
    double max_capacity = 0; 
    foreach(IMyGasTank t in tanks){ 
       		if(t.DefinitionDisplayNameText.Contains(gasType)){ 
           			current_amount = t.Capacity * t.FilledRatio; 
           			max_capacity = max_capacity + t.Capacity; 
    	    } 
    } 
    return Math.Round((current_amount / max_capacity) * 100); 
}

public double getUsedCargoSpace() //% of max
{
	double maxVolume = 0;
	double currentVolume = 0;
	List<IMyCargoContainer> cargos = new List<IMyCargoContainer>();
	GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(cargos, x => x.CubeGrid == Me.CubeGrid);
	foreach(IMyCargoContainer container in cargos){
		maxVolume = maxVolume + (double)container.GetInventory(0).MaxVolume;
		currentVolume  = currentVolume + (double)container.GetInventory(0).CurrentVolume;
	}
	
	return Math.Round((currentVolume/maxVolume)*100);
}

public double getGridVelocity()
{
	double result = 0;
	List<IMyShipController> controls = new List<IMyShipController>();
	GridTerminalSystem.GetBlocksOfType<IMyShipController>(controls, x => x.CubeGrid == Me.CubeGrid);
	result = (double)controls[0].GetShipVelocities().LinearVelocity.Length();
	return result;
}

public bool isLowFuel(out string ftype){
	bool result = false;
	//check battery charges
	ftype = "";
	List<string> ftypes = new List<string>();
	if (getGridBatteryCharge() < energyTreshold){
		result = true;
		ftypes.Add("\"energy\"");
	}
	//check gas
	if (getCurrentGasAmount("Hydrogen") < gasTreshold){
		result = true;
		ftypes.Add("\"gas\"");
	}	
	//todo check uranium (if reactors)
	
	if (ftypes.Count > 0){
		ftype = "[" + String.Join(",", ftypes) + "]";
	}
	return result;
}

public string getEnemyTargetsData(){
	string result = "";
	List<string> t = new List<string>();
	foreach(MyDetectedEntityInfo target in targets){
		result = result + "{\"Name\":\"" + target.Name + "\",";
		result = result + "\"Type\":\"" + target.Type.ToString() + "\",";
		result = result + "\"Position\":\"" + target.Position.ToString() + "\"}";
		t.Add(result);
		result = "";
	}
	if (t.Count > 0){
		result = "["+ String.Join(",", t) + "]";
	}
	return result;
}

public void Setup(){
	List<IMyTerminalBlock> list = new List<IMyTerminalBlock>();
	GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(list, x => x.CubeGrid == Me.CubeGrid);
	antenna = list.ConvertAll(x => (IMyRadioAntenna)x);
	
	if(antenna.Count() > 0){
		Echo("Setup complete");
		setupcomplete = true;
	}
	else{
		Echo("Setup failed. No antenna found");
	}
}

public void Main(string arg){
	if (arg != ""){
		//change parameters
		if (arg.Contains("=")){
			List<string> args = arg.Split('=').Select(a => a.Trim()).ToList();
			string cmd = args[0];
			string val = args[1];

			switch(cmd){
				case "ammo":
					//todo: set ammo request
					break;
				case "items":
					//todo: set items request
					break;
				case "energy":
					energyTreshold = Int32.Parse(val);
					break;
				case "gas":
					gasTreshold= Int32.Parse(val);
					break;				
				case "damage":
					damageTreshold = Int32.Parse(val);
					break;
				case "uranium":
					uraniumTreshold = Int32.Parse(val);
					break;
				default:
					Echo("Unknown argument");
					break;
			}
		}
		
		//set flags
		if (arg == "locked"){
			lockedState = true;
			Echo("Grid is locked");
		}

		if (arg == "unlocked"){
			lockedState = false;
			Echo("Manual override lock status");
		}		
		
		if (arg == "fix"){
			fixGridState();
			Echo("Grid state saved");
		}		

	}
	if(!setupcomplete){
		//If setupcomplete is false, run Setup method.
		Echo("Running setup");
		Setup();
		fixGridState();
	}
	else{
		targets.Clear();
		string statusMessage = "";
		string ftype;
		List<string> events = new List<string>();
		List<IMyTextPanel> status_displays = new List<IMyTextPanel>();
		List<IMyTextPanel> request_displays = new List<IMyTextPanel>();
		
		statusMessage = "\"green\"";
		if (isDamaged() || isLowAmmo() || isLowFuel(out ftype)){
			statusMessage = "\"orange\"";
			if (isLowAmmo()){
				//todo check what type of ammo need and how many, add to details
				string tmp = "{\"event\":\"lowAmmo\"}";
				events.Add(tmp);
			}
			if (isDamaged()){
				string tmp = "{\"event\":\"damaged\"";
				tmp = tmp +  ",\"blocks\":[" + getDamagedBlocks() + "]";
				tmp = tmp + "}";
				events.Add(tmp);
			}
			if (isLowFuel(out ftype)){
				string tmp = "{\"event\":\"lowFuel\"";
				tmp = tmp + ",\"fueltype\":" + ftype;
				tmp = tmp + "}";
				events.Add(tmp);
			}
		}		
		if (isAttacked()){
			statusMessage = "\"red\"";
			string tmp = "{\"event\":\"underAttack\"";
			string t = "";
			if (lockedState){
				tmp = tmp + ",\"Locked\": \"true\"";
			}
			if (damagedBlockRatio > 0){
				tmp = tmp + ",\"Damaged\": " + (damagedBlockRatio*100).ToString();
			}
			
			if (destroyedAmount > 0){
				tmp = tmp + ",\"Destroyed\": " + destroyedAmount.ToString();
			}
				
			t = getEnemyTargetsData();
			if (t != ""){
				tmp = tmp + ",\"Targets\":" + t;
				tmp = tmp + "}";
			}
			events.Add(tmp);
		}
	
		statusMessage = "{\"Name\":\"" + Me.CubeGrid.CustomName + "\",\"Status\":" + statusMessage;
		
		statusMessage = statusMessage + ",\"Info\":[";
		statusMessage = statusMessage + "{\"GasAmount\":" + getCurrentGasAmount("Hydrogen") + "},";
		statusMessage = statusMessage + "{\"BatteryCharge\":" + getGridBatteryCharge() + "},";
		statusMessage = statusMessage + "{\"CargoUsed\":" + getUsedCargoSpace() + "},";
		statusMessage = statusMessage + "{\"Position\":\"" + getMyCoords() + "\"},";
		statusMessage = statusMessage + "{\"Velocity\":" + getGridVelocity() + "}";
		statusMessage = statusMessage + "]";
		
		if (events.Count > 0){
			statusMessage = statusMessage + ",\"Events\":[";
			foreach(string e in events){
				statusMessage = statusMessage + e + ",";
			}
			statusMessage = statusMessage.Remove(statusMessage.Length-1) + "]";
		}
		//finish message
		statusMessage = statusMessage + "}";
		Echo(statusMessage);
		
		GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(status_displays, x => x.CubeGrid == Me.CubeGrid && x.CustomName.Contains("STATUS"));
		GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(request_displays, x => x.CubeGrid == Me.CubeGrid && x.CustomName.Contains("REQ"));
		
		//Me.CustomData = statusMessage;
		//string cmdTag = commandChannelTag + "." + Me.CubeGrid.CustomName;
		//statusListener = IGC.RegisterBroadcastListener(statusChannelTag);
		
		IGC.SendBroadcastMessage(statusChannelTag, statusMessage);
		commandListener = IGC.RegisterBroadcastListener(commandChannelTag);
		
		while (commandListener.HasPendingMessage)
		{
			string message;
			MyIGCMessage newRequest = commandListener.AcceptMessage();
			if(commandListener.Tag == commandChannelTag)
			{
				if(newRequest.Data is string)
				{
					message = newRequest.Data.ToString()
					foreach(IMyTextSurface d in request_displays){
						d.WriteText(message);
					}
				}
			}
		}
		
	}
}