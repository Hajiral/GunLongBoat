using System;
using System.Collections.Generic;
using SeongseoGO.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace SeongseoGO.UI
{
    public enum ScreenId
    {
        Main,
        RoomSelect,
        DestinationConfirm,
        MapGuide,
        ArriveStart,
        ArTurn,
        ArStraight,
        ArNear,
        Arrived,
    }

    /// <summary>
    /// 화면 9개의 전환과 뒤로가기, 선택한 목적지(건물·층·강의실)를 관리한다.
    /// UIDocument 하나에 화면 UXML을 바꿔 끼우는 방식이다.
    /// </summary>
    [DefaultExecutionOrder(100)] // UIDocument가 rootVisualElement를 만든 뒤에 실행
    [RequireComponent(typeof(UIDocument))]
    public class ScreenManager : MonoBehaviour
    {
        [Serializable]
        struct ScreenEntry
        {
            public ScreenId id;
            public VisualTreeAsset uxml;
        }

        [SerializeField] CampusData campus;
        [SerializeField] ScreenEntry[] screens;
        [SerializeField] VisualTreeAsset buildingCardTemplate;

        // TODO(GPS·AR 연결 후 끄기): 지도·AR 화면에서 탭 또는 "다음" 버튼으로 다음 단계로 넘긴다.
        [Header("임시: GPS·AR 연결 전 확인용")]
        [Tooltip("켜면 지도 안내·AR 화면에 임시 '다음' 버튼이 생기고, 화면을 탭해도 다음 단계로 넘어간다. GPS·AR이 연결되면 끈다.")]
        [SerializeField] bool debugAdvance = true;

        public CampusData Campus => campus;
        public VisualTreeAsset BuildingCardTemplate => buildingCardTemplate;
        public bool DebugAdvance => debugAdvance;

        public BuildingData Building { get; private set; }
        public FloorData Floor { get; private set; }
        public RoomData Room { get; private set; }
        public bool HasDestination => Building != null && Floor != null && Room != null;

        public ScreenId Current { get; private set; }

        readonly Stack<ScreenId> history = new Stack<ScreenId>();
        UIDocument document;
        ScreenPresenter presenter;
        bool started;

        void OnEnable()
        {
            document = GetComponent<UIDocument>();
            // 비활성화 후 다시 켜지면 UIDocument가 화면을 비우므로 지금 화면을 다시 띄운다
            Show(started ? Current : ScreenId.Main);
            started = true;
        }

        void Update()
        {
            // 안드로이드 뒤로 키 (에디터에서는 Esc)
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                Back();
        }

        public void SelectDestination(BuildingData building, FloorData floor, RoomData room)
        {
            Building = building;
            Floor = floor;
            Room = room;
        }

        /// <summary>다음 화면으로 이동. 뒤로가기로 지금 화면에 돌아올 수 있다.</summary>
        public void Go(ScreenId id)
        {
            history.Push(Current);
            Show(id);
        }

        /// <summary>지금 화면을 바꾼다. 뒤로가기 기록에 남기지 않는다 (AR 단계 진행용).</summary>
        public void Replace(ScreenId id)
        {
            Show(id);
        }

        public void Back()
        {
            if (presenter != null && presenter.HandleBack()) return;
            if (history.Count > 0) Show(history.Pop());
        }

        /// <summary>기록을 비우고 메인으로. 필요하면 메인 위에 화면 하나를 더 띄운다.</summary>
        public void GoHome(ScreenId? then = null)
        {
            history.Clear();
            Show(ScreenId.Main);
            if (then.HasValue) Go(then.Value);
        }

        void Show(ScreenId id)
        {
            var root = document.rootVisualElement;
            if (root == null) return;

            var uxml = FindUxml(id);
            if (uxml == null)
            {
                Debug.LogError($"[ScreenManager] {id} 화면 UXML이 연결되지 않았습니다.", this);
                return;
            }

            // 목적지가 필요한 화면인데 아직 고르지 않았다면 첫 강의실로 채운다 (에디터에서 바로 띄울 때 대비)
            if (id >= ScreenId.DestinationConfirm && !HasDestination && campus != null
                && campus.TryGetFirstRoom(out var b, out var f, out var r))
                SelectDestination(b, f, r);

            root.Clear();
            var screen = uxml.Instantiate();
            screen.style.flexGrow = 1;
            root.Add(screen);

            Current = id;
            presenter = CreatePresenter(id);
            presenter.Bind(screen, this);

            // 화면 안의 뒤로가기 버튼은 모두 같은 동작
            screen.Query("back-button").ForEach(e => e.RegisterCallback<ClickEvent>(_ => Back()));
        }

        VisualTreeAsset FindUxml(ScreenId id)
        {
            foreach (var entry in screens)
                if (entry.id == id) return entry.uxml;
            return null;
        }

        static ScreenPresenter CreatePresenter(ScreenId id)
        {
            switch (id)
            {
                case ScreenId.Main: return new MainScreenPresenter();
                case ScreenId.RoomSelect: return new RoomSelectScreenPresenter();
                case ScreenId.DestinationConfirm: return new DestinationConfirmScreenPresenter();
                case ScreenId.MapGuide: return new MapGuideScreenPresenter();
                case ScreenId.ArriveStart: return new ArriveStartScreenPresenter();
                case ScreenId.ArTurn: return new ArScreenPresenter(ScreenId.ArStraight);
                case ScreenId.ArStraight: return new ArScreenPresenter(ScreenId.ArNear);
                case ScreenId.ArNear: return new ArScreenPresenter(ScreenId.Arrived);
                case ScreenId.Arrived: return new ArrivedScreenPresenter();
                default: throw new ArgumentOutOfRangeException(nameof(id), id, null);
            }
        }
    }
}
