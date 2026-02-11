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
using static System.Runtime.InteropServices.JavaScript.JSType;

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

            StartTimer();
        }

        int liczbaKart = 12; //zmienić, by jakoś automatycznie zliczyło ile jest kart, na wypadek gdydy w xaml'u się zmieniła liczba kart
        int licznikOdkrytychKart = 0;
        string idPierwszejKarty = ""; //uid to string

        int punkty = 0;
        int wygrane = 0;

        //zapamiętuje pierwszą wybraną kartę
        Image pierwszyImg;
        Button pierwszyButton;

        bool czyAnimacjaTrwa = false;

        private CancellationTokenSource _timerCts;

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

        private void ZakryjWszystkieKarty() //bez async, żeby wszystkie na raz zakryło
        {
            for (int i = 1; i <= liczbaKart; i++)
            {

                Image img = (Image)this.FindName("imgKarta" + i);
                Button btn = (Button)this.FindName("btnKarta" + i);
                ScaleTransform scaleTransform = (ScaleTransform)this.FindName("FlipTransform" + i);

                SimpleFlip(img, "0", scaleTransform);
                btn.IsEnabled = true;
            }
        }

        private async Task LiczeniePunktow(Image img, Button btn, ScaleTransform scale)
        {

            licznikOdkrytychKart++;

            if (licznikOdkrytychKart == 1)
            {
                idPierwszejKarty = img.Uid;

                pierwszyImg = img;
                pierwszyButton = btn;
            }
            else if (licznikOdkrytychKart == 2)
            {
                czyAnimacjaTrwa = true;
                //sprawdzenie czy karty są takie same
                if (idPierwszejKarty == img.Uid)
                {
                    punkty++;
                    textBlockPunkty.Text = $"Punkty: {punkty}";

                    //zniknięcie pary kart po odgadnięciu
                    await Task.Delay(300);
                    pierwszyButton.Visibility = Visibility.Hidden;
                    btn.Visibility = Visibility.Hidden;

                    //wygrana opiera się na odkryciu wszystkich kart, czyli zdobycie 6 punktów
                    if (punkty == 6)
                    {
                        MessageBox.Show("Poziom ukończony");
                        wygrane++;
                        textBlockIloscWygranych.Text = $"Ilość wygranych: {wygrane}";
                        buttonNowaGra.Visibility = Visibility.Visible;
                        
                    }
                }
                else
                {
                    czyAnimacjaTrwa = true;
                    await Task.Delay(500);

                    // Odwracamy obie karty z powrotem (równolegle)
                    var task1 = SimpleFlip(pierwszyImg, "0", (ScaleTransform)this.FindName("FlipTransform" + pierwszyButton.Uid));
                    var task2 = SimpleFlip(img, "0", scale);

                    await Task.WhenAll(task1, task2);

                    // Przywracamy przyciski
                    pierwszyButton.IsEnabled = true;
                    btn.IsEnabled = true;

                }
                await Task.Delay(500);
                czyAnimacjaTrwa = false;

                licznikOdkrytychKart = 0;
            }
        }

        private async void ClickOdkryjKarte(object sender, RoutedEventArgs e)
        {
            if (czyAnimacjaTrwa) return;

            Button btn = (Button)sender;

            btn.IsEnabled = false;


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

            licznikOdkrytychKart = 0;

            punkty = 0;
            textBlockPunkty.Text = $"Punkty: {punkty}";

            //resetuje gre zostawiając liczbę wygranych
            ZakryjWszystkieKarty();

            PrzypisanieUidImgKarty();

            for (int i = 1; i <= liczbaKart; i++)
            {
                Button btn = (Button)this.FindName("btnKarta" + i);
                btn.Visibility = Visibility.Visible;
            }

            StartTimer();
        }


        public async Task SimpleFlip(Image img, string imageUid, ScaleTransform scale)
        {
            czyAnimacjaTrwa = true;

            var tcs = new TaskCompletionSource<bool>();
            // 1. The Shrink and Grow Animation (ScaleX from 1 to -1)
            DoubleAnimation flipAnim = new DoubleAnimation
            {
                From = 1,
                To = -1,
                Duration = TimeSpan.FromSeconds(0.5),
                // Adding an Ease makes it look more natural/fluid
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };


            // 2. The Image Swap (Exactly at 0.25 seconds)
            ObjectAnimationUsingKeyFrames imageSwapAnim = new ObjectAnimationUsingKeyFrames();
            imageSwapAnim.Duration = TimeSpan.FromSeconds(0.5);

            var newImage = new BitmapImage(new Uri($"pack://application:,,,/img/card_{imageUid}.png"));
            var keyFrame = new DiscreteObjectKeyFrame(newImage, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.25)));

            imageSwapAnim.KeyFrames.Add(keyFrame);

            flipAnim.Completed += (s, e) => tcs.SetResult(true);

            // 3. Run them together
            Console.WriteLine(scale);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, flipAnim);
            img.BeginAnimation(Image.SourceProperty, imageSwapAnim);

            await tcs.Task;
            czyAnimacjaTrwa = false;
        }

        public async void StartTimer(int seconds = 45)
        {
            textBlockTimer.Foreground = Brushes.Black;

            // 1. Cancel any existing timer before starting a new one
            _timerCts?.Cancel();
            _timerCts = new CancellationTokenSource();
            var token = _timerCts.Token;

            var endTime = DateTime.Now + TimeSpan.FromSeconds(seconds);

            try
            {
                while (DateTime.Now < endTime)
                {
                    // 2. Check if we should stop (points reached or cancellation requested)
                    if (token.IsCancellationRequested || punkty >= 6)
                    {
                        break;
                    }

                    var remaining = endTime - DateTime.Now;

                    // 3. Update UI safely
                    textBlockTimer.Text = remaining.ToString(@"mm\:ss");

                    // 4. Wait 1 second without blocking the UI thread
                    await Task.Delay(1000, token);
                }

                if (!token.IsCancellationRequested)
                {
                    textBlockTimer.Text = "00:00";
                    textBlockTimer.Foreground = Brushes.Red;

                    buttonNowaGra.Visibility = Visibility.Visible;

                    for (int i = 1; i <= liczbaKart; i++)
                    {
                        Button btn = (Button)this.FindName("btnKarta" + i);

                        btn.IsEnabled = false;
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // This is expected when we stop the timer manually
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _timerCts?.Cancel(); // Stops the timer background task immediately
            base.OnClosing(e);
        }
    }
}
