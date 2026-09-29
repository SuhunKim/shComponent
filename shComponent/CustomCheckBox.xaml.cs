using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using shComponent.Theming;

namespace shComponent
{
    /// <summary>Windows 11 스타일 체크박스. CheckBox를 상속하므로 IsChecked/Checked/Unchecked/Command 를 그대로 사용.</summary>
    [ToolboxItem(true)]
    [Description("Windows 11 스타일 체크박스")]
    public partial class CustomCheckBox : UserControl
    {
        private const string Category = "sh 모양";
        private const string BehaviorCategory = "sh 동작";

        static CustomCheckBox()
        {
            // WHY: UserControl 기본 테마 스타일(Border 템플릿)이 배경/테두리를 따로 그리지 않도록 스타일 조회를 끊고 ContentPresenter만 남긴다.
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomCheckBox), new FrameworkPropertyMetadata(typeof(CustomCheckBox)));
            TemplateProperty.OverrideMetadata(typeof(CustomCheckBox), new FrameworkPropertyMetadata(ShBareTemplate.Instance));
            FocusableProperty.OverrideMetadata(typeof(CustomCheckBox), new FrameworkPropertyMetadata(false));
            BackgroundProperty.OverrideMetadata(typeof(CustomCheckBox), new FrameworkPropertyMetadata(ShPalette.ControlBackground));
            BorderBrushProperty.OverrideMetadata(typeof(CustomCheckBox), new FrameworkPropertyMetadata(ShPalette.ControlStrongBorder));
            BorderThicknessProperty.OverrideMetadata(typeof(CustomCheckBox), new FrameworkPropertyMetadata(new Thickness(1)));
            ForegroundProperty.OverrideMetadata(typeof(CustomCheckBox), new FrameworkPropertyMetadata(ShPalette.TextPrimary, FrameworkPropertyMetadataOptions.Inherits));
            FontFamilyProperty.OverrideMetadata(typeof(CustomCheckBox), new FrameworkPropertyMetadata(ShPalette.TextFont, FrameworkPropertyMetadataOptions.Inherits));
            FontSizeProperty.OverrideMetadata(typeof(CustomCheckBox), new FrameworkPropertyMetadata(ShPalette.BodyFontSize, FrameworkPropertyMetadataOptions.Inherits));
            PaddingProperty.OverrideMetadata(typeof(CustomCheckBox), new FrameworkPropertyMetadata(new Thickness(8, 0, 0, 0)));
            MinHeightProperty.OverrideMetadata(typeof(CustomCheckBox), new FrameworkPropertyMetadata(32d));
        }

        public CustomCheckBox()
        {
            InitializeComponent();
        }

        #region Events

        // WHY: 안쪽 CheckBox의 라우티드 이벤트를 AddOwner로 함께 등록해, 위로 버블링된 이벤트를 이 컨트롤에서 그대로 받는다.
        public static readonly RoutedEvent CheckedEvent = ToggleButton.CheckedEvent.AddOwner(typeof(CustomCheckBox));
        public static readonly RoutedEvent UncheckedEvent = ToggleButton.UncheckedEvent.AddOwner(typeof(CustomCheckBox));

        [Category(BehaviorCategory)]
        public event RoutedEventHandler Checked
        {
            add => AddHandler(CheckedEvent, value);
            remove => RemoveHandler(CheckedEvent, value);
        }

        [Category(BehaviorCategory)]
        public event RoutedEventHandler Unchecked
        {
            add => AddHandler(UncheckedEvent, value);
            remove => RemoveHandler(UncheckedEvent, value);
        }

        public static readonly RoutedEvent IndeterminateEvent = ToggleButton.IndeterminateEvent.AddOwner(typeof(CustomCheckBox));

        [Category(BehaviorCategory)]
        public event RoutedEventHandler Indeterminate
        {
            add => AddHandler(IndeterminateEvent, value);
            remove => RemoveHandler(IndeterminateEvent, value);
        }

        public static readonly RoutedEvent ClickEvent = ButtonBase.ClickEvent.AddOwner(typeof(CustomCheckBox));

        [Category(BehaviorCategory)]
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        #endregion

        #region Dependency Properties

        // WHY: UserControl.Content는 XAML 안쪽 CheckBox가 차지하므로, 사용자가 쓰는 Content(체크박스 글자)는 별도 속성으로 가려 둔다.
        //      XAML의 Content="..." 및 <sh:CustomCheckBox>글자</sh:CustomCheckBox> 는 이 속성으로 들어온다.
        public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            nameof(Content), typeof(object), typeof(CustomCheckBox), new FrameworkPropertyMetadata(null));

        [Category(Category), Description("체크박스 글자")]
        public new object? Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            nameof(IsChecked), typeof(bool?), typeof(CustomCheckBox),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        [Category(BehaviorCategory), Description("체크 상태(true / false / null=불확정)")]
        public bool? IsChecked
        {
            get => (bool?)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public static readonly DependencyProperty IsThreeStateProperty = DependencyProperty.Register(
            nameof(IsThreeState), typeof(bool), typeof(CustomCheckBox), new FrameworkPropertyMetadata(false));

        [Category(BehaviorCategory), Description("true면 체크 / 해제 / 불확정 3상태")]
        public bool IsThreeState
        {
            get => (bool)GetValue(IsThreeStateProperty);
            set => SetValue(IsThreeStateProperty, value);
        }

        public static readonly DependencyProperty AccentBrushProperty = DependencyProperty.Register(
            nameof(AccentBrush), typeof(Brush), typeof(CustomCheckBox), new FrameworkPropertyMetadata(ShPalette.Accent));

        [Category(Category), Description("체크됐을 때 상자 색")]
        public Brush AccentBrush
        {
            get => (Brush)GetValue(AccentBrushProperty);
            set => SetValue(AccentBrushProperty, value);
        }

        public static readonly DependencyProperty CheckGlyphBrushProperty = DependencyProperty.Register(
            nameof(CheckGlyphBrush), typeof(Brush), typeof(CustomCheckBox), new FrameworkPropertyMetadata(ShPalette.OnAccent));

        [Category(Category), Description("체크 표시(✓) 색")]
        public Brush CheckGlyphBrush
        {
            get => (Brush)GetValue(CheckGlyphBrushProperty);
            set => SetValue(CheckGlyphBrushProperty, value);
        }

        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register(
            nameof(HoverBackground), typeof(Brush), typeof(CustomCheckBox), new FrameworkPropertyMetadata(ShPalette.ControlHover));

        [Category(Category), Description("체크 안 된 상태에서 마우스를 올렸을 때 상자 배경")]
        public Brush HoverBackground
        {
            get => (Brush)GetValue(HoverBackgroundProperty);
            set => SetValue(HoverBackgroundProperty, value);
        }

        public static readonly DependencyProperty BoxCornerRadiusProperty = DependencyProperty.Register(
            nameof(BoxCornerRadius), typeof(CornerRadius), typeof(CustomCheckBox), new FrameworkPropertyMetadata(new CornerRadius(4)));

        [Category(Category), Description("상자 모서리 둥글기")]
        public CornerRadius BoxCornerRadius
        {
            get => (CornerRadius)GetValue(BoxCornerRadiusProperty);
            set => SetValue(BoxCornerRadiusProperty, value);
        }

        public static readonly DependencyProperty BoxSizeProperty = DependencyProperty.Register(
            nameof(BoxSize), typeof(double), typeof(CustomCheckBox), new FrameworkPropertyMetadata(20d));

        [Category(Category), Description("상자 크기(px)")]
        public double BoxSize
        {
            get => (double)GetValue(BoxSizeProperty);
            set => SetValue(BoxSizeProperty, value);
        }

        #endregion
    }
}
