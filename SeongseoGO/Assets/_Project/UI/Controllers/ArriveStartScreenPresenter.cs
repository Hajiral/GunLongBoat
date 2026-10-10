using UnityEngine.UIElements;

namespace SeongseoGO.UI
{
    /// <summary>
    /// 도착 후 시작 화면. 고른 건물·강의실 이름과 건물 사진(BuildingData.photo)을 넣고, 시작하기 → 실내 AR(회전).
    /// </summary>
    public class ArriveStartScreenPresenter : ScreenPresenter
    {
        protected override void OnBind()
        {
            SetText("title", $"{Manager.Building.buildingName}에 도착하셨나요?");
            SetText("description", $"이제 카메라를 켜고 {Manager.Room.roomName}까지\nAR 안내선을 따라 이동해 보세요.");

            if (Manager.Building.photo != null)
                Root.Q("building-photo").style.backgroundImage = new StyleBackground(Manager.Building.photo);

            // TODO(AR): 카메라 권한 요청 후 AR 세션 시작
            OnClick("start-button", () => Manager.Go(ScreenId.ArTurn));
        }
    }
}
