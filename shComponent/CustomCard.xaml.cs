using System.ComponentModel;
using System.Windows.Controls;

namespace shComponent.Internal
{
    /// <summary>CustomCard의 모양(XAML). 디자이너에서 이 파일(CustomCard.xaml)을 열어 편집한다. 속성 값은 부모 CustomCard에서 바인딩으로 받는다.</summary>
    // WHY: 내부 뷰이므로 도구상자에는 겉 컨트롤(CustomCard)만 보이게 한다.
    [ToolboxItem(false)]
    public partial class CustomCardView : UserControl
    {
        public CustomCardView()
        {
            InitializeComponent();
        }
    }
}
