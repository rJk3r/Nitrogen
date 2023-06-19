using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Net.Http;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Interop;



// *using static Chalk;
//using Needle;
//using static ProxyAgent;
//using static YamlDotNet.Serialization;

namespace Darkgen
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    /// 


    public class MyViewModel : INotifyPropertyChanged
    {
        private string _text;

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class UserAgentProvider
    {
        private static readonly Random random = new Random();
        private static readonly string[] userAgents =
        {
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Edge/92.0.902.78 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:90.0) Gecko/20100101 Firefox/90.0",
        // Добавьте другие User-Agent строки по вашему усмотрению
    };

        public static string GetRandomUserAgent()
        {
            int index = random.Next(userAgents.Length);
            return userAgents[index];
        }
    }

    public partial class MainWindow : Window
    {
        private MyViewModel _viewModel;
        public MainWindow()
        {
            InitializeComponent();
            _viewModel = (MyViewModel)DataContext;
        }

        public static string data = "Works!";
        public static string msgdata = "";
        public static int generated = 0;
        public string Message { get; set; }
        public string SubscriptionPlan { get; set; }
        public static int checkFlag = 0;

        public class MyData
        {
            public string Message { get; set; }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                checkBox.Content = "True";
                checkFlag = 1;
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox != null)
            {
                checkBox.Content = "False";
                checkFlag = 0;
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && Keyboard.Modifiers == ModifierKeys.Control)
            {
                TextBlock textBlock = (TextBlock)sender;
                Clipboard.SetText(textBlock.Text);
            }
        }


        private async void GenerateCode_Click(object sender, RoutedEventArgs e)
        {
            var dispatcher = Application.Current.Dispatcher;

            if (int.TryParse(CountForm.Text, out int numCodes))
            {
                if (numCodes <= 15000)
                {
                    // Продолжайте выполнение кода здесь после успешной проверки и сохранения числа в переменную "number"
                    var nitrobox = (TextBlock)FindName("Nitrobox");
                    var countbox = (TextBox)FindName("Counterbox");

                    MessageBox.Show("Программа сгенерирует " + numCodes + " ссылок на Nitro");
                    generated = 1;

                    int validCodes = 0;
                    int invalidCodes = 0;

                    for (int i = 1; i < numCodes+1; i++)
                    {
                        string code = GenerateNitroCode();
                        string url = $"https://discord.com/api/v9/entitlements/gift-codes/{code}?with_application=false&with_subscription_plan=true";
                        string link = $"https://discord.gift/{code}";

                        try
                        {
                            string userAgent = UserAgentProvider.GetRandomUserAgent();
                            using (HttpClient client = new HttpClient())
                            {
                                HttpResponseMessage response = await client.GetAsync(url);
                                client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
                                if (response.IsSuccessStatusCode)
                                {
                                    string json = await response.Content.ReadAsStringAsync();
                                    JObject jsonObject = JObject.Parse(json);
                                    string message = jsonObject.GetValue("message").ToString();
                                    message = msgdata;
                                }

                                if (msgdata == "Unknown Gift Code" && msgdata != "")
                                {

                                    if (checkFlag == 0)
                                        {
                                            invalidCodes++;
                                            nitrobox.Text += $"{link}\n";
                                            Counterbox.Text = $"{i} / {numCodes}";
                                            
                                        }

                                }
                                else if (msgdata != "Unknown Gift Code" && msgdata != "" && checkFlag == 0)
                                {
                                    nitrobox.Text += $"+++++++++++++++++++++++++++++++++++++++++++";
                                    nitrobox.Text += $"\t\tVALID!!!!!!\t\t";
                                    validCodes++;
                                    nitrobox.Text += $"{link}\n";
                                    nitrobox.Text += $"+++++++++++++++++++++++++++++++++++++++++++";
                                    Counterbox.Text = $"{i} / {numCodes}";

                                }
                                else if (msgdata != "Unknown Gift Code" && msgdata != "" && checkFlag == 1)
                                {
                                    _viewModel.Text += $"{link}\n";
                                    nitrobox.UpdateLayout();
                                    Counterbox.Text = $"{i} / {numCodes}";

                                }
                                else if (checkFlag == 0)
                                {
                                    _viewModel.Text += $"{link}\n";
                                    nitrobox.UpdateLayout();
                                    Counterbox.Text = $"{i} / {numCodes}";
                                }
                            }
                        }
                        catch (WebException ex)
                        {
                            invalidCodes++;
                        }
                        nitrobox.UpdateLayout();
                    }
                    _viewModel.Text += $"\n=========================\n";
                    _viewModel.Text += $"\nValid links: {validCodes}\t" + $"Invalid links: {invalidCodes}\n";
                    _viewModel.Text += $"\n=========================\n";
                }
                else
                {
                    MessageBox.Show("Введенное число превышает 15000", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Некорректный ввод. Введите число.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    private string GenerateNitroCode()
        {
            // Генерируем случайный код в формате Discord Nitro
            StringBuilder sb = new StringBuilder();
            Random random = new Random();

            // Генерируем 16 символов в коде
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            for (int i = 0; i < 16; i++)
            {
                sb.Append(chars[random.Next(chars.Length)]);
            }

            return sb.ToString();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
