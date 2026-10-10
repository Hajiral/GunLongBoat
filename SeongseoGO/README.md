# 성서대 GO (SeongseoGO)

캠퍼스 강의실을 지도(GPS)와 실내 AR로 안내하는 Unity 앱입니다.

## 여는 방법

1. Unity Hub에서 **Unity 6000.3.24f1**을 설치합니다. (모듈: Android Build Support, iOS Build Support)
2. Unity Hub → Add → 이 `SeongseoGO` 폴더를 선택해 엽니다. 처음 열 때 Library를 만드느라 시간이 걸립니다.
3. `Assets/_Project/Scenes/Main.unity`를 엽니다.

## Play로 확인하기

1. Game 뷰 해상도를 **412 x 915**(세로)로 추가해 선택합니다. 노치 확인은 Game 탭을 Simulator로 바꿔 iPhone을 고르면 됩니다.
2. Play를 누르면 메인 화면이 뜹니다.
   - 메인 검색창에 강의실 이름(예: `실습`, `일50`)을 입력 → 결과를 누르면 목적지 확인
   - "건물별로 찾아보기" → 건물 카드 → 층·강의실 목록 → 강의실 선택
3. 지도 안내·AR 화면은 GPS·AR 연결 전이라, 주황색 **"다음 (임시)"** 버튼이나 화면 탭으로 다음 단계로 넘어갑니다.
   끄려면 Main 씬 `MainUI` 오브젝트 → ScreenManager → **Debug Advance** 체크 해제.

흐름: 메인 → 강의실 선택 → 목적지 확인 → 건물까지 지도 안내 → 도착 후 시작 → 실내 AR(회전 → 직진 → 도착 직전) → 도착 확인 → 메인

## 폴더

| 경로 | 내용 |
|---|---|
| `Assets/_Project/UI/Screens` | 화면 9개 (UXML + USS) |
| `Assets/_Project/UI/Components` | 건물 카드, AR 공통 오버레이 |
| `Assets/_Project/UI/Styles/Common.uss` | 공통 색·글자·간격·아이콘 |
| `Assets/_Project/UI/Controllers` | `ScreenManager`(화면 전환), 화면별 `*ScreenPresenter` |
| `Assets/_Project/Data` | `rooms.csv`(강의실 원본), `Campus.asset`, 건물 에셋 |
| `Assets/_Project/Art` | 시안 PNG(`Figma`), 폰트(Pretendard), 임시 아이콘(`Icons`) |
| `Assets/_Project/Features/GPS`, `Features/AR` | GPS·AR 기능을 넣을 곳 (아직 비어 있음) |

강의실을 바꿀 때는 `rooms.csv`를 고친 뒤 메뉴 **SeongseoGO > Import Rooms CSV**를 실행합니다.

## GPS·AR 기능을 붙일 위치

기능 코드는 `Features/GPS`, `Features/AR`에 만들고, 화면과는 아래 Presenter에서 연결합니다.
선택한 목적지는 `ScreenManager.Building / Floor / Room`으로 읽을 수 있습니다. 코드에서 `TODO(GPS)`, `TODO(AR)`을 검색하면 자리가 나옵니다.

**GPS** (`Features/GPS`)
- `MapGuideScreenPresenter.cs` (건물까지 지도 안내)
  - "길찾기 시작하기"(`start-button`) → GPS 안내 시작
  - 거리·시간: `route-distance`, `route-time` 라벨
  - 현재 위치 점 `current-location`, 목적지 핀 `destination-pin`의 위치 (지금은 시안 위치에 고정)
  - 건물 입구 도착 감지 → `Manager.Go(ScreenId.ArriveStart)`
- 건물 입구 좌표: `BuildingData.entranceLatitude / entranceLongitude` (`hasEntranceGps` 체크)

**AR** (`Features/AR`)
- `ArriveStartScreenPresenter.cs`: "시작하기"에서 카메라 권한 요청과 AR 세션 시작
- `ArScreenPresenter.cs` (AR 회전·직진·도착 직전 공용)
  - 남은 거리 `distance` 라벨, 방향 표시 `direction-graphic`, 소리 버튼 `sound-button`
  - 단계 전환: `Manager.Replace(ScreenId.ArStraight / ArNear / Arrived)`
- 강의실별 실내 경로: `RoomData.indoorRoute` (AR 팀 `FinalRoute.json` 연결용)
- AR 화면 UI는 배경이 투명하므로 뒤에 AR 카메라가 보입니다. 바닥 화살표는 AR에서 3D로 그립니다.

GPS·AR이 연결되면 ScreenManager의 Debug Advance를 끄고, `TODO(... 연결 후 삭제)` 표시된 임시 코드를 지웁니다.
