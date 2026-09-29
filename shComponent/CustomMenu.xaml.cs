using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using shComponent.Internal;
using shComponent.Navigation;
using shComponent.Theming;

namespace shComponent
{
    /// <summary>
    /// 1차 메뉴. 항목은 <see cref="SetItems"/>로 넣고, 사용자가 선택하면 <see cref="SelectionChanged"/> 이벤트와
    /// <see cref="SelectCommand"/>로 부모에게 알린다. 메뉴 자체는 "무엇을 할지" 모른다.
    /// </summary>
    [ToolboxItem(true)]
    [Description("1차 내비게이션 메뉴 (좌측 세로 / 상단 가로)")]
    public partial class CustomMenu : UserControl
    {
        private const string Category = "sh 모양";
        private const string BehaviorCategory = "sh 동작";

        private readonly NavButtonHost _host;

        public CustomMenu()
        {
            InitializeComponent();
            // WHY: XAML의 ItemsHost 자식은 디자이너 자리표시 항목이다. 실제 항목은 SetItems로만 채운다.
            ItemsHost.Children.Clear();
            _host = new NavButtonHost(ItemsHost, CreateItemButton);
            _host.SelectionChanged += OnHostSelectionChanged;

            ApplyResources();
            ApplyOrientation();

            if (DesignTimeData.IsDesignMode(this))
            {
                // WHY: 디자이너에서 빈 메뉴는 스타일 조정 결과를 볼 수 없으므로 샘플 항목을 보여 준다(런타임에는 실행 안 됨).
                SetItems(DesignTimeData.CreateMenu());
                Select(Items.FirstOrDefault(), notify: false);
            }
        }

        #region Public API

        /// <summary>사용자가 다른 항목을 선택했을 때(또는 Select(..., notify: true)) 발생.</summary>
        public event EventHandler<NavItemEventArgs>? SelectionChanged;

        [Browsable(false)]
        public IReadOnlyList<INavItem> Items => _host.Items;

        [Browsable(false)]
        public INavItem? SelectedItem => _host.SelectedItem;

        /// <summary>메뉴 항목을 교체한다. 선택은 초기화된다.</summary>
        public void SetItems(IEnumerable<INavItem>? items) => _host.SetItems(items);

        /// <summary>항목을 선택한다. notify=false 이면 이벤트 없이 화면 표시만 바꾼다(프로그램에서 상태 동기화용).</summary>
        public void Select(INavItem? item, bool notify = true) => _host.Select(item, notify);

        /// <summary>Key로 항목을 선택한다. 없으면 false.</summary>
        public bool Select(string key, bool notify = true)
        {
            var item = _host.FindByKey(key);
            if (item == null) return false;
            _host.Select(item, notify);
            return true;
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            nameof(Orientation), typeof(Orientation), typeof(CustomMenu),
            new FrameworkPropertyMetadata(Orientation.Vertical, (d, _) => ((CustomMenu)d).ApplyOrientation()));

        [Category(Category), Description("Vertical = 좌측 세로 메뉴, Horizontal = 상단 가로 메뉴")]
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty IsCompactProperty = DependencyProperty.Register(
            nameof(IsCompact), typeof(bool), typeof(CustomMenu), new FrameworkPropertyMetadata(false, OnItemLayoutChanged));

        [Category(Category), Description("true면 아이콘만 표시(좁은 메뉴). 제목은 툴팁으로 표시")]
        public bool IsCompact
        {
            get => (bool)GetValue(IsCompactProperty);
            set => SetValue(IsCompactProperty, value);
        }

        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
            nameof(ItemHeight), typeof(double), typeof(CustomMenu), new FrameworkPropertyMetadata(38d, OnItemLayoutChanged));

        [Category(Category), Description("메뉴 항목 높이")]
        public double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public static readonly DependencyProperty ItemFontSizeProperty = DependencyProperty.Register(
            nameof(ItemFontSize), typeof(double), typeof(CustomMenu), new FrameworkPropertyMetadata(14d, OnItemLayoutChanged));

