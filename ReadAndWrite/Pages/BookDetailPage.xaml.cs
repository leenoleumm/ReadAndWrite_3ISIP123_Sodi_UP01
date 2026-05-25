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
using Microsoft.VisualBasic;

namespace ReadAndWrite
{
    public partial class BookDetailPage : Page
    {
        private int _bookId;

        public BookDetailPage(int bookId)
        {
            InitializeComponent();
            _bookId = bookId;
            LoadBook();
            LoadReviews();
            ShowAdminButtons();
        }

        private void LoadBook()
        {
            using (var db = new ReadAndWriteDBEntities())
            {
                var book = db.Books.Include("Users").FirstOrDefault(b => b.BookId == _bookId);
                if (book != null)
                {
                    BookTitle.Text = book.Title;
                    BookAuthor.Text = "Автор: " + (book.Users?.DisplayName ?? "неизвестен");

                    if (!string.IsNullOrEmpty(book.CoverPath) && System.IO.File.Exists(book.CoverPath))
                    {
                        BookCover.Source = new BitmapImage(new Uri(book.CoverPath));
                        BookCover.Visibility = Visibility.Visible;
                        NoCoverIcon.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        BookCover.Visibility = Visibility.Collapsed;
                        NoCoverIcon.Visibility = Visibility.Visible;
                    }

                    var reviews = db.Reviews.Where(r => r.BookId == _bookId && !r.IsFrozen);
                    double avg = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
                    int count = reviews.Count();
                    BookRating.Text = $"★ {avg:F1} | Отзывов: {count}";

                    BookDesc.Text = book.Description ?? "Описание отсутствует";
                    BookText.Text = book.TextContent ?? "Текст книги отсутствует";

                    var genreNames = db.BookGenres
                        .Where(bg => bg.BookId == _bookId)
                        .Select(bg => bg.Genres.Name)
                        .ToList();
                    BookGenres.Text = "Жанры: " + (genreNames.Any() ? string.Join(", ", genreNames) : "не указаны");
                }
            }
        }

