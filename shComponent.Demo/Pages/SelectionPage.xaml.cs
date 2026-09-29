using System.Windows.Controls;

namespace shComponent.Demo.Pages
{
    public partial class SelectionPage : UserControl
    {
        public SelectionPage()
        {
            InitializeComponent();

            // 바인딩 대신 코드로 항목을 채우고, 선택 변화는 이벤트로 받는다.
            for (var i = 1; i <= 4; i++)
                CodeCombo.Items.Add($"Item{i}");

            CodeCombo.SelectionChanged += (_, __) => SelectionText.Text = $"선택: {CodeCombo.SelectedItem}";
        }
    }
}
