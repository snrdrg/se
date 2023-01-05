bool setupcomplete = false;
List<IMyRadioAntenna> antenna;
IMyBroadcastListener statusListener;

string statusChannelTag = "RDOStatusChannel";
string commandChannelTag = "RDOCommandChannel";

public Program(){
	// Set the script to run every 100 ticks, so no timer needed.
	Runtime.UpdateFrequency = UpdateFrequency.Update100;
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

public void Main(string arg, UpdateType updateSource){
	// If setupcomplete is false, run Setup method.
	if(!setupcomplete){
		Echo("Running setup");
		Setup();
	}
	else{		
		statusListener = IGC.RegisterBroadcastListener(statusChannelTag);
		while (statusListener.HasPendingMessage)
		{
			string message;
			MyIGCMessage newStatus = statusListener.AcceptMessage();
			if(statusListener.Tag == statusChannelTag)
			{
				if(newStatus.Data is string)
				{
					message = newStatus.Data.ToString()
					Me.CustomData = message;
					Echo(message);
				}
			}
		}
	}
}