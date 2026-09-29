using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using shComponent.Navigation;

namespace shComponent.Demo.Pages
{
    /// <summary>로딩 실패 → 오류 화면 → "다시 시도" 흐름 예시. 파라미터(NavItem&lt;int&gt;.Data)만큼 실패 후 성공.</summary>
    public class FailingPage : UserControl, IAsyncLoadable
    {
        private int _attempts;

        public async Task LoadAsync(object? parameter, CancellationToken cancellationToken)
        {
            await Task.Delay(800, cancellationToken);
            var failCount = parameter is int n ? n : 1;
            if (++_attempts <= failCount)
                throw new InvalidOperationException($"서버 응답 시간 초과 (시도 {_attempts}/{failCount + 1})");

            Content = new TextBlock { Text = $"{_attempts}번째 시도에서 로딩 성공!", FontSize = 16 };
        }
    }
}
