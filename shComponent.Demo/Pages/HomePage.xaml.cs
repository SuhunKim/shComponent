using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using shComponent.Navigation;

namespace shComponent.Demo.Pages
{
    /// <summary>IAsyncLoadable 예시: 로딩하는 1.2초 동안 프레임이 자동으로 스켈레톤을 보여 준다.</summary>
    public partial class HomePage : UserControl, IAsyncLoadable, INavigationAware
    {
        public HomePage()
        {
            InitializeComponent();
        }

        public async Task LoadAsync(object? parameter, CancellationToken cancellationToken)
        {
            // 실제 앱이라면: var items = await _service.GetSummaryAsync(cancellationToken);
            var items = await Task.Run(async () =>
            {
                await Task.Delay(1200, cancellationToken);
                return Enumerable.Range(1, 4)
                    .Select(i => (Key: $"menu{i}", Title: $"Menu{i}", Description: $"Menu{i} 바로가기"))
                    .ToArray();
            }, cancellationToken);

            CardsHost.Children.Clear();
            foreach (var item in items)
            {
                // WHY: 페이지는 프레임(CustomMain)을 모른다. 이동할 Key만 Tag에 담아 두면
                //      Click 라우티드 이벤트가 위로 올라가 MainWindow가 이동을 처리한다.
                CardsHost.Children.Add(new CustomCard
                {
                    Glyph = "",
                    Header = item.Title,
                    Description = item.Description,
                    IsClickable = true,
                    Tag = item.Key,
                    Margin = new Thickness(0, 0, 0, 4),
                });
            }
        }

        public void OnNavigatedTo(object? parameter) => WelcomeText.Text = $"마지막 새로 고침: {DateTime.Now:HH:mm:ss}";

        public void OnNavigatedFrom() { }
    }
}