        [Category(Category), Description("메뉴 글자 크기")]
        public double ItemFontSize
        {
            get => (double)GetValue(ItemFontSizeProperty);
            set => SetValue(ItemFontSizeProperty, value);
        }

        public static readonly DependencyProperty IconSizeProperty = DependencyProperty.Register(
            nameof(IconSize), typeof(double), typeof(CustomMenu), new FrameworkPropertyMetadata(16d, OnItemLayoutChanged));

        [Category(Category), Description("메뉴 아이콘 크기")]
        public double IconSize
        {
            get => (double)GetValue(IconSizeProperty);
            set => SetValue(IconSizeProperty, value);
        }

        public static readonly DependencyProperty ItemMarginProperty = DependencyProperty.Register(
            nameof(ItemMargin), typeof(Thickness), typeof(CustomMenu), new FrameworkPropertyMetadata(new Thickness(8, 2, 8, 2), OnItemLayoutChanged));

        [Category(Category), Description("메뉴 항목 바깥 여백(항목 간 간격)")]
        public Thickness ItemMargin
        {
            get => (Thickness)GetValue(ItemMarginProperty);
            set => SetValue(ItemMarginProperty, value);
        }

        public static readonly DependencyProperty ItemForegroundProperty = DependencyProperty.Register(
            nameof(ItemForeground), typeof(Brush), typeof(CustomMenu), new FrameworkPropertyMetadata(ShPalette.TextPrimary, OnItemLayoutChanged));

        [Category(Category), Description("메뉴 글자색")]
        public Brush ItemForeground
        {
            get => (Brush)GetValue(ItemForegroundProperty);
            set => SetValue(ItemForegroundProperty, value);
        }

        public static readonly DependencyProperty ItemCornerRadiusProperty = DependencyProperty.Register(
            nameof(ItemCornerRadius), typeof(CornerRadius), typeof(CustomMenu), new FrameworkPropertyMetadata(ShPalette.ControlCornerRadius, OnResourceChanged));

        [Category(Category), Description("메뉴 항목 모서리 둥글기")]
        public CornerRadius ItemCornerRadius
        {
            get => (CornerRadius)GetValue(ItemCornerRadiusProperty);
            set => SetValue(ItemCornerRadiusProperty, value);
        }

        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register(
            nameof(HoverBackground), typeof(Brush), typeof(CustomMenu), new FrameworkPropertyMetadata(ShPalette.NavHover, OnResourceChanged));

        [Category(Category), Description("마우스를 올렸을 때 항목 배경")]
        public Brush HoverBackground
        {
            get => (Brush)GetValue(HoverBackgroundProperty);
            set => SetValue(HoverBackgroundProperty, value);
        }

        public static readonly DependencyProperty PressedBackgroundProperty = DependencyProperty.Register(
            nameof(PressedBackground), typeof(Brush), typeof(CustomMenu), new FrameworkPropertyMetadata(ShPalette.NavPressed, OnResourceChanged));

        [Category(Category), Description("눌렀을 때 항목 배경")]
        public Brush PressedBackground
        {
            get => (Brush)GetValue(PressedBackgroundProperty);
            set => SetValue(PressedBackgroundProperty, value);
        }

        public static readonly DependencyProperty SelectedBackgroundProperty = DependencyProperty.Register(
            nameof(SelectedBackground), typeof(Brush), typeof(CustomMenu), new FrameworkPropertyMetadata(ShPalette.NavSelected, OnResourceChanged));

        [Category(Category), Description("선택된 항목 배경")]
        public Brush SelectedBackground
        {
            get => (Brush)GetValue(SelectedBackgroundProperty);
            set => SetValue(SelectedBackgroundProperty, value);
        }

        public static readonly DependencyProperty AccentBrushProperty = DependencyProperty.Register(
            nameof(AccentBrush), typeof(Brush), typeof(CustomMenu), new FrameworkPropertyMetadata(ShPalette.Accent, OnResourceChanged));

