using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using shComponent.Navigation;

namespace shComponent.Demo.Pages
{
    /// <summary>
    /// 새 페이지 템플릿. 필요 없는 인터페이스는 지워도 된다.
    ///  - IAsyncLoadable   : 데이터 로딩(로딩 중 스켈레톤 자동 표시)
    ///  - INavigationAware : 화면 진입/이탈 시점 통지
    /// </summary>
    public partial class TemplatePage : UserControl, IAsyncLoadable, INavigationAware
    {
        public TemplatePage()
        {
            InitializeComponent();
        }

        public async Task LoadAsync(object? parameter, CancellationToken cancellationToken)
        {
            // TODO: 실제 데이터 조회로 교체. 무거운 작업은 Task.Run 안에서.
            var items = await Task.Run(async () =>
            {
                await Task.Delay(800, cancellationToken);
                return Enumerable.Range(1, 3).Select(i => $"Item{i}").ToArray();
            }, cancellationToken);

            ItemsHost.Children.Clear();
            foreach (var item in items)
                ItemsHost.Children.Add(new CustomCard { Header = item, Description = "Description", Margin = new Thickness(0, 0, 0, 4) });
        }

        public void OnNavigatedTo(object? parameter)
        {
            InfoText.Text = "이 페이지(TemplatePage)를 복사해서 새 화면을 만드세요.";
        }

        public void OnNavigatedFrom()
        {
            // TODO: 타이머 정지, 이벤트 구독 해제 등
        }

        private void OnPrimaryClick(object sender, RoutedEventArgs e)
        {
            // TODO: 업무 동작. (ViewModel을 쓴다면 CustomButton.Command 에 연결)
        }
    }
}
