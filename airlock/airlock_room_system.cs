// +----------------------------------------+
// |                                        |       outside
// |                            +----+      |                             +----+
// |      room A                | V2 |      +----+                        | V3 |
// |                            +----+        D4                          +----+
// |                                        +----+
// |                                        |
// |                                        |
// +--------+    +------------------+    +--+
//          | D1 |                  | D2 |
//       +--+    +------------------+    +-------------+
//       |                                             |    +------------------+
//       |                                 +----+      |    |        +----+    |
//       |                                 | V1 |      +----+        | V4 |    +----+
//       |     room B                      +----+        D3          +----+      D5
//       |                                             +----+  roomm C         +----+
//       |                                             |    +------------------+
//       |                                             |
//       |                                             |
//       +---------------------------------------------+
//
// Rooms: room A, room B, room C, outside
// Vents: V1, V2, V3, V4
// Doors: D1, D2, D3, D4, D5
//
//
// Groups:
// <room> room A
// 	D1, D2, D4
// 	V2
// <room> room B
// 	D1, D2, D3
// 	V1
// <room> room C
// 	D3, D5
// 	V4
// <room> Outside
// 	D4, D5
// 	V3

private const string ROOM_IDENTIFIER = "<room>";

public Program() {
    Runtime.UpdateFrequency = UpdateFrequency.Update1;
}

private Station _station = null;
public void Main(string argument, UpdateType updateType) {
	if (_station == null || argument == "detect") {
		var roomBlockGroups = new List<IMyBlockGroup>();
		GridTerminalSystem.GetBlockGroups(roomBlockGroups, group => group.Name.Contains(ROOM_IDENTIFIER));
		_station = new Station(roomBlockGroups);
	}

	if ((updateType & UpdateType.Update1) != 0) {
		_station.Update();
	}
}

public class Station {

	// todo: Room, Door, Vent classes?

	// room(s) <-> door(s)
	private Dictionary<IMyBlockGroup, List<IMyDoor>>       _roomDoorDictionary = new Dictionary<IMyBlockGroup, List<IMyDoor>>();
	private Dictionary<IMyDoor,       List<IMyBlockGroup>> _doorRoomDictionary = new Dictionary<IMyDoor,       List<IMyBlockGroup>>();

	// room(s) <-> vent(s)
	private Dictionary<IMyBlockGroup, List<IMyAirVent>>    _roomVentDictionary = new Dictionary<IMyBlockGroup, List<IMyAirVent>>();
	private Dictionary<IMyAirVent,    List<IMyBlockGroup>> _ventRoomDictionary = new Dictionary<IMyAirVent,    List<IMyBlockGroup>>();

	public Station(List<IMyBlockGroup> roomBlockGroups) {
		foreach (var room in roomBlockGroups) {
			var vents = new List<IMyAirVent>();
			var doors = new List<IMyDoor>();
			room.GetBlocksOfType(vents);
			room.GetBlocksOfType(doors);

			if (_roomDoorDictionary.ContainsKey(room)) continue;
			_roomDoorDictionary[room] = doors;
			_roomVentDictionary[room] = vents;

			foreach (var door in doors) {
				List<IMyBlockGroup> rooms;
				if (!_doorRoomDictionary.TryGetValue(door, out rooms)) {
					_doorRoomDictionary[door] = rooms = new List<IMyBlockGroup>();
				}
				rooms.Add(room);
			}
			foreach (var vent in vents) {
				List<IMyBlockGroup> rooms;
				if (!_ventRoomDictionary.TryGetValue(vent, out rooms)) {
					_ventRoomDictionary[vent] = rooms = new List<IMyBlockGroup> { room };
				}
				rooms.Add(room);
			}
		}

		// todo: check resulting arrays:
		// rooms without vents?           -> assume unpressurized?
		// doors with fewer than 2 rooms? -> assume unpressurized other room?
		// doors with more than 2 rooms?  -> error message?

	}

	public void Update() {
		foreach (var doorRoom in _doorRoomDictionary) {

			bool shouldLock = (doorRoom.Value.Count == 0);
			foreach (var room in doorRoom.Value) {

				// todo:
				// check more than one vent?
				// check multiple ticks to account for buggy vents?
				// check specific pressure percentages?
				if (_roomVentDictionary[room][0].Status != VentStatus.Pressurized) {
					shouldLock = true;
					break;
				}
			}
			if (shouldLock) {
				doorRoom.Key.Enabled = (doorRoom.Key.Status != DoorStatus.Closed);
				if (doorRoom.Key.Enabled) {
					doorRoom.Key.CloseDoor();
				}
			} else {
				doorRoom.Key.Enabled = true;
			}
		}
	}
}
