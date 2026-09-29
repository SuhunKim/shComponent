using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using shComponent.Theming;

namespace shComponent
{
    /// <summary>
    /// Windows 11 스타일 콤보박스(선택 전용). 안쪽에 진짜 ComboBox를 두므로 Items.Add / SelectedItem / SelectionChanged 를 평소처럼 사용한다.
    /// </summary>
    [ToolboxItem(true)]
    [Description("Windows 11 스타일 콤보박스")]
    [ContentProperty(nameof(Items))]
    public partial class CustomComboBox : UserControl
    {
        private const string Category = "sh 모양";
        private const string BehaviorCategory = "sh 동작";

        // 드롭다운 팝업 안이 DynamicResource로 참조하는 키.
        private const string AccentKey = "sh.Combo.Accent";
        private const string ItemHoverKey = "sh.Combo.ItemHover";
        private const string ItemSelectedKey = "sh.Combo.ItemSelected";
        private const string PopupBackgroundKey = "sh.Combo.PopupBackground";

        // 안쪽 ComboBox ↔ 이 컨트롤의 선택 속성을 서로 밀어 넣을 때 무한 반복을 막는 표시.
        private bool _syncing;
        // XAML 속성(SelectedIndex="1")이 자식 항목(<ComboBoxItem>)보다 먼저 적용되는 경우, 항목이 생길 때까지 보관한다.
        private int _pendingIndex = -1;
        private object? _pendingItem;