        private void LoadReviews()
        {
            ReviewsPanel.Children.Clear();
            using (var db = new ReadAndWriteDBEntities())
            {
                var reviews = db.Reviews
                    .Where(r => r.BookId == _bookId && !r.IsFrozen)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToList();

                foreach (var review in reviews)
                {
                    var user = db.Users.Find(review.UserId);
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
                        Text = $"{user?.DisplayName ?? "Пользователь"} — ★ {review.Rating}/10",
                        FontWeight = FontWeights.Bold,
                        Margin = new Thickness(0, 0, 0, 5)
                    });
                    stack.Children.Add(new TextBlock
                    {
                        Text = review.Text,
                        TextWrapping = TextWrapping.Wrap
                    });
                    stack.Children.Add(new TextBlock
                    {
                        Text = review.CreatedAt.ToString("dd.MM.yyyy"),
                        FontSize = 11,
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(0, 5, 0, 0)
                    });

                    if (CurrentUser.Role != "Admin")
                    {
                        var reportBtn = new Button
                        {
                            Content = "🚩 Пожаловаться",
                            FontSize = 11,
                            Background = Brushes.Transparent,
                            Foreground = new SolidColorBrush(Color.FromRgb(231, 76, 60)),
                            BorderThickness = new Thickness(0),
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Cursor = System.Windows.Input.Cursors.Hand
                        };
                        reportBtn.Click += (s, e) => ReportReview(review.ReviewId);
                        stack.Children.Add(reportBtn);
                    }

                    if (CurrentUser.Role == "Admin")
                    {
                        var freezeBtn = new Button
                        {
                            Content = "🔒 Заморозить отзыв",
                            FontSize = 11,
                            Background = Brushes.Transparent,
                            Foreground = new SolidColorBrush(Color.FromRgb(192, 57, 43)),
                            BorderThickness = new Thickness(0),
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Cursor = System.Windows.Input.Cursors.Hand
                        };
                        freezeBtn.Click += (s, e) => FreezeReview(review.ReviewId);
                        stack.Children.Add(freezeBtn);
                    }

                    border.Child = stack;
                    ReviewsPanel.Children.Add(border);
                }
            }
        }

        private void ShowAdminButtons()
        {
            if (CurrentUser.Role == "Admin")
                FreezeBookBtn.Visibility = Visibility.Visible;
        }

        private void SubmitReview_Click(object sender, RoutedEventArgs e)
        {
            var text = ReviewText.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                MessageBox.Show("Введите текст отзыва!", "Ошибка");
                return;
            }

            var rating = int.Parse(((ComboBoxItem)ReviewRating.SelectedItem).Tag.ToString());

            using (var db = new ReadAndWriteDBEntities())
            {
                db.Reviews.Add(new Reviews
                {
                    BookId = _bookId,
                    UserId = CurrentUser.Id,
                    Text = text,
                    Rating = rating,
                    IsFrozen = false,
                    CreatedAt = System.DateTime.Now
                });
                db.SaveChanges();
            }

            ReviewText.Text = "";
            LoadBook();
            LoadReviews();
            MessageBox.Show("Отзыв добавлен!", "Успех");
        }

        private void ReportBook_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FreezeAppealDialog
            {
                Title = "Жалоба на книгу"
            };
            dialog.ShowDialog();
            if (!string.IsNullOrEmpty(dialog.Result))
            {
                using (var db = new ReadAndWriteDBEntities())
                {
                    db.Reports.Add(new Reports
                    {
                        ReporterId = CurrentUser.Id,
                        TargetType = "Book",
                        TargetId = _bookId,
                        Reason = dialog.Result,
                        Status = "Pending",
                        CreatedAt = System.DateTime.Now
                    });
                    db.SaveChanges();
                }
                MessageBox.Show("Жалоба отправлена!", "Успех");
            }
        }

        private void ReportAuthor_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new ReadAndWriteDBEntities())
            {
                var book = db.Books.Find(_bookId);
                if (book != null)
                {
                    var dialog = new FreezeAppealDialog
                    {
                        Title = "Жалоба на автора"
                    };
                    dialog.ShowDialog();
                    if (!string.IsNullOrEmpty(dialog.Result))
                    {
                        db.Reports.Add(new Reports
                        {
                            ReporterId = CurrentUser.Id,
                            TargetType = "Author",
                            TargetId = book.AuthorId,
                            Reason = dialog.Result,
                            Status = "Pending",
                            CreatedAt = System.DateTime.Now
                        });
                        db.SaveChanges();
                        MessageBox.Show("Жалоба на автора отправлена!", "Успех");
                    }
                }
            }
        }

        private void ReportReview(int reviewId)
        {
            var dialog = new FreezeAppealDialog
            {
                Title = "Жалоба на отзыв"
            };
            dialog.ShowDialog();
            if (!string.IsNullOrEmpty(dialog.Result))
            {
                using (var db = new ReadAndWriteDBEntities())
                {
                    db.Reports.Add(new Reports
                    {
                        ReporterId = CurrentUser.Id,
                        TargetType = "Review",
                        TargetId = reviewId,
                        Reason = dialog.Result,
                        Status = "Pending",
                        CreatedAt = System.DateTime.Now
                    });
                    db.SaveChanges();
                }
                MessageBox.Show("Жалоба отправлена!", "Успех");
            }
        }

        private void FreezeBook_Click(object sender, RoutedEventArgs e)
        {
            using (var db = new ReadAndWriteDBEntities())
            {
                var book = db.Books.Find(_bookId);
                if (book != null)
                {
                    book.IsFrozen = !book.IsFrozen;
                    db.SaveChanges();
                    FreezeBookBtn.Content = book.IsFrozen ? "🔓 Разморозить книгу" : "🔒 Заморозить книгу";
                    MessageBox.Show(book.IsFrozen ? "Книга заморожена!" : "Книга разморожена!", "Готово");
                }
            }
        }

        private void FreezeReview(int reviewId)
        {
            using (var db = new ReadAndWriteDBEntities())
            {
                var review = db.Reviews.Find(reviewId);
                if (review != null)
                {
                    review.IsFrozen = true;
                    db.SaveChanges();
                    LoadReviews();
                    MessageBox.Show("Отзыв заморожен!", "Готово");
                }
            }
        }

        private void BackToCatalog_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow main)
                main.ContentFrame.Navigate(new CatalogPage());
        }
    }
}