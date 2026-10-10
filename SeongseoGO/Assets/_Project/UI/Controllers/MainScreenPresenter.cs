using UnityEngine.UIElements;

namespace SeongseoGO.UI
{
    /// <summary>
    /// 메인 화면. 검색창에 강의실 이름을 입력하면 rooms.csv(CampusData) 기준 결과가 아래에 뜨고,
    /// 결과를 누르면 목적지 확인으로 간다. "건물별로 찾아보기"나 대표 사진을 누르면 강의실 선택으로 간다.
    /// </summary>
    public class MainScreenPresenter : ScreenPresenter
    {
        protected override void OnBind()
        {
            var input = Root.Q<TextField>("search-input");
            var results = Root.Q("search-results");
            var browse = Root.Q("browse-buildings");
            results.style.display = DisplayStyle.None;

            OnClick("browse-buildings", () => Manager.Go(ScreenId.RoomSelect));
            OnClick("hero-image", () => Manager.Go(ScreenId.RoomSelect));

            input.RegisterValueChangedCallback(evt =>
            {
                bool hasQuery = !string.IsNullOrWhiteSpace(evt.newValue);
                results.style.display = hasQuery ? DisplayStyle.Flex : DisplayStyle.None;
                browse.style.display = hasQuery ? DisplayStyle.None : DisplayStyle.Flex;
                if (!hasQuery) return;

                RoomList.FillSearchResults(results, RoomList.Search(Manager.Campus, evt.newValue), m =>
                {
                    Manager.SelectDestination(m.building, m.floor, m.room);
                    Manager.Go(ScreenId.DestinationConfirm);
                });
            });
        }
    }
}
