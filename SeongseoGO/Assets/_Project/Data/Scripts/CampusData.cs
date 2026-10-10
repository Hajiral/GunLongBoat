using System.Collections.Generic;
using UnityEngine;

namespace SeongseoGO.Data
{
    [CreateAssetMenu(fileName = "Campus", menuName = "SeongseoGO/Campus Data")]
    public class CampusData : ScriptableObject
    {
        public List<BuildingData> buildings = new List<BuildingData>();

        /// <summary>강의실이 있는 첫 건물·층·강의실을 찾는다. 화면 미리보기용.</summary>
        public bool TryGetFirstRoom(out BuildingData building, out FloorData floor, out RoomData room)
        {
            foreach (var b in buildings)
            {
                if (b == null) continue;
                foreach (var f in b.floors)
                {
                    if (f.rooms.Count == 0) continue;
                    building = b;
                    floor = f;
                    room = f.rooms[0];
                    return true;
                }
            }

            building = null;
            floor = null;
            room = null;
            return false;
        }
    }
}
