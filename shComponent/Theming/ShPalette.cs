using System.Windows;
using System.Windows.Media;

namespace shComponent.Theming
{
    /// <summary>
    /// 라이브러리 전체가 공유하는 기본 색상/서체 토큰(Windows 11 라이트 톤).
    /// WHY: 모든 컨트롤의 의존성 프로퍼티 "기본값"을 이 한 곳에서 가져오게 해서,
    ///      전사 톤을 바꾸고 싶으면 이 파일만 고치면 되고, 화면별 변경은 각 컨트롤 속성으로 하도록 역할을 나눈다.
    /// </summary>
    public static class ShPalette
    {
        // ── 표면(Surface) ───────────────────────────────
        public static readonly SolidColorBrush WindowBackground = Solid("#F3F3F3");
        public static readonly SolidColorBrush PaneBackground = Solid("#F3F3F3");
        public static readonly SolidColorBrush ContentBackground = Solid("#F9F9F9");
        public static readonly SolidColorBrush ContentBorder = Solid("#E5E5E5");
        public static readonly SolidColorBrush CardBackground = Solid("#FDFDFD");
        public static readonly SolidColorBrush CardBorder = Solid("#E5E5E5");
        public static readonly SolidColorBrush PopupBackground = Solid("#FCFCFC");

        // ── 입력 컨트롤 ─────────────────────────────────
        public static readonly SolidColorBrush ControlBackground = Solid("#FEFEFE");
        public static readonly SolidColorBrush ControlHover = Solid("#F5F5F5");
        public static readonly SolidColorBrush ControlPressed = Solid("#EDEDED");
        public static readonly SolidColorBrush ControlBorder = Solid("#DEDEDE");
        public static readonly SolidColorBrush ControlStrongBorder = Solid("#8D8D8D");

        // ── 내비게이션 ──────────────────────────────────
        public static readonly SolidColorBrush NavHover = Solid("#EAEAEA");
        public static readonly SolidColorBrush NavPressed = Solid("#E2E2E2");
        public static readonly SolidColorBrush NavSelected = Solid("#E6E6E6");

        // ── 강조(Accent) ────────────────────────────────
        public static readonly SolidColorBrush Accent = Solid("#005FB8");
        public static readonly SolidColorBrush OnAccent = Solid("#FFFFFF");
        public static readonly SolidColorBrush Danger = Solid("#C42B1C");

        // ── 텍스트 ──────────────────────────────────────
        public static readonly SolidColorBrush TextPrimary = Solid("#1B1B1B");
        public static readonly SolidColorBrush TextSecondary = Solid("#616161");
        public static readonly SolidColorBrush TextDisabled = Solid("#A0A0A0");

        // ── 스켈레톤 ────────────────────────────────────
        public static readonly Color SkeletonBaseColor = (Color)ColorConverter.ConvertFromString("#E6E6E6");
        public static readonly Color SkeletonHighlightColor = (Color)ColorConverter.ConvertFromString("#F6F6F6");

        // ── 서체 ────────────────────────────────────────
        // WHY: Windows 11 기본 서체(Segoe UI Variable)를 우선 쓰고, 없는 OS(Win10)에서는 Segoe UI로 자연스럽게 대체.
        public static readonly FontFamily TextFont = new FontFamily("Segoe UI Variable Text, Segoe UI, Malgun Gothic");
        public static readonly FontFamily DisplayFont = new FontFamily("Segoe UI Variable Display, Segoe UI, Malgun Gothic");
        public static readonly FontFamily IconFont = new FontFamily("Segoe Fluent Icons, Segoe MDL2 Assets");
        public const double BodyFontSize = 14d;

        // ── 모양 ────────────────────────────────────────
        public static readonly CornerRadius ControlCornerRadius = new CornerRadius(6);
        public static readonly CornerRadius OverlayCornerRadius = new CornerRadius(8);

        /// <summary>16진수 색 문자열로 Freeze된 브러시를 만든다.</summary>
        public static SolidColorBrush Solid(string hex)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
            // WHY: DP 기본값은 모든 인스턴스가 공유한다. Freeze하지 않으면 한 곳에서 색을 바꿀 때 전체가 바뀌고 성능도 나빠진다.
            brush.Freeze();
            return brush;
        }
    }
}
