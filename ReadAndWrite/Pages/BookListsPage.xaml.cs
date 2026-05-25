using System;
using System.Collections.Generic;
using System.Data.Entity;
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
    public partial class BookListsPage : Page
    {
        private string _currentStatus = "Reading";
        private List<Books> _allBooks;

        public BookListsPage()
        {
            InitializeComponent();
            LoadGenres();
            LoadBooks();
        }

        private void LoadGenres()
        {
            using (var db = new ReadAndWriteDBEntities())
            {
                var genres = db.Genres.OrderBy(g => g.Name).ToList();
                foreach (var genre in genres)
                    GenreFilter.Items.Add(new ComboBoxItem { Content = genre.Name });
            }
        }

        private void LoadBooks()
        {
            HighlightButton(_currentStatus);
            using (var db = new ReadAndWriteDBEntities())
            {
                var bookIds = db.UserBookList
                    .Where(ubl => ubl.UserId == CurrentUser.Id && ubl.Status == _currentStatus)
                    .Select(ubl => ubl.BookId)
                    .ToList();

                _allBooks = db.Books
                    .Where(b => bookIds.Contains(b.BookId) && !b.IsFrozen)
                    .Include("Users")
                    .ToList();
            }
            ApplyFilters();
        }

        private void HighlightButton(string status)
        {
            var buttons = new Dictionary<string, Button>
            {
                { "Reading", BtnReading }, { "Read", BtnRead },
                { "Planned", BtnPlanned }, { "Abandoned", BtnAbandoned }
            };

            foreach (var kvp in buttons)
            {
                kvp.Value.Background = kvp.Key == status
                    ? new SolidColorBrush(Color.FromRgb(30, 58, 95))
                    : new SolidColorBrush(Color.FromRgb(44, 82, 130));
            }
        }

        private void DisplayBooks(List<Books> books)
        {
            BookGrid.Children.Clear();

            foreach (var book in books)
            {
                var card = new Border
                {
                    Width = 200,
                    Height = 340,
                    Margin = new Thickness(10),
                    Background = Brushes.White,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(12),
                    Cursor = Cursors.Hand
                };

                var panel = new StackPanel();

                var cover = new Border
                {
                    Width = 180,
                    Height = 180,
                    Background = new SolidColorBrush(Color.FromRgb(30, 58, 95)),
                    CornerRadius = new CornerRadius(10),
                    Margin = new Thickness(10, 10, 10, 5),
                    ClipToBounds = true
                };

                if (!string.IsNullOrEmpty(book.CoverPath) && System.IO.File.Exists(book.CoverPath))
                {
                    var img = new Image
                    {
                        Source = new BitmapImage(new Uri(book.CoverPath)),
                        Stretch = Stretch.UniformToFill,
                        Width = 180,
                        Height = 180,
                        Clip = new RectangleGeometry { RadiusX = 10, RadiusY = 10, Rect = new Rect(0, 0, 180, 180) }
                    };
                    cover.Child = img;
                    cover.Background = Brushes.Transparent;
                }
                else
                {
                    cover.Child = new TextBlock
                    {
                        Text = "📖",
                        FontSize = 60,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = Brushes.White
                    };
                }
                panel.Children.Add(cover);

                panel.Children.Add(new TextBlock
                {
                    Text = book.Title,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(10, 5, 10, 2),
                    TextWrapping = TextWrapping.Wrap
                });

                panel.Children.Add(new TextBlock
                {
                    Text = book.Users?.DisplayName ?? "Автор неизвестен",
                    FontSize = 12,
                    Foreground = Brushes.Gray,
                    Margin = new Thickness(10, 0, 10, 2)
                });

                using (var db = new ReadAndWriteDBEntities())
                {
                    var reviews = db.Reviews.Where(r => r.BookId == book.BookId && !r.IsFrozen);
                    double avg = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
                    panel.Children.Add(new TextBlock
                    {
                        Text = $"★ {avg:F1}",
                        FontSize = 12,
                        Foreground = new SolidColorBrush(Color.FromRgb(241, 196, 15)),
                        Margin = new Thickness(10, 5, 10, 0)
                    });
                }

                card.MouseRightButtonDown += (s, e) => MoveBook(book.BookId);
                card.MouseLeftButtonDown += (s, e) =>
                {
                    var main = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
                    main?.ContentFrame.Navigate(new BookDetailPage(book.BookId));
                };

                card.Child = panel;
                BookGrid.Children.Add(card);
            }
        }

        private void MoveBook(int bookId)
        {
            var window = new Window
            {
                Title = "Переместить книгу",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var stack = new StackPanel { Margin = new Thickness(20) };
            stack.Children.Add(new TextBlock
            {
                Text = "Выберите новый список:",
                FontSize = 14,
                Margin = new Thickness(0, 0, 0, 15)
            });

            foreach (var status in new[] { "Reading", "Read", "Planned", "Abandoned" })
            {
                if (status != _currentStatus)
                {
                    var btn = new Button
                    {
                        Content = status == "Reading" ? "📖 Читаю" :
                                  status == "Read" ? "✅ Прочитано" :
                                  status == "Planned" ? "📋 В планах" : "🚫 Заброшено",
                        Margin = new Thickness(0, 3, 0, 0),
                        Tag = status
                    };
                    btn.Click += (s, e) =>
                    {
                        using (var db = new ReadAndWriteDBEntities())
                        {
                            var item = db.UserBookList
                                .FirstOrDefault(ubl => ubl.UserId == CurrentUser.Id && ubl.BookId == bookId);
                            if (item != null)
                            {
                                item.Status = (string)((Button)s).Tag;
                                db.SaveChanges();
                            }
                        }
                        window.Close();
                        LoadBooks();
                    };
                    stack.Children.Add(btn);
                }
            }
            window.Content = stack;
            window.ShowDialog();
        }

        private void ApplyFilters()
        {
            if (_allBooks == null) return;
            var filtered = _allBooks.AsEnumerable();
            var search = SearchBox.Text?.ToLower() ?? "";

            if (!string.IsNullOrEmpty(search))
                filtered = filtered.Where(b =>
                    b.Title.ToLower().Contains(search) ||
                    (b.Users?.DisplayName ?? "").ToLower().Contains(search));

            if (GenreFilter.SelectedIndex > 0)
            {
                var genreName = ((ComboBoxItem)GenreFilter.SelectedItem).Content.ToString();
                using (var db = new ReadAndWriteDBEntities())
                {
                    var ids = db.BookGenres
                        .Where(bg => bg.Genres.Name == genreName)
                        .Select(bg => bg.BookId).ToList();
                    filtered = filtered.Where(b => ids.Contains(b.BookId));
                }
            }

            var sortTag = ((ComboBoxItem)SortOrder.SelectedItem).Tag.ToString();
            if (sortTag == "rating")
            {
                filtered = filtered.OrderByDescending(b =>
                {
                    using (var db = new ReadAndWriteDBEntities())
                    {
                        var reviews = db.Reviews.Where(r => r.BookId == b.BookId && !r.IsFrozen);
                        return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
                    }
                });
            }
            else
            {
                filtered = filtered.OrderBy(b => b.Title);
            }

            DisplayBooks(filtered.ToList());
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void GenreFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
        private void SortOrder_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();

        private void BtnReading_Click(object sender, RoutedEventArgs e) { _currentStatus = "Reading"; LoadBooks(); }
        private void BtnRead_Click(object sender, RoutedEventArgs e) { _currentStatus = "Read"; LoadBooks(); }
        private void BtnPlanned_Click(object sender, RoutedEventArgs e) { _currentStatus = "Planned"; LoadBooks(); }
        private void BtnAbandoned_Click(object sender, RoutedEventArgs e) { _currentStatus = "Abandoned"; LoadBooks(); }
    }
}