using System.Windows;
using System.Windows.Markup;

// WHY: 컨트롤 기본 스타일을 이 어셈블리의 Themes/Generic.xaml 에서 찾도록 WPF에 알린다.
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]

// WHY: 사용하는 쪽 XAML에서 clr-namespace를 여러 줄 쓰지 않고
//      xmlns:sh="urn:shComponent" 한 줄로 모든 컨트롤/모델을 쓰도록 네임스페이스를 묶는다.
[assembly: XmlnsDefinition("urn:shComponent", "shComponent")]
[assembly: XmlnsDefinition("urn:shComponent", "shComponent.Navigation")]
[assembly: XmlnsDefinition("urn:shComponent", "shComponent.Theming")]
[assembly: XmlnsPrefix("urn:shComponent", "sh")]
