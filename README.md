# shComponent

[![CI](https://github.com/SuhunKim/shComponent/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/SuhunKim/shComponent/actions/workflows/ci.yml)

Windows 11 설정 앱 스타일의 **재사용 WPF UI 프레임** 라이브러리.
새 프로그램마다 반복하던 "메인 프레임 / 메뉴 / 서브메뉴 / 콘텐츠 패널 / 기본 컨트롤 / 로딩 화면"을 한 세트로 제공한다.

- 대상: `.NET Framework 4.8` + `.NET 8 (net8.0-windows)` — 한 번 빌드로 DLL 두 개 생성
- 테마: 라이트 (모든 색은 속성으로 변경 가능, 전사 기본 톤은 `Theming/ShPalette.cs` 한 파일)

## 파일 구조

```
shComponent/
├─ CustomMain.xaml / .cs         최상위 프레임 (UserControl) : 레이아웃 + 메뉴→서브메뉴→패널 연결
├─ CustomMenu.xaml / .cs         1차 메뉴 (좌측 세로 / 상단 가로, 컴팩트 모드)
├─ CustomSubMenu.xaml / .cs      2차 서브메뉴 (콘텐츠 상단 가로 탭 / 세로 목록)
├─ CustomPanel.xaml / .cs        콘텐츠 패널 : 페이지 표시, 스켈레톤 로딩, 오류/다시 시도, 전환 효과
├─ CustomSkeleton.xaml / .cs     로딩용 스켈레톤 (shimmer 애니메이션)
├─ CustomButton.xaml / .cs       버튼 (Standard / Accent / Subtle, 아이콘)
├─ CustomComboBox.xaml / .cs     콤보박스 (PlaceholderText, 둥근 드롭다운)
├─ CustomCheckBox.xaml / .cs     체크박스 (3상태 지원)
├─ CustomRadioButton.xaml / .cs  라디오 버튼
├─ CustomGroupBox.xaml / .cs     제목 + 둥근 컨테이너
├─ CustomCard.xaml / .cs         설정 카드 (아이콘/제목/설명/오른쪽 컨트롤, 클릭 가능) 또는 일반 카드
├─ Navigation/                   INavItem, NavItem, NavItem<T>, IPageFactory, PageFactory,
│                                INavigationAware, IAsyncLoadable, 이벤트 인자, NavTree
├─ Theming/                      ShPalette(색/서체 토큰), ShFocusVisual(둥근 포커스 링)
├─ Themes/                       Generic.xaml(현재 비어 있음), ScrollBar.xaml(얇은 스크롤바)
├─ Internal/                     NavButtonHost(메뉴 선택 로직 공유), DesignTimeData(디자이너 샘플)
└─ Common/                       ButtonAppearance, MenuPlacement
shComponent.Demo/                사용 예제 앱 (Pages/TemplatePage = 새 페이지 시작용 템플릿)
shComponent.Tests/               xUnit 테스트 (WPF 컨트롤은 [WpfFact])
```

> **모든 컴포넌트는 Visual Studio 디자이너에서 열린다** (`Custom*.xaml` 더블클릭 → 디자인 뷰)
> - `CustomMain / Menu / SubMenu / Panel / Skeleton`, `CustomButton / CheckBox / RadioButton / ComboBox` : `x:Class` UserControl.
>   Button·CheckBox·RadioButton·ComboBox는 안쪽에 진짜 네이티브 컨트롤을 넣어(래핑) Click/IsChecked/GroupName/Items 등 동작을 그대로 쓴다.
>   글자(`Content`)는 UserControl의 Content와 겹쳐서 컨트롤마다 별도 속성으로 가려 두었다(사용법은 동일).
> - `CustomGroupBox / CustomCard` (내용물을 담는 컨테이너) : `CustomX.xaml` = 모양(내부 뷰 `CustomXView`, 디자이너에서 편집),
>   `CustomX.cs` = 실제 컨트롤(XAML 없이 코드로만 정의).
>
> 컨테이너를 이렇게 나눈 이유: `x:Class` XAML로 정의된 타입 **안에 넣은 요소에는 `x:Name`을 쓸 수 없다**(WPF 이름 범위 규칙, 오류 MC3093).
> 겉 컨트롤을 코드로만 정의하면 사용자가 `<sh:CustomGroupBox><TextBox x:Name="..."/></sh:CustomGroupBox>` 처럼 평소대로 이름을 쓸 수 있다.
> (Button/CheckBox/RadioButton/ComboBox 안쪽에 `x:Name` 자식을 넣는 사용은 지원하지 않는다.)

## 빠른 시작

**MainWindow.xaml**
```xml
<Window ... xmlns:sh="urn:shComponent">
    <sh:CustomMain x:Name="Shell" AppTitle="내 프로그램" AppGlyph="&#xE80F;"/>
</Window>
```

**MainWindow.xaml.cs**
```csharp
// 1) Key → 페이지 등록
Shell.PageFactory = new PageFactory()
    .Register<HomePage>("home")
    .Register<SubMenu1Page>("menu1.sub1", cache: true)                // cache: 인스턴스 재사용(입력 상태 유지)
    .Register("menu1.sub2", () => new SubMenu2Page(myService))        // 서비스 주입
    .Register<Menu2Page>("menu2");

// 2) 메뉴 트리 — 이름은 자리표시자, 프로젝트에 맞게 바꿔 쓴다 (Children이 있으면 서브메뉴 탭으로 표시)
Shell.SetMenu(new INavItem[]
{
    new NavItem("home", "홈", ""),
    new NavItem("menu1", "Menu1", "")
        .Add(new NavItem("menu1.sub1", "SubMenu1"))
        .Add(new NavItem<int>("menu1.sub2", "SubMenu2", data: 2025)), // 형식 있는 파라미터
    new NavItem("menu2", "Menu2", ""),
});

// 3) 상태 변화는 이벤트로 받는다 (업무 로직은 여기서)
Shell.Navigating     += (s, e) => { if (hasUnsavedChanges) e.Cancel = true; };
Shell.Navigated      += (s, e) => log.Info($"{e.Previous?.Key} -> {e.Item.Key}");
Shell.PageLoadFailed += (s, e) => log.Error(e.Exception);

// 코드에서 이동
Shell.Navigate("menu1.sub1");
```

## 페이지 작성

페이지는 아무 `FrameworkElement`(보통 UserControl)면 된다. 필요한 인터페이스만 골라 구현한다.

```csharp
public partial class SubMenu1Page : UserControl, IAsyncLoadable, INavigationAware
{
    // 구현하면 로딩 동안 스켈레톤이 자동 표시된다. 예외를 던지면 오류 화면 + "다시 시도".
    public async Task LoadAsync(object? parameter, CancellationToken ct)
    {
        var rows = await Task.Run(() => _service.GetRows(), ct);   // 무거운 작업은 Task.Run
        MainGrid.ItemsSource = rows;
    }

    public void OnNavigatedTo(object? parameter) { /* 화면 표시 직후 */ }
    public void OnNavigatedFrom()                 { /* 떠나기 직전 (타이머 정지 등) */ }
}
```

- 로딩이 `SkeletonDelayMs`(기본 120ms)보다 빠르면 스켈레톤을 보이지 않는다 → 번쩍임 방지
- 한 번 보이면 `MinSkeletonMs`(기본 400ms) 동안 유지 → 깜빡임 방지
- 로딩 중 다른 메뉴를 누르면 `CancellationToken`이 취소되고, 늦게 끝난 이전 결과는 버려진다
- DataGrid처럼 자체 스크롤이 있는 페이지: `Shell.Panel.IsScrollEnabled = false`
- IAsyncLoadable 없이 직접 제어: `Shell.Panel.IsLoading = true / false`

## 커스터마이징 (디자이너 속성 창의 **"sh 모양" / "sh 동작"** 범주)

| 컨트롤 | 주요 속성 |
|---|---|
| CustomMain | `MenuPlacement`(Left/Top), `PaneWidth`, `IsPaneCompact`, `PaneBackground`, `ContentBackground`, `AccentBrush`(하위 메뉴로 전달), `ContentPadding`, `PageTitleFontSize` |
| CustomMenu / SubMenu | `Orientation`, `ItemHeight`, `ItemFontSize`, `HoverBackground`, `SelectedBackground`, `AccentBrush`, `ItemCornerRadius`, `SelectCommand` |
| CustomButton | `Appearance`, `CornerRadius`, `Background`, `HoverBackground`, `PressedBackground`, `AccentBrush`, `Glyph` |
| CustomComboBox | `CornerRadius`, `PlaceholderText`, `PopupBackground`, `ItemHoverBackground`, `AccentBrush` |
| CustomCheckBox / RadioButton | `AccentBrush`, `BoxSize`/`CircleSize`, `HoverBackground` |
| CustomCard | `Header`, `Description`, `Glyph`, `IsClickable`(+`Click`/`Command`), `CornerRadius`, `HoverBackground` |
| CustomSkeleton | `RowCount`, `RowHeight`, `ShowHeader`, `BaseColor`, `HighlightColor` |

전사 기본 톤을 바꾸려면 `Theming/ShPalette.cs`의 색만 수정 후 다시 빌드.
아이콘은 [Segoe Fluent Icons](https://learn.microsoft.com/windows/apps/design/style/segoe-fluent-icons-font) 글리프 코드(예: `&#xE80F;`)를 사용.

## 도구상자 등록

1. `Release`로 빌드: `dotnet build shComponent/shComponent.csproj -c Release`
   → `shComponent/bin/Release/net48/shComponent.dll`, `.../net8.0-windows/shComponent.dll`
2. **.NET Framework 프로젝트**: 도구 상자 빈 곳 우클릭 → *항목 선택* → *WPF 구성 요소* 탭 → *찾아보기* → `net48\shComponent.dll` 선택
3. **.NET 8 프로젝트**: 프로젝트에 DLL(또는 이 프로젝트)을 참조로 추가하면 도구 상자에 *shComponent 컨트롤* 그룹이 자동으로 나타난다
   (VS의 "항목 선택" 대화상자는 .NET Core 어셈블리를 제대로 읽지 못하는 경우가 있음)
4. 도구 상자에서 끌어다 놓으면 XAML에 `xmlns:sh="urn:shComponent"`가 추가된다.

## 설계 원칙

- **범용성**: 메뉴는 `INavItem` 인터페이스만 요구한다. 기존 모델에 인터페이스만 구현하거나 `NavItem` / `NavItem<T>`를 쓴다. 페이지 생성은 `IPageFactory`(DI 컨테이너 연동 가능).
- **비즈니스 로직 없음**: 컨트롤은 상태 변화를 이벤트(`SelectionChanged`, `Navigating`, `Navigated`, `Click`, `PageLoadFailed`)와 `Command`로만 알린다.
- **바인딩 최소화**: 메뉴 버튼은 `ItemsSource` 없이 코드에서 직접 생성하고, 속성 변경은 콜백에서 명시적으로 화면에 반영한다. 사용자 코드에서 바인딩 없이 전부 쓸 수 있다. (컨트롤 템플릿 내부의 `TemplateBinding`은 WPF 표준 방식이라 유지)
- **SRP**: 프레임(배치·연결) / 메뉴(선택) / 패널(표시·로딩) / 스켈레톤(로딩 모양) / 선택 로직(NavButtonHost) / 색상 토큰(ShPalette)을 분리.

## 현재 제약

- 메뉴 깊이는 2단계(1차 메뉴 → 서브메뉴)까지
- `CustomComboBox`는 선택 전용 (`IsEditable` 미지원)
- 라이트 테마만 제공

## 개발 / 빌드

**준비물**: Windows 10/11, [.NET SDK 10.0.401](https://dotnet.microsoft.com/download/dotnet/10.0) 이상 패치(`global.json`으로 고정), Visual Studio 2022+ (선택)

```powershell
git clone https://github.com/SuhunKim/shComponent.git
cd shComponent
.\build.ps1              # CI와 완전히 같은 단계: restore → format 검사 → build → test → coverage
.\build.ps1 -SkipFormat  # 작업 중 빠르게
dotnet format shComponent.sln   # format 검사 실패 시 자동 수정
```

결과물은 `artifacts\` 에 모인다.

| 폴더 | 내용 |
|---|---|
| `artifacts\lib` | `net48\shComponent.dll`, `net8.0-windows\shComponent.dll` |
| `artifacts\demo` | 데모 앱 |
| `artifacts\test-results` | `.trx` 테스트 결과 (VS에서 열림) |
| `artifacts\coverage\index.html` | 커버리지 리포트 (현재는 측정만, 최소 기준 없음) |

빌드 규칙 파일
- `global.json` : SDK 버전 고정 + 테스트 러너(Microsoft Testing Platform)
- `Directory.Build.props` : 모든 프로젝트 공통 (결정적 빌드, SourceLink, Release에서 경고 = 오류)
- `.editorconfig` / `.gitattributes` : 코드 스타일, 줄끝(LF, `.sln`만 CRLF)
- 버전: csproj에 적지 않는다. [MinVer](https://github.com/adamralph/minver)가 Git 태그(`v1.2.3`)로 계산

## 테스트

`shComponent.Tests` (xUnit v3, net48 + net8.0-windows 둘 다 실행)

- WPF 객체를 만드는 테스트는 `[Fact]` 대신 **`[WpfFact]`** (Xunit.StaFact) — STA 스레드 + Dispatcher에서 실행된다
- 모델/유틸 테스트는 일반 `[Fact]`
- VS 테스트 탐색기 또는 `dotnet test --solution shComponent.sln`

## CI (GitHub Actions)

`.github/workflows/ci.yml` — PR(→main)과 main push 때 `windows-latest`에서 `build.ps1`을 실행한다.
실행 결과의 **Summary** 탭에 커버리지 표, **Artifacts**에 테스트 결과·DLL·데모가 올라간다.

**작업 흐름 (GitHub Flow)**
```powershell
git switch -c feature/메뉴-아이콘     # main에서 브랜치
# ... 작업 ...
.\build.ps1                          # 푸시 전에 로컬에서 CI 미리 확인
git push -u origin feature/메뉴-아이콘  # → GitHub에서 PR 생성 → CI 통과 후 머지
```

**main 보호 규칙 (최초 1회, 저장소 관리자)**
1. CI가 한 번 이상 실행된 뒤(그래야 검사 이름이 목록에 뜬다)
2. GitHub 저장소 → *Settings* → *Rules* → *Rulesets* → *New branch ruleset*
3. Target: `Default branch`, Enforcement: `Active`
4. ✅ *Require a pull request before merging*
5. ✅ *Require status checks to pass* → *Add checks* → **`build`** 선택
6. ✅ *Block force pushes* → 저장

## 릴리스 (CD — 구축 예정)

버전은 태그로 정한다. 태그가 없는 커밋은 `1.0.0-alpha.0.N` 처럼 자동으로 붙는다.
CD 워크플로가 추가되면 `git tag v1.0.0; git push origin v1.0.0` 한 번으로 NuGet 패키지 + Release가 만들어진다.

## 트러블슈팅

| 증상 | 원인 | 해결 |
|---|---|---|
| `error ENDOFLINE: 줄의 끝 마커를 수정하세요` (format 검사) | 편집기/Git 설정 때문에 파일이 CRLF로 저장됨 | `dotnet format shComponent.sln` 실행 후 커밋. 줄끝은 `.editorconfig`(LF)와 `.gitattributes`가 정한다 |
| `A compatible .NET SDK was not found` / `global.json` 관련 오류 | `global.json`의 SDK(10.0.401 이상 패치) 미설치 | `dotnet --list-sdks`로 확인 후 SDK 설치. 다른 기능 대역(10.0.5xx 등)만 있다면 `global.json` 버전을 팀 합의 후 올린다 |
| `MSB3644: .NETFramework,Version=v4.8 참조 어셈블리를 찾을 수 없습니다` | .NET Framework 4.8 Developer Pack 미설치 + 오프라인이라 NuGet 대체 패키지(`Microsoft.NETFramework.ReferenceAssemblies`)도 못 받음 | [4.8 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net48) 설치 또는 nuget.org 접근 허용 |
| `호출 스레드는 STA여야 합니다` (테스트) | WPF 객체를 `[Fact]` 테스트에서 생성 | `[WpfFact]` / `[WpfTheory]` 사용 |
| DLL 버전이 `0.0.0` 이거나 태그가 반영 안 됨 | 얕은 복제(태그/이력 없음) 또는 태그를 push 안 함 | CI: `fetch-depth: 0` 유지, 로컬: `git fetch --tags`, 태그는 `git push origin v1.2.3` |
| `.\build.ps1` 실행 불가 (`실행 정책`) | PowerShell 실행 정책 | `powershell -ExecutionPolicy Bypass -File .\build.ps1` |
