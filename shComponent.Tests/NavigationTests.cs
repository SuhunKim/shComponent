using System;
using shComponent.Navigation;
using Xunit;

namespace shComponent.Tests
{
    /// <summary>메뉴 트리 모델/탐색 테스트. WPF 객체가 없으므로 일반 [Fact](스레드 제약 없음).</summary>
    public class NavigationTests
    {
        private static INavItem[] CreateMenu() => new INavItem[]
        {
            new NavItem("home", "홈"),
            new NavItem("menu1", "Menu1")
                .Add(new NavItem("menu1.sub1", "SubMenu1") { IsEnabled = false })
                .Add(new NavItem("menu1.sub2", "SubMenu2")),
        };

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void NavItem_빈_Key는_거부한다(string key)
        {
            Assert.Throws<ArgumentException>(() => new NavItem(key, "제목"));
        }

        [Fact]
        public void Find_서브메뉴는_부모와_함께_찾고_대소문자를_무시한다()
        {
            var item = NavTree.Find(CreateMenu(), "MENU1.SUB2", out var parent);

            Assert.Equal("menu1.sub2", item?.Key);
            Assert.Equal("menu1", parent?.Key);
        }

        [Fact]
        public void ResolveLeaf_비활성_자식은_건너뛴다()
        {
            var leaf = NavTree.ResolveLeaf(CreateMenu()[1]);

            Assert.Equal("menu1.sub2", leaf.Key);
        }
    }
}
