using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
            //zrobić, by wszystkie karty na początku gry odsłoniły się na ułamek sekundy!!!
        }

        int liczbaKart = 12; //zmienić, by jakoś automatycznie zliczyło ile jest kart, na wypadek gdydy w xaml'u się zmieniła liczba kart
        int licznikOdkrytychKart = 0;
        string idPierwszejKarty = ""; //uid to string

        int punkty = 0;
        int wygrane = 0;

        //zapamiętuje pierwszą wybraną kartę
        Image pierwszyImg;
        Button pierwszyButton;

        Random rand = new Random();


        private void PrzypisanieUidImgKarty()
        {
            //lista zawierająca po dwie takie same wartości dla kart, po wybraniu usuwa jeden element
            //z tej listy
            List<int> list = new List<int> { 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6 };


            for (int i = 1; i <= liczbaKart; i++)
            {
                int r = rand.Next(0, list.Count);

                Image img = (Image)this.FindName("imgKarta" + i);

                if (img != null)
                {
                    img.Uid = list[r].ToString();
                    list.RemoveAt(r);
                }
            }
        }

        private void ZakryjWszystkieKarty()
        {
            for (int i = 1; i <= liczbaKart; i++)
            {

                Image img = (Image)this.FindName("imgKarta" + i);
                Button btn = (Button)this.FindName("btnKarta" + i);

                if (img != null)
                {
                    img.Source = new BitmapImage(new Uri(@"img/card_0.png", UriKind.Relative));
                    btn.IsEnabled = true;
                }
            }
        }

        private async Task LiczeniePunktow(Image img, Button btn, ScaleTransform scale)
        {
            btn.IsEnabled = false;

            licznikOdkrytychKart++;

            if (licznikOdkrytychKart == 1)
            {
                idPierwszejKarty = img.Uid;

                pierwszyImg = img;
                pierwszyButton = btn;
            }
            else if (licznikOdkrytychKart == 2)
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
                        buttonNowaGra.Visibility = Visibility.Visible;

                    }
                }
                else
                {
                    await Task.Delay(1000);

                    // Odwracamy obie karty z powrotem (równolegle)
                    var task1 = SimpleFlip(pierwszyImg, "0", (ScaleTransform)this.FindName("FlipTransform" + pierwszyButton.Uid));
                    var task2 = SimpleFlip(img, "0", scale);

                    await Task.WhenAll(task1, task2);

                    // Przywracamy przyciski
                    pierwszyButton.IsEnabled = true;
                    btn.IsEnabled = true;

                }

                licznikOdkrytychKart = 0;
            }
        }

        private async void ClickOdkryjKarte(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            string targetImageName = btn.Tag.ToString(); //szukamy odpowiedniego img związanego z buttonem

            // Szukamy kontrolki po nazwie zapisanej w Tagu
            Image img = (Image)this.FindName(targetImageName);

            ScaleTransform scaleTransform = (ScaleTransform)this.FindName("FlipTransform" + btn.Uid);

            // Czekamy aż karta się obróci
            await SimpleFlip(img, img.Uid, scaleTransform);

            // Dopiero po obróceniu liczymy punkty i ewentualnie zakrywamy
            await LiczeniePunktow(img, btn, scaleTransform);

        }

        private void ClickNowaGra(object sender, RoutedEventArgs e)
        {
            buttonNowaGra.Visibility = Visibility.Collapsed;

            //resetuje gre zostawiając liczbę punktów i wygranych
            ZakryjWszystkieKarty();

            PrzypisanieUidImgKarty();
        }


        public async Task SimpleFlip(Image img, string imageUid, ScaleTransform scale)
        {
            var tcs = new TaskCompletionSource<bool>();
            // 1. The Shrink and Grow Animation (ScaleX from 1 to -1)
            DoubleAnimation flipAnim = new DoubleAnimation
            {
                From = 1,
                To = -1,
                Duration = TimeSpan.FromSeconds(1),
                // Adding an Ease makes it look more natural/fluid
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            // 2. The Image Swap (Exactly at 0.5 seconds)
            ObjectAnimationUsingKeyFrames imageSwapAnim = new ObjectAnimationUsingKeyFrames();
            imageSwapAnim.Duration = TimeSpan.FromSeconds(1);

            var newImage = new BitmapImage(new Uri($"pack://application:,,,/img/card_{imageUid}.png"));
            var keyFrame = new DiscreteObjectKeyFrame(newImage, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5)));

            imageSwapAnim.KeyFrames.Add(keyFrame);

            flipAnim.Completed += (s, e) => tcs.SetResult(true);

            // 3. Run them together
            Console.WriteLine(scale);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, flipAnim);
            img.BeginAnimation(Image.SourceProperty, imageSwapAnim);

            await tcs.Task;
        }
    }
}
