using System.Threading;
using System.Threading.Tasks;

namespace shComponent.Navigation
{
    /// <summary>
    /// 표시 전에 데이터를 비동기로 불러와야 하는 페이지용 계약.
    /// 이 인터페이스를 구현하면 CustomPanel이 로딩 동안 스켈레톤을 자동으로 보여 준다.
    /// WHY: 로딩 UI 처리를 각 페이지마다 반복하지 않고 프레임 한 곳에서 일관되게 처리하기 위함.
    /// </summary>
    public interface IAsyncLoadable
    {
        /// <summary>
        /// 데이터를 불러온다. UI 스레드에서 호출되므로 무거운 작업은 내부에서 <c>await Task.Run(...)</c> 으로 분리할 것.
        /// 사용자가 로딩 중 다른 메뉴로 이동하면 <paramref name="cancellationToken"/>이 취소된다.
        /// 예외를 던지면 패널이 오류 화면(다시 시도 버튼 포함)을 표시한다.
        /// </summary>
        Task LoadAsync(object? parameter, CancellationToken cancellationToken);
    }
}
