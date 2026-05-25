using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ReadAndWrite
{
    public partial class LoginWindow : Window
    {
        private bool _isRegisterMode = false;

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (_isRegisterMode) return; 

            var login = TbLogin.Text.Trim();
            var password = PbPassword.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                TbError.Text = "Заполните логин и пароль!";
                return;
            }

            using (var db = new ReadAndWriteDBEntities())
            {
                var user = db.Users.FirstOrDefault(u => u.Login == login && u.PasswordHash == password);

                if (user != null)
                {

                    CurrentUser.Id = user.UserId;
                    CurrentUser.Login = user.Login;
                    CurrentUser.DisplayName = user.DisplayName;
                    CurrentUser.Role = user.Role;
                    CurrentUser.IsFrozen = user.IsFrozen;
                    CurrentUser.Email = user.Email;

                    var main = new MainWindow();
                    main.Show();
                    if (user.IsFrozen)
                    {
                        main.ContentFrame.Navigate(new ProfilePage());
                    }
                    this.Close();
                }
                else
                {
                    TbError.Text = "Неверный логин или пароль!";
                }
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRegisterMode)
            {
                _isRegisterMode = true;
                TbTitle.Text = "РЕГИСТРАЦИЯ";
                BtnLogin.Visibility = Visibility.Collapsed;
                BtnRegister.Content = "Зарегистрироваться";
                BtnSwitchToLogin.Visibility = Visibility.Visible;
                TbDisplayName.Visibility = Visibility.Visible;
                TbEmail.Visibility = Visibility.Visible;
                LblDisplayName.Visibility = Visibility.Visible;
                LblEmail.Visibility = Visibility.Visible;
                TbError.Text = "";
            }
            else
            {
                RegisterUser();
            }
        }

        private void BtnSwitchToLogin_Click(object sender, RoutedEventArgs e)
        {
            _isRegisterMode = false;
            TbTitle.Text = "ВХОД";
            BtnLogin.Visibility = Visibility.Visible;
            BtnRegister.Content = "Регистрация";
            BtnSwitchToLogin.Visibility = Visibility.Collapsed;
            TbDisplayName.Visibility = Visibility.Collapsed;
            TbEmail.Visibility = Visibility.Collapsed;
            LblDisplayName.Visibility = Visibility.Collapsed;
            LblEmail.Visibility = Visibility.Collapsed;
            TbError.Text = "";
            TbDisplayName.Text = "";
            TbEmail.Text = "";
        }

        private void RegisterUser()
        {
            var login = TbLogin.Text.Trim();
            var password = PbPassword.Password.Trim();
            var displayName = TbDisplayName.Text.Trim();
            var email = TbEmail.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password) ||
                string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(email))
            {
                TbError.Text = "Все поля обязательны для заполнения!";
                return;
            }

            if (login.Length < 3 || password.Length < 3)
            {
                TbError.Text = "Логин и пароль должны быть от 3 символов!";
                return;
            }

            if (displayName.Length < 2)
            {
                TbError.Text = "Имя должно быть от 2 символов!";
                return;
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                TbError.Text = "Некорректный email!";
                return;
            }

            using (var db = new ReadAndWriteDBEntities())
            {
                if (db.Users.Any(u => u.Login == login))
                {
                    TbError.Text = "Пользователь с таким логином уже существует!";
                    return;
                }

                if (db.Users.Any(u => u.Email == email))
                {
                    TbError.Text = "Пользователь с таким email уже существует!";
                    return;
                }

                var selectedRole = "Reader";

                var newUser = new Users
                {
                    Login = login,
                    PasswordHash = password,
                    Email = email,
                    DisplayName = displayName,
                    Role = selectedRole,
                    IsFrozen = false
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                MessageBox.Show("Регистрация успешна! Теперь войдите.", "Успех");

                BtnSwitchToLogin_Click(null, null);
            }
        }
    }

    public static class CurrentUser
    {
        public static int Id { get; set; }
        public static string Login { get; set; }
        public static string DisplayName { get; set; }
        public static string Role { get; set; }
        public static bool IsFrozen { get; set; }
        public static string Email { get; set; }
    }
}