        static CustomComboBox()
        {
            // WHY: UserControl 기본 테마 스타일(Border 템플릿)이 배경/테두리를 따로 그리지 않도록 스타일 조회를 끊고 ContentPresenter만 남긴다.
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomComboBox), new FrameworkPropertyMetadata(typeof(CustomComboBox)));
            TemplateProperty.OverrideMetadata(typeof(CustomComboBox), new FrameworkPropertyMetadata(ShBareTemplate.Instance));
            FocusableProperty.OverrideMetadata(typeof(CustomComboBox), new FrameworkPropertyMetadata(false));
            BackgroundProperty.OverrideMetadata(typeof(CustomComboBox), new FrameworkPropertyMetadata(ShPalette.ControlBackground));
            BorderBrushProperty.OverrideMetadata(typeof(CustomComboBox), new FrameworkPropertyMetadata(ShPalette.ControlBorder));
            BorderThicknessProperty.OverrideMetadata(typeof(CustomComboBox), new FrameworkPropertyMetadata(new Thickness(1)));
            ForegroundProperty.OverrideMetadata(typeof(CustomComboBox), new FrameworkPropertyMetadata(ShPalette.TextPrimary, FrameworkPropertyMetadataOptions.Inherits));
            FontFamilyProperty.OverrideMetadata(typeof(CustomComboBox), new FrameworkPropertyMetadata(ShPalette.TextFont, FrameworkPropertyMetadataOptions.Inherits));
            FontSizeProperty.OverrideMetadata(typeof(CustomComboBox), new FrameworkPropertyMetadata(ShPalette.BodyFontSize, FrameworkPropertyMetadataOptions.Inherits));
            PaddingProperty.OverrideMetadata(typeof(CustomComboBox), new FrameworkPropertyMetadata(new Thickness(12, 5, 12, 6)));
            MinHeightProperty.OverrideMetadata(typeof(CustomComboBox), new FrameworkPropertyMetadata(32d));
            MinWidthProperty.OverrideMetadata(typeof(CustomComboBox), new FrameworkPropertyMetadata(120d));
            HorizontalContentAlignmentProperty.OverrideMetadata(typeof(CustomComboBox), new FrameworkPropertyMetadata(HorizontalAlignment.Left));
        }

        public CustomComboBox()
        {
            InitializeComponent();
            ApplyPopupResources();

            ((INotifyCollectionChanged)Combo.Items).CollectionChanged += (_, __) => ApplyPending();
            Combo.SelectionChanged += OnInnerSelectionChanged;
            Loaded += (_, __) => ApplyPending();
        }

        #region Events

        /// <summary>안쪽 ComboBox의 SelectionChanged와 같은 라우티드 이벤트(AddOwner) — 위로 버블링되어 이 컨트롤에서 받는다.</summary>
        public static readonly RoutedEvent SelectionChangedEvent = Selector.SelectionChangedEvent.AddOwner(typeof(CustomComboBox));

        [Category(BehaviorCategory)]
        public event SelectionChangedEventHandler SelectionChanged
        {
            add => AddHandler(SelectionChangedEvent, value);
            remove => RemoveHandler(SelectionChangedEvent, value);
        }

        #endregion

        #region Items / selection

        /// <summary>항목 목록. XAML 자식(&lt;ComboBoxItem&gt;)과 코드의 Items.Add 가 여기로 들어간다.</summary>
        [Category(BehaviorCategory), Description("드롭다운 항목")]
        public ItemCollection Items => Combo.Items;

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(IEnumerable), typeof(CustomComboBox),
            new FrameworkPropertyMetadata(null, (d, e) => ((CustomComboBox)d).Combo.ItemsSource = (IEnumerable?)e.NewValue));

        [Category(BehaviorCategory), Description("항목 목록을 바인딩할 컬렉션")]
        public IEnumerable? ItemsSource
        {
            get => (IEnumerable?)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(
            nameof(SelectedIndex), typeof(int), typeof(CustomComboBox),
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (d, e) => ((CustomComboBox)d).OnSelectedIndexChanged((int)e.NewValue)));

        [Category(BehaviorCategory), Description("선택된 항목의 위치(-1=선택 없음)")]
        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem), typeof(object), typeof(CustomComboBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (d, e) => ((CustomComboBox)d).OnSelectedItemChanged(e.NewValue)));

        [Category(BehaviorCategory), Description("선택된 항목")]
        public object? SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register(
            nameof(SelectedValue), typeof(object), typeof(CustomComboBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (d, e) => { var c = (CustomComboBox)d; if (!c._syncing) c.Combo.SelectedValue = e.NewValue; }));

        [Category(BehaviorCategory), Description("SelectedValuePath로 뽑은 선택 값")]
        public object? SelectedValue
        {
            get => GetValue(SelectedValueProperty);
            set => SetValue(SelectedValueProperty, value);
        }

        public static readonly DependencyProperty SelectedValuePathProperty = DependencyProperty.Register(
            nameof(SelectedValuePath), typeof(string), typeof(CustomComboBox), new FrameworkPropertyMetadata(string.Empty));

        [Category(BehaviorCategory)]
        public string SelectedValuePath
        {
            get => (string)GetValue(SelectedValuePathProperty);
            set => SetValue(SelectedValuePathProperty, value);
        }

        public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register(
            nameof(DisplayMemberPath), typeof(string), typeof(CustomComboBox), new FrameworkPropertyMetadata(string.Empty));

        [Category(BehaviorCategory)]
        public string DisplayMemberPath
        {
            get => (string)GetValue(DisplayMemberPathProperty);
            set => SetValue(DisplayMemberPathProperty, value);
        }

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            nameof(IsDropDownOpen), typeof(bool), typeof(CustomComboBox),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        [Category(BehaviorCategory), Description("드롭다운이 열려 있는지")]
        public bool IsDropDownOpen
        {
            get => (bool)GetValue(IsDropDownOpenProperty);
            set => SetValue(IsDropDownOpenProperty, value);
        }

        public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register(
            nameof(MaxDropDownHeight), typeof(double), typeof(CustomComboBox),
            new FrameworkPropertyMetadata(SystemParameters.PrimaryScreenHeight / 3));

        [Category(BehaviorCategory), Description("드롭다운 목록의 최대 높이")]
        public double MaxDropDownHeight
        {
            get => (double)GetValue(MaxDropDownHeightProperty);
            set => SetValue(MaxDropDownHeightProperty, value);
        }

        private void OnSelectedIndexChanged(int index)
        {
            if (_syncing) return;
            if (index < 0 || index < Combo.Items.Count)
            {
                _pendingIndex = -1;
                Combo.SelectedIndex = index;
            }
            else
            {
                _pendingIndex = index; // 항목이 아직 없다 → 항목이 채워질 때 적용
            }
        }

        private void OnSelectedItemChanged(object? item)
        {
            if (_syncing) return;
            if (item == null || Combo.Items.Contains(item))
            {
                _pendingItem = null;
                Combo.SelectedItem = item;
            }
            else
            {
                _pendingItem = item;
            }
        }

        private void ApplyPending()
        {
            if (_pendingIndex >= 0 && _pendingIndex < Combo.Items.Count)
            {
                var index = _pendingIndex;
                _pendingIndex = -1;
                Combo.SelectedIndex = index;
            }
            else if (_pendingItem != null && Combo.Items.Contains(_pendingItem))
            {
                var item = _pendingItem;
                _pendingItem = null;
                Combo.SelectedItem = item;
            }
        }

        private void OnInnerSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _syncing = true;
            try
            {
                SelectedIndex = Combo.SelectedIndex;
                SelectedItem = Combo.SelectedItem;
                SelectedValue = Combo.SelectedValue;
            }
            finally
            {
                _syncing = false;
            }
        }

        #endregion

        #region Dependency Properties

        // WHY: UserControl.Content는 XAML 안쪽 ComboBox가 차지한다. 이 컨트롤의 XAML 자식은 Content가 아니라 Items로 들어가므로(ContentProperty),
        //      Content 속성은 사용하지 않는다.

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(CustomComboBox), new FrameworkPropertyMetadata(ShPalette.ControlCornerRadius));

        [Category(Category), Description("모서리 둥글기")]
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register(
            nameof(HoverBackground), typeof(Brush), typeof(CustomComboBox), new FrameworkPropertyMetadata(ShPalette.ControlHover));

        [Category(Category), Description("마우스를 올렸을 때 배경")]
        public Brush HoverBackground
        {
            get => (Brush)GetValue(HoverBackgroundProperty);
            set => SetValue(HoverBackgroundProperty, value);
        }

        public static readonly DependencyProperty PressedBackgroundProperty = DependencyProperty.Register(
            nameof(PressedBackground), typeof(Brush), typeof(CustomComboBox), new FrameworkPropertyMetadata(ShPalette.ControlPressed));

        [Category(Category), Description("드롭다운이 열려 있을 때 배경")]
        public Brush PressedBackground
        {
            get => (Brush)GetValue(PressedBackgroundProperty);
            set => SetValue(PressedBackgroundProperty, value);
        }

        public static readonly DependencyProperty PopupBackgroundProperty = DependencyProperty.Register(
            nameof(PopupBackground), typeof(Brush), typeof(CustomComboBox),
            new FrameworkPropertyMetadata(ShPalette.PopupBackground, OnPopupResourceChanged));

        [Category(Category), Description("드롭다운 목록 배경")]
        public Brush PopupBackground
        {
            get => (Brush)GetValue(PopupBackgroundProperty);
            set => SetValue(PopupBackgroundProperty, value);
        }

        public static readonly DependencyProperty AccentBrushProperty = DependencyProperty.Register(
            nameof(AccentBrush), typeof(Brush), typeof(CustomComboBox),
            new FrameworkPropertyMetadata(ShPalette.Accent, OnPopupResourceChanged));

        [Category(Category), Description("선택 항목 왼쪽 표시줄 색")]
        public Brush AccentBrush
        {
            get => (Brush)GetValue(AccentBrushProperty);
            set => SetValue(AccentBrushProperty, value);
        }

        public static readonly DependencyProperty ItemHoverBackgroundProperty = DependencyProperty.Register(
            nameof(ItemHoverBackground), typeof(Brush), typeof(CustomComboBox),
            new FrameworkPropertyMetadata(ShPalette.NavHover, OnPopupResourceChanged));

        [Category(Category), Description("드롭다운 항목 hover 배경")]
        public Brush ItemHoverBackground
        {
            get => (Brush)GetValue(ItemHoverBackgroundProperty);
            set => SetValue(ItemHoverBackgroundProperty, value);
        }

        public static readonly DependencyProperty ItemSelectedBackgroundProperty = DependencyProperty.Register(
            nameof(ItemSelectedBackground), typeof(Brush), typeof(CustomComboBox),
            new FrameworkPropertyMetadata(ShPalette.NavSelected, OnPopupResourceChanged));

        [Category(Category), Description("드롭다운 선택 항목 배경")]
        public Brush ItemSelectedBackground
        {
            get => (Brush)GetValue(ItemSelectedBackgroundProperty);
            set => SetValue(ItemSelectedBackgroundProperty, value);
        }

        public static readonly DependencyProperty PlaceholderTextProperty = DependencyProperty.Register(
            nameof(PlaceholderText), typeof(string), typeof(CustomComboBox), new FrameworkPropertyMetadata(string.Empty));

        [Category(Category), Description("아무것도 선택되지 않았을 때 보이는 안내 문구")]
        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        #endregion

        private static void OnPopupResourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 생성자에서 InitializeComponent 전에 기본값 메타데이터가 먼저 호출될 수 있어 Combo 준비 여부로 거른다.
            if (((CustomComboBox)d).Combo != null) ((CustomComboBox)d).ApplyPopupResources();
        }

        /// <summary>드롭다운 팝업이 쓰는 색을 Resources에 명시적으로 기록한다.</summary>
        private void ApplyPopupResources()
        {
            Resources[AccentKey] = AccentBrush;
            Resources[ItemHoverKey] = ItemHoverBackground;
            Resources[ItemSelectedKey] = ItemSelectedBackground;
            Resources[PopupBackgroundKey] = PopupBackground;
        }
    }
}
