using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using shComponent.Theming;

namespace shComponent
{
    /// <summary>
    /// Windows 11 톤의 둥근 버튼. 안쪽에 진짜 Button을 두므로 Click 이벤트 / Command 를 평소처럼 사용한다.
    /// 모든 모양 속성은 의존성 프로퍼티라 XAML 디자이너 속성 창에서 바로 바꾸고 결과를 볼 수 있다.
    /// </summary>
    [ToolboxItem(true)]
    [Description("Windows 11 스타일 버튼 (Standard / Accent / Subtle)")]
    public partial class CustomButton : UserControl
    {
        private const string Category = "sh 모양";
        private const string BehaviorCategory = "sh 동작";

        static CustomButton()
        {
            // WHY: UserControl 기본 테마 스타일(Border 템플릿)이 배경/테두리를 따로 그리지 않도록 스타일 조회를 끊고,
            //      기본 템플릿을 "ContentPresenter만" 템플릿으로 바꾼다(ShBareTemplate 참고).
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomButton), new FrameworkPropertyMetadata(typeof(CustomButton)));
            TemplateProperty.OverrideMetadata(typeof(CustomButton), new FrameworkPropertyMetadata(ShBareTemplate.Instance));
            FocusableProperty.OverrideMetadata(typeof(CustomButton), new FrameworkPropertyMetadata(false));
            BackgroundProperty.OverrideMetadata(typeof(CustomButton), new FrameworkPropertyMetadata(ShPalette.ControlBackground));
            BorderBrushProperty.OverrideMetadata(typeof(CustomButton), new FrameworkPropertyMetadata(ShPalette.ControlBorder));
            BorderThicknessProperty.OverrideMetadata(typeof(CustomButton), new FrameworkPropertyMetadata(new Thickness(1)));
            ForegroundProperty.OverrideMetadata(typeof(CustomButton), new FrameworkPropertyMetadata(ShPalette.TextPrimary, FrameworkPropertyMetadataOptions.Inherits));
            FontFamilyProperty.OverrideMetadata(typeof(CustomButton), new FrameworkPropertyMetadata(ShPalette.TextFont, FrameworkPropertyMetadataOptions.Inherits));
            FontSizeProperty.OverrideMetadata(typeof(CustomButton), new FrameworkPropertyMetadata(ShPalette.BodyFontSize, FrameworkPropertyMetadataOptions.Inherits));
            PaddingProperty.OverrideMetadata(typeof(CustomButton), new FrameworkPropertyMetadata(new Thickness(14, 6, 14, 6)));
            MinHeightProperty.OverrideMetadata(typeof(CustomButton), new FrameworkPropertyMetadata(32d));
            HorizontalContentAlignmentProperty.OverrideMetadata(typeof(CustomButton), new FrameworkPropertyMetadata(HorizontalAlignment.Center));
            VerticalContentAlignmentProperty.OverrideMetadata(typeof(CustomButton), new FrameworkPropertyMetadata(VerticalAlignment.Center));
        }

        public CustomButton()
        {
            InitializeComponent();
        }

        #region Events

        /// <summary>안쪽 Button의 Click과 같은 라우티드 이벤트(AddOwner) — 그대로 위로 버블링되어 이 컨트롤에서 받을 수 있다.</summary>
        public static readonly RoutedEvent ClickEvent = ButtonBase.ClickEvent.AddOwner(typeof(CustomButton));

        [Category(BehaviorCategory)]
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        #endregion

        #region Dependency Properties

        // WHY: UserControl.Content는 XAML 안쪽 Button이 차지하므로, 사용자가 쓰는 Content(버튼 글자)는 별도 속성으로 가려 둔다.
        //      XAML의 Content="..." 및 <sh:CustomButton>글자</sh:CustomButton> 는 이 속성으로 들어온다.
        public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            nameof(Content), typeof(object), typeof(CustomButton), new FrameworkPropertyMetadata(null));

        [Category(Category), Description("버튼에 표시할 내용(글자 등)")]
        public new object? Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            nameof(Command), typeof(ICommand), typeof(CustomButton), new FrameworkPropertyMetadata(null));

        [Category(BehaviorCategory), Description("클릭 시 실행할 Command (ViewModel 연결용)")]
        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register(
            nameof(CommandParameter), typeof(object), typeof(CustomButton), new FrameworkPropertyMetadata(null));

        [Category(BehaviorCategory)]
        public object? CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public static readonly DependencyProperty IsDefaultProperty = DependencyProperty.Register(
            nameof(IsDefault), typeof(bool), typeof(CustomButton), new FrameworkPropertyMetadata(false));

        [Category(BehaviorCategory), Description("Enter 키로 눌리는 기본 버튼")]
        public bool IsDefault
        {
            get => (bool)GetValue(IsDefaultProperty);
            set => SetValue(IsDefaultProperty, value);
        }

        public static readonly DependencyProperty AppearanceProperty = DependencyProperty.Register(
            nameof(Appearance), typeof(ButtonAppearance), typeof(CustomButton),
            new FrameworkPropertyMetadata(ButtonAppearance.Standard));

        /// <summary>Standard(기본) / Accent(주요 동작) / Subtle(배경 없음).</summary>
        [Category(Category), Description("버튼 종류: Standard / Accent / Subtle")]
        public ButtonAppearance Appearance
        {
            get => (ButtonAppearance)GetValue(AppearanceProperty);
            set => SetValue(AppearanceProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(CustomButton),
            new FrameworkPropertyMetadata(ShPalette.ControlCornerRadius));

        [Category(Category), Description("모서리 둥글기")]
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register(
            nameof(HoverBackground), typeof(Brush), typeof(CustomButton),
            new FrameworkPropertyMetadata(ShPalette.ControlHover));

        [Category(Category), Description("마우스를 올렸을 때 배경 (Standard/Subtle)")]
        public Brush HoverBackground
        {
            get => (Brush)GetValue(HoverBackgroundProperty);
            set => SetValue(HoverBackgroundProperty, value);
        }

        public static readonly DependencyProperty PressedBackgroundProperty = DependencyProperty.Register(
            nameof(PressedBackground), typeof(Brush), typeof(CustomButton),
            new FrameworkPropertyMetadata(ShPalette.ControlPressed));

        [Category(Category), Description("눌렀을 때 배경 (Standard/Subtle)")]
        public Brush PressedBackground
        {
            get => (Brush)GetValue(PressedBackgroundProperty);
            set => SetValue(PressedBackgroundProperty, value);
        }

        public static readonly DependencyProperty AccentBrushProperty = DependencyProperty.Register(
            nameof(AccentBrush), typeof(Brush), typeof(CustomButton),
            new FrameworkPropertyMetadata(ShPalette.Accent));

        [Category(Category), Description("Accent 모드 배경색")]
        public Brush AccentBrush
        {
            get => (Brush)GetValue(AccentBrushProperty);
            set => SetValue(AccentBrushProperty, value);
        }

        public static readonly DependencyProperty AccentForegroundProperty = DependencyProperty.Register(
            nameof(AccentForeground), typeof(Brush), typeof(CustomButton),
            new FrameworkPropertyMetadata(ShPalette.OnAccent));

        [Category(Category), Description("Accent 모드 글자색")]
        public Brush AccentForeground
        {
            get => (Brush)GetValue(AccentForegroundProperty);
            set => SetValue(AccentForegroundProperty, value);
        }

        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register(
            nameof(Glyph), typeof(string), typeof(CustomButton),
            new FrameworkPropertyMetadata(null, null, CoerceEmptyToNull));

        /// <summary>Segoe Fluent Icons 글리프(예: &amp;#xE710; = 추가). 비우면 아이콘 없음.</summary>
        [Category(Category), Description("아이콘 글리프 (Segoe Fluent Icons, 예: &#xE710;)")]
        public string? Glyph
        {
            get => (string?)GetValue(GlyphProperty);
            set => SetValue(GlyphProperty, value);
        }

        public static readonly DependencyProperty GlyphSizeProperty = DependencyProperty.Register(
            nameof(GlyphSize), typeof(double), typeof(CustomButton),
            new FrameworkPropertyMetadata(14d));

        [Category(Category), Description("아이콘 크기")]
        public double GlyphSize
        {
            get => (double)GetValue(GlyphSizeProperty);
            set => SetValue(GlyphSizeProperty, value);
        }

        // WHY: 디자이너에서 Glyph 값을 지우면 null이 아니라 ""가 들어온다. 트리거는 null만 보므로 ""를 null로 통일한다.
        internal static object? CoerceEmptyToNull(DependencyObject d, object? value)
            => value is string s && s.Length == 0 ? null : value;

        #endregion
    }
}
