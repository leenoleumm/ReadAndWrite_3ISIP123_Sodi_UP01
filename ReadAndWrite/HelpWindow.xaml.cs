using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ReadAndWrite
{
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
            HighlightButton(BtnIntro);
            ShowIntro();
        }

        private void HighlightButton(Button active)
        {
            var all = new[] { BtnIntro, BtnReg, BtnCatalog, BtnAuthor, BtnAdmin };
            foreach (var btn in all)
            {
                btn.Background = btn == active
                    ? new SolidColorBrush(Color.FromRgb(44, 82, 130))
                    : Brushes.Transparent;
            }
        }

        private void ShowContent(string title, string text)
        {
            ContentPanel.Children.Clear();
            ContentPanel.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(Color.FromRgb(30, 58, 95)),
                Margin = new Thickness(0, 0, 0, 15)
            });
            ContentPanel.Children.Add(new TextBlock
            {
                Text = text,
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 24
            });
        }

        private void ShowIntro()
        {
            ShowContent("1. О программе",
                "Программа «Читай, Пиши и не спиши» — это прототип приложения для свободного распространения книг начинающих авторов. " +
                "Программа позволяет читателям находить и читать книги, оставлять отзывы, формировать личные списки чтения. " +
                "Авторы могут публиковать свои произведения, редактировать их и загружать обложки. " +
                "Администратор управляет пользователями, обрабатывает жалобы и заявки.\n\n" +
                "Программа разработана компанией «ШутИКроль» и предназначена для работы под управлением Windows 10/11.");
        }

        private void ShowRegistration()
        {
            ShowContent("2. Регистрация и вход",
                "При запуске программы открывается окно авторизации.\n\n" +
                "Для входа в систему:\n" +
                "• Введите логин и пароль в соответствующие поля.\n" +
                "• Нажмите кнопку «Войти».\n\n" +
                "Для регистрации нового пользователя:\n" +
                "• Нажмите кнопку «Регистрация».\n" +
                "• Заполните поля: имя, email, логин, пароль.\n" +
                "• Нажмите «Зарегистрироваться».\n" +
                "• После успешной регистрации нажмите «Войти».\n\n" +
                "Примечание: все новые пользователи получают роль «Читатель».");
        }

        private void ShowCatalog()
        {
            ShowContent("3. Каталог книг",
                "После входа в систему открывается каталог книг.\n\n" +
                "Поиск и фильтрация:\n" +
                "• Введите текст в поле поиска для фильтрации по названию или автору.\n" +
                "• Выберите жанр из выпадающего списка для фильтрации по жанру.\n" +
                "• Выберите способ сортировки: по названию или по рейтингу.\n\n" +
                "Действия с книгами:\n" +
                "• Щёлкните левой кнопкой мыши по книге, чтобы открыть её.\n" +
                "• На странице книги можно читать текст, оставлять отзывы и подавать жалобы.\n" +
                "• Щёлкните правой кнопкой мыши по книге, чтобы добавить её в список чтения.\n\n" +
                "Списки чтения доступны через кнопку 📑 в боковом меню.");
        }

        private void ShowAuthor()
        {
            ShowContent("4. Кабинет автора",
                "Кабинет автора доступен пользователям с ролью «Автор» через кнопку ✏️ в боковом меню.\n\n" +
                "Добавление книги:\n" +
                "• Нажмите кнопку «+ Добавить новую книгу».\n" +
                "• Заполните название, описание и текст книги.\n" +
                "• Выберите файл обложки (JPG, PNG, BMP).\n" +
                "• Отметьте галочками нужные жанры.\n" +
                "• Нажмите «Опубликовать».\n\n" +
                "Редактирование книги:\n" +
                "• Щёлкните правой кнопкой мыши по книге.\n" +
                "• Внесите изменения и нажмите «Сохранить».\n\n" +
                "Замороженные книги:\n" +
                "• Нажмите кнопку «🔒 Замороженные книги» для просмотра.\n" +
                "• Нажмите «📩 Оспорить» для подачи заявки на разморозку.");
        }

        private void ShowAdmin()
        {
            ShowContent("5. Администрирование",
                "Панель администратора доступна через кнопку 🛡️ в боковом меню и содержит пять вкладок:\n\n" +
                "🚩 Жалобы:\n" +
                "• Просмотр жалоб на книги и отзывы.\n" +
                "• Кнопки «✅ Принять» и «❌ Отклонить» для обработки.\n\n" +
                "👤 Заявки на автора:\n" +
                "• Просмотр заявок от читателей на получение роли Автора.\n" +
                "• Кнопки «✅ Одобрить» и «❌ Отклонить».\n\n" +
                "🔓 Заявки на разморозку:\n" +
                "• Просмотр заявок на разморозку аккаунтов и книг.\n" +
                "• Кнопки «✅ Разморозить» и «❌ Отклонить».\n\n" +
                "📋 Пользователи:\n" +
                "• Полный список пользователей с ролями и статусами.\n" +
                "• Кнопки смены роли, заморозки и смены пароля.\n\n" +
                "🔒 Заморожено:\n" +
                "• Сводка всех замороженных пользователей, книг и отзывов.");
        }

        private void BtnIntro_Click(object sender, RoutedEventArgs e)
        { HighlightButton(BtnIntro); ShowIntro(); }
        private void BtnReg_Click(object sender, RoutedEventArgs e)
        { HighlightButton(BtnReg); ShowRegistration(); }
        private void BtnCatalog_Click(object sender, RoutedEventArgs e)
        { HighlightButton(BtnCatalog); ShowCatalog(); }
        private void BtnAuthor_Click(object sender, RoutedEventArgs e)
        { HighlightButton(BtnAuthor); ShowAuthor(); }
        private void BtnAdmin_Click(object sender, RoutedEventArgs e)
        { HighlightButton(BtnAdmin); ShowAdmin(); }
    }
}