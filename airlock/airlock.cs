private const string AIRLOCK_GROUP_IDENTIFIER = "<airlock>";

private List<List<IMyDoor>> airlocks = new List<List<IMyDoor>>();
public Program() {
	// get blocks once -> recompile to update
	GridTerminalSystem.GetBlockGroups(null, group => {
		if (group.Name.Contains(AIRLOCK_GROUP_IDENTIFIER)) {
			airlocks.Add(new List<IMyDoor>());
			group.GetBlocksOfType(airlocks.Last());
		}
		return false;
	});

	Runtime.UpdateFrequency = UpdateFrequency.Update1;
}

public void Main() {
	// if one door opens in an airlock, disable all other doors
	foreach (List<IMyDoor> airlock in airlocks) {
		IMyDoor openDoor = null;
		foreach (IMyDoor door in airlock) {
			if (door.Status != DoorStatus.Closed) {
				openDoor = door;
				break;
			}
		}
		foreach (IMyDoor door in airlock) {
			door.Enabled = (openDoor == null || openDoor == door);
		}
	}
}
