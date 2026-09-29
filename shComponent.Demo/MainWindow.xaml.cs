using System.Diagnostics;
using System.Windows;
using shComponent.Demo.Pages;
using shComponent.Navigation;

namespace shComponent.Demo
{
    public partial class MainWindow : Window
    {
        private const string MenuGlyph = "";

        public MainWindow()
        {
            InitializeComponent();

            // 1) 어떤 Key가 어떤 화면인지 등록 (페이지 생성 규칙은 앱의 책임)
            Shell.PageFactory = new PageFactory()
                .Register<HomePage>("home")
                .Register<ButtonsPage>("menu1.sub1", cache: true)
                .Register<SelectionPage>("menu1.sub2", cache: true)
                .Register<CardsPage>("menu1.sub3", cache: true)
                .Register<TemplatePage>("menu2.sub1")
                .Register<TemplatePage>("menu2.sub2")
                .Register<FailingPage>("menu3");

            // 2) 메뉴 트리 — 이름은 자리표시자. 실제 프로젝트에서 이 부분만 바꾸면 된다.
            //    (Children이 있으면 서브메뉴 탭으로 표시)
            Shell.SetMenu(new INavItem[]
            {
                new NavItem("home", "홈", ""),
                new NavItem("menu1", "Menu1", MenuGlyph)          // 컨트롤 샘플
                    .Add(new NavItem("menu1.sub1", "SubMenu1"))   //  - 버튼
                    .Add(new NavItem("menu1.sub2", "SubMenu2"))   //  - 체크박스/라디오/콤보
                    .Add(new NavItem("menu1.sub3", "SubMenu3")),  //  - 카드/그룹/스켈레톤
                new NavItem("menu2", "Menu2", MenuGlyph)          // 새 페이지 템플릿
                    .Add(new NavItem("menu2.sub1", "SubMenu1"))
                    .Add(new NavItem("menu2.sub2", "SubMenu2")),
                new NavItem<int>("menu3", "Menu3", data: 1, glyph: MenuGlyph), // 로딩 실패 → 다시 시도 예시
                new NavItem("menu4", "Menu4", MenuGlyph),                      // 페이지 미등록 예시
                new NavItem("menu5", "Menu5", MenuGlyph) { IsEnabled = false }, // 비활성 메뉴 예시
            });

            // 3) 상태 변화는 이벤트로 받는다 (업무 로직/로그는 여기서)
            Shell.Navigated += (_, e) => Debug.WriteLine($"[Navigated] {e.Previous?.Key} -> {e.Item.Key}");
            Shell.PageLoadFailed += (_, e) => Debug.WriteLine($"[LoadFailed] {e.Exception.Message}");

            // 페이지 안의 카드 클릭(라우티드 이벤트)을 받아 Tag에 담긴 Key로 이동 — 홈 화면 바로가기
            Shell.AddHandler(CustomCard.ClickEvent, new RoutedEventHandler((_, e) =>
            {
                if (e.OriginalSource is CustomCard { Tag: string key }) Shell.Navigate(key);
            }));

            SnapshotRunner.AttachIfRequested(this, Shell);
        }
    }
}
