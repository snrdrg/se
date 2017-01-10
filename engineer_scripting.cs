Vector3D last_position = new Vector3D(0,0,0); 
int last_speed = 0; 
 
double getGridBatteryCharge() { 
//return percent of total batteries charge of the current grid 
    	List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>(); 
    	GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries); 
    	float current_capacity = 0; 
    	float max_capacity = 0; 
    	foreach(IMyBatteryBlock b in batteries){ 
        		current_capacity = current_capacity + b.CurrentStoredPower; 
        		max_capacity = max_capacity + b.MaxStoredPower; 
    	} 
    	return Math.Round((current_capacity / max_capacity) * 100); 
} 
 
double getCurrentGasAmount(string gasType) { 
//return pecent of total hydrogen amount in current grid. Not defined if no hydrogen tanks in the grid 
//gasType {"Oxygen","Hydrogen"}
    	List<IMyOxygenTank> tanks = new List<IMyOxygenTank>(); 
    	GridTerminalSystem.GetBlocksOfType<IMyOxygenTank>(tanks); 
    	double current_capacity = 0; 
    	double max_capacity = 0; 
    	foreach(IMyOxygenTank t  in tanks){ 
        		if(t.DefinitionDisplayNameText.StartsWith(gasType)){ 
            			current_capacity = current_capacity + t.GetOxygenLevel(); 
            			max_capacity = 			max_capacity + 1; 
    		    } 
    	} 
    	return Math.Round((current_capacity / max_capacity) * 100); 
} 

double getUraniumAmount(){
    double uranium_amount = 0;
    List<IMyReactor> reactors = new List<IMyReactor>();  
    GridTerminalSystem.GetBlocksOfType<IMyReactor>(reactors);  
    List<IMyInventoryItem> items = new List<IMyInventoryItem>();  
    foreach(IMyReactor reactor in reactors)  
    {  
        items = reactor.GetInventory(0).GetItems();  
        for(int p = 0; p < items.Count; p++)  
        { 
            uranium_amount = uranium_amount + (double)items[p].Amount;
        }   
    }  
    return uranium_amount;
}

double getSpeed()
{
    Vector3D current_speed = last_position - Me.GetPosition();
    last_position = Me.GetPosition();
	return Math.Round(current_speed.Length()/1.83);
}

double getPlanetSurfaceElevation(string shipName) 
{ 
    IMyShipController ship = GridTerminalSystem.GetBlockWithName(shipName + "_RC") as IMyShipController;  
    double elevation = 0; 
    bool flag = ship.TryGetPlanetElevation(MyPlanetElevation.Surface, out elevation); 
    return elevation; 
} 

double getScavRoll()
{
    double roll = 0;
    Vector3D gravity = new Vector3D();
    Vector3D direction_side = new Vector3D();
    IMyShipController ship = GridTerminalSystem.GetBlockWithName("A_Scavenger") as IMyShipController;
    IMyCubeBlock point_side = GridTerminalSystem.GetBlockWithName("SCAV Ore Detector") as IMyCubeBlock;
    direction_side = Me.GetPosition() - point_side.GetPosition();
    double a = direction_side.Length();
    gravity = ship.GetNaturalGravity();
    double b = gravity.Length();
    double c = (direction_side - gravity).Length();
    double cos_alpha = 1;
    if ((a != 0) & (b != 0) & (c != 0))
    {
        cos_alpha = (Math.Pow(c, 2) - Math.Pow(a ,2) - Math.Pow(b, 2))/(2 * b * a);
    }
    roll = (Math.Acos(cos_alpha) * (180 / Math.PI)) - 90;
    return Math.Round(roll);    
}

double getScavPitch()
{
    double pitch = 0;
    Vector3D gravity = new Vector3D(); 
    Vector3D direction_fwd = new Vector3D(); 
    IMyShipController ship = GridTerminalSystem.GetBlockWithName("A_Scavenger") as IMyShipController; 
    IMyCubeBlock point_fwd = GridTerminalSystem.GetBlockWithName("Scavenger") as IMyCubeBlock; 
    direction_fwd = Me.GetPosition() - point_fwd.GetPosition(); 
    double a = direction_fwd.Length(); 
    gravity = ship.GetNaturalGravity(); 
    double b = gravity.Length(); 
    double c = (direction_fwd - gravity).Length(); 
    double cos_alpha = 1; 
    if ((a != 0) & (b != 0) & (c != 0)) 
    { 
        cos_alpha = (Math.Pow(c, 2) - Math.Pow(a ,2) - Math.Pow(b, 2))/(2 * b * a); 
    } 
    pitch = (Math.Acos(cos_alpha) * (180 / Math.PI)) - 90;
    return Math.Round(pitch);
}

double getVerticalThrusterLoad(double gravity_multiplier, string shipName,double out cargo_load)
{
	double result = 0;
	double gravity = gravity_multiplier * 9.81;
	var gts = GridTerminalSystem;
	var ship = gts.GetBlockWithName("A_" + shipName) as IMyShipController;
	var Masses = ship.CalculateShipMass();
	double dryMass = Masses.BaseMass;
	double totalMass = Masses.TotalMass;
	double totalVerticalThrust = 0;
	cargo_load = totalMass - dryMass;
	
	List <IMyThrust> thrusters = new List<IMyThrust>();
	gts.GetBlocksOfType<IMyThrust>(thrusters);
	foreach(var t in thrusters)
	{
//		if (t.Orientation){}
	}

	//calculate
	
	return result;
}

string getDamagedTerminalBlocks()
{
	var slim = new List<IMySlimBlock>();
	var grid = new List<IMyTerminalBlock>();
	GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(grid);
	string result = "";
	foreach(var terminalBlock in grid)
	{
		var slimBlock = terminalBlock.CubeGrid.GetCubeBlock(terminalBlock.Position);
		slim.Add(slimBlock);
		if(slimBlock.CurrentDamage > 0)
		{
			result = result + terminalBlock.DisplayNameText + " damaged: ";
			result = result + Math.Round(slimBlock.CurrentDamage).ToString() + "\n";
		}
	}
		
	return result;
}

string getOreTotal()
{
	string result = "";
	return result;
}

string getIngotsTotal()
{
	string result = "";
	return result;
}

string getComponentsTotal()
{
	string result = "";
	return result;
}

public void Main(string argument) { 
    	IMyTextPanel left_lcd = GridTerminalSystem.GetBlockWithName("Left LCD") as IMyTextPanel; 
    	IMyTextPanel right_lcd = GridTerminalSystem.GetBlockWithName("Right LCD") as IMyTextPanel;	 
    	string left_text = ""; 
    	string right_text = ""; 
    
		double battery_cap = getGridBatteryCharge();
		double hydro_cap = getCurrentGasAmount("Hydrogen"); 
		double oxy_cap = getCurrentGasAmount("Oxygen"); 
		left_text = "Battery capacity: " + battery_cap.ToString() + "   "; 
		left_text = left_text + "Hydrogen level: " + hydro_cap.ToString() + "\n";
		left_text = left_text + "Oxygen level: " + oxy_cap.ToString() + "   ";
//Current position 
 
//Current Speed 
 
//Current Acceleration 
 
//Output on panels 
    	left_lcd.WritePublicText(left_text, false); 
    	right_lcd.WritePublicText(right_text, false); 
    	left_lcd.ShowPublicTextOnScreen(); 
    	right_lcd.ShowPublicTextOnScreen(); 
} 