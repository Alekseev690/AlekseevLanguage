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

            if (currentClient.GenderCode == "2")
            {
                FemaleRB.IsChecked = true;
            }
            else
            {
                if (currentClient.GenderCode == "1")
                {
                    MaleRB.IsChecked = true;
                }
            }

            if (currentClient.ID == 0)
            {
                RegistrationDP.Text = DateTime.Now.ToShortDateString();
                RegistrationDP.IsEnabled = true;
                IDTB.Visibility = Visibility.Hidden;
                IDTBlock.Visibility = Visibility.Hidden;
                currentClient.Birthday = DateTime.Now;
            }

            ClientBirthday = currentClient.Birthday;
            BirthdayDPicker.Text = ClientBirthday.ToString();
            DataContext = currentClient;
        }
        private void PhotoChange_Click(object sender, RoutedEventArgs e)
        {
            string projectRootPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            string clientsFolderPath = System.IO.Path.Combine(projectRootPath, "Клиенты");

            OpenFileDialog myOpenFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.jpg; *.jpeg; *.png)|*.jpg;*.jpeg;*.png",
                Title = "Выберите изображение клиента",
                InitialDirectory = clientsFolderPath
            };

            if (myOpenFileDialog.ShowDialog() == true)
            {
                string fileName = System.IO.Path.GetFileName(myOpenFileDialog.FileName);
                currentClient.PhotoPath = $"Клиенты/{fileName}";
                PhotoClient.Source = new BitmapImage(new Uri(myOpenFileDialog.FileName));
            }
        }
        private bool IsValidEmail()
        {
            string pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]{2,}\.[a-zA-Z]{2,}$";
            return Regex.IsMatch(currentClient.Email, pattern);
        }
        private bool IsValidName(string name)
        {
            string pattern = @"^[А-Яа-яЁё\s\-]+$";
            return Regex.IsMatch(name, pattern);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(currentClient.LastName))
                errors.AppendLine("Укажите фамилию клиента!");
            else
            {
                if (currentClient.LastName.Length > 50)
                    errors.AppendLine("В поле «Фамилия» должено быть меньше 50 символов!");
                if (!IsValidName(currentClient.LastName))
                    errors.AppendLine("В поле «Фамилия» указаны некорректные входные данные!");
            }

            if (string.IsNullOrWhiteSpace(currentClient.FirstName))
                errors.AppendLine("Укажите имя клиента");
            else
            {
                if (currentClient.FirstName.Length > 50)
                    errors.AppendLine("В поле «Имя» должено быть меньше 50 символов");
                if (!IsValidName(currentClient.FirstName))
                    errors.AppendLine("В поле «Имя» указаны некорректные входные данные!");
            }

            if (string.IsNullOrWhiteSpace(currentClient.Patronymic))
                errors.AppendLine("Укажите отчество клиента");
            else
            {
                if (currentClient.Patronymic.Length > 50)
                    errors.AppendLine("В поле «Отчество» должено быть не более 50 символов");
                if (!IsValidName(currentClient.Patronymic))
                    errors.AppendLine("В поле «Отчество» указаны некорректные входные данные!");
            }

            if (ClientBirthday == null)
                errors.AppendLine("Укажите дату рождения клиента!");
            else
            {
                if (ClientBirthday > DateTime.Today)
                    errors.AppendLine("Дата рождения указана неверно!");
            }

            if (string.IsNullOrWhiteSpace(currentClient.Email))
                errors.AppendLine("Укажите почту клиента!");
            else
            {
                if (!IsValidEmail())
                    errors.AppendLine("Укажите корректную почту!");
            }

            if (FemaleRB.IsChecked != true && MaleRB.IsChecked != true)
            {
                errors.AppendLine("Укажите пол клиента!");
            }
            else
            {
                currentClient.GenderCode = FemaleRB.IsChecked == true ? "2" : "1"; // жен - 2, муж - 1
            }

            if (string.IsNullOrWhiteSpace(currentClient.Phone))
            {
                errors.AppendLine("Укажите номер телефона!");
            }
            else
            {
                string phone = currentClient.Phone.Trim();
                string ph = currentClient.Phone.Replace("(", "").Replace(")", "").Replace("-", "").Replace("+", "");

                if (!phone.StartsWith("+7") && !phone.StartsWith("8"))
                {
                    errors.AppendLine("Телефон должен начинаться с +7 или 8 !");
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

                if ((ph[1] == '9' || ph[1] == '4' || ph[1] == '8') && ph.Length != 11 || (ph[1] == '3' && ph.Length != 12))
                    errors.AppendLine("Укажите правильно телефон клиента!");
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
