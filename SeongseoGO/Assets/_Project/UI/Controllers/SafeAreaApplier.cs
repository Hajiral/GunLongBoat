using UnityEngine;
using UnityEngine.UIElements;

namespace SeongseoGO.UI
{
    /// <summary>
    /// Screen.safeArea만큼 "safe-area" 요소에 padding을 넣어 노치·홈 인디케이터에 가리지 않게 한다.
    /// ScreenManager가 화면을 바꾸면 새 화면의 "safe-area"를 다시 찾아 적용한다.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class SafeAreaApplier : MonoBehaviour
    {
        [SerializeField] string safeAreaElementName = "safe-area";

        UIDocument document;
        VisualElement safeArea;
        Rect lastSafeArea;
        Vector2Int lastScreenSize;
        float lastPanelWidth;

        void OnEnable()
        {
            document = GetComponent<UIDocument>();
            safeArea = null;
        }

        void Update()
        {
            var root = document.rootVisualElement;
            if (root == null) return;

            // 화면이 바뀌어 이전 요소가 떨어져 나갔으면 새로 찾고 다시 적용
            if (safeArea == null || safeArea.panel == null)
            {
                safeArea = root.Q(safeAreaElementName);
                lastPanelWidth = -1f;
                if (safeArea == null) return;
            }

            float panelWidth = root.resolvedStyle.width;
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            if (float.IsNaN(panelWidth) || panelWidth <= 0f || Screen.width <= 0) return;
            if (Screen.safeArea == lastSafeArea && screenSize == lastScreenSize && Mathf.Approximately(panelWidth, lastPanelWidth)) return;

            lastSafeArea = Screen.safeArea;
            lastScreenSize = screenSize;
            lastPanelWidth = panelWidth;

            // 화면 픽셀 → 패널 단위
            float scale = panelWidth / Screen.width;
            Rect area = Screen.safeArea;
            safeArea.style.paddingLeft = area.xMin * scale;
            safeArea.style.paddingRight = (Screen.width - area.xMax) * scale;
            safeArea.style.paddingTop = (Screen.height - area.yMax) * scale;
            safeArea.style.paddingBottom = area.yMin * scale;
        }
    }
}
