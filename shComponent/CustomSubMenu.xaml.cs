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
    /// 2차 서브메뉴(가로 탭 / 세로 목록). 사용법은 CustomMenu와 같다: SetItems → SelectionChanged 이벤트 수신.
    /// 항목이 없으면 AutoHideWhenEmpty에 따라 자동으로 숨는다.
    /// </summary>
    [ToolboxItem(true)]
    [Description("2차 서브메뉴 (가로 탭 / 세로 목록)")]
    public partial class CustomSubMenu : UserControl
    {
        private const string Category = "sh 모양";
        private const string BehaviorCategory = "sh 동작";

        private readonly NavButtonHost _host;

        public CustomSubMenu()
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
                SetItems(DesignTimeData.CreateSubMenu());
                Select(Items.FirstOrDefault(), notify: false);
            }
            else
            {
                UpdateAutoHide();
            }
        }

        #region Public API

        public event EventHandler<NavItemEventArgs>? SelectionChanged;

        [Browsable(false)]
        public IReadOnlyList<INavItem> Items => _host.Items;

        [Browsable(false)]
        public INavItem? SelectedItem => _host.SelectedItem;

        public void SetItems(IEnumerable<INavItem>? items)
        {
            _host.SetItems(items);
            UpdateAutoHide();
        }

        public void Select(INavItem? item, bool notify = true) => _host.Select(item, notify);

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
            nameof(Orientation), typeof(Orientation), typeof(CustomSubMenu),
            new FrameworkPropertyMetadata(Orientation.Horizontal, (d, _) => ((CustomSubMenu)d).ApplyOrientation()));

        [Category(Category), Description("Horizontal = 가로 탭(기본), Vertical = 세로 목록")]
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public static readonly DependencyProperty AutoHideWhenEmptyProperty = DependencyProperty.Register(
            nameof(AutoHideWhenEmpty), typeof(bool), typeof(CustomSubMenu),
            new FrameworkPropertyMetadata(true, (d, _) => ((CustomSubMenu)d).UpdateAutoHide()));

        [Category(BehaviorCategory), Description("항목이 없으면 자동으로 숨김")]
        public bool AutoHideWhenEmpty
        {
            get => (bool)GetValue(AutoHideWhenEmptyProperty);
            set => SetValue(AutoHideWhenEmptyProperty, value);
        }

        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
            nameof(ItemHeight), typeof(double), typeof(CustomSubMenu), new FrameworkPropertyMetadata(38d, OnItemLayoutChanged));

        [Category(Category), Description("항목 높이")]
        public double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        public static readonly DependencyProperty ItemFontSizeProperty = DependencyProperty.Register(
            nameof(ItemFontSize), typeof(double), typeof(CustomSubMenu), new FrameworkPropertyMetadata(14d, OnItemLayoutChanged));

        [Category(Category), Description("글자 크기")]
        public double ItemFontSize
        {
            get => (double)GetValue(ItemFontSizeProperty);
            set => SetValue(ItemFontSizeProperty, value);
        }

        public static readonly DependencyProperty ItemMarginProperty = DependencyProperty.Register(
            nameof(ItemMargin), typeof(Thickness), typeof(CustomSubMenu), new FrameworkPropertyMetadata(new Thickness(0, 0, 4, 0), OnItemLayoutChanged));

        [Category(Category), Description("항목 바깥 여백(항목 간 간격)")]
        public Thickness ItemMargin
        {
            get => (Thickness)GetValue(ItemMarginProperty);
            set => SetValue(ItemMarginProperty, value);
        }

        public static readonly DependencyProperty ItemForegroundProperty = DependencyProperty.Register(
            nameof(ItemForeground), typeof(Brush), typeof(CustomSubMenu), new FrameworkPropertyMetadata(ShPalette.TextSecondary, OnResourceChanged));

        [Category(Category), Description("선택 안 된 항목 글자색")]
        public Brush ItemForeground
        {
            get => (Brush)GetValue(ItemForegroundProperty);
            set => SetValue(ItemForegroundProperty, value);
        }

        public static readonly DependencyProperty SelectedForegroundProperty = DependencyProperty.Register(
            nameof(SelectedForeground), typeof(Brush), typeof(CustomSubMenu), new FrameworkPropertyMetadata(ShPalette.TextPrimary, OnResourceChanged));

        [Category(Category), Description("선택된 항목 글자색")]
        public Brush SelectedForeground
        {
            get => (Brush)GetValue(SelectedForegroundProperty);
            set => SetValue(SelectedForegroundProperty, value);
        }

        public static readonly DependencyProperty HoverBackgroundProperty = DependencyProperty.Register(
            nameof(HoverBackground), typeof(Brush), typeof(CustomSubMenu), new FrameworkPropertyMetadata(ShPalette.NavHover, OnResourceChanged));

        [Category(Category), Description("마우스를 올렸을 때 배경")]
        public Brush HoverBackground
        {
            get => (Brush)GetValue(HoverBackgroundProperty);
            set => SetValue(HoverBackgroundProperty, value);
        }

        public static readonly DependencyProperty AccentBrushProperty = DependencyProperty.Register(
            nameof(AccentBrush), typeof(Brush), typeof(CustomSubMenu), new FrameworkPropertyMetadata(ShPalette.Accent, OnResourceChanged));

        [Category(Category), Description("선택 표시(밑줄) 색")]
        public Brush AccentBrush
        {
            get => (Brush)GetValue(AccentBrushProperty);
            set => SetValue(AccentBrushProperty, value);
        }

        public static readonly DependencyProperty ItemCornerRadiusProperty = DependencyProperty.Register(
            nameof(ItemCornerRadius), typeof(CornerRadius), typeof(CustomSubMenu), new FrameworkPropertyMetadata(ShPalette.ControlCornerRadius, OnResourceChanged));

        [Category(Category), Description("항목 모서리 둥글기")]
        public CornerRadius ItemCornerRadius
        {
            get => (CornerRadius)GetValue(ItemCornerRadiusProperty);
            set => SetValue(ItemCornerRadiusProperty, value);
        }

        public static readonly DependencyProperty SelectCommandProperty = DependencyProperty.Register(
            nameof(SelectCommand), typeof(ICommand), typeof(CustomSubMenu), new FrameworkPropertyMetadata(null));

        [Category(BehaviorCategory), Description("선택 시 실행할 Command (파라미터: INavItem)")]
        public ICommand? SelectCommand
        {
            get => (ICommand?)GetValue(SelectCommandProperty);
            set => SetValue(SelectCommandProperty, value);
        }

        #endregion

        #region Visual update

        private static void OnResourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((CustomSubMenu)d).ApplyResources();

        private static void OnItemLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((CustomSubMenu)d)._host?.Rebuild();

        private void ApplyResources()
        {
            Resources["sh.Sub.Foreground"] = ItemForeground;
            Resources["sh.Sub.SelectedForeground"] = SelectedForeground;
            Resources["sh.Sub.Hover"] = HoverBackground;
            Resources["sh.Sub.Accent"] = AccentBrush;
            Resources["sh.Sub.ItemCornerRadius"] = ItemCornerRadius;
        }

        private void ApplyOrientation()
        {
            var horizontal = Orientation == Orientation.Horizontal;
            ItemsHost.Orientation = Orientation;
            Scroller.HorizontalScrollBarVisibility = horizontal ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled;
            Scroller.VerticalScrollBarVisibility = horizontal ? ScrollBarVisibility.Disabled : ScrollBarVisibility.Auto;
            Resources["sh.Sub.IndicatorH"] = horizontal ? Visibility.Visible : Visibility.Collapsed;
            Resources["sh.Sub.IndicatorV"] = horizontal ? Visibility.Collapsed : Visibility.Visible;
            _host?.Rebuild();
        }

        private void UpdateAutoHide()
        {
            if (DesignTimeData.IsDesignMode(this)) return;
            Visibility = AutoHideWhenEmpty && Items.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        private ToggleButton CreateItemButton(INavItem item)
        {
            var horizontal = Orientation == Orientation.Horizontal;
            return new ToggleButton
            {
                Style = (Style)Resources["SubItemStyle"],
                Content = new TextBlock
                {
                    Text = item.Title,
                    FontSize = ItemFontSize,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                },
                Height = ItemHeight,
                Margin = ItemMargin,
                Padding = new Thickness(horizontal ? 12 : 16, 0, 12, 1),
                FontFamily = ShPalette.TextFont,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                FocusVisualStyle = ShFocusVisual.Rounded,
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
