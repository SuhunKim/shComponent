using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using shComponent.Internal;
using shComponent.Navigation;
using shComponent.Theming;

namespace shComponent
{
    /// <summary>
    /// 최상위 프레임(메뉴 + 서브메뉴 + 콘텐츠 패널).
    /// <code>
    /// Shell.PageFactory = new PageFactory().Register&lt;HomePage&gt;("home");
    /// Shell.SetMenu(new[] { new NavItem("home", "홈", "") });
    /// Shell.Navigated += (s, e) => log.Info(e.Item.Key);
    /// </code>
    /// 흐름: 메뉴 클릭 → Navigating(취소 가능) → PageFactory로 페이지 생성 → CustomPanel 표시 → Navigated.
    /// </summary>
    [ToolboxItem(true)]
    [Description("Windows 11 설정 앱 스타일 최상위 프레임")]
    public partial class CustomMain : UserControl
    {
        private const string Category = "sh 모양";
        private const string BehaviorCategory = "sh 동작";

        private IReadOnlyList<INavItem> _menuItems = Array.Empty<INavItem>();
        private INavItem? _currentRoot;
        private INavItem? _currentLeaf;
        private bool _initialNavigationPending;

        static CustomMain()
        {
            BackgroundProperty.OverrideMetadata(typeof(CustomMain), new FrameworkPropertyMetadata(ShPalette.WindowBackground));
            ForegroundProperty.OverrideMetadata(typeof(CustomMain), new FrameworkPropertyMetadata(ShPalette.TextPrimary, FrameworkPropertyMetadataOptions.Inherits));
            FontFamilyProperty.OverrideMetadata(typeof(CustomMain), new FrameworkPropertyMetadata(ShPalette.TextFont, FrameworkPropertyMetadataOptions.Inherits));
            FontSizeProperty.OverrideMetadata(typeof(CustomMain), new FrameworkPropertyMetadata(ShPalette.BodyFontSize, FrameworkPropertyMetadataOptions.Inherits));
        }

        public CustomMain()
        {
            InitializeComponent();

            MainMenu.SelectionChanged += OnMenuSelectionChanged;
            SubMenuView.SelectionChanged += OnSubMenuSelectionChanged;
            ContentPanel.PageLoadFailed += (_, e) => PageLoadFailed?.Invoke(this, e);
            Loaded += OnLoaded;

            ApplyAll();

            if (DesignTimeData.IsDesignMode(this))
            {
                // WHY: 디자이너에서 전체 프레임 모양(메뉴/제목/서브메뉴)을 바로 확인할 수 있도록 샘플 메뉴로 채운다.
                SetMenu(DesignTimeData.CreateMenu());
                var menu1 = _menuItems[1];   // 서브메뉴가 있는 Menu1을 선택해 서브메뉴 탭까지 보이게 한다.
                NavigateCore(menu1, NavTree.ResolveLeaf(menu1), null);
            }
        }

        #region Public API

        /// <summary>이동 직전. e.Cancel = true 로 이동을 막을 수 있다(저장하지 않은 변경 확인 등).</summary>
        public event EventHandler<NavigatingEventArgs>? Navigating;

        /// <summary>이동 완료(페이지 표시 요청 후).</summary>
        public event EventHandler<NavItemEventArgs>? Navigated;

        /// <summary>페이지 로딩 실패(CustomPanel에서 전달).</summary>
        public event EventHandler<PageLoadFailedEventArgs>? PageLoadFailed;

        /// <summary>메뉴 항목 → 페이지 생성 규칙. SetMenu 전에 설정할 것을 권장.</summary>
        [Browsable(false)]
        public IPageFactory? PageFactory { get; set; }

        [Browsable(false)] public CustomMenu MenuView => MainMenu;
        [Browsable(false)] public CustomSubMenu SubMenu => SubMenuView;
        [Browsable(false)] public CustomPanel Panel => ContentPanel;

        /// <summary>현재 표시 중인 페이지 항목(서브메뉴가 있으면 서브메뉴 항목).</summary>
        [Browsable(false)] public INavItem? CurrentItem => _currentLeaf;

        [Browsable(false)] public IReadOnlyList<INavItem> MenuItems => _menuItems;

