using System.Collections.Generic;
using System.IO;
using System.Text;
using SeongseoGO.Data;
using UnityEditor;
using UnityEngine;

namespace SeongseoGO.Data.Editor
{
    /// <summary>
    /// rooms.csv(건물,층,강의실·실습실)를 원본으로 CampusData / BuildingData의 층·강의실 목록을 다시 만든다.
    /// 건물 GPS와 강의실별 실내 경로(indoorRoute)는 건물명·강의실명으로 찾아 그대로 유지한다.
    /// </summary>
    public static class RoomsCsvImporter
    {
        const string CsvPath = "Assets/_Project/Data/rooms.csv";
        const string CampusPath = "Assets/_Project/Data/Campus.asset";
        const string BuildingFolder = "Assets/_Project/Data/Destinations";

        [MenuItem("SeongseoGO/Import Rooms CSV")]
        public static void Import()
        {
            if (!File.Exists(CsvPath))
            {
                Debug.LogError($"[RoomsCsvImporter] CSV가 없습니다: {CsvPath}");
                return;
            }

            // 건물명 → (층 이름 → 강의실 목록), CSV 등장 순서 유지
            var buildingOrder = new List<string>();
            var floorsByBuilding = new Dictionary<string, List<KeyValuePair<string, List<string>>>>();

            var lines = File.ReadAllLines(CsvPath, Encoding.UTF8);
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim().TrimStart('﻿');
                if (line.Length == 0) continue;

                var cols = line.Split(',');
                if (cols.Length < 3)
                {
                    Debug.LogWarning($"[RoomsCsvImporter] {i + 1}행 열 개수 부족, 건너뜀: {line}");
                    continue;
                }

                string building = cols[0].Trim();
                string floor = cols[1].Trim();
                string room = cols[2].Trim();

                if (!floorsByBuilding.TryGetValue(building, out var floors))
                {
                    floors = new List<KeyValuePair<string, List<string>>>();
                    floorsByBuilding[building] = floors;
                    buildingOrder.Add(building);
                }

                var floorEntry = floors.Find(f => f.Key == floor);
                if (floorEntry.Value == null)
                {
                    floorEntry = new KeyValuePair<string, List<string>>(floor, new List<string>());
                    floors.Add(floorEntry);
                }
                floorEntry.Value.Add(room);
            }

            var campus = AssetDatabase.LoadAssetAtPath<CampusData>(CampusPath);
            if (campus == null)
            {
                campus = ScriptableObject.CreateInstance<CampusData>();
                AssetDatabase.CreateAsset(campus, CampusPath);
            }

            // 기존 건물 에셋을 건물명으로 찾는다 (Campus에 연결된 것 + 폴더 안의 것)
            var existing = new Dictionary<string, BuildingData>();
            foreach (var b in campus.buildings)
                if (b != null && !existing.ContainsKey(b.buildingName)) existing[b.buildingName] = b;
            foreach (var guid in AssetDatabase.FindAssets("t:BuildingData", new[] { BuildingFolder }))
            {
                var b = AssetDatabase.LoadAssetAtPath<BuildingData>(AssetDatabase.GUIDToAssetPath(guid));
                if (b != null && !existing.ContainsKey(b.buildingName)) existing[b.buildingName] = b;
            }

            var newBuildings = new List<BuildingData>();
            var report = new StringBuilder("[RoomsCsvImporter] 갱신 완료\n");

            foreach (var name in buildingOrder)
            {
                if (!existing.TryGetValue(name, out var building))
                {
                    building = ScriptableObject.CreateInstance<BuildingData>();
                    building.buildingName = name;
                    var path = AssetDatabase.GenerateUniqueAssetPath($"{BuildingFolder}/Building_{name}.asset");
                    AssetDatabase.CreateAsset(building, path);
                    report.AppendLine($"  새 건물 에셋 생성: {path}");
                }

                // 강의실명 → 기존 실내 경로 (층 이름이 바뀌어도 유지되도록 강의실명으로 찾는다)
                var routes = new Dictionary<string, TextAsset>();
                foreach (var f in building.floors)
                    foreach (var r in f.rooms)
                        if (r.indoorRoute != null) routes[r.roomName] = r.indoorRoute;

                var usedRoutes = new HashSet<string>();
                var newFloors = new List<FloorData>();
                int roomCount = 0;
                foreach (var floorEntry in floorsByBuilding[name])
                {
                    var floor = new FloorData { floorName = floorEntry.Key };
                    foreach (var roomName in floorEntry.Value)
                    {
                        routes.TryGetValue(roomName, out var route);
                        if (route != null) usedRoutes.Add(roomName);
                        floor.rooms.Add(new RoomData { roomName = roomName, indoorRoute = route });
                        roomCount++;
                    }
                    newFloors.Add(floor);
                }

                foreach (var kv in routes)
                    if (!usedRoutes.Contains(kv.Key))
                        Debug.LogWarning($"[RoomsCsvImporter] {name} / {kv.Key}: CSV에 없는 강의실이라 실내 경로 연결({kv.Value.name})이 빠졌습니다.");

                Undo.RecordObject(building, "Import Rooms CSV");
                building.floors = newFloors;
                EditorUtility.SetDirty(building);
                newBuildings.Add(building);
                report.AppendLine($"  {name}: {newFloors.Count}개 층, 강의실 {roomCount}개");
            }

            foreach (var kv in existing)
                if (!newBuildings.Contains(kv.Value))
                    report.AppendLine($"  CSV에 없는 건물이라 Campus 목록에서 뺐습니다(에셋은 남겨둠): {kv.Key}");

            Undo.RecordObject(campus, "Import Rooms CSV");
            campus.buildings = newBuildings;
            EditorUtility.SetDirty(campus);
            AssetDatabase.SaveAssets();

            Debug.Log(report.ToString());
        }
    }
}
