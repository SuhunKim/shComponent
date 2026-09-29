using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using shComponent.Internal;
using shComponent.Theming;

namespace shComponent
{
    /// <summary>제목이 있는 둥근 컨테이너(섹션). Header를 비우면 순수 컨테이너로 쓸 수 있다.</summary>
    [ToolboxItem(true)]
    [Description("Windows 11 스타일 그룹/컨테이너")]
    public class CustomGroupBox : UserControl
    {
        private const string Category = "sh 모양";

        static CustomGroupBox()
        {
            // WHY: UserControl 기본 테마 스타일(Border 템플릿)이 배경/테두리를 따로 그리지 않도록 스타일 조회를 끊고 ContentPresenter만 남긴다.
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomGroupBox), new FrameworkPropertyMetadata(typeof(CustomGroupBox)));
            TemplateProperty.OverrideMetadata(typeof(CustomGroupBox), new FrameworkPropertyMetadata(ShBareTemplate.Instance));
            BackgroundProperty.OverrideMetadata(typeof(CustomGroupBox), new FrameworkPropertyMetadata(ShPalette.CardBackground));
            BorderBrushProperty.OverrideMetadata(typeof(CustomGroupBox), new FrameworkPropertyMetadata(ShPalette.CardBorder));
            BorderThicknessProperty.OverrideMetadata(typeof(CustomGroupBox), new FrameworkPropertyMetadata(new Thickness(1)));
            ForegroundProperty.OverrideMetadata(typeof(CustomGroupBox), new FrameworkPropertyMetadata(ShPalette.TextPrimary, FrameworkPropertyMetadataOptions.Inherits));
            FontFamilyProperty.OverrideMetadata(typeof(CustomGroupBox), new FrameworkPropertyMetadata(ShPalette.TextFont, FrameworkPropertyMetadataOptions.Inherits));
            FontSizeProperty.OverrideMetadata(typeof(CustomGroupBox), new FrameworkPropertyMetadata(ShPalette.BodyFontSize, FrameworkPropertyMetadataOptions.Inherits));
            PaddingProperty.OverrideMetadata(typeof(CustomGroupBox), new FrameworkPropertyMetadata(new Thickness(16)));
        }

        public CustomGroupBox()
        {
            // WHY: 모양(XAML)은 내부 뷰가 담당한다. 이 클래스는 XAML 없이 코드로만 정의되어야
            //      사용하는 쪽 XAML에서 자식 요소에 x:Name을 쓸 수 있다(x:Class XAML 타입은 이름 범위가 막힌다).
            base.Content = new CustomGroupBoxView();
        }

        #region Dependency Properties

        // WHY: UserControl.Content는 XAML의 루트 요소가 차지하므로, 사용자가 쓰는 Content(안쪽 내용)는 별도 속성으로 가려 둔다.
        //      XAML의 자식 요소 / Content="..." 는 이 속성으로 들어온다.
        public new static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            nameof(Content), typeof(object), typeof(CustomGroupBox), new FrameworkPropertyMetadata(null));

        [Category(Category), Description("컨테이너 안에 들어갈 내용")]
        public new object? Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            nameof(Header), typeof(object), typeof(CustomGroupBox),
            new FrameworkPropertyMetadata(null, null, CustomButton.CoerceEmptyToNull));

        [Category(Category), Description("섹션 제목")]
        public object? Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(CustomGroupBox), new FrameworkPropertyMetadata(ShPalette.OverlayCornerRadius));

        [Category(Category), Description("모서리 둥글기")]
        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty HeaderFontSizeProperty = DependencyProperty.Register(
            nameof(HeaderFontSize), typeof(double), typeof(CustomGroupBox), new FrameworkPropertyMetadata(14d));

        [Category(Category), Description("제목 글자 크기")]
        public double HeaderFontSize
        {
            get => (double)GetValue(HeaderFontSizeProperty);
            set => SetValue(HeaderFontSizeProperty, value);
        }

        public static readonly DependencyProperty HeaderForegroundProperty = DependencyProperty.Register(
            nameof(HeaderForeground), typeof(Brush), typeof(CustomGroupBox), new FrameworkPropertyMetadata(ShPalette.TextPrimary));

        [Category(Category), Description("제목 글자색")]
        public Brush HeaderForeground
        {
            get => (Brush)GetValue(HeaderForegroundProperty);
            set => SetValue(HeaderForegroundProperty, value);
        }

        public static readonly DependencyProperty HeaderMarginProperty = DependencyProperty.Register(
            nameof(HeaderMargin), typeof(Thickness), typeof(CustomGroupBox), new FrameworkPropertyMetadata(new Thickness(2, 0, 0, 8)));

        [Category(Category), Description("제목과 컨테이너 사이 여백")]
        public Thickness HeaderMargin
        {
            get => (Thickness)GetValue(HeaderMarginProperty);
            set => SetValue(HeaderMarginProperty, value);
        }

        #endregion
    }
}
