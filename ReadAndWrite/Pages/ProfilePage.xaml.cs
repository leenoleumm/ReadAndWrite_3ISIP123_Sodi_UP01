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
    public partial class ProfilePage : Page
    {
        public ProfilePage()
        {
            InitializeComponent();
            LoadProfile();
            LoadMyReviews();
        }

        private void LoadProfile()
        {
            ProfileName.Text = "Имя: " + CurrentUser.DisplayName;
            ProfileLogin.Text = "Логин: " + CurrentUser.Login;
            ProfileEmail.Text = "Email: " + CurrentUser.Email;
            ProfileRole.Text = "Роль: " + (CurrentUser.Role == "Reader" ? "Читатель" :
                                            CurrentUser.Role == "Author" ? "Автор" : "Администратор");

            if (CurrentUser.Role == "Author" || CurrentUser.Role == "Admin")
                BtnRequestAuthor.Visibility = Visibility.Collapsed;

            if (CurrentUser.IsFrozen)
            {
                FreezeWarning.Visibility = Visibility.Visible;
                using (var db = new ReadAndWriteDBEntities())
                {
                    var user = db.Users.Find(CurrentUser.Id);
                    if (user != null && !string.IsNullOrEmpty(user.FreezeReason))
                        FreezeReason.Text = "Причина: " + user.FreezeReason;
                    else
                        FreezeReason.Text = "Причина не указана";
                }
            }
        }

        private void LoadMyReviews()
        {
            MyReviewsPanel.Children.Clear();
            using (var db = new ReadAndWriteDBEntities())
            {
                var reviews = db.Reviews
                    .Where(r => r.UserId == CurrentUser.Id)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList();

                foreach (var review in reviews)
                {
                    var book = db.Books.Find(review.BookId);
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
                    stack.Children.Add(new TextBlock
                    {
                        Text = $"📖 {book?.Title ?? "Книга удалена"} — ★ {review.Rating}/10",
                        FontWeight = FontWeights.Bold
                    });
                    stack.Children.Add(new TextBlock
                    {
                        Text = review.Text,
                        TextWrapping = TextWrapping.Wrap,
                        Margin = new Thickness(0, 5, 0, 0)
                    });

                    border.Child = stack;
                    MyReviewsPanel.Children.Add(border);
                }

                if (reviews.Count == 0)
                    MyReviewsPanel.Children.Add(new TextBlock
                    {
                        Text = "Вы пока не оставили ни одного отзыва.",
                        Foreground = Brushes.Gray,
                        FontStyle = FontStyles.Italic
                    });
            }
        }

        private void BtnRequestAuthor_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new ReadAndWriteDBEntities())
            {
                db.AuthorRequests.Add(new AuthorRequests
                {
                    UserId = CurrentUser.Id,
                    Status = "Pending",
                    CreatedAt = System.DateTime.Now
                });
                db.SaveChanges();
            }
            MessageBox.Show("Заявка на роль Автора отправлена!", "Успех");
            BtnRequestAuthor.IsEnabled = false;
            BtnRequestAuthor.Content = "⏳ Заявка на рассмотрении...";
        }

        private void AppealFreeze_Click(object sender, RoutedEventArgs e)
        {
            _ = new BookDetailPage(0).GetType() 
                .GetMethod("ShowInputDialog", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var dialog = new FreezeAppealDialog();
            dialog.ShowDialog();

            if (!string.IsNullOrEmpty(dialog.Result))
            {
                using (var db = new ReadAndWriteDBEntities())
                {
                    db.UnfreezeRequests.Add(new UnfreezeRequests
                    {
                        UserId = CurrentUser.Id,
                        TargetType = "Account",
                        TargetId = null,
                        Reason = dialog.Result,
                        Status = "Pending",
                        CreatedAt = System.DateTime.Now
                    });
                    db.SaveChanges();
                }
                MessageBox.Show("Заявка на разморозку отправлена!", "Успех");
            }
        }
    }
}