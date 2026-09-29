namespace shComponent.Navigation
{
    /// <summary>
    /// 페이지에 넘길 업무 데이터를 형식 안전하게 들고 다니는 메뉴 항목.
    /// WHY: object 파라미터를 매번 캐스팅하면 실수가 런타임에야 드러난다. 제네릭으로 컴파일 시점에 형식을 고정한다.
    /// <code>new NavItem&lt;int&gt;("menu2", "Menu2", data: 42)</code>
    /// </summary>
    public class NavItem<T> : NavItem
    {
        public NavItem(string key, string title, T data, string? glyph = null)
            : base(key, title, glyph)
        {
            Data = data;
            Parameter = data;
        }

        /// <summary>형식이 보장된 업무 데이터. 페이지에서는 Parameter를 T로 캐스팅해 사용.</summary>
        public T Data { get; }
    }
}