        /// <summary>메뉴 트리(1차 + 서브메뉴 Children)를 설정한다. AutoSelectFirst=true 이면 첫 항목으로 이동.</summary>
        public void SetMenu(IEnumerable<INavItem>? items)
        {
            _menuItems = items?.ToList() ?? new List<INavItem>();
            _currentRoot = null;
            _currentLeaf = null;
            MainMenu.SetItems(_menuItems);
            SubMenuView.SetItems(null);

            if (!AutoSelectFirst || DesignTimeData.IsDesignMode(this)) return;

            // WHY: 생성자에서 SetMenu를 부르면 아직 PageFactory가 설정되지 않았을 수 있다.
            //      화면이 로드된 뒤 첫 이동을 하면 호출 순서에 상관없이 안전하다.
            if (IsLoaded) NavigateToFirst();
            else _initialNavigationPending = true;
        }

        /// <summary>Key로 이동한다(1차/서브메뉴 모두 검색). 없는 Key면 false.</summary>
        public bool Navigate(string key, object? parameter = null)
        {
            var item = NavTree.Find(_menuItems, key, out var parent);
            if (item == null || !item.IsEnabled) return false;

            var root = parent ?? item;
            var leaf = parent != null ? item : NavTree.ResolveLeaf(item);
            return NavigateCore(root, leaf, parameter);
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty AppTitleProperty = DependencyProperty.Register(
            nameof(AppTitle), typeof(string), typeof(CustomMain),
            new FrameworkPropertyMetadata("shComponent", (d, _) => ((CustomMain)d).ApplyHeader()));

        [Category(Category), Description("좌측 상단 앱 제목")]
        public string AppTitle
        {
            get => (string)GetValue(AppTitleProperty);
            set => SetValue(AppTitleProperty, value);
        }

        public static readonly DependencyProperty AppGlyphProperty = DependencyProperty.Register(
            nameof(AppGlyph), typeof(string), typeof(CustomMain),
            new FrameworkPropertyMetadata(null, (d, _) => ((CustomMain)d).ApplyHeader(), CustomButton.CoerceEmptyToNull));

        [Category(Category), Description("앱 제목 옆 아이콘 글리프 (Segoe Fluent Icons)")]
        public string? AppGlyph
        {
            get => (string?)GetValue(AppGlyphProperty);
            set => SetValue(AppGlyphProperty, value);
        }

        public static readonly DependencyProperty MenuPlacementProperty = DependencyProperty.Register(
            nameof(MenuPlacement), typeof(MenuPlacement), typeof(CustomMain),
            new FrameworkPropertyMetadata(MenuPlacement.Left, OnLayoutChanged));

        [Category(Category), Description("1차 메뉴 위치: Left(좌측 세로) / Top(상단 가로)")]
        public MenuPlacement MenuPlacement
        {
            get => (MenuPlacement)GetValue(MenuPlacementProperty);
            set => SetValue(MenuPlacementProperty, value);
        }

        public static readonly DependencyProperty PaneWidthProperty = DependencyProperty.Register(
            nameof(PaneWidth), typeof(double), typeof(CustomMain), new FrameworkPropertyMetadata(280d, OnLayoutChanged));

        [Category(Category), Description("좌측 메뉴 폭")]
        public double PaneWidth
        {
            get => (double)GetValue(PaneWidthProperty);
            set => SetValue(PaneWidthProperty, value);
        }

        public static readonly DependencyProperty CompactPaneWidthProperty = DependencyProperty.Register(
            nameof(CompactPaneWidth), typeof(double), typeof(CustomMain), new FrameworkPropertyMetadata(64d, OnLayoutChanged));

        [Category(Category), Description("접힌(아이콘만) 메뉴 폭")]
        public double CompactPaneWidth
        {
            get => (double)GetValue(CompactPaneWidthProperty);
            set => SetValue(CompactPaneWidthProperty, value);
        }

        public static readonly DependencyProperty IsPaneCompactProperty = DependencyProperty.Register(
            nameof(IsPaneCompact), typeof(bool), typeof(CustomMain), new FrameworkPropertyMetadata(false, OnLayoutChanged));

        [Category(Category), Description("true면 좌측 메뉴를 아이콘만 보이게 접음")]
        public bool IsPaneCompact
        {
            get => (bool)GetValue(IsPaneCompactProperty);
            set => SetValue(IsPaneCompactProperty, value);
        }

        public static readonly DependencyProperty ShowPaneToggleProperty = DependencyProperty.Register(
            nameof(ShowPaneToggle), typeof(bool), typeof(CustomMain), new FrameworkPropertyMetadata(true, OnLayoutChanged));

        [Category(Category), Description("메뉴 접기(≡) 버튼 표시")]
        public bool ShowPaneToggle
        {
            get => (bool)GetValue(ShowPaneToggleProperty);
            set => SetValue(ShowPaneToggleProperty, value);
        }

        public static readonly DependencyProperty PaneBackgroundProperty = DependencyProperty.Register(
            nameof(PaneBackground), typeof(Brush), typeof(CustomMain), new FrameworkPropertyMetadata(ShPalette.PaneBackground, OnColorsChanged));

        [Category(Category), Description("메뉴 영역 배경")]
        public Brush PaneBackground
        {
            get => (Brush)GetValue(PaneBackgroundProperty);
            set => SetValue(PaneBackgroundProperty, value);
        }

        public static readonly DependencyProperty ContentBackgroundProperty = DependencyProperty.Register(
            nameof(ContentBackground), typeof(Brush), typeof(CustomMain), new FrameworkPropertyMetadata(ShPalette.ContentBackground, OnColorsChanged));

        [Category(Category), Description("콘텐츠 영역 배경")]
        public Brush ContentBackground
        {
            get => (Brush)GetValue(ContentBackgroundProperty);
            set => SetValue(ContentBackgroundProperty, value);
        }

        public static readonly DependencyProperty ContentBorderBrushProperty = DependencyProperty.Register(
            nameof(ContentBorderBrush), typeof(Brush), typeof(CustomMain), new FrameworkPropertyMetadata(ShPalette.ContentBorder, OnColorsChanged));

        [Category(Category), Description("콘텐츠 영역 테두리")]
        public Brush ContentBorderBrush
        {
            get => (Brush)GetValue(ContentBorderBrushProperty);
            set => SetValue(ContentBorderBrushProperty, value);
        }

        public static readonly DependencyProperty AccentBrushProperty = DependencyProperty.Register(
            nameof(AccentBrush), typeof(Brush), typeof(CustomMain), new FrameworkPropertyMetadata(ShPalette.Accent, OnColorsChanged));

        /// <summary>메뉴/서브메뉴 선택 표시 색. 여기 한 번만 바꾸면 하위 메뉴에 전달된다.</summary>
        [Category(Category), Description("강조색 (메뉴·서브메뉴 선택 표시에 전달)")]
        public Brush AccentBrush
        {
            get => (Brush)GetValue(AccentBrushProperty);
            set => SetValue(AccentBrushProperty, value);
        }

        public static readonly DependencyProperty ContentPaddingProperty = DependencyProperty.Register(
            nameof(ContentPadding), typeof(Thickness), typeof(CustomMain),
            new FrameworkPropertyMetadata(new Thickness(36, 28, 36, 0), OnLayoutChanged));

        [Category(Category), Description("콘텐츠 영역 안쪽 여백")]
        public Thickness ContentPadding
        {
            get => (Thickness)GetValue(ContentPaddingProperty);
            set => SetValue(ContentPaddingProperty, value);
        }

        public static readonly DependencyProperty ShowPageTitleProperty = DependencyProperty.Register(
            nameof(ShowPageTitle), typeof(bool), typeof(CustomMain), new FrameworkPropertyMetadata(true, OnLayoutChanged));

        [Category(Category), Description("콘텐츠 상단 페이지 제목 표시")]
        public bool ShowPageTitle
        {
            get => (bool)GetValue(ShowPageTitleProperty);
            set => SetValue(ShowPageTitleProperty, value);
        }

        public static readonly DependencyProperty PageTitleFontSizeProperty = DependencyProperty.Register(
            nameof(PageTitleFontSize), typeof(double), typeof(CustomMain), new FrameworkPropertyMetadata(28d, OnLayoutChanged));

        [Category(Category), Description("페이지 제목 글자 크기")]
        public double PageTitleFontSize
        {
            get => (double)GetValue(PageTitleFontSizeProperty);
            set => SetValue(PageTitleFontSizeProperty, value);
        }

        public static readonly DependencyProperty AutoSelectFirstProperty = DependencyProperty.Register(
            nameof(AutoSelectFirst), typeof(bool), typeof(CustomMain), new FrameworkPropertyMetadata(true));

        [Category(BehaviorCategory), Description("SetMenu 후 첫 번째 메뉴로 자동 이동")]
        public bool AutoSelectFirst
        {
            get => (bool)GetValue(AutoSelectFirstProperty);
            set => SetValue(AutoSelectFirstProperty, value);
        }

        #endregion

        #region Navigation (메뉴 → 서브메뉴 → 패널 연결)

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!_initialNavigationPending) return;
            _initialNavigationPending = false;
            NavigateToFirst();
        }

