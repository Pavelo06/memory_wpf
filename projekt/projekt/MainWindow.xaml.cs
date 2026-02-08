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

        string test = "";
        int licznikOdkrytychKart = 0;
        string idPierwszejKarty = ""; //uid to string

        int punkty = 0;
        int wygrane = 0;

        Random rand = new Random();

        //zrobić całą logikę odkrywania kart i zakrywania z powrotem przy nieodgadnięciu,
        //naliczanie puntków itd.

        private void PrzypisanieUidImgKarty()
        {
            //lista zawierająca po dwie takie same wartości dla kart, po wybraniu usuwa jeden element
            //z tej listy
            List<int> list = new List<int>{ 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6 };


            for (int i = 1; i <= 12; i++)
            {
                int r = rand.Next(0, list.Count);

                Image img = (Image)this.FindName("imgKarta" + i);

                if (img != null)
                {
                    img.Uid = list[r].ToString();
                    test = test + img.Uid;
                    list.Remove(r); //to wpływa na list count i nie wypełnia wszystkich kart przez to
                    //naprawić to, że występuje więcej niż 2 takie same karty
                }
            }
            Console.WriteLine(test);
        }

        private void LiczeniePunktow(Image img)
        {
            licznikOdkrytychKart++;

            if (licznikOdkrytychKart == 1)
            {
                idPierwszejKarty = img.Uid;
            }

            //przemyśleć, gdzie powinno znaleźć się sprawdzanie czy poprzednio odkryta karta
            //jest taka sama jak aktualnie odkryta

            if (licznikOdkrytychKart == 2)
            {

                //sprawdzenie czy karty są takie same
                if (idPierwszejKarty == img.Uid)
                {
                    punkty++;
                    textBlockPunkty.Text = $"Punkty: {punkty}";

                    //wygrana opiera się na odkryciu wszystkich kart, czyli zdobycie 6 punktów
                    if (punkty == 6)
                    {
                        wygrane++;
                        textBlockIloscWygranych.Text = $"Ilość wygranych: {wygrane}";

                    }
                }

                licznikOdkrytychKart = 0;
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
                //Console.WriteLine(img.Uid);
            }

            LiczeniePunktow(img);

            btn.IsEnabled = false;
        }
    }
}