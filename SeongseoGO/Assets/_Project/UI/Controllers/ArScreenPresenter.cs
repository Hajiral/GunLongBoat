using UnityEngine.UIElements;

namespace SeongseoGO.UI
{
    /// <summary>
    /// 실내 AR 화면 3개(회전·직진·도착 직전) 공통. 목적지 카드를 채운다.
    /// 단계 전환은 AR 경로 진행으로 판단할 예정이라, 지금은 임시 진행(화면 탭·"다음" 버튼)으로 넘어간다.
    /// AR 단계끼리는 뒤로가기 기록에 남기지 않아서, 닫기(X)나 뒤로가기는 도착 후 시작으로 돌아간다.
    /// </summary>
    public class ArScreenPresenter : ScreenPresenter
    {
        readonly ScreenId next;

        public ArScreenPresenter(ScreenId next)
        {
            this.next = next;
        }

        protected override void OnBind()
        {
            SetText("destination-building", Manager.Building.buildingName);
            SetText("destination-room", $"{Manager.Floor.floorName} {Manager.Room.roomName}");

            OnClick("close-button", Manager.Back);
            // TODO(AR): 소리 안내 켜기/끄기 (sound-button)

            // TODO(AR 연결 후 삭제): 임시 진행. 버튼이 아닌 빈 곳을 탭하면 다음 단계
            if (Manager.DebugAdvance)
            {
                var overlay = Root.Q("ar-overlay");
                overlay.RegisterCallback<ClickEvent>(evt =>
                {
                    if (evt.target == overlay) Next();
                });
                AddDebugNextButton(Next);
            }
        }

        void Next() => Manager.Replace(next);
    }
}