        private void NavigateToFirst()
        {
            var root = _menuItems.FirstOrDefault(i => i.IsEnabled);
            if (root != null) NavigateCore(root, NavTree.ResolveLeaf(root), null);
        }

        private void OnMenuSelectionChanged(object? sender, NavItemEventArgs e)
            => NavigateCore(e.Item, NavTree.ResolveLeaf(e.Item), null);

        private void OnSubMenuSelectionChanged(object? sender, NavItemEventArgs e)
        {
            if (_currentRoot != null) NavigateCore(_currentRoot, e.Item, null);
        }

        /// <summary>
        /// 모든 이동이 거치는 단일 경로.
        /// WHY: 메뉴 클릭 / 서브메뉴 클릭 / 코드에서 Navigate(key) 가 모두 같은 순서(확인→상태 반영→표시→통지)를 따르게 해
        ///      "어디서 이동했느냐"에 따라 동작이 달라지는 버그를 막는다.
        /// </summary>
        private bool NavigateCore(INavItem root, INavItem leaf, object? parameter)
        {
            var navigating = new NavigatingEventArgs(leaf, _currentLeaf, parameter);
            Navigating?.Invoke(this, navigating);
            if (navigating.Cancel)
            {
                // 메뉴는 이미 클릭된 항목으로 표시가 바뀌었으므로 이전 상태로 되돌린다.
                SyncMenus(_currentRoot, _currentLeaf);
                return false;
            }

            var previous = _currentLeaf;
            _currentRoot = root;
            _currentLeaf = leaf;
            SyncMenus(root, leaf);
            UpdatePageTitle(root, leaf);
            ShowPage(leaf, parameter ?? leaf.Parameter);

            Navigated?.Invoke(this, new NavItemEventArgs(leaf, previous));
            return true;
        }

