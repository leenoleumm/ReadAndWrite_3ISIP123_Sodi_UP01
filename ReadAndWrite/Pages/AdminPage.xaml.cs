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
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            LoadReports();
            LoadAuthorRequests();
            LoadUnfreezeRequests();
            LoadUsers();

            MainTabControl.SelectionChanged += (s, e) =>
            {
                if (MainTabControl.SelectedIndex == 0) LoadReports();
                else if (MainTabControl.SelectedIndex == 1) LoadAuthorRequests();
                else if (MainTabControl.SelectedIndex == 2) LoadUnfreezeRequests();
                else if (MainTabControl.SelectedIndex == 3) LoadUsers();
                else if (MainTabControl.SelectedIndex == 4) LoadFrozen();
            };
        }

        private void LoadReports()
        {
            ReportsPanel.Children.Clear();
            using (var db = new ReadAndWriteDBEntities())
            {
                var reports = db.Reports.Where(r => r.Status == "Pending").ToList();
                foreach (var report in reports)
                {
                    var user = db.Users.Find(report.ReporterId);
                    var border = new Border
                    {
                        Background = Brushes.White,
                        BorderBrush = new SolidColorBrush(Color.FromRgb(221, 221, 221)),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(15),
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    var stack = new StackPanel();
                    stack.Children.Add(new TextBlock { Text = $"От: {user?.DisplayName ?? "Неизвестный"}", FontWeight = FontWeights.Bold });
                    stack.Children.Add(new TextBlock { Text = $"Тип: {report.TargetType} | ID: {report.TargetId}" });
                    stack.Children.Add(new TextBlock { Text = $"Причина: {report.Reason}", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 5, 0, 0) });

                    var btns = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };
                    var approveBtn = new Button { Content = "✅ Принять", Background = new SolidColorBrush(Color.FromRgb(39, 174, 96)), Foreground = Brushes.White, BorderThickness = new Thickness(0), Margin = new Thickness(0, 0, 10, 5), Padding = new Thickness(10, 5, 10, 5) };
                    var rejectBtn = new Button { Content = "❌ Отклонить", Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)), Foreground = Brushes.White, BorderThickness = new Thickness(0), Margin = new Thickness(0, 0, 10, 5), Padding = new Thickness(10, 5, 10, 5) };

                    approveBtn.Click += (snd, args) => { ResolveReport(report.ReportId, "Approved"); };
                    rejectBtn.Click += (snd, args) => { ResolveReport(report.ReportId, "Rejected"); };

                    btns.Children.Add(approveBtn);
                    btns.Children.Add(rejectBtn);
                    stack.Children.Add(btns);
                    border.Child = stack;
                    ReportsPanel.Children.Add(border);
                }
                if (reports.Count == 0)
                    ReportsPanel.Children.Add(new TextBlock { Text = "Нет активных жалоб.", Foreground = Brushes.Gray });
            }
        }

        private void ResolveReport(int reportId, string status)
        {
            using (var db = new ReadAndWriteDBEntities())
            {
                var report = db.Reports.Find(reportId);
                if (report != null) { report.Status = status; db.SaveChanges(); }
            }
            LoadReports();
        }

        private void LoadFrozen()
        {
            FrozenUsersPanel.Children.Clear();
            FrozenBooksPanel.Children.Clear();
            FrozenReviewsPanel.Children.Clear();

            using (var db = new ReadAndWriteDBEntities())
            {
                var frozenUsers = db.Users.Where(u => u.IsFrozen).ToList();
                foreach (var user in frozenUsers)
                {
                    FrozenUsersPanel.Children.Add(new TextBlock
                    {
                        Text = $"👤 {user.DisplayName} ({user.Login}) | Причина: {user.FreezeReason ?? "не указана"}",
                        Margin = new Thickness(0, 3, 0, 0)
                    });
                }
                if (frozenUsers.Count == 0)
                    FrozenUsersPanel.Children.Add(new TextBlock { Text = "Нет замороженных пользователей.", Foreground = Brushes.Gray });

                var frozenBooks = db.Books.Where(b => b.IsFrozen).ToList();
                foreach (var book in frozenBooks)
                {
                    var author = db.Users.Find(book.AuthorId);
                    FrozenBooksPanel.Children.Add(new TextBlock
                    {
                        Text = $"📖 \"{book.Title}\" | Автор: {author?.DisplayName ?? "неизвестен"}",
                        Margin = new Thickness(0, 3, 0, 0)
                    });
                }
                if (frozenBooks.Count == 0)
                    FrozenBooksPanel.Children.Add(new TextBlock { Text = "Нет замороженных книг.", Foreground = Brushes.Gray });

                var frozenReviews = db.Reviews.Where(r => r.IsFrozen).ToList();
                foreach (var review in frozenReviews)
                {
                    var user = db.Users.Find(review.UserId);
                    var book = db.Books.Find(review.BookId);
                    var preview = review.Text != null && review.Text.Length > 50
                        ? review.Text.Substring(0, 50) + "..."
                        : review.Text ?? "";

                    FrozenReviewsPanel.Children.Add(new TextBlock
                    {
                        Text = $"💬 Отзыв на \"{book?.Title}\" | Автор: {user?.DisplayName} | Текст: \"{preview}\"",
                        Margin = new Thickness(0, 3, 0, 0),
                        TextWrapping = TextWrapping.Wrap
                    });
                }
                if (frozenReviews.Count == 0)
                    FrozenReviewsPanel.Children.Add(new TextBlock { Text = "Нет замороженных отзывов.", Foreground = Brushes.Gray });
            }
        }

        private void LoadAuthorRequests()
        {
            AuthorRequestsPanel.Children.Clear();
            using (var db = new ReadAndWriteDBEntities())
            {
                var requests = db.AuthorRequests.Where(r => r.Status == "Pending").ToList();
                foreach (var req in requests)
                {
                    var user = db.Users.Find(req.UserId);
                    var border = new Border
                    {
                        Background = Brushes.White,
                        BorderBrush = new SolidColorBrush(Color.FromRgb(221, 221, 221)),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(15),
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    var stack = new StackPanel();
                    stack.Children.Add(new TextBlock { Text = $"Пользователь: {user?.DisplayName} ({user?.Login})", FontWeight = FontWeights.Bold });
                    stack.Children.Add(new TextBlock { Text = $"Email: {user?.Email}" });
                    stack.Children.Add(new TextBlock { Text = $"Дата: {req.CreatedAt}" });

                    var btns = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
                    var approveBtn = new Button { Content = "✅ Одобрить", Background = new SolidColorBrush(Color.FromRgb(39, 174, 96)), Foreground = Brushes.White, BorderThickness = new Thickness(0), Margin = new Thickness(0, 0, 10, 0) };
                    var rejectBtn = new Button { Content = "❌ Отклонить", Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)), Foreground = Brushes.White, BorderThickness = new Thickness(0) };

                    approveBtn.Click += (snd, args) => { ResolveAuthorRequest(req.AuthorRequestId, "Approved", req.UserId); };
                    rejectBtn.Click += (snd, args) => { ResolveAuthorRequest(req.AuthorRequestId, "Rejected", req.UserId); };

                    btns.Children.Add(approveBtn);
                    btns.Children.Add(rejectBtn);
                    stack.Children.Add(btns);
                    border.Child = stack;
                    AuthorRequestsPanel.Children.Add(border);
                }
                if (requests.Count == 0)
                    AuthorRequestsPanel.Children.Add(new TextBlock { Text = "Нет заявок на роль автора.", Foreground = Brushes.Gray });
            }
        }

        private void ResolveAuthorRequest(int requestId, string status, int userId)
        {
            using (var db = new ReadAndWriteDBEntities())
            {
                var req = db.AuthorRequests.Find(requestId);
                if (req != null)
                {
                    req.Status = status;
                    if (status == "Approved")
                    {
                        var user = db.Users.Find(userId);
                        if (user != null) user.Role = "Author";
                    }
                    db.SaveChanges();
                }
            }
            LoadAuthorRequests();
        }

        private void LoadUnfreezeRequests()
        {
            UnfreezePanel.Children.Clear();
            using (var db = new ReadAndWriteDBEntities())
            {
                var requests = db.UnfreezeRequests.Where(r => r.Status == "Pending").ToList();
                foreach (var req in requests)
                {
                    var user = db.Users.Find(req.UserId);
                    var border = new Border
                    {
                        Background = Brushes.White,
                        BorderBrush = new SolidColorBrush(Color.FromRgb(221, 221, 221)),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(15),
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    var stack = new StackPanel();
                    stack.Children.Add(new TextBlock { Text = $"От: {user?.DisplayName}", FontWeight = FontWeights.Bold });
                    stack.Children.Add(new TextBlock { Text = $"Тип: {req.TargetType} | ID: {(req.TargetId?.ToString() ?? "Аккаунт")}" });
                    stack.Children.Add(new TextBlock { Text = $"Причина: {req.Reason}", TextWrapping = TextWrapping.Wrap });

                    var btns = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
                    var approveBtn = new Button { Content = "✅ Разморозить", Background = new SolidColorBrush(Color.FromRgb(39, 174, 96)), Foreground = Brushes.White, BorderThickness = new Thickness(0), Margin = new Thickness(0, 0, 10, 0) };
                    var rejectBtn = new Button { Content = "❌ Отклонить", Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)), Foreground = Brushes.White, BorderThickness = new Thickness(0) };

                    approveBtn.Click += (snd, args) => { ResolveUnfreeze(req.UnfreezeRequestId, "Approved"); };
                    rejectBtn.Click += (snd, args) => { ResolveUnfreeze(req.UnfreezeRequestId, "Rejected"); };

                    btns.Children.Add(approveBtn);
                    btns.Children.Add(rejectBtn);
                    stack.Children.Add(btns);
                    border.Child = stack;
                    UnfreezePanel.Children.Add(border);
                }
                if (requests.Count == 0)
                    UnfreezePanel.Children.Add(new TextBlock { Text = "Нет заявок на разморозку.", Foreground = Brushes.Gray });
            }
        }

        private void ResolveUnfreeze(int requestId, string status)
        {
            using (var db = new ReadAndWriteDBEntities())
            {
                var req = db.UnfreezeRequests.Find(requestId);
                if (req != null)
                {
                    req.Status = status;
                    if (status == "Approved")
                    {
                        if (req.TargetType == "Account")
                        {
                            var user = db.Users.Find(req.UserId);
                            if (user != null) user.IsFrozen = false;
                        }
                        else if (req.TargetType == "Book" && req.TargetId != null)
                        {
                            var book = db.Books.Find(req.TargetId);
                            if (book != null) book.IsFrozen = false;
                        }
                    }
                    db.SaveChanges();
                }
            }
            LoadUnfreezeRequests();
        }

        private void LoadUsers()
        {
            UsersPanel.Children.Clear();
            using (var db = new ReadAndWriteDBEntities())
            {
                var users = db.Users.OrderBy(u => u.Login).ToList();
                foreach (var user in users)
                {
                    var border = new Border
                    {
                        Background = Brushes.White,
                        BorderBrush = new SolidColorBrush(Color.FromRgb(221, 221, 221)),
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(15),
                        Margin = new Thickness(0, 0, 0, 10)
                    };
                    var stack = new StackPanel();
                    stack.Children.Add(new TextBlock { Text = $"{user.DisplayName} ({user.Login})", FontWeight = FontWeights.Bold });
                    stack.Children.Add(new TextBlock { Text = $"Роль: {user.Role} | Email: {user.Email}" });
                    stack.Children.Add(new TextBlock { Text = $"Заморожен: {(user.IsFrozen ? "Да" : "Нет")}" });

                    var btns = new WrapPanel { Margin = new Thickness(0, 10, 0, 0) };

                    var roleBtn = new Button
                    {
                        Content = "🔄 Сменить роль",
                        Background = new SolidColorBrush(Color.FromRgb(52, 152, 219)),
                        Foreground = Brushes.White,
                        BorderThickness = new Thickness(0),
                        Margin = new Thickness(0, 0, 8, 5),
                        Padding = new Thickness(10, 5, 10, 5)
                    };
                    roleBtn.Click += (snd, args) => { ChangeUserRole(user.UserId); };

                    var freezeBtn = new Button
                    {
                        Content = user.IsFrozen ? "🔓 Разморозить" : "🔒 Заморозить",
                        Background = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                        Foreground = Brushes.White,
                        BorderThickness = new Thickness(0),
                        Margin = new Thickness(0, 0, 8, 5),
                        Padding = new Thickness(10, 5, 10, 5)
                    };
                    freezeBtn.Click += (snd, args) =>
                    {
                        if (!user.IsFrozen)
                        {
                            var dialog = new FreezeAppealDialog
                            {
                                Title = "Укажите причину заморозки"
                            };
                            dialog.ShowDialog();
                            if (!string.IsNullOrEmpty(dialog.Result))
                            {
                                using (var db2 = new ReadAndWriteDBEntities())
                                {
                                    var u = db2.Users.Find(user.UserId);
                                    if (u != null) { u.IsFrozen = true; u.FreezeReason = dialog.Result; db2.SaveChanges(); }
                                }
                            }
                        }
                        else
                        {
                            using (var db2 = new ReadAndWriteDBEntities())
                            {
                                var u = db2.Users.Find(user.UserId);
                                if (u != null) { u.IsFrozen = false; u.FreezeReason = null; db2.SaveChanges(); }
                            }
                        }
                        LoadUsers();
                    };

                    var passBtn = new Button
                    {
                        Content = "🔑 Сменить пароль",
                        Background = new SolidColorBrush(Color.FromRgb(155, 89, 182)),
                        Foreground = Brushes.White,
                        BorderThickness = new Thickness(0),
                        Margin = new Thickness(0, 0, 8, 5),
                        Padding = new Thickness(10, 5, 10, 5)
                    };
                    passBtn.Click += (snd, args) => ChangePassword(user.UserId);

                    btns.Children.Add(roleBtn);
                    btns.Children.Add(freezeBtn);
                    btns.Children.Add(passBtn);

                    stack.Children.Add(btns);
                    border.Child = stack;
                    UsersPanel.Children.Add(border);
                }
            }
        }

        private void ChangePassword(int userId)
        {
            var dialog = new Window
            {
                Title = "Сменить пароль",
                Width = 350,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            var stack = new StackPanel { Margin = new Thickness(20) };
            stack.Children.Add(new TextBlock { Text = "Введите новый пароль:", FontSize = 14, Margin = new Thickness(0, 0, 0, 10) });
            var passBox = new TextBox { Height = 25, Margin = new Thickness(0, 0, 0, 15) };
            stack.Children.Add(passBox);

            var btn = new Button
            {
                Content = "Сохранить",
                Height = 30,
                Width = 100,
                Background = new SolidColorBrush(Color.FromRgb(30, 58, 95)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0)
            };
            btn.Click += (s, args) =>
            {
                if (string.IsNullOrEmpty(passBox.Text)) { MessageBox.Show("Введите пароль!"); return; }
                using (var db = new ReadAndWriteDBEntities())
                {
                    var user = db.Users.Find(userId);
                    if (user != null) { user.PasswordHash = passBox.Text; db.SaveChanges(); }
                }
                dialog.Close();
                MessageBox.Show("Пароль изменён!", "Успех");
                LoadUsers();
            };
            stack.Children.Add(btn);
            dialog.Content = stack;
            dialog.ShowDialog();
        }

        private void ChangeUserRole(int userId)
        {
            using (var db = new ReadAndWriteDBEntities())
            {
                var user = db.Users.Find(userId);
                if (user != null)
                {
                    user.Role = user.Role == "Admin" ? "Reader" :
                                user.Role == "Author" ? "Admin" : "Author";
                    db.SaveChanges();
                }
            }
            LoadUsers();
        }
    }
}