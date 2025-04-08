using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
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

namespace AlekseevLanguage
{
    /// <summary>
    /// Логика взаимодействия для AddEditPage.xaml
    /// </summary>
    public partial class AddEditPage : Page
    {
        private Client currentClient = new Client();
        DateTime? ClientBirthday;
        public AddEditPage(Client selectedClient)
        {
            InitializeComponent();
            if (selectedClient != null) {
                currentClient = selectedClient;
                RegistrationDP.Text = selectedClient.RegistrationDate.ToShortDateString();
                RegistrationDP.IsEnabled = true;
            }
            DataContext = currentClient;

            if (currentClient.GenderCode == "2")
            {
                FemaleRB.IsChecked = true;
            }
            else
            {
                MaleRB.IsChecked = true;
            }

            if (currentClient.ID == 0)
            {
                RegistrationDP.Text = DateTime.Now.ToShortDateString();
                RegistrationDP.IsEnabled = true;
                IDTB.Visibility = Visibility.Hidden;
                currentClient.Birthday = DateTime.Now;
            }

            ClientBirthday = currentClient.Birthday;
            BirthdayDPicker.Text = ClientBirthday.ToString();
        }

        private void PhotoChange_Click(object sender, RoutedEventArgs e)
        {
            string clientsFolderPath = @"C:\Users\УАТ\source\repos\Alekseev690\AlekseevLanguage\Клиенты\";

            OpenFileDialog myOpenFileDialog = new OpenFileDialog
            {
                InitialDirectory = clientsFolderPath,
                Filter = "Image files (*.jpg; *.jpeg; *.png)|*.jpg;*.jpeg;*.png",
                Title = "Выберите изображение клиента"
            };

            if (myOpenFileDialog.ShowDialog() == true)
            {
                currentClient.PhotoPath = myOpenFileDialog.FileName;

                try
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.UriSource = new Uri(currentClient.PhotoPath, UriKind.Absolute);
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    PhotoClient.Source = bitmapImage;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(currentClient.LastName))
                errors.AppendLine("Укажите фамилию клиента!");

            if (currentClient.LastName.Count() > 50)
                errors.AppendLine("Слишком много симовлов фамилии!");

            if (string.IsNullOrWhiteSpace(currentClient.FirstName))
                errors.AppendLine("Укажите имя клиента!");

            if (currentClient.FirstName.Count() > 50)
                errors.AppendLine("Слишком много символов в имени!");

            if (string.IsNullOrWhiteSpace(currentClient.Patronymic))
                errors.AppendLine("Укажите отчество клиента!");

            if (currentClient.Patronymic.Count() > 50)
                errors.AppendLine("Слишком много символов отчества!");

            if (ClientBirthday == null)
                errors.AppendLine("Укажите дату рождения клиента");
            else
            {
                if (ClientBirthday > DateTime.Today)
                    errors.AppendLine("Дата рождения клиента указана неверно");
            }

            if (string.IsNullOrWhiteSpace(currentClient.Email))
            {
                errors.AppendLine("Укажите email клиента!");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(currentClient.Email) || !currentClient.Email.Contains("@") || currentClient.Email.IndexOf("@") >= currentClient.Email.LastIndexOf(".") - 1 ||
                    currentClient.Email.Split('@')[1].Length < 2 || currentClient.Email.Split('@')[1].Split('.')[0].Length < 2 ||
                    currentClient.Email.Split('.').Last().Length < 2 || currentClient.Email.Any(c => !char.IsLetterOrDigit(c) && c != '@' && c != '.' && c != '_' && c != '-' && c != '(' && c != ')'))
                {
                    errors.AppendLine("Укажите корректный email клиента!");
                }
            }

            if (string.IsNullOrWhiteSpace(currentClient.Phone))
            {
                errors.AppendLine("Укажите номер телефона клиента!");
            }
            else
            {
                string phone = currentClient.Phone.Trim();

                if (!phone.StartsWith("+7") && !phone.StartsWith("8"))
                {
                    errors.AppendLine("Телефон должен начинаться с +7 или 8!");
                }

                string cleanedNumber = new string(phone.Where(char.IsDigit).ToArray());

                if (cleanedNumber.Length < 10)
                {
                    errors.AppendLine("Телефон должен содержать не менее 10 цифр!");
                }

                if (currentClient.Phone.Any(char.IsLetter))
                {
                    errors.AppendLine("Телефон не должен содержать букв!");
                }

                if (System.Text.RegularExpressions.Regex.IsMatch(currentClient.Phone, @"(\(\))|(--)|(\-\()|(\-\))|(\(\-)|(\)\-)|(\(\()|(\)\))"))
                {
                    errors.AppendLine("Телефон содержит недопустимые последовательности символов!");
                }

                string ph = currentClient.Phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace("+", "");
                if ((ph.Length != 11 && (ph[0] == '9' || ph[0] == '4' || ph[0] == '8')) ||
                    (ph.Length != 12 && ph[0] == '3'))
                {
                    errors.AppendLine("Укажите правильную длину телефонного номера клиента!");
                }
            }

            if (FemaleRB.IsChecked == true)
            {
                currentClient.GenderCode = "2"; // жен
            }
            else
            {
                currentClient.GenderCode = "1"; // муж
            }
            currentClient.Birthday = ClientBirthday;


            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            if (currentClient.ID == 0)
            {
                currentClient.RegistrationDate = DateTime.Now;
                АлексеевLanguageEntities.GetContext().Client.Add(currentClient);
            }
            try
            {
                АлексеевLanguageEntities.GetContext().SaveChanges();
                MessageBox.Show("Информация сохранена!");
                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.ToString());
            }
        }

        private void BirthdayDPicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ClientBirthday = (DateTime)(((DatePicker)sender).SelectedDate);
        }
    }
}