        /// <summary>메뉴/서브메뉴의 선택 표시를 현재 상태와 맞춘다(이벤트 없이).</summary>
        private void SyncMenus(INavItem? root, INavItem? leaf)
        {
            MainMenu.Select(root, notify: false);

            var children = root?.Children ?? (IReadOnlyList<INavItem>)Array.Empty<INavItem>();
            if (!SubMenuView.Items.SequenceEqual(children))
                SubMenuView.SetItems(children);
            SubMenuView.Select(children.Count > 0 ? leaf : null, notify: false);
        }

        private void ShowPage(INavItem leaf, object? parameter)
        {
            var page = PageFactory?.CreatePage(leaf);
            if (page == null)
            {
                // WHY: 페이지 미등록은 개발 중 흔한 실수다. 빈 화면 대신 원인을 화면에 알려 준다.
                ContentPanel.ShowMessage($"'{leaf.Title}' 페이지가 등록되지 않았습니다",
                    $"PageFactory.Register(\"{leaf.Key}\", ...) 로 페이지를 등록하세요.");
                return;
            }
            // WHY: 로딩 실패는 CustomPanel 내부에서 처리/통지된다. 반환 Task를 기다리지 않아 메뉴 반응이 즉시 끝나게 한다.
            _ = ContentPanel.ShowPageAsync(page, parameter);
        }

