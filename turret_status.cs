public void Main(string argument) {
    IMyTextPanel panel_hangar = GridTerminalSystem.GetBlockWithName("Base Status LCD") as IMyTextPanel;
    string text = "Base status report\n";
    string text_garage = "Pathfinder Platoon\nForward Operation Base\n";
    text_garage = text_garage + "--------------------------------------\nDefence Systems Status\n";
    bool defence_status = true;
    bool low_ammo = false;
    panel_hangar.WritePublicText(text, false);
    List<IMyLargeInteriorTurret>turrets = new List<IMyLargeInteriorTurret>();
    GridTerminalSystem.GetBlocksOfType<IMyLargeInteriorTurret>(turrets);
    foreach(IMyLargeInteriorTurret t in turrets){
        text_garage =  text_garage + (String)t.DisplayNameText + ":";
        List<IMyInventoryItem> inv = t.GetInventory(0).GetItems();
        if (inv.Count == 0) {
            text_garage = text_garage + "NO AMMO\n";
            defence_status = false;
        }
        else{
            if (inv[0].Content.SubtypeName.StartsWith("NATO_"))
            {
                double amount = Math.Round((double)inv[0].Amount);
                if (amount > 2)
                {
                    text_garage = text_garage + amount + " mags. OK\n";
                }
                else
                {
                    text_garage = text_garage + amount + " mags. LOW AMMO\n";
                    low_ammo = true;
                }
            }
        }
    }
    /*text_garage = text_garage + (String)panel_garage.GetValue<Color>("FontColor").ToString();*/
    VRageMath.Color garage_text_color;
    if (defence_status){
        if(low_ammo){garage_text_color = new VRageMath.Color(160, 90, 0);}
        else{garage_text_color = new VRageMath.Color(60, 190, 0);}
    }
    else{garage_text_color = new VRageMath.Color(190, 60, 0);}
    panel_garage.SetValue<Color>("FontColor", garage_text_color);
    text_garage = text_garage + "--------------------------------------\nGrid Integrity Status\n";
    panel_hangar.WritePublicText(text_garage, false);
    panel_garage.WritePublicText(text_garage,false);
    panel_hangar.ShowPublicTextOnScreen();
    panel_garage.ShowPublicTextOnScreen();
}
