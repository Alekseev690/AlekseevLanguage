using System;
using System.Collections.Generic;
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

namespace AlekseevLanguage
{
    /// <summary>
    /// Логика взаимодействия для ClientPage.xaml
    /// </summary>
    public partial class ClientPage : Page
    {
        int CountRecords;
        int CountPage;
        int CurrentPage = 0;
        List<Client> CurrentPageList = new List<Client>();
        List<Client> TableList;

        public ClientPage()
        {
            InitializeComponent();
            var currentClient = АлексеевLanguageEntities.GetContext().Client.ToList();
            ClientListView.ItemsSource = currentClient;

            GenderCB.SelectedIndex = 0;
            SortCB.SelectedIndex = 0;
            OutputCB.SelectedIndex = 0;

            UpdateClient();
        }

        private void TBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateClient();
        }

        private void GenderCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClient();
        }

        private void SortCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClient();
        }

        private void LeftDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(1, null);
        }

        private void PageListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ChangePage(0, Convert.ToInt32(PageListBox.SelectedItem.ToString()) - 1);
        }

        private void RigthDirButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(2, null);
        }

        private void UpdateClient()
        {
            var currentClient = АлексеевLanguageEntities.GetContext().Client.ToList();

            if (GenderCB.SelectedIndex == 1)
            {
                currentClient = currentClient.Where(p => (Convert.ToInt32(p.GenderCode) <= 1)).ToList();
            }
            if (GenderCB.SelectedIndex == 2)
            {
                currentClient = currentClient.Where(p => (Convert.ToInt32(p.GenderCode) >= 2)).ToList();
            }
            if (SortCB.SelectedIndex == 1)
            {
                currentClient = currentClient.OrderBy(p => p.LastName).ToList();
            }
            if (SortCB.SelectedIndex == 2)
            {
                currentClient = currentClient.OrderBy(p => p.LastDate).ToList();
            }
            if (SortCB.SelectedIndex == 3)
            {
                currentClient = currentClient.OrderBy(p => p.VisitCount).ToList();
            }

            currentClient = currentClient.Where(p => (p.FirstName + p.LastName + p.Patronymic).ToLower().Contains(TBoxSearch.Text.ToLower())).ToList();

            ClientListView.ItemsSource = currentClient;
            TableList = currentClient;
            ChangePage(0, 0);
        }

        private void ChangePage(int direction, int? selectedPage)
        {
            CurrentPageList.Clear();
            CountRecords = TableList.Count;
            int currentRecordsOnPage = 10;

            if (OutputCB.SelectedIndex == 0)
            {
                currentRecordsOnPage = 10;
            }
            else if (OutputCB.SelectedIndex == 1)
            {
                currentRecordsOnPage = 50;
            }
            else if (OutputCB.SelectedIndex == 2)
            {
                currentRecordsOnPage = 200;
            }
            else if (OutputCB.SelectedIndex == 3)
            {
                currentRecordsOnPage = АлексеевLanguageEntities.GetContext().Client.Select(p => p.ID).Count();
                CountPage = 1;
            }

            if (CountRecords % currentRecordsOnPage > 0)
            {
                CountPage = CountRecords / currentRecordsOnPage + 1;
            }
            else
            {
                CountPage = CountRecords / currentRecordsOnPage;
            }

            Boolean Ifupdate = true;
            int min;

            if (selectedPage.HasValue)
            {
                if (selectedPage >= 0 && selectedPage <= CountPage)
                {
                    CurrentPage = (int)selectedPage;
                    min = CurrentPage * currentRecordsOnPage + currentRecordsOnPage < CountRecords
                        ? CurrentPage * currentRecordsOnPage + currentRecordsOnPage
                        : CountRecords;
                    for (int i = CurrentPage * currentRecordsOnPage; i < min; i++)
                    {
                        CurrentPageList.Add(TableList[i]);
                    }
                }
            }
            else
            {
                switch (direction)
                {
                    case 1:
                        if (CurrentPage > 0)
                        {
                            CurrentPage--;
                            min = CurrentPage * currentRecordsOnPage + currentRecordsOnPage < CountRecords
                                ? CurrentPage * currentRecordsOnPage + currentRecordsOnPage
                                : CountRecords;
                            for (int i = CurrentPage * currentRecordsOnPage; i < min; i++)
                            {
                                CurrentPageList.Add(TableList[i]);
                            }
                        }
                        else
                        {
                            Ifupdate = false;
                        }
                        break;

                    case 2:
                        if (CurrentPage < CountPage - 1)
                        {
                            CurrentPage++;
                            min = CurrentPage * currentRecordsOnPage + currentRecordsOnPage < CountRecords
                                ? CurrentPage * currentRecordsOnPage + currentRecordsOnPage
                                : CountRecords;
                            for (int i = CurrentPage * currentRecordsOnPage; i < min; i++)
                            {
                                CurrentPageList.Add(TableList[i]);
                            }
                        }
                        else
                        {
                            Ifupdate = false;
                        }
                        break;
                }
            }

            if (Ifupdate)
            {
                PageListBox.Items.Clear();
                for (int i = 1; i <= CountPage; i++)
                {
                    PageListBox.Items.Add(i);
                }
                PageListBox.SelectedIndex = CurrentPage;

                min = CurrentPage * currentRecordsOnPage + currentRecordsOnPage < CountRecords
                    ? CurrentPage * currentRecordsOnPage + currentRecordsOnPage
                    : CountRecords;
                TBCount.Text = min.ToString();
                TBAllRecords.Text = " из " + CountRecords.ToString();

                ClientListView.ItemsSource = CurrentPageList;
                ClientListView.Items.Refresh();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var currentDataClient = (sender as Button).DataContext as Client;

            var currentClientService = АлексеевLanguageEntities.GetContext().ClientService.ToList();
            currentClientService = currentClientService.Where(p => p.ClientID == currentDataClient.ID).ToList();

            if (currentClientService.Count != 0)
            {
                MessageBox.Show("Невозможно выполнить удаление, т.к. у этого клиента есть информация о посещениях!");
            }
            else
            {
                if (MessageBox.Show("Вы точно хотите выполнить удаление?", "Внимание!",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        АлексеевLanguageEntities.GetContext().Client.Remove(currentDataClient);
                        АлексеевLanguageEntities.GetContext().SaveChanges();
                        ClientListView.ItemsSource = АлексеевLanguageEntities.GetContext().Client.ToList();
                        UpdateClient();
                    }
                    catch (Exception ex) {
                        MessageBox.Show(ex.Message.ToString());
                    }
                }
            }
        }

        private void OutputCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateClient();
        }
    }
}
