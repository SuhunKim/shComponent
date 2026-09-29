using System;
using System.ComponentModel;
using System.Windows;

namespace shComponent.Navigation
{
    /// <summary>메뉴 선택/이동 완료 이벤트 데이터.</summary>
    public class NavItemEventArgs : EventArgs
    {
        public NavItemEventArgs(INavItem item, INavItem? previous)
        {
            Item = item;
            Previous = previous;
        }

        public INavItem Item { get; }
        public INavItem? Previous { get; }
    }

    /// <summary>
    /// 이동 직전 이벤트 데이터. Cancel = true 로 이동을 막을 수 있다(저장 안 된 변경 확인 등).
    /// WHY: "이동해도 되는가"는 업무 규칙이므로 프레임이 판단하지 않고 부모에게 묻는다.
    /// </summary>
    public class NavigatingEventArgs : CancelEventArgs
    {
        public NavigatingEventArgs(INavItem item, INavItem? current, object? parameter)
        {
            Item = item;
            Current = current;
            Parameter = parameter;
        }

        /// <summary>이동하려는 대상 항목.</summary>
        public INavItem Item { get; }

        /// <summary>현재 표시 중인 항목(최초 이동이면 null).</summary>
        public INavItem? Current { get; }

        public object? Parameter { get; }
    }

    /// <summary>페이지 로딩(IAsyncLoadable.LoadAsync) 실패 이벤트 데이터.</summary>
    public class PageLoadFailedEventArgs : EventArgs
    {
        public PageLoadFailedEventArgs(FrameworkElement page, Exception exception)
        {
            Page = page;
            Exception = exception;
        }

        public FrameworkElement Page { get; }
        public Exception Exception { get; }
    }
}
