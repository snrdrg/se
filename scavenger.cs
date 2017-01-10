Vector3D last_position = new Vector3D(0,0,0);   
int last_speed = 0;
 
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
 
double getPlanetSurfaceElevation() 
{ 
    IMyShipController ship = GridTerminalSystem.GetBlockWithName("A_Scavenger") as IMyShipController;  
    double elevation = 0; 
    bool flag = ship.TryGetPlanetElevation(MyPlanetElevation.Surface, out elevation); 
    return elevation; 
} 
 
public void Main(string argument)  
{ 
    IMyTextPanel left_lcd = GridTerminalSystem.GetBlockWithName("Left LCD") as IMyTextPanel; 
    IMyTextPanel right_lcd = GridTerminalSystem.GetBlockWithName("Right LCD") as IMyTextPanel; 
    IMyTextPanel center_lcd = GridTerminalSystem.GetBlockWithName("Center LCD") as IMyTextPanel; 
    IMyBatteryBlock scav_battery = GridTerminalSystem.GetBlockWithName("SCAVBattery") as IMyBatteryBlock; 
    string left_text = "Battery charge: "; 
    double ship_roll = getScavRoll(); 
    double ship_pitch = getScavPitch(); 
    string center_text = "Speed: " + getSpeed() + "m/s\n" + "Roll: "; 
    if (ship_roll > 0) center_text = center_text + "left " + ship_roll.ToString() + "  "; 
    if (ship_roll < 0) center_text = center_text + "right " + (-1 * ship_roll).ToString() + "  "; 
    if (ship_roll == 0) center_text = center_text + ship_roll.ToString() + "  "; 
    center_text = center_text + "Pitch: " + getScavPitch().ToString() + "\n" + "Elevation: "; 
    center_text = center_text + (Math.Round(getPlanetSurfaceElevation()*100)/100).ToString() + "m\n"; 
    double charge_amount = 0; 
    double uranium = Math.Round(getUraniumAmount() * 100) / 100; 
    charge_amount = scav_battery.CurrentStoredPower/scav_battery.MaxStoredPower; 
    left_text = left_text + Math.Round(charge_amount * 100).ToString() + "%\nUranium amount:" + uranium; 
    left_lcd.WritePublicText(left_text, false); 
    left_lcd.ShowPublicTextOnScreen(); 
 
//------------------------------------- 
//    string debug_text = ""; 
//    Vector3D dir = getScavRoll(); 
//    debug_text = debug_text + ship.GetNaturalGravity().X.ToString() + "   " + dir.X.ToString() + "\n"; 
//   debug_text = debug_text + ship.GetNaturalGravity().Y.ToString() + "   " + dir.Y.ToString() + "\n"; 
//    debug_text = debug_text + ship.GetNaturalGravity().Z.ToString() + "   " + dir.Z.ToString(); 
//    debug_text = getScavRoll().ToString(); 
//    right_lcd.WritePublicText(debug_text, false);  
//-------------------------------------- 
 
    center_lcd.WritePublicText(center_text, false);  
    right_lcd.ShowPublicTextOnScreen();  
    center_lcd.ShowPublicTextOnScreen();  
} 