        [Category(Category), Description("선택 표시 막대 색")]
        public Brush AccentBrush
        {
            get => (Brush)GetValue(AccentBrushProperty);
            set => SetValue(AccentBrushProperty, value);
        }

        public static readonly DependencyProperty SelectCommandProperty = DependencyProperty.Register(
            nameof(SelectCommand), typeof(ICommand), typeof(CustomMenu), new FrameworkPropertyMetadata(null));

        /// <summary>선택 시 실행할 Command. CommandParameter로 선택된 INavItem이 전달된다.</summary>
        [Category(BehaviorCategory), Description("선택 시 실행할 Command (파라미터: INavItem)")]
        public ICommand? SelectCommand
        {
            get => (ICommand?)GetValue(SelectCommandProperty);
            set => SetValue(SelectCommandProperty, value);
        }

        #endregion

        #region Visual update (코드비하인드에서 명시적으로 화면 갱신)

        private static void OnResourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((CustomMenu)d).ApplyResources();

        private static void OnItemLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((CustomMenu)d)._host?.Rebuild();

        /// <summary>버튼 템플릿이 DynamicResource로 참조하는 색/모양 값을 기록한다.</summary>
        private void ApplyResources()
        {
            Resources["sh.Menu.Hover"] = HoverBackground;
            Resources["sh.Menu.Pressed"] = PressedBackground;
            Resources["sh.Menu.Selected"] = SelectedBackground;
            Resources["sh.Menu.Accent"] = AccentBrush;
            Resources["sh.Menu.ItemCornerRadius"] = ItemCornerRadius;
        }

        private void ApplyOrientation()
        {
            var vertical = Orientation == Orientation.Vertical;
            ItemsHost.Orientation = Orientation;
            Scroller.VerticalScrollBarVisibility = vertical ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
            Scroller.HorizontalScrollBarVisibility = vertical ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;
            Resources["sh.Menu.IndicatorV"] = vertical ? Visibility.Visible : Visibility.Collapsed;
            Resources["sh.Menu.IndicatorH"] = vertical ? Visibility.Collapsed : Visibility.Visible;
            _host?.Rebuild();
        }

        /// <summary>항목 하나의 버튼을 만든다. [아이콘] [제목] 구성.</summary>
        private ToggleButton CreateItemButton(INavItem item)
        {
            var hasGlyph = !string.IsNullOrEmpty(item.Glyph);
            var showTitle = !IsCompact || !hasGlyph;

            var content = new StackPanel { Orientation = Orientation.Horizontal };
            if (hasGlyph)
            {
                content.Children.Add(new TextBlock
                {
                    Text = item.Glyph,
                    FontFamily = ShPalette.IconFont,
                    FontSize = IconSize,
                    VerticalAlignment = VerticalAlignment.Center,
                });
            }
            if (showTitle)
            {
                content.Children.Add(new TextBlock
                {
                    Text = item.Title,
                    FontSize = ItemFontSize,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(hasGlyph ? 14 : 0, 0, 0, 1),
                });
            }

            return new ToggleButton
            {
                Template = (ControlTemplate)Resources["NavItemTemplate"],
                Content = content,
                Height = ItemHeight,
                Margin = ItemMargin,
                Padding = new Thickness(Orientation == Orientation.Vertical ? 14 : 12, 0, 12, 0),
                Foreground = ItemForeground,
                FontFamily = ShPalette.TextFont,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                FocusVisualStyle = ShFocusVisual.Rounded,
                // WHY: 컴팩트 모드에서는 제목이 보이지 않으므로 툴팁으로 대신 알려 준다.
                ToolTip = showTitle ? null : item.Title,
            };
        }

        #endregion

        private void OnHostSelectionChanged(object? sender, NavItemEventArgs e)
        {
            SelectionChanged?.Invoke(this, e);

            var command = SelectCommand;
            if (command != null && command.CanExecute(e.Item))
                command.Execute(e.Item);
        }
    }
}
