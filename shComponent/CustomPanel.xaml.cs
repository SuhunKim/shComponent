using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using shComponent.Internal;
using shComponent.Navigation;
using shComponent.Theming;

namespace shComponent
{
    /// <summary>
    /// 메인 콘텐츠 패널. <see cref="ShowPageAsync"/>로 페이지를 넘기면:
    ///  1) 페이지가 <see cref="IAsyncLoadable"/>이면 로딩하는 동안 스켈레톤을 보여 주고,
    ///  2) 완료되면 부드럽게 페이지를 표시하고 <see cref="INavigationAware.OnNavigatedTo"/>를 호출,
    ///  3) 실패하면 오류 화면(다시 시도) + <see cref="PageLoadFailed"/> 이벤트로 부모에게 알린다.
    /// </summary>
    [ToolboxItem(true)]
    [Description("메뉴 화면을 표시하는 콘텐츠 패널 (스켈레톤 로딩 포함)")]
    public partial class CustomPanel : UserControl
    {
        private const string Category = "sh 모양";
        private const string BehaviorCategory = "sh 동작";
        private const string ErrorGlyph = "";
        private const string InfoGlyph = "";

        private CancellationTokenSource? _loadCts;
        private FrameworkElement? _lastRequestedPage;
        private object? _lastParameter;

        public CustomPanel()
        {
            InitializeComponent();
            RetryButton.Content = RetryText;

            if (DesignTimeData.IsDesignMode(this))
                ShowMessage("콘텐츠 영역", "CustomMain.PageFactory에 페이지를 등록하면 이곳에 표시됩니다.");
        }

        #region Public API

        /// <summary>페이지가 화면에 표시된 직후 발생.</summary>
        public event EventHandler? PageShown;

        /// <summary>IAsyncLoadable.LoadAsync가 예외를 던졌을 때 발생(로그 기록 등은 부모가 처리).</summary>
        public event EventHandler<PageLoadFailedEventArgs>? PageLoadFailed;

        /// <summary>현재 표시 중인 페이지.</summary>
        [Browsable(false)]
        public FrameworkElement? CurrentPage { get; private set; }

        /// <summary>내부 스켈레톤(세부 모양을 코드에서 바꾸고 싶을 때).</summary>
        [Browsable(false)]
        public CustomSkeleton SkeletonView => Skeleton;

        /// <summary>
        /// 페이지를 표시한다. 이전 로딩이 진행 중이면 취소한다.
        /// WHY: 사용자가 메뉴를 빠르게 연속 클릭하면 늦게 끝난 이전 로딩이 새 화면을 덮어쓰는 문제가 생긴다.
        ///      매 호출마다 새 CancellationToken을 발급하고, 완료 시점에 "내가 아직 최신 요청인가"를 확인한다.
        /// </summary>
        public async Task ShowPageAsync(FrameworkElement? page, object? parameter = null)
        {
            _loadCts?.Cancel();
            var cts = new CancellationTokenSource();
            _loadCts = cts;
            var token = cts.Token;

            _lastRequestedPage = page;
            _lastParameter = parameter;
            HideMessage();

            if (!ReferenceEquals(CurrentPage, page))
                (CurrentPage as INavigationAware)?.OnNavigatedFrom();

            if (page == null)
            {
                SetPage(null);
                return;
            }

            if (page is IAsyncLoadable loadable)
            {
                var loaded = await LoadWithSkeletonAsync(page, loadable, parameter, token);
                if (!loaded) return;
            }

            if (token.IsCancellationRequested) return;
            SetPage(page);
            (page as INavigationAware)?.OnNavigatedTo(parameter);
            PageShown?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>페이지 대신 안내 문구를 보여 준다(미등록 페이지, 권한 없음 등).</summary>
        public void ShowMessage(string title, string? body = null, string? glyph = null)
        {
            _loadCts?.Cancel();
            SetPage(null);
            ShowMessageCore(glyph ?? InfoGlyph, title, body, showRetry: false);
        }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
            nameof(IsLoading), typeof(bool), typeof(CustomPanel),
            new FrameworkPropertyMetadata(false, (d, _) => ((CustomPanel)d).ApplyLoadingVisual()));

