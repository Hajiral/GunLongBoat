using SeongseoGO.Data;
using UnityEngine.UIElements;

namespace SeongseoGO.UI
{
    /// <summary>
    /// 강의실 선택 화면.
    /// 건물 카드(CampusData에서 자동 생성) → 같은 화면 안에 층·강의실 목록 → 강의실을 고르면 목적지 확인.
    /// 검색창에 입력하면 전체 강의실 검색 결과를 보여준다.
    /// </summary>
    public class RoomSelectScreenPresenter : ScreenPresenter
    {
        const string DefaultSubtitle = "찾으시는 건물을 선택해 주세요.";

        VisualElement grid;
        VisualElement roomList;
        VisualElement searchResults;
        Label subtitle;
        BuildingData openedBuilding;

        protected override void OnBind()
        {
            grid = Root.Q("building-grid");
            roomList = Root.Q("room-list");
            searchResults = Root.Q("search-results");
            subtitle = Root.Q<Label>("subtitle");

            BuildCards();
            ShowBuildings();

            Root.Q<TextField>("search-input").RegisterValueChangedCallback(evt =>
            {
                if (string.IsNullOrWhiteSpace(evt.newValue))
                {
                    if (openedBuilding != null) ShowRooms(openedBuilding);
                    else ShowBuildings();
                    return;
                }
                SetVisible(searchResults);
                RoomList.FillSearchResults(searchResults, RoomList.Search(Manager.Campus, evt.newValue), m => Select(m.building, m.floor, m.room));
            });
        }

        public override bool HandleBack()
        {
            if (openedBuilding == null) return false;
            openedBuilding = null;
            ShowBuildings();
            return true;
        }

        void BuildCards()
        {
            grid.Clear();
            if (Manager.Campus == null || Manager.BuildingCardTemplate == null) return;

            foreach (var building in Manager.Campus.buildings)
            {
                if (building == null) continue;
                Manager.BuildingCardTemplate.CloneTree(grid);
                var card = grid[grid.childCount - 1];
                card.Q<Label>("name").text = building.buildingName;
                if (building.photo != null)
                    card.Q("photo").style.backgroundImage = new StyleBackground(building.photo);

                var b = building;
                card.RegisterCallback<ClickEvent>(_ => ShowRooms(b));
            }
        }

        void ShowBuildings()
        {
            subtitle.text = DefaultSubtitle;
            SetVisible(grid);
        }

        void ShowRooms(BuildingData building)
        {
            openedBuilding = building;
            subtitle.text = $"{building.buildingName}의 강의실을 선택해 주세요.";

            roomList.Clear();
            foreach (var floor in building.floors)
            {
                if (floor.rooms.Count == 0) continue;
                roomList.Add(RoomList.CreateSectionTitle(floor.floorName));
                foreach (var room in floor.rooms)
                {
                    var f = floor;
                    var r = room;
                    roomList.Add(RoomList.CreateRow(room.roomName, null, () => Select(building, f, r)));
                }
            }
            SetVisible(roomList);
        }

        void Select(BuildingData building, FloorData floor, RoomData room)
        {
            Manager.SelectDestination(building, floor, room);
            Manager.Go(ScreenId.DestinationConfirm);
        }

        void SetVisible(VisualElement target)
        {
            grid.style.display = target == grid ? DisplayStyle.Flex : DisplayStyle.None;
            roomList.style.display = target == roomList ? DisplayStyle.Flex : DisplayStyle.None;
            searchResults.style.display = target == searchResults ? DisplayStyle.Flex : DisplayStyle.None;
            Root.Q<ScrollView>("building-scroll").scrollOffset = UnityEngine.Vector2.zero;
        }
    }
}
