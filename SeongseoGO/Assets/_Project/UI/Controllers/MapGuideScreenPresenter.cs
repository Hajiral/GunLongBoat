namespace SeongseoGO.UI
{
    /// <summary>
    /// 건물까지 지도 안내 화면. 목적지 말풍선을 채운다.
    /// 건물 도착은 GPS로 판단할 예정이라, 지금은 임시 진행(지도 탭·"다음" 버튼)으로 도착 후 시작에 넘어간다.
    /// </summary>
    public class MapGuideScreenPresenter : ScreenPresenter
    {
        protected override void OnBind()
        {
            SetText("pin-room", Manager.Room.roomName);
            SetText("pin-location", Place);

            // TODO(GPS): "길찾기 시작하기"는 GPS 안내를 시작하고, 건물 입구 도착을 감지하면 도착 후 시작으로 넘긴다.

            // TODO(GPS 연결 후 삭제): 임시 진행
            if (Manager.DebugAdvance)
            {
                OnClick("map-image", Next);
                AddDebugNextButton(Next);
            }
        }

        void Next() => Manager.Go(ScreenId.ArriveStart);
    }
}
