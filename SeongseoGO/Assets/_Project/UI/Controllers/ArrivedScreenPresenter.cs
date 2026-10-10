namespace SeongseoGO.UI
{
    /// <summary>
    /// 도착 확인 화면. 홈 → 메인(기록 비움), 다른 목적지 → 메인 위에 강의실 선택.
    /// </summary>
    public class ArrivedScreenPresenter : ScreenPresenter
    {
        protected override void OnBind()
        {
            SetText("description", $"{Place} 강의실에\n무사히 도착했습니다.");
            SetText("place-chip", $"{Manager.Building.buildingName} · {Manager.Floor.floorName} {Manager.Room.roomName}");
            SetText("card-building", Manager.Building.buildingName);
            SetText("card-room", $"{Manager.Floor.floorName} · {Manager.Room.roomName}");

            OnClick("home-button", () => Manager.GoHome());
            OnClick("other-destination", () => Manager.GoHome(ScreenId.RoomSelect));
        }

        // 도착 후 뒤로가기는 AR로 돌아가지 않고 메인으로
        public override bool HandleBack()
        {
            Manager.GoHome();
            return true;
        }
    }
}
