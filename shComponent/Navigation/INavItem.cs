using System.Collections.Generic;

namespace shComponent.Navigation
{
    /// <summary>
    /// 메뉴 한 항목의 최소 계약.
    /// WHY: 메뉴 컨트롤이 특정 모델(DB 엔티티, 권한 모델 등)에 묶이지 않도록 "화면에 필요한 정보"만 인터페이스로 요구한다.
    ///      기존 모델이 있다면 이 인터페이스만 구현하면 되고, 없다면 <see cref="NavItem"/>을 그대로 쓰면 된다.
    /// </summary>
    public interface INavItem
    {
        /// <summary>항목 식별자. 페이지 등록(PageFactory)과 Navigate(key)에 사용.</summary>
        string Key { get; }

        /// <summary>화면에 표시할 제목.</summary>
        string Title { get; }

        /// <summary>Segoe Fluent Icons 글리프 문자(예: ""). 없으면 null.</summary>
        string? Glyph { get; }

        /// <summary>페이지로 전달할 기본 파라미터(INavigationAware.OnNavigatedTo / IAsyncLoadable.LoadAsync).</summary>
        object? Parameter { get; }

        /// <summary>false면 메뉴에 비활성으로 표시되고 선택할 수 없다.</summary>
        bool IsEnabled { get; }

        /// <summary>2차 메뉴(서브메뉴) 항목. 비어 있으면 이 항목 자체가 페이지.</summary>
        IReadOnlyList<INavItem> Children { get; }
    }
}
