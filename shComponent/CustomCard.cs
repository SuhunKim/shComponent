using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using shComponent.Internal;
using shComponent.Theming;

namespace shComponent
{
    /// <summary>
    /// Windows 11 설정 카드. IsClickable=true 이면 Click 이벤트 / Command 로 부모에게 "눌렸다"만 알린다.
    /// WHY: 카드가 눌렸을 때 무엇을 할지(페이지 이동, 대화상자 등)는 업무 로직이므로 카드 안에서 처리하지 않는다.
    /// </summary>
    [ToolboxItem(true)]
    [Description("Windows 11 설정 앱 스타일 카드")]
    public class CustomCard : UserControl
    {
        private const string Category = "sh 모양";
        private const string BehaviorCategory = "sh 동작";

        static CustomCard()
        {
            // WHY: UserControl 기본 테마 스타일(Border 템플릿)이 배경/테두리를 따로 그리지 않도록 스타일 조회를 끊고 ContentPresenter만 남긴다.
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomCard), new FrameworkPropertyMetadata(typeof(CustomCard)));
            TemplateProperty.OverrideMetadata(typeof(CustomCard), new FrameworkPropertyMetadata(ShBareTemplate.Instance));
            FocusVisualStyleProperty.OverrideMetadata(typeof(CustomCard), new FrameworkPropertyMetadata(ShFocusVisual.Rounded));
            BackgroundProperty.OverrideMetadata(typeof(CustomCard), new FrameworkPropertyMetadata(ShPalette.CardBackground));
            BorderBrushProperty.OverrideMetadata(typeof(CustomCard), new FrameworkPropertyMetadata(ShPalette.CardBorder));
            BorderThicknessProperty.OverrideMetadata(typeof(CustomCard), new FrameworkPropertyMetadata(new Thickness(1)));
            ForegroundProperty.OverrideMetadata(typeof(CustomCard), new FrameworkPropertyMetadata(ShPalette.TextPrimary, FrameworkPropertyMetadataOptions.Inherits));
            FontFamilyProperty.OverrideMetadata(typeof(CustomCard), new FrameworkPropertyMetadata(ShPalette.TextFont, FrameworkPropertyMetadataOptions.Inherits));
            FontSizeProperty.OverrideMetadata(typeof(CustomCard), new FrameworkPropertyMetadata(ShPalette.BodyFontSize, FrameworkPropertyMetadataOptions.Inherits));
            PaddingProperty.OverrideMetadata(typeof(CustomCard), new FrameworkPropertyMetadata(new Thickness(16, 12, 16, 12)));
            MinHeightProperty.OverrideMetadata(typeof(CustomCard), new FrameworkPropertyMetadata(64d));
            // WHY: 클릭 불가 카드는 Tab 이동 대상에서 빠져야 키보드 사용자가 불필요하게 멈추지 않는다.
            FocusableProperty.OverrideMetadata(typeof(CustomCard), new FrameworkPropertyMetadata(false));
        }

        public CustomCard()
        {
            // WHY: 모양(XAML)은 내부 뷰가 담당한다. 이 클래스는 XAML 없이 코드로만 정의되어야
            //      사용하는 쪽 XAML에서 자식 요소에 x:Name을 쓸 수 있다(x:Class XAML 타입은 이름 범위가 막힌다).
            base.Content = new CustomCardView();
        }

        #region Events

        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
            nameof(Click), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(CustomCard));

        /// <summary>IsClickable=true 일 때 카드를 클릭(또는 Enter/Space)하면 발생.</summary>
        [Category(BehaviorCategory)]
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        #endregion

        #region Dependency Properties

        // WHY: UserControl.Content는 XAML의 루트 요소가 차지하므로, 사용자가 쓰는 Content(오른쪽 컨트롤 또는 카드 전체 내용)는 별도 속성으로 가려 둔다.
        //      XAML의 자식 요소 / Content="..." 는 이 속성으로 들어온다.
        public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            nameof(Content), typeof(object), typeof(CustomCard), new FrameworkPropertyMetadata(null));

        [Category(Category), Description("카드 오른쪽에 표시할 컨트롤(토글·콤보 등). 제목/설명/아이콘이 없으면 카드 전체를 채움")]
        public new object? Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header), typeof(object), typeof(CustomCard),
            new FrameworkPropertyMetadata(null, null, CustomButton.CoerceEmptyToNull));

        [Category(Category), Description("카드 제목")]
        public object? Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
            nameof(Description), typeof(string), typeof(CustomCard),
            new FrameworkPropertyMetadata(null, null, CustomButton.CoerceEmptyToNull));

        [Category(Category), Description("제목 아래 보조 설명")]
        public string? Description
        {
            get => (string?)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
            nameof(Glyph), typeof(string), typeof(CustomCard),
            new FrameworkPropertyMetadata(null, null, CustomButton.CoerceEmptyToNull));

        [Category(Category), Description("왼쪽 아이콘 글리프 (Segoe Fluent Icons)")]
        public string? Glyph
        {
            get => (string?)GetValue(GlyphProperty);
            set => SetValue(GlyphProperty, value);
        }

        public static readonly DependencyProperty GlyphSizeProperty = DependencyProperty.Register(
            nameof(GlyphSize), typeof(double), typeof(CustomCard), new FrameworkPropertyMetadata(20d));

        [Category(Category), Description("아이콘 크기")]
        public double GlyphSize
        {
            get => (double)GetValue(GlyphSizeProperty);
            set => SetValue(GlyphSizeProperty, value);
        }

        public static readonly DependencyProperty HeaderFontSizeProperty = DependencyProperty.Register(
            nameof(HeaderFontSize), typeof(double), typeof(CustomCard), new FrameworkPropertyMetadata(14d));

        [Category(Category), Description("제목 글자 크기")]
        public double HeaderFontSize
        {
            get => (double)GetValue(HeaderFontSizeProperty);
            set => SetValue(HeaderFontSizeProperty, value);
        }

        public static readonly DependencyProperty DescriptionForegroundProperty = DependencyProperty.Register(
            nameof(DescriptionForeground), typeof(Brush), typeof(CustomCard), new FrameworkPropertyMetadata(ShPalette.TextSecondary));

        [Category(Category), Description("설명 글자색")]
        public Brush DescriptionForeground
        {
            get => (Brush)GetValue(DescriptionForegroundProperty);
            set => SetValue(DescriptionForegroundProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(CustomCard), new FrameworkPropertyMetadata(ShPalette.OverlayCornerRadius));

        [Category(Category), Description("모서리 둥글기")]
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register(
            nameof(HoverBackground), typeof(Brush), typeof(CustomCard), new FrameworkPropertyMetadata(ShPalette.ControlHover));

        [Category(Category), Description("클릭 가능 카드의 hover 배경")]
        public Brush HoverBackground
        {
            get => (Brush)GetValue(HoverBackgroundProperty);
            set => SetValue(HoverBackgroundProperty, value);
        }

        public static readonly DependencyProperty PressedBackgroundProperty = DependencyProperty.Register(
            nameof(PressedBackground), typeof(Brush), typeof(CustomCard), new FrameworkPropertyMetadata(ShPalette.ControlPressed));

        [Category(Category), Description("클릭 가능 카드의 눌림 배경")]
        public Brush PressedBackground
        {
            get => (Brush)GetValue(PressedBackgroundProperty);
            set => SetValue(PressedBackgroundProperty, value);
        }

        public static readonly DependencyProperty IsClickableProperty = DependencyProperty.Register(
            nameof(IsClickable), typeof(bool), typeof(CustomCard),
            new FrameworkPropertyMetadata(false, (d, e) => ((CustomCard)d).Focusable = (bool)e.NewValue));

        [Category(BehaviorCategory), Description("true면 hover 효과 + Click 이벤트 발생")]
        public bool IsClickable
        {
            get => (bool)GetValue(IsClickableProperty);
            set => SetValue(IsClickableProperty, value);
        }

        public static readonly DependencyProperty ShowChevronProperty = DependencyProperty.Register(
            nameof(ShowChevron), typeof(bool), typeof(CustomCard), new FrameworkPropertyMetadata(true));

        [Category(BehaviorCategory), Description("클릭 가능할 때 오른쪽 > 표시")]
        public bool ShowChevron
        {
            get => (bool)GetValue(ShowChevronProperty);
            set => SetValue(ShowChevronProperty, value);
        }

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            nameof(Command), typeof(ICommand), typeof(CustomCard), new FrameworkPropertyMetadata(null));

        [Category(BehaviorCategory), Description("클릭 시 실행할 Command (ViewModel 연결용)")]
        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            nameof(CommandParameter), typeof(object), typeof(CustomCard), new FrameworkPropertyMetadata(null));

        [Category(BehaviorCategory)]
        public object? CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        private static readonly DependencyPropertyKey IsPressedPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(IsPressed), typeof(bool), typeof(CustomCard), new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsPressedProperty = IsPressedPropertyKey.DependencyProperty;

        /// <summary>마우스로 누르고 있는 중인지(템플릿 트리거용, 읽기 전용).</summary>
        [Browsable(false)]
        public bool IsPressed => (bool)GetValue(IsPressedProperty);

        #endregion

        #region Input handling

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            // WHY: 카드 안의 체크박스 등이 이미 처리한 클릭(Handled)은 카드 클릭으로 취급하지 않는다.
            if (e.Handled || !IsClickable || !IsEnabled) return;

            // WHY: 캡처해야 누른 채 카드 밖으로 나갔다 떼는 경우를 "취소"로 정확히 판단할 수 있다.
            //      캡처에 실패하면(다른 창이 마우스를 잡고 있는 등) 눌림 상태로 두지 않는다 — 떼는 이벤트를 못 받아 눌린 채 굳기 때문.
            if (!CaptureMouse()) return;
            SetValue(IsPressedPropertyKey, true);
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);
            if (!IsMouseCaptured) return;

            var position = e.GetPosition(this);
            var releasedInside = position.X >= 0 && position.Y >= 0 && position.X <= ActualWidth && position.Y <= ActualHeight;

            ReleaseMouseCapture();
            SetValue(IsPressedPropertyKey, false);
            e.Handled = true;

            if (releasedInside) RaiseClick();
        }

        protected override void OnLostMouseCapture(MouseEventArgs e)
        {
            base.OnLostMouseCapture(e);
            SetValue(IsPressedPropertyKey, false);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Handled || !IsClickable || !IsEnabled) return;
            if (e.Key != Key.Enter && e.Key != Key.Space) return;

            RaiseClick();
            e.Handled = true;
        }

        private void RaiseClick()
        {
            RaiseEvent(new RoutedEventArgs(ClickEvent, this));

            var command = Command;
            if (command != null && command.CanExecute(CommandParameter))
                command.Execute(CommandParameter);
        }

        #endregion
    }
}
