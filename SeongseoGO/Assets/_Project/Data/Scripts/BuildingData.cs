using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeongseoGO.Data
{
    [CreateAssetMenu(fileName = "Building", menuName = "SeongseoGO/Building Data")]
    public class BuildingData : ScriptableObject
    {
        public string buildingName;

        [Tooltip("건물 카드 사진. 비어 있으면 회색 박스로 보인다.")]
        public Sprite photo;

        [Tooltip("입구 GPS 좌표. 아직 측정 전이면 hasEntranceGps를 끈다.")]
        public bool hasEntranceGps;
        public double entranceLatitude;
        public double entranceLongitude;

        public List<FloorData> floors = new List<FloorData>();
    }

    [Serializable]
    public class FloorData
    {
        [Tooltip("예: 지하1층, 1층")]
        public string floorName;
        public List<RoomData> rooms = new List<RoomData>();
    }

    [Serializable]
    public class RoomData
    {
        public string roomName;

        [Tooltip("AR 팀의 FinalRoute.json 연결용. 비어 있으면 실내 경로 없음.")]
        public TextAsset indoorRoute;
    }
}
