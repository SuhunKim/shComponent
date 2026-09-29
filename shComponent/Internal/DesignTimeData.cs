using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using shComponent.Navigation;

namespace shComponent.Internal
{
    /// <summary>
    /// XAML 디자이너 전용 샘플 데이터(Menu1, SubMenu1 ... 형태의 자리표시자).
    /// WHY: 디자이너에서 메뉴가 비어 있으면 색/간격을 조정해도 결과를 볼 수 없다.
    ///      실제 메뉴 구성은 프로젝트마다 바뀌므로 특정 업무 이름을 쓰지 않고 번호만 붙인다.
    ///      런타임에는 절대 쓰이지 않도록 IsDesignMode 검사와 함께만 호출한다.
    /// </summary>
    internal static class DesignTimeData
    {
        public const string HomeGlyph = "";
        public const string MenuGlyph = "";

        public static bool IsDesignMode(DependencyObject element) => DesignerProperties.GetIsInDesignMode(element);

        public static IReadOnlyList<INavItem> CreateMenu()
        {
            var menu1 = new NavItem("menu1", "Menu1", MenuGlyph);
            foreach (var sub in CreateSubMenu()) menu1.Add(sub);

            return new INavItem[]
            {
                new NavItem("home", "홈", HomeGlyph),
                menu1,
                new NavItem("menu2", "Menu2", MenuGlyph),
                new NavItem("menu3", "Menu3", MenuGlyph),
                new NavItem("menu4", "Menu4", MenuGlyph),
            };
        }

        public static IReadOnlyList<INavItem> CreateSubMenu()
        {
            return Enumerable.Range(1, 3)
                .Select(i => (INavItem)new NavItem($"menu1.sub{i}", $"SubMenu{i}"))
                .ToList();
        }
    }
}
