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

namespace ReadAndWrite
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ConfigureSidebar();

            this.Loaded += (s, e) =>
            {
                ContentFrame.Navigate(new CatalogPage());
                SetActiveButton(NavCatalog);
            };
        }

        private void ConfigureSidebar()
        {
            if (CurrentUser.Role == "Admin")
                NavAdminPanel.Visibility = Visibility.Visible;

            if (CurrentUser.Role == "Author")
                NavAuthorRoom.Visibility = Visibility.Visible;

            if (CurrentUser.IsFrozen)
            {
                NavFreezeWarning.Visibility = Visibility.Visible;
                NavCatalog.IsEnabled = false;
                NavMyLists.IsEnabled = false;
                NavAdminPanel.IsEnabled = false;
                NavAuthorRoom.IsEnabled = false;
            }
        }

        private void SetActiveButton(Button active)
        {
            var allButtons = new[] { NavCatalog, NavMyLists, NavAdminPanel,
                                     NavAuthorRoom, NavMyProfile };
            foreach (var btn in allButtons)
            {
                btn.Opacity = 0.5;
            }
            active.Opacity = 1;
        }

        private void NavCatalog_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.IsFrozen) { MessageBox.Show("Аккаунт заморожен!"); return; }
            ContentFrame.Navigate(new CatalogPage());
            SetActiveButton(NavCatalog);
        }

        private void NavMyLists_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.IsFrozen)
            {
                MessageBox.Show("Аккаунт заморожен!");
                return;
            }
            ContentFrame.Navigate(new BookListsPage());
            SetActiveButton(NavMyLists);
        }

        private void NavAdminPanel_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new AdminPage());
            SetActiveButton(NavAdminPanel);
        }

        private void NavAuthorRoom_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.IsFrozen) { MessageBox.Show("Аккаунт заморожен!"); return; }
            ContentFrame.Navigate(new AuthorPage());
            SetActiveButton(NavAuthorRoom);
        }

        private void NavMyProfile_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new ProfilePage());
            SetActiveButton(NavMyProfile);
        }

        private void NavFreezeWarning_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Ваш аккаунт временно заморожен администрацией.",
                           "Ограничение доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            var help = new HelpWindow();
            help.Show();
        }

        private void NavExit_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}