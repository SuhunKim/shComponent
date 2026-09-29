using System.Windows;

namespace shComponent.Navigation
{
    /// <summary>
    /// 메뉴 항목 → 화면(View) 생성 규칙.
    /// WHY: 프레임(CustomMain)이 어떤 페이지 클래스가 있는지 몰라야 재사용 가능하다.
    ///      "무엇을 보여줄지"는 앱이, "어떻게 보여줄지"는 프레임이 책임지도록 경계를 인터페이스로 끊는다.
    ///      DI 컨테이너를 쓰는 프로젝트라면 이 인터페이스를 직접 구현하면 된다.
    /// </summary>
    public interface IPageFactory
    {
        /// <summary>항목에 해당하는 페이지를 만든다. 등록되지 않았으면 null.</summary>
        FrameworkElement? CreatePage(INavItem item);
    }
}
