using System;
using System.Collections.Generic;

namespace shComponent.Navigation
{
    /// <summary>
    /// <see cref="INavItem"/>의 기본 구현. 코드비하인드에서 메뉴 트리를 명시적으로 조립하기 위한 용도.
    /// <code>
    /// var menu = new[]
    /// {
    ///     new NavItem("home", "홈", ""),
    ///     new NavItem("menu1", "Menu1", "")
    ///         .Add(new NavItem("menu1.sub1", "SubMenu1"))
    ///         .Add(new NavItem("menu1.sub2", "SubMenu2")),
    /// };
    /// </code>
    /// </summary>
    public class NavItem : INavItem
    {
        private readonly List<INavItem> _children = new List<INavItem>();

        public NavItem(string key, string title, string? glyph = null)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key는 비어 있을 수 없습니다.", nameof(key));
            Key = key;
            Title = title ?? string.Empty;
            Glyph = glyph;
        }

        public string Key { get; }
        public string Title { get; }
        public string? Glyph { get; }
        public object? Parameter { get; set; }
        public bool IsEnabled { get; set; } = true;
        public IReadOnlyList<INavItem> Children => _children;

        /// <summary>서브메뉴 항목을 추가한다. WHY: 트리를 한 문장으로 선언할 수 있도록 자기 자신을 반환(Fluent).</summary>
        public NavItem Add(INavItem child)
        {
            if (child == null) throw new ArgumentNullException(nameof(child));
            _children.Add(child);
            return this;
        }

        public override string ToString() => $"{Title} ({Key})";
    }
}