        /// <summary>true면 스켈레톤 표시. IAsyncLoadable을 쓰지 않는 페이지는 이 값을 직접 켜고 끄면 된다.</summary>
        [Category(BehaviorCategory), Description("true면 스켈레톤 로딩 화면 표시")]
        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public static readonly DependencyProperty SkeletonDelayMsProperty = DependencyProperty.Register(
            nameof(SkeletonDelayMs), typeof(int), typeof(CustomPanel), new FrameworkPropertyMetadata(120));

        /// <summary>WHY: 수십 ms 안에 끝나는 로딩에서 스켈레톤이 번쩍이면 오히려 불안해 보인다. 이 시간보다 오래 걸릴 때만 표시.</summary>
        [Category(BehaviorCategory), Description("로딩이 이 시간(ms)보다 오래 걸릴 때만 스켈레톤 표시")]
        public int SkeletonDelayMs
        {
            get => (int)GetValue(SkeletonDelayMsProperty);
            set => SetValue(SkeletonDelayMsProperty, value);
        }

        public static readonly DependencyProperty MinSkeletonMsProperty = DependencyProperty.Register(
            nameof(MinSkeletonMs), typeof(int), typeof(CustomPanel), new FrameworkPropertyMetadata(400));

        /// <summary>WHY: 스켈레톤이 보였다가 바로 사라지면 깜빡임으로 느껴진다. 한 번 보이면 최소 이 시간은 유지.</summary>
        [Category(BehaviorCategory), Description("스켈레톤이 표시되면 최소 유지 시간(ms)")]
        public int MinSkeletonMs
        {
            get => (int)GetValue(MinSkeletonMsProperty);
            set => SetValue(MinSkeletonMsProperty, value);
        }

        public static readonly DependencyProperty SkeletonRowCountProperty = DependencyProperty.Register(
            nameof(SkeletonRowCount), typeof(int), typeof(CustomPanel),
            new FrameworkPropertyMetadata(4, (d, e) => ((CustomPanel)d).Skeleton.RowCount = (int)e.NewValue));

        [Category(Category), Description("스켈레톤 카드 행 개수")]
        public int SkeletonRowCount
        {
            get => (int)GetValue(SkeletonRowCountProperty);
            set => SetValue(SkeletonRowCountProperty, value);
        }

        public static readonly DependencyProperty UseTransitionProperty = DependencyProperty.Register(
            nameof(UseTransition), typeof(bool), typeof(CustomPanel), new FrameworkPropertyMetadata(true));

        [Category(BehaviorCategory), Description("페이지 전환 시 살짝 떠오르는 애니메이션 사용")]
        public bool UseTransition
        {
            get => (bool)GetValue(UseTransitionProperty);
            set => SetValue(UseTransitionProperty, value);
        }

        public static readonly DependencyProperty IsScrollEnabledProperty = DependencyProperty.Register(
            nameof(IsScrollEnabled), typeof(bool), typeof(CustomPanel),
            new FrameworkPropertyMetadata(true, (d, e) => ((CustomPanel)d).PageScroller.VerticalScrollBarVisibility =
                (bool)e.NewValue ? ScrollBarVisibility.Auto : ScrollBarVisibility.Disabled));

        /// <summary>false로 두면 페이지가 패널 높이에 맞춰진다(DataGrid처럼 자체 스크롤이 있는 페이지용).</summary>
        [Category(BehaviorCategory), Description("페이지 세로 스크롤 사용 (자체 스크롤이 있는 페이지는 false)")]
        public bool IsScrollEnabled
        {
            get => (bool)GetValue(IsScrollEnabledProperty);
            set => SetValue(IsScrollEnabledProperty, value);
        }

        public static readonly DependencyProperty ErrorTitleProperty = DependencyProperty.Register(
            nameof(ErrorTitle), typeof(string), typeof(CustomPanel), new FrameworkPropertyMetadata("데이터를 불러오지 못했습니다"));

        [Category(Category), Description("로딩 실패 시 제목")]
        public string ErrorTitle
        {
            get => (string)GetValue(ErrorTitleProperty);
            set => SetValue(ErrorTitleProperty, value);
        }

        public static readonly DependencyProperty RetryTextProperty = DependencyProperty.Register(
            nameof(RetryText), typeof(string), typeof(CustomPanel),
            new FrameworkPropertyMetadata("다시 시도", (d, e) => ((CustomPanel)d).RetryButton.Content = e.NewValue));

