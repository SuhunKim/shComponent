using System.Windows;
using System.Windows.Controls;

namespace shComponent.Theming
{
    /// <summary>
    /// 키보드 포커스 시 표시되는 둥근 포커스 링.
    /// WHY: WPF 기본 점선 포커스는 Windows 11 톤과 어울리지 않는다. FocusVisualStyle은 "키보드로 이동했을 때만" 나타나므로
    ///      마우스 사용자에게는 보이지 않고 키보드 사용자(접근성)에게만 보이는 Windows 11 동작과 일치한다.
    /// </summary>
    public static class ShFocusVisual
    {
        public static readonly Style Rounded = Create();

        private static Style Create()
        {
            var ring = new FrameworkElementFactory(typeof(Border));
            ring.SetValue(FrameworkElement.MarginProperty, new Thickness(-3));
            ring.SetValue(Border.BorderThicknessProperty, new Thickness(2));
            ring.SetValue(Border.BorderBrushProperty, ShPalette.TextPrimary);
            ring.SetValue(Border.CornerRadiusProperty, new CornerRadius(8));
            ring.SetValue(UIElement.SnapsToDevicePixelsProperty, true);

            var template = new ControlTemplate(typeof(Control)) { VisualTree = ring };
            template.Seal();

            var style = new Style(typeof(Control));
            style.Setters.Add(new Setter(Control.TemplateProperty, template));
            style.Seal();
            return style;
        }
    }
}
