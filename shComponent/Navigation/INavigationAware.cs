namespace shComponent.Navigation
{
    /// <summary>
    /// 페이지(또는 페이지의 ViewModel을 호출하는 코드비하인드)가 화면 전환 시점을 통지받기 위한 선택적 계약.
    /// WHY: 프레임은 페이지 내부 로직을 모른다. 대신 "들어왔다/나갔다" 시점만 알려 주고 처리는 페이지에 맡긴다.
    /// </summary>
    public interface INavigationAware
    {
        /// <summary>페이지가 화면에 표시된 직후 호출(로딩이 있으면 로딩 완료 후).</summary>
        void OnNavigatedTo(object? parameter);

        /// <summary>다른 페이지로 이동하기 직전 호출(타이머 정지, 구독 해제 등).</summary>
        void OnNavigatedFrom();
    }
}
