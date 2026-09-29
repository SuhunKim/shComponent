using System.Windows;
using System.Windows.Controls;
using shComponent.Navigation;
using Xunit;

namespace shComponent.Tests
{
    /// <summary>
    /// WPF 객체를 만드는 테스트.
    /// WHY: [Fact]로 돌리면 "호출 스레드는 STA여야 합니다" 예외가 난다. [WpfFact]는 STA 스레드 + Dispatcher에서 실행한다.
    /// </summary>
    public class WpfControlTests
    {
        [WpfFact]
        public void PageFactory_cache_true면_같은_인스턴스를_재사용한다()
        {
            var factory = new PageFactory()
                .Register<UserControl>("cached", cache: true)
                .Register<UserControl>("fresh");
            var cached = new NavItem("cached", "캐시");
            var fresh = new NavItem("fresh", "새로");

            Assert.Same(factory.CreatePage(cached), factory.CreatePage(cached));
            Assert.NotSame(factory.CreatePage(fresh), factory.CreatePage(fresh));
            Assert.Null(factory.CreatePage(new NavItem("unknown", "없음")));
        }

        [WpfFact]
        public void CustomButton_XAML을_로드하고_Click_이벤트를_전달한다()
        {
            var button = new CustomButton { Content = "확인" };
            var clicked = 0;
            button.Click += (s, e) => clicked++;

            button.RaiseEvent(new RoutedEventArgs(CustomButton.ClickEvent));

            Assert.Equal(ButtonAppearance.Standard, button.Appearance);
            Assert.Equal(1, clicked);
        }
    }
}
