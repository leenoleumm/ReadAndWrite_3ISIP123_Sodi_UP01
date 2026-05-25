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
    public partial class CatalogPage : Page
    {
        private List<Books> _allBooks;

        public CatalogPage()
        {
            InitializeComponent();
            LoadGenresIntoFilter();
            LoadAllBooks();
        }

        private void LoadGenresIntoFilter()
        {
            using (var db = new ReadAndWriteDBEntities())
            {
                var genres = db.Genres.OrderBy(g => g.Name).ToList();
                foreach (var genre in genres)
                    GenreFilter.Items.Add(new ComboBoxItem { Content = genre.Name });
            }
        }

        private void LoadAllBooks()
        {
            using (var db = new ReadAndWriteDBEntities())
            {
                _allBooks = db.Books
                    .Where(b => !b.IsFrozen)
                    .Include("Users")
                    .ToList();
            }
            ShowBooks(_allBooks);
        }

        private void ShowBooks(List<Books> books)
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

                card.Child = panel;
                card.MouseLeftButtonDown += (s, e) => OpenBook(book.BookId);
                card.MouseRightButtonDown += (s, e) => AddToList(book);

                BookGrid.Children.Add(card);
            }
        }

        private void OpenBook(int bookId)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
                mainWindow.ContentFrame.Navigate(new BookDetailPage(bookId));
        }

        private void AddToList(Books book)
        {
            var window = new Window
            {
                Title = $"Добавить книгу в список",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var stack = new StackPanel { Margin = new Thickness(20) };
            stack.Children.Add(new TextBlock
            {
                Text = $"Куда добавить \"{book.Title}\"?",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 15)
            });

            var lists = new Dictionary<string, string>
    {
        { "Reading", "📖 Читаю" },
        { "Read", "✅ Прочитано" },
        { "Planned", "📋 В планах" },
        { "Abandoned", "🚫 Заброшено" }
    };

            foreach (var kvp in lists)
            {
                var btn = new Button
                {
                    Content = kvp.Value,
                    Margin = new Thickness(0, 3, 0, 0),
                    Tag = kvp.Key
                };
                btn.Click += (s, e) =>
                {
                    var status = (string)((Button)s).Tag;
                    using (var db = new ReadAndWriteDBEntities())
                    {
                        var existing = db.UserBookList
                            .FirstOrDefault(ubl => ubl.UserId == CurrentUser.Id && ubl.BookId == book.BookId);

                        if (existing != null)
                        {
                            existing.Status = status;
                        }
                        else
                        {
                            db.UserBookList.Add(new UserBookList
                            {
                                UserId = CurrentUser.Id,
                                BookId = book.BookId,
                                Status = status
                            });
                        }
                        db.SaveChanges();
                    }
                    window.Close();
                    MessageBox.Show($"Книга добавлена в список \"{kvp.Value}\"!", "Готово");
                };
                stack.Children.Add(btn);
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

            ShowBooks(filtered.ToList());
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e) => ApplyFilters();
        private void GenreFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
        private void SortOrder_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyFilters();
    }
}