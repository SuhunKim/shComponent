using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using shComponent.Theming;

namespace shComponent
{
    /// <summary>
    /// 로딩용 스켈레톤 화면. 단독으로 배치해도 되고, CustomPanel이 로딩 중에 자동으로 사용하기도 한다.
    /// 구성: (선택) 제목 블록 + RowCount개의 카드 행(아이콘 / 두 줄 텍스트 / 오른쪽 컨트롤 자리).
    /// </summary>
    [ToolboxItem(true)]
    [Description("로딩 중 스켈레톤 화면")]
    public partial class CustomSkeleton : UserControl
    {
        private const string Category = "sh 모양";

        // WHY: 행마다 텍스트 길이를 조금씩 달리해야 "진짜 목록"처럼 보인다(모두 같은 길이면 기계적으로 보임).
        private static readonly double[] LineRatios = { 0.55, 0.7, 0.45, 0.62, 0.5, 0.66 };

        private readonly TranslateTransform _shimmerShift = new TranslateTransform();
        private readonly LinearGradientBrush _shimmerBrush;
        private readonly GradientStop _baseStart;
        private readonly GradientStop _highlight;
        private readonly GradientStop _baseEnd;
        private bool _isAnimating;

        public CustomSkeleton()
        {
            InitializeComponent();

            _baseStart = new GradientStop(BaseColor, 0.0);
            _highlight = new GradientStop(HighlightColor, 0.5);
            _baseEnd = new GradientStop(BaseColor, 1.0);

            // WHY: 모든 블록이 브러시 하나를 공유하고, 그 브러시의 위치만 애니메이션한다.
            //      블록마다 애니메이션을 돌리는 것보다 훨씬 가볍고 모든 블록의 빛이 동시에 움직여 자연스럽다.
            _shimmerBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0.5),
                EndPoint = new Point(1, 0.5),
                RelativeTransform = _shimmerShift,
                GradientStops = { _baseStart, _highlight, _baseEnd },
            };

            BuildBlocks();

