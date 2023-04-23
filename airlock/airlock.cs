private const string AIRLOCK_GROUP_IDENTIFIER = "<airlock>";

public Program() {
	Runtime.UpdateFrequency = UpdateFrequency.Update1;
}

private List<List<IMyDoor>> airlocks = null;
public void Main(string argument, UpdateType updateType) {
	// get blocks once -> recompile to update
	if (airlocks == null) {
		airlocks = new List<List<IMyDoor>>();
		GridTerminalSystem.GetBlockGroups(null, group => {
			if (!group.Name.Contains(AIRLOCK_GROUP_IDENTIFIER)) return false;
			airlocks.Add(new List<IMyDoor>());
			group.GetBlocksOfType(airlocks.Last());
			return false;
		});
	}

	// if one door opens in an airlock, disable all other doors
	foreach (List<IMyDoor> airlock in airlocks) {
		IMyDoor openDoor = null;
		foreach (IMyDoor door in airlock) {
			if (door.Status == DoorStatus.Closed) continue;
			openDoor = door;
			break;
		}
		foreach (IMyDoor door in airlock) {
			door.Enabled = (openDoor == null || openDoor == door);
		}
	}
}
