using System.Windows;
using System.Windows.Controls;

namespace shComponent.Demo.Pages
{
    public partial class CardsPage : UserControl
    {
        public CardsPage()
        {
            InitializeComponent();
        }

        private void OnCardClick(object sender, RoutedEventArgs e) => MessageBox.Show("Card3 클릭");
    }
}
