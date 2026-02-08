using System.Collections.Generic;
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

        //zrobić całą logikę odkrywania kart i zakrywania z powrotem przy nieodgadnięciu,
        //naliczanie puntków itd.

        private void PrzypisanieUidImgKarty()
        {
            //lista zawierająca po dwie takie same wartości dla kart, po wybraniu usuwa jeden element
            //z tej listy
            List<int> list = new List<int>{ 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6 };


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

        private void LiczeniePunktow(Image img, Button btn)
        {
            btn.IsEnabled = false;

            licznikOdkrytychKart++;

            if (licznikOdkrytychKart == 1)
            {
                idPierwszejKarty = img.Uid;

                pierwszyImg = img;
                pierwszyButton = btn;
            }

            
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
                        buttonNowaGra.Visibility = Visibility.Visible;

                    }
                }
                else
                {
                    //na pewno coś ze spowolnieniem czasu jak się karty zakrywają (przy odkrywaniu w sumie też)
                    pierwszyImg.Source = new BitmapImage(new Uri(@"img/card_0.png", UriKind.Relative));
                    pierwszyButton.IsEnabled = true;

                    img.Source = new BitmapImage(new Uri(@"img/card_0.png", UriKind.Relative));
                    btn.IsEnabled = true;
                }

                licznikOdkrytychKart = 0;
            }

        }

        private void ClickOdkryjKarte(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;

            SimpleFlip();

            string targetImageName = btn.Tag.ToString(); //szukamy odpowiedniego img związanego z buttonem

            // Szukamy kontrolki po nazwie zapisanej w Tagu
            Image img = (Image)this.FindName(targetImageName);

            if (img != null)
            {
                img.Source = new BitmapImage(new Uri(@$"img/card_{img.Uid}.png", UriKind.Relative));
                //Console.WriteLine(img.Uid);
            }

            

            LiczeniePunktow(img, btn);

        }

        private void ClickNowaGra(object sender, RoutedEventArgs e)
        {
            buttonNowaGra.Visibility = Visibility.Collapsed;

            //resetuje gre zostawiając liczbę punktów i wygranych
            ZakryjWszystkieKarty();

            PrzypisanieUidImgKarty();
        }


        public void SimpleFlip() //naprawić!!!
        {
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

            var newImage = new BitmapImage(new Uri("pack://application:,,,/img/card_1.png"));
                var keyFrame = new DiscreteObjectKeyFrame(newImage, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5)));

            imageSwapAnim.KeyFrames.Add(keyFrame);

            // 3. Run them together
            FlipTransform.BeginAnimation(ScaleTransform.ScaleXProperty, flipAnim);
            imgKarta1.BeginAnimation(Image.SourceProperty, imageSwapAnim);
        }
    }
}