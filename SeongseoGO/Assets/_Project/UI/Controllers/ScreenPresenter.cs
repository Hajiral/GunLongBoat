using System;
using UnityEngine.UIElements;

namespace SeongseoGO.UI
{
    /// <summary>
    /// 화면 하나의 버튼·글자를 연결한다. ScreenManager가 화면을 띄울 때마다 새로 만든다.
    /// </summary>
    public abstract class ScreenPresenter
    {
        protected ScreenManager Manager { get; private set; }
        protected VisualElement Root { get; private set; }

        public void Bind(VisualElement root, ScreenManager manager)
        {
            Root = root;
            Manager = manager;
            OnBind();
        }

        protected abstract void OnBind();

        /// <summary>뒤로가기를 화면 안에서 처리했으면 true. (예: 강의실 목록 → 건물 목록)</summary>
        public virtual bool HandleBack() => false;

        protected void OnClick(string elementName, Action action)
        {
            var element = Root.Q(elementName);
            element?.RegisterCallback<ClickEvent>(_ => action());
        }

        protected void SetText(string labelName, string text)
        {
            var label = Root.Q<Label>(labelName);
            if (label != null) label.text = text;
        }

        protected string Place => $"{Manager.Building.buildingName} {Manager.Floor.floorName}";

        // TODO(GPS·AR 연결 후 삭제): ScreenManager.debugAdvance가 켜져 있을 때만 임시 "다음" 버튼을 띄운다.
        protected void AddDebugNextButton(Action next)
        {
            if (!Manager.DebugAdvance) return;

            var button = new VisualElement { name = "debug-next" };
            button.AddToClassList("debug-next");
            var label = new Label("다음 (임시)");
            label.AddToClassList("debug-next__label");
            button.Add(label);
            button.RegisterCallback<ClickEvent>(evt =>
            {
                evt.StopPropagation();
                next();
            });

            var host = Root.Q("safe-area") ?? Root;
            host.Add(button);
        }
    }
}
