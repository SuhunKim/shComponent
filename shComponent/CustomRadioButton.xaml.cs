using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using shComponent.Theming;

namespace shComponent
{
    /// <summary>Windows 11 스타일 라디오 버튼. RadioButton을 상속하므로 GroupName/IsChecked 를 그대로 사용.</summary>
    [ToolboxItem(true)]
    [Description("Windows 11 스타일 라디오 버튼")]
    public partial class CustomRadioButton : UserControl
    {
        private const string Category = "sh 모양";
        private const string BehaviorCategory = "sh 동작";

        static CustomRadioButton()
        {
            // WHY: UserControl 기본 테마 스타일(Border 템플릿)이 배경/테두리를 따로 그리지 않도록 스타일 조회를 끊고 ContentPresenter만 남긴다.
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomRadioButton), new FrameworkPropertyMetadata(typeof(CustomRadioButton)));
            TemplateProperty.OverrideMetadata(typeof(CustomRadioButton), new FrameworkPropertyMetadata(ShBareTemplate.Instance));
            FocusableProperty.OverrideMetadata(typeof(CustomRadioButton), new FrameworkPropertyMetadata(false));
            BackgroundProperty.OverrideMetadata(typeof(CustomRadioButton), new FrameworkPropertyMetadata(ShPalette.ControlBackground));
            BorderBrushProperty.OverrideMetadata(typeof(CustomRadioButton), new FrameworkPropertyMetadata(ShPalette.ControlStrongBorder));
            ForegroundProperty.OverrideMetadata(typeof(CustomRadioButton), new FrameworkPropertyMetadata(ShPalette.TextPrimary, FrameworkPropertyMetadataOptions.Inherits));
            FontFamilyProperty.OverrideMetadata(typeof(CustomRadioButton), new FrameworkPropertyMetadata(ShPalette.TextFont, FrameworkPropertyMetadataOptions.Inherits));
            FontSizeProperty.OverrideMetadata(typeof(CustomRadioButton), new FrameworkPropertyMetadata(ShPalette.BodyFontSize, FrameworkPropertyMetadataOptions.Inherits));
            PaddingProperty.OverrideMetadata(typeof(CustomRadioButton), new FrameworkPropertyMetadata(new Thickness(8, 0, 0, 0)));
            MinHeightProperty.OverrideMetadata(typeof(CustomRadioButton), new FrameworkPropertyMetadata(32d));
        }

        public CustomRadioButton()
        {
            InitializeComponent();
        }

        #region Events

        // WHY: 안쪽 RadioButton의 라우티드 이벤트를 AddOwner로 함께 등록해, 위로 버블링된 이벤트를 이 컨트롤에서 그대로 받는다.
        public static readonly RoutedEvent CheckedEvent = ToggleButton.CheckedEvent.AddOwner(typeof(CustomRadioButton));
        public static readonly RoutedEvent UncheckedEvent = ToggleButton.UncheckedEvent.AddOwner(typeof(CustomRadioButton));

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

        public static readonly RoutedEvent ClickEvent = ButtonBase.ClickEvent.AddOwner(typeof(CustomRadioButton));

        [Category(BehaviorCategory)]
        public event RoutedEventHandler Click
        {
            add => AddHandler(ClickEvent, value);
            remove => RemoveHandler(ClickEvent, value);
        }

        #endregion

        #region Dependency Properties

        // WHY: UserControl.Content는 XAML 안쪽 RadioButton가 차지하므로, 사용자가 쓰는 Content(라디오 버튼 글자)는 별도 속성으로 가려 둔다.
        //      XAML의 Content="..." 및 <sh:CustomRadioButton>글자</sh:CustomRadioButton> 는 이 속성으로 들어온다.
        public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            nameof(Content), typeof(object), typeof(CustomRadioButton), new FrameworkPropertyMetadata(null));

        [Category(Category), Description("라디오 버튼 글자")]
        public new object? Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            nameof(IsChecked), typeof(bool?), typeof(CustomRadioButton),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        [Category(BehaviorCategory), Description("선택 여부")]
        public bool? IsChecked
        {
            get => (bool?)GetValue(IsCheckedProperty);
            set => SetValue(IsCheckedProperty, value);
        }

        public static readonly DependencyProperty GroupNameProperty = DependencyProperty.Register(
            nameof(GroupName), typeof(string), typeof(CustomRadioButton), new FrameworkPropertyMetadata(string.Empty));

        [Category(BehaviorCategory), Description("같은 GroupName끼리는 하나만 선택된다")]
        public string GroupName
        {
            get => (string)GetValue(GroupNameProperty);
            set => SetValue(GroupNameProperty, value);
        }

        public static readonly DependencyProperty AccentBrushProperty = DependencyProperty.Register(
            nameof(AccentBrush), typeof(Brush), typeof(CustomRadioButton), new FrameworkPropertyMetadata(ShPalette.Accent));

        [Category(Category), Description("선택됐을 때 원 색")]
        public Brush AccentBrush
        {
            get => (Brush)GetValue(AccentBrushProperty);
            set => SetValue(AccentBrushProperty, value);
        }

        public static readonly DependencyProperty DotBrushProperty = DependencyProperty.Register(
            nameof(DotBrush), typeof(Brush), typeof(CustomRadioButton), new FrameworkPropertyMetadata(ShPalette.OnAccent));

        [Category(Category), Description("선택 표시(가운데 점) 색")]
        public Brush DotBrush
        {
            get => (Brush)GetValue(DotBrushProperty);
            set => SetValue(DotBrushProperty, value);
        }

        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register(
            nameof(HoverBackground), typeof(Brush), typeof(CustomRadioButton), new FrameworkPropertyMetadata(ShPalette.ControlHover));

        [Category(Category), Description("선택 안 된 상태에서 마우스를 올렸을 때 원 배경")]
        public Brush HoverBackground
        {
            get => (Brush)GetValue(HoverBackgroundProperty);
            set => SetValue(HoverBackgroundProperty, value);
        }

        public static readonly DependencyProperty CircleSizeProperty = DependencyProperty.Register(
            nameof(CircleSize), typeof(double), typeof(CustomRadioButton), new FrameworkPropertyMetadata(20d));

        [Category(Category), Description("원 크기(px)")]
        public double CircleSize
        {
            get => (double)GetValue(CircleSizeProperty);
            set => SetValue(CircleSizeProperty, value);
        }

        #endregion
    }
}
