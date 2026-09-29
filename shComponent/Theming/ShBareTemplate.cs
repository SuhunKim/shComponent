using System.Windows;
using System.Windows.Controls;

namespace shComponent.Theming
{
    /// <summary>
    /// UserControl 기본 템플릿(Border + ContentPresenter) 대신 쓰는 "ContentPresenter만" 템플릿.
    /// WHY: UserControl 기본 템플릿은 Background/BorderBrush/Padding를 자기가 그려서, 그 값을 다시 내부 요소에 바인딩하면
    ///      이중으로 그려지거나(사각 배경이 둥근 버튼 뒤에 보임) 여백이 두 번 적용된다.
    ///      이 템플릿을 쓰면 그 속성들은 "값 저장소"일 뿐이고 실제 그리기는 컨트롤 XAML의 내부 요소가 전담한다.
    /// </summary>
    internal static class ShBareTemplate
    {
        public static readonly ControlTemplate Instance = Create();

        private static ControlTemplate Create()
        {
            var presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            var template = new ControlTemplate(typeof(UserControl)) { VisualTree = presenter };
            template.Seal();
            return template;
        }
    }
}