        private void UpdatePageTitle(INavItem root, INavItem leaf)
        {
            PageTitleText.Inlines.Clear();
            if (ReferenceEquals(root, leaf))
            {
                PageTitleText.Inlines.Add(new Run(root.Title));
                return;
            }
            // 경로 제목: "Menu1  ›  SubMenu1"
            PageTitleText.Inlines.Add(new Run(root.Title) { Foreground = ShPalette.TextSecondary });
            PageTitleText.Inlines.Add(new Run("  ›  ") { Foreground = ShPalette.TextSecondary });
            PageTitleText.Inlines.Add(new Run(leaf.Title));
        }

        #endregion

        #region Layout / visual update (코드비하인드에서 명시적으로 적용)

        private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((CustomMain)d).ApplyLayout();

        private static void OnColorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((CustomMain)d).ApplyColors();

        private void ApplyAll()
        {
            ApplyHeader();
            ApplyColors();
            ApplyLayout();
        }

        private void ApplyHeader()
        {
            AppTitleText.Text = AppTitle;
            AppGlyphText.Text = AppGlyph ?? string.Empty;
            AppGlyphText.Visibility = AppGlyph == null ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ApplyColors()
        {
            PaneHost.Background = PaneBackground;
            ContentHost.Background = ContentBackground;
            ContentHost.BorderBrush = ContentBorderBrush;
            // WHY: 강조색을 프레임 한 곳에서 지정하면 하위 메뉴들이 같은 톤을 쓰도록 명시적으로 전달한다(상속 바인딩 대신).
            MainMenu.AccentBrush = AccentBrush;
            SubMenuView.AccentBrush = AccentBrush;
        }

        private void ApplyLayout()
        {
            var left = MenuPlacement == MenuPlacement.Left;
            var compact = left && IsPaneCompact;

            // 그리드 배치
            Grid.SetRow(PaneHost, 0);
            Grid.SetColumn(PaneHost, 0);
            Grid.SetRowSpan(PaneHost, left ? 2 : 1);
            Grid.SetColumnSpan(PaneHost, left ? 1 : 2);

            Grid.SetRow(ContentHost, left ? 0 : 1);
            Grid.SetColumn(ContentHost, left ? 1 : 0);
            Grid.SetRowSpan(ContentHost, left ? 2 : 1);
            Grid.SetColumnSpan(ContentHost, left ? 1 : 2);

            PaneColumn.Width = left ? new GridLength(compact ? CompactPaneWidth : PaneWidth) : new GridLength(0);

            // 헤더는 좌측 모드에서 위, 상단 모드에서 왼쪽에 붙는다.
            DockPanel.SetDock(PaneHeader, left ? Dock.Top : Dock.Left);
            MainMenu.Orientation = left ? Orientation.Vertical : Orientation.Horizontal;
            MainMenu.IsCompact = compact;
            MainMenu.Margin = left ? new Thickness(0, 4, 0, 8) : new Thickness(8, 6, 8, 6);
            MainMenu.VerticalAlignment = left ? VerticalAlignment.Stretch : VerticalAlignment.Center;

            PaneToggleButton.Visibility = left && ShowPaneToggle ? Visibility.Visible : Visibility.Collapsed;
            AppTitleText.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
            AppGlyphText.Visibility = compact || AppGlyph == null ? Visibility.Collapsed : Visibility.Visible;

            // 콘텐츠 영역: 좌측 모드는 왼쪽 위만 둥글게, 상단 모드는 위쪽 양 모서리를 둥글게.
            ContentHost.CornerRadius = left ? new CornerRadius(8, 0, 0, 0) : new CornerRadius(8, 8, 0, 0);
            ContentHost.BorderThickness = left ? new Thickness(1, 1, 0, 0) : new Thickness(1, 1, 1, 0);
            ContentHost.Margin = left ? new Thickness(0) : new Thickness(8, 0, 8, 0);

            ContentGrid.Margin = ContentPadding;
            PageTitleText.FontSize = PageTitleFontSize;
            PageTitleText.Visibility = ShowPageTitle ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnPaneToggleClick(object sender, RoutedEventArgs e) => IsPaneCompact = !IsPaneCompact;

        #endregion
    }
}