        [Category(Category), Description("다시 시도 버튼 문구")]
        public string RetryText
        {
            get => (string)GetValue(RetryTextProperty);
            set => SetValue(RetryTextProperty, value);
        }

        #endregion

        #region Loading

        private async Task<bool> LoadWithSkeletonAsync(FrameworkElement page, IAsyncLoadable loadable, object? parameter, CancellationToken token)
        {
            Task loadTask;
            try
            {
                loadTask = loadable.LoadAsync(parameter, token);
            }
            catch (Exception ex)
            {
                // LoadAsync가 await 이전에 동기적으로 예외를 던지는 경우
                ShowError(page, ex);
                return false;
            }

            Stopwatch? skeletonClock = null;
            var first = await Task.WhenAny(loadTask, Task.Delay(Math.Max(0, SkeletonDelayMs)));
            if (first != loadTask && !token.IsCancellationRequested)
            {
                IsLoading = true;
                skeletonClock = Stopwatch.StartNew();
            }

            try
            {
                await loadTask;
            }
            catch (Exception ex)
            {
                if (token.IsCancellationRequested) return false;   // 취소로 인한 예외는 오류가 아님
                IsLoading = false;
                ShowError(page, ex);
                return false;
            }

            if (token.IsCancellationRequested) return false;

            if (skeletonClock != null)
            {
                var remain = MinSkeletonMs - (int)skeletonClock.ElapsedMilliseconds;
                if (remain > 0) await Task.Delay(remain);
                if (token.IsCancellationRequested) return false;
                IsLoading = false;
            }
            return true;
        }

        private void ApplyLoadingVisual()
        {
            var loading = IsLoading;
            Skeleton.Visibility = loading ? Visibility.Visible : Visibility.Collapsed;
            Skeleton.IsActive = loading;
            PageScroller.Visibility = loading ? Visibility.Hidden : Visibility.Visible;
            if (loading) HideMessage();
        }

        #endregion

        #region Page / message display

        private void SetPage(FrameworkElement? page)
        {
            IsLoading = false;
            CurrentPage = page;
            PageHost.Content = page;
            PageScroller.ScrollToTop();
            if (page != null) PlayEntrance();
        }

        /// <summary>Windows 11 페이지 진입 효과: 아래에서 살짝 떠오르며 나타남.</summary>
        private void PlayEntrance()
        {
            if (!UseTransition || DesignTimeData.IsDesignMode(this)) return;

            var duration = TimeSpan.FromMilliseconds(280);
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };
            PageHost.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, duration) { EasingFunction = ease });
            PageShift.BeginAnimation(System.Windows.Media.TranslateTransform.YProperty, new DoubleAnimation(24, 0, duration) { EasingFunction = ease });
        }

        private void ShowError(FrameworkElement page, Exception ex)
        {
            SetPage(null);
            ShowMessageCore(ErrorGlyph, ErrorTitle, ex.Message, showRetry: true);
            MessageGlyph.Foreground = ShPalette.Danger;
            PageLoadFailed?.Invoke(this, new PageLoadFailedEventArgs(page, ex));
        }

        private void ShowMessageCore(string glyph, string title, string? body, bool showRetry)
        {
            MessageGlyph.Text = glyph;
            MessageGlyph.Foreground = ShPalette.TextSecondary;
            MessageTitle.Text = title;
            MessageBody.Text = body ?? string.Empty;
            MessageBody.Visibility = string.IsNullOrEmpty(body) ? Visibility.Collapsed : Visibility.Visible;
            RetryButton.Visibility = showRetry ? Visibility.Visible : Visibility.Collapsed;
            MessageHost.Visibility = Visibility.Visible;
        }

        private void HideMessage() => MessageHost.Visibility = Visibility.Collapsed;

        private async void OnRetryClick(object sender, RoutedEventArgs e)
        {
            // WHY: async void는 이벤트 핸들러에서만 사용. 여기서 난 예외는 Dispatcher로 올라가 디버깅 시 바로 보인다.
            if (_lastRequestedPage != null)
                await ShowPageAsync(_lastRequestedPage, _lastParameter);
        }

        #endregion
    }
}
