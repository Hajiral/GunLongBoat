namespace SeongseoGO.UI
{
    /// <summary>목적지 확인 화면. 고른 건물·층·강의실을 보여주고, 길 찾기 → 지도 안내.</summary>
    public class DestinationConfirmScreenPresenter : ScreenPresenter
    {
        protected override void OnBind()
        {
            SetText("title-place", Place);
            SetText("room-name", Manager.Room.roomName);
            SetText("room-location", Place);

            OnClick("find-route-button", () => Manager.Go(ScreenId.MapGuide));
        }
    }
}