            Loaded += (_, __) => UpdateAnimation();
            Unloaded += (_, __) => UpdateAnimation();
            IsVisibleChanged += (_, __) => UpdateAnimation();
        }

        #region Dependency Properties

        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
            nameof(IsActive), typeof(bool), typeof(CustomSkeleton),
            new FrameworkPropertyMetadata(true, (d, _) => ((CustomSkeleton)d).UpdateAnimation()));

        /// <summary>shimmer 애니메이션 동작 여부. 화면에 보이지 않으면 자동으로 멈춘다(CPU 절약).</summary>
        [Category(Category), Description("빛이 지나가는 애니메이션 동작 여부")]
        public bool IsActive
        {
            get => (bool)GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        public static readonly DependencyProperty RowCountProperty = DependencyProperty.Register(
            nameof(RowCount), typeof(int), typeof(CustomSkeleton),
            new FrameworkPropertyMetadata(4, OnLayoutChanged), v => (int)v >= 0 && (int)v <= 50);

        [Category(Category), Description("카드 행 개수 (0~50)")]
        public int RowCount
        {
            get => (int)GetValue(RowCountProperty);
            set => SetValue(RowCountProperty, value);
        }

        public static readonly DependencyProperty RowHeightProperty = DependencyProperty.Register(
            nameof(RowHeight), typeof(double), typeof(CustomSkeleton), new FrameworkPropertyMetadata(68d, OnLayoutChanged));

        [Category(Category), Description("카드 행 높이")]
        public double RowHeight
        {
            get => (double)GetValue(RowHeightProperty);
            set => SetValue(RowHeightProperty, value);
        }

        public static readonly DependencyProperty ShowHeaderProperty = DependencyProperty.Register(
            nameof(ShowHeader), typeof(bool), typeof(CustomSkeleton), new FrameworkPropertyMetadata(true, OnLayoutChanged));

        [Category(Category), Description("맨 위 제목 블록 표시")]
        public bool ShowHeader
        {
            get => (bool)GetValue(ShowHeaderProperty);
            set => SetValue(ShowHeaderProperty, value);
        }

        public static readonly DependencyProperty RowBackgroundProperty = DependencyProperty.Register(
            nameof(RowBackground), typeof(Brush), typeof(CustomSkeleton), new FrameworkPropertyMetadata(ShPalette.CardBackground, OnLayoutChanged));

        [Category(Category), Description("카드 행 배경")]
        public Brush RowBackground
        {
            get => (Brush)GetValue(RowBackgroundProperty);
            set => SetValue(RowBackgroundProperty, value);
        }

        public static readonly DependencyProperty RowBorderBrushProperty = DependencyProperty.Register(
            nameof(RowBorderBrush), typeof(Brush), typeof(CustomSkeleton), new FrameworkPropertyMetadata(ShPalette.CardBorder, OnLayoutChanged));

        [Category(Category), Description("카드 행 테두리")]
        public Brush RowBorderBrush
        {
            get => (Brush)GetValue(RowBorderBrushProperty);
            set => SetValue(RowBorderBrushProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(CustomSkeleton), new FrameworkPropertyMetadata(ShPalette.OverlayCornerRadius, OnLayoutChanged));

        [Category(Category), Description("카드 행 모서리 둥글기")]
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty BaseColorProperty = DependencyProperty.Register(
            nameof(BaseColor), typeof(Color), typeof(CustomSkeleton), new FrameworkPropertyMetadata(ShPalette.SkeletonBaseColor, OnColorChanged));

        [Category(Category), Description("블록 기본 색")]
        public Color BaseColor
        {
            get => (Color)GetValue(BaseColorProperty);
            set => SetValue(BaseColorProperty, value);
        }

        public static readonly DependencyProperty HighlightColorProperty = DependencyProperty.Register(
            nameof(HighlightColor), typeof(Color), typeof(CustomSkeleton), new FrameworkPropertyMetadata(ShPalette.SkeletonHighlightColor, OnColorChanged));

        [Category(Category), Description("지나가는 빛 색")]
        public Color HighlightColor
        {
            get => (Color)GetValue(HighlightColorProperty);
            set => SetValue(HighlightColorProperty, value);
        }

        #endregion

        private static void OnLayoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((CustomSkeleton)d).BuildBlocks();

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (CustomSkeleton)d;
            // 생성자에서 GradientStop을 만들기 전에 기본값 콜백이 올 일은 없지만, 안전하게 null 검사.
            if (self._baseStart == null) return;
            self._baseStart.Color = self.BaseColor;
            self._baseEnd.Color = self.BaseColor;
            self._highlight.Color = self.HighlightColor;
        }

        #region Block building

        private void BuildBlocks()
        {
            // 생성자 InitializeComponent 이전 콜백 방지
            if (BlocksHost == null || _shimmerBrush == null) return;

            BlocksHost.Children.Clear();
            if (ShowHeader)
            {
                var title = CreateBlock(220, 28, 6);
                title.HorizontalAlignment = HorizontalAlignment.Left;
                title.Margin = new Thickness(0, 0, 0, 20);
                BlocksHost.Children.Add(title);
            }

            for (var i = 0; i < RowCount; i++)
                BlocksHost.Children.Add(CreateRow(LineRatios[i % LineRatios.Length]));
        }

        private FrameworkElement CreateRow(double lineRatio)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var icon = CreateBlock(28, 28, 6);
            icon.Margin = new Thickness(0, 0, 16, 0);
            Grid.SetColumn(icon, 0);
            grid.Children.Add(icon);

            var lines = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            lines.Children.Add(CreateRatioLine(lineRatio, 12));
            lines.Children.Add(CreateRatioLine(lineRatio * 0.6, 10, top: 8));
            Grid.SetColumn(lines, 1);
            grid.Children.Add(lines);

            var trailing = CreateBlock(64, 24, 12);
            trailing.Margin = new Thickness(16, 0, 0, 0);
            Grid.SetColumn(trailing, 2);
            grid.Children.Add(trailing);

            return new Border
            {
                Height = RowHeight,
                Margin = new Thickness(0, 0, 0, 4),
                Padding = new Thickness(16, 0, 16, 0),
                CornerRadius = CornerRadius,
                Background = RowBackground,
                BorderBrush = RowBorderBrush,
                BorderThickness = new Thickness(1),
                Child = grid,
            };
        }

        /// <summary>부모 폭의 ratio 비율만큼 차지하는 텍스트 줄 블록(창 크기가 바뀌어도 비율 유지).</summary>
        private FrameworkElement CreateRatioLine(double ratio, double height, double top = 0)
        {
            var host = new Grid { Margin = new Thickness(0, top, 0, 0) };
            host.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(ratio, GridUnitType.Star) });
            host.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1 - ratio, GridUnitType.Star) });
            host.Children.Add(CreateBlock(double.NaN, height, height / 2));
            return host;
        }

        private Border CreateBlock(double width, double height, double radius)
        {
            return new Border
            {
                Width = width,
                Height = height,
                CornerRadius = new CornerRadius(radius),
                Background = _shimmerBrush,
            };
        }

        #endregion

        #region Animation

        private void UpdateAnimation()
        {
            var shouldAnimate = IsActive && IsLoaded && IsVisible;
            if (shouldAnimate == _isAnimating) return;
            _isAnimating = shouldAnimate;

            if (!shouldAnimate)
            {
                // WHY: 보이지 않는 애니메이션도 렌더 스레드를 계속 깨운다. 숨겨지면 반드시 멈춘다.
                _shimmerShift.BeginAnimation(TranslateTransform.XProperty, null);
                return;
            }

            var sweep = new DoubleAnimation(-1, 1, TimeSpan.FromMilliseconds(1400))
            {
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut },
            };
            _shimmerShift.BeginAnimation(TranslateTransform.XProperty, sweep);
        }

        #endregion
    }
}
