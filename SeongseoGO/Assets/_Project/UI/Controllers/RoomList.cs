using System;
using System.Collections.Generic;
using SeongseoGO.Data;
using UnityEngine.UIElements;

namespace SeongseoGO.UI
{
    /// <summary>강의실 검색과 목록 행 만들기. 메인·강의실 선택 화면이 같이 쓴다.</summary>
    public static class RoomList
    {
        public struct Match
        {
            public BuildingData building;
            public FloorData floor;
            public RoomData room;
        }

        /// <summary>강의실 이름(또는 "건물 층 강의실" 전체)에 검색어가 들어간 강의실을 찾는다.</summary>
        public static List<Match> Search(CampusData campus, string query, int max = 30)
        {
            var results = new List<Match>();
            query = query?.Trim();
            if (campus == null || string.IsNullOrEmpty(query)) return results;

            foreach (var b in campus.buildings)
            {
                if (b == null) continue;
                foreach (var f in b.floors)
                {
                    foreach (var r in f.rooms)
                    {
                        string full = $"{b.buildingName} {f.floorName} {r.roomName}";
                        if (r.roomName.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0
                            && full.IndexOf(query, StringComparison.OrdinalIgnoreCase) < 0) continue;

                        results.Add(new Match { building = b, floor = f, room = r });
                        if (results.Count >= max) return results;
                    }
                }
            }
            return results;
        }

        /// <summary>목록 한 줄: 강의실 이름 + 보조 글자 + 오른쪽 화살표 자리.</summary>
        public static VisualElement CreateRow(string title, string subtitle, Action onClick)
        {
            var row = new VisualElement();
            row.AddToClassList("list-row");

            var texts = new VisualElement();
            texts.AddToClassList("list-row__texts");
            var titleLabel = new Label(title);
            titleLabel.AddToClassList("list-row__title");
            texts.Add(titleLabel);
            if (!string.IsNullOrEmpty(subtitle))
            {
                var sub = new Label(subtitle);
                sub.AddToClassList("list-row__subtitle");
                texts.Add(sub);
            }
            row.Add(texts);

            var chevron = new VisualElement();
            chevron.AddToClassList("icon-slot");
            chevron.AddToClassList("list-row__chevron");
            chevron.AddToClassList("icon--chevron-right");
            chevron.AddToClassList("icon-tint--muted");
            row.Add(chevron);

            row.RegisterCallback<ClickEvent>(_ => onClick());
            return row;
        }

        public static Label CreateSectionTitle(string text)
        {
            var label = new Label(text);
            label.AddToClassList("list-section-title");
            return label;
        }

        public static Label CreateEmpty(string text)
        {
            var label = new Label(text);
            label.AddToClassList("list-empty");
            return label;
        }

        /// <summary>검색 결과를 container에 채운다. 결과를 누르면 onSelect.</summary>
        public static void FillSearchResults(VisualElement container, List<Match> matches, Action<Match> onSelect)
        {
            container.Clear();
            if (matches.Count == 0)
            {
                container.Add(CreateEmpty("검색 결과가 없어요."));
                return;
            }
            foreach (var m in matches)
            {
                var match = m;
                container.Add(CreateRow(m.room.roomName, $"{m.building.buildingName} {m.floor.floorName}", () => onSelect(match)));
            }
        }
    }
}
