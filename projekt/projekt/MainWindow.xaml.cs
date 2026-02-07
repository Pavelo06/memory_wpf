using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace projekt
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            PrzypisanieUidImgKarty();
        }

        Random rand = new Random();

        //zrobić całą logikę odkrywania kart i zakrywania z powrotem przy nieodgadnięciu,
        //naliczanie puntków itd.

        private void PrzypisanieUidImgKarty()
        {
            //lista zawierająca po dwie takie same wartości dla kart, po wybraniu usuwa jeden element
            //z tej listy
            List<int> list = new List<int>{ 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6 };

            int r = rand.Next(0, list.Count+1);

            for (int i = 1; i <= list.Count; i++)
            {
                Image img = (Image)this.FindName("imgKarta" + i);

                if (img != null)
                {
                    img.Uid = list[r].ToString();
                    list.Remove(r);
                }
            }
        }

        private void ClickOdkryjKarte(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            string targetImageName = btn.Tag.ToString(); //szukamy odpowiedniego img związanego z buttonem

            // Szukamy kontrolki po nazwie zapisanej w Tagu
            Image img = (Image)this.FindName(targetImageName);

            if (img != null)
            {
                img.Source = new BitmapImage(new Uri(@$"img/card_{img.Uid}.png", UriKind.Relative));
            }

            //przemyśleć, gdzie powinno znaleźć się sprawdzanie czy poprzednio odkryta karta
            //jest taka sama jak aktualnie odkryta

            btn.IsEnabled = false;
        }
    }
}