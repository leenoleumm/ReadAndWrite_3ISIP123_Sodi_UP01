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
using Microsoft.Win32;

namespace ReadAndWrite
{
    public partial class AuthorPage : Page
    {
        public AuthorPage()
        {
            InitializeComponent();
            LoadMyBooks();
        }

        private void LoadMyBooks()
        {
            AuthorBooksPanel.Children.Clear();
            using (var db = new ReadAndWriteDBEntities())
            {
                var books = db.Books.Where(b => b.AuthorId == CurrentUser.Id && !b.IsFrozen).ToList();

                foreach (var book in books)
                {
                    var card = CreateBookCard(book);
                    card.MouseLeftButtonDown += (s, e) =>
                    {
                        var main = Window.GetWindow(this) as MainWindow;
                        if (main != null) main.ContentFrame.Navigate(new BookDetailPage(book.BookId));
                    };
                    card.MouseRightButtonDown += (s, e) => EditBook(book);
                    AuthorBooksPanel.Children.Add(card);
                }

                if (books.Count == 0)
                    AuthorBooksPanel.Children.Add(new TextBlock
                    {
                        Text = "У вас пока нет опубликованных книг.",
                        FontSize = 16,
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(10, 20, 0, 0)
                    });
            }
        }

        private Border CreateBookCard(Books book)
        {
            var card = new Border
            {
                Width = 200,
                Height = 360,
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
                    Height = 180
                };
                img.Clip = new RectangleGeometry { RadiusX = 10, RadiusY = 10, Rect = new Rect(0, 0, 180, 180) };
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
                Margin = new Thickness(10, 10, 10, 2),
                TextWrapping = TextWrapping.Wrap
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
                    Margin = new Thickness(10, 0, 10, 5)
                });
            }

            panel.Children.Add(new TextBlock
            {
                Text = "ПКМ — редактировать",
                FontSize = 10,
                Foreground = Brushes.Gray,
                Margin = new Thickness(10, 0, 10, 0)
            });

            card.Child = panel;
            return card;
        }

        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            string selectedCoverPath = null;

            var window = new Window
            {
                Title = "Добавить книгу",
                Width = 450,
                Height = 650,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            var stack = new StackPanel { Margin = new Thickness(20) };

            stack.Children.Add(new TextBlock { Text = "Название:", FontWeight = FontWeights.Bold });
            var titleBox = new TextBox { Height = 25, Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(titleBox);

            stack.Children.Add(new TextBlock { Text = "Описание:", FontWeight = FontWeights.Bold });
            var descBox = new TextBox { Height = 60, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(descBox);

            stack.Children.Add(new TextBlock { Text = "Текст книги:", FontWeight = FontWeights.Bold });
            var textBox = new TextBox { Height = 150, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10), AcceptsReturn = true };
            stack.Children.Add(textBox);

            stack.Children.Add(new TextBlock { Text = "Обложка:", FontWeight = FontWeights.Bold });
            var coverPathBox = new TextBlock { Text = "Файл не выбран", Margin = new Thickness(0, 0, 0, 5), Foreground = Brushes.Gray };
            var chooseCoverBtn = new Button { Content = "📁 Выбрать файл", Height = 25, Margin = new Thickness(0, 0, 0, 10) };

            chooseCoverBtn.Click += (s, args) =>
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp",
                    Title = "Выберите обложку книги"
                };
                if (dialog.ShowDialog() == true)
                {
                    selectedCoverPath = dialog.FileName;
                    coverPathBox.Text = "Выбран: " + System.IO.Path.GetFileName(selectedCoverPath);
                    coverPathBox.Foreground = Brushes.Green;
                }
            };
            stack.Children.Add(chooseCoverBtn);
            stack.Children.Add(coverPathBox);

            stack.Children.Add(new TextBlock { Text = "Жанры:", FontWeight = FontWeights.Bold });
            var genresPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };
            var genreCheckboxes = new List<CheckBox>();
            using (var db = new ReadAndWriteDBEntities())
            {
                foreach (var genre in db.Genres.OrderBy(g => g.Name))
                {
                    var cb = new CheckBox { Content = genre.Name, Tag = genre.GenreId };
                    genreCheckboxes.Add(cb);
                    genresPanel.Children.Add(cb);
                }
            }
            stack.Children.Add(genresPanel);

            var btn = new Button
            {
                Content = "Опубликовать",
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(30, 58, 95)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 10, 0, 0)
            };
            btn.Click += (s, args) =>
            {
                if (string.IsNullOrEmpty(titleBox.Text)) { MessageBox.Show("Введите название!"); return; }
                using (var db = new ReadAndWriteDBEntities())
                {
                    var newBook = new Books
                    {
                        Title = titleBox.Text,
                        Description = descBox.Text,
                        TextContent = textBox.Text,
                        CoverPath = selectedCoverPath,
                        AuthorId = CurrentUser.Id,
                        IsFrozen = false,
                        CreatedAt = System.DateTime.Now
                    };
                    db.Books.Add(newBook);
                    db.SaveChanges();

                    foreach (var cb in genreCheckboxes)
                    {
                        if (cb.IsChecked == true)
                            db.BookGenres.Add(new BookGenres { BookId = newBook.BookId, GenreId = (int)cb.Tag });
                    }
                    db.SaveChanges();
                }
                window.Close();
                LoadMyBooks();
                MessageBox.Show("Книга опубликована!", "Успех");
            };
            stack.Children.Add(btn);

            window.Content = new ScrollViewer { Content = stack, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            window.ShowDialog();
        }

        private void EditBook(Books book)
        {
            string selectedCoverPath = book.CoverPath;

            var window = new Window
            {
                Title = "Редактировать книгу",
                Width = 450,
                Height = 650,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            var stack = new StackPanel { Margin = new Thickness(20) };

            stack.Children.Add(new TextBlock { Text = "Название:", FontWeight = FontWeights.Bold });
            var titleBox = new TextBox { Text = book.Title, Height = 25, Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(titleBox);

            stack.Children.Add(new TextBlock { Text = "Описание:", FontWeight = FontWeights.Bold });
            var descBox = new TextBox { Text = book.Description, Height = 60, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) };
            stack.Children.Add(descBox);

            stack.Children.Add(new TextBlock { Text = "Текст книги:", FontWeight = FontWeights.Bold });
            var textBox = new TextBox { Text = book.TextContent, Height = 150, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10), AcceptsReturn = true };
            stack.Children.Add(textBox);

            stack.Children.Add(new TextBlock { Text = "Обложка:", FontWeight = FontWeights.Bold });
            var coverPathBox = new TextBlock
            {
                Text = string.IsNullOrEmpty(selectedCoverPath) ? "Файл не выбран" : "Текущая: " + System.IO.Path.GetFileName(selectedCoverPath),
                Margin = new Thickness(0, 0, 0, 5),
                Foreground = string.IsNullOrEmpty(selectedCoverPath) ? Brushes.Gray : Brushes.Green
            };
            var chooseCoverBtn = new Button { Content = "📁 Выбрать файл", Height = 25, Margin = new Thickness(0, 0, 0, 10) };

            chooseCoverBtn.Click += (s, args) =>
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp",
                    Title = "Выберите обложку книги"
                };
                if (dialog.ShowDialog() == true)
                {
                    selectedCoverPath = dialog.FileName;
                    coverPathBox.Text = "Выбран: " + System.IO.Path.GetFileName(selectedCoverPath);
                    coverPathBox.Foreground = Brushes.Green;
                }
            };
            stack.Children.Add(chooseCoverBtn);
            stack.Children.Add(coverPathBox);

            stack.Children.Add(new TextBlock { Text = "Жанры:", FontWeight = FontWeights.Bold });
            var genresPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };
            var genreCheckboxes = new List<CheckBox>();
            using (var db = new ReadAndWriteDBEntities())
            {
                var existingGenreIds = db.BookGenres.Where(bg => bg.BookId == book.BookId).Select(bg => bg.GenreId).ToList();
                foreach (var genre in db.Genres.OrderBy(g => g.Name))
                {
                    var cb = new CheckBox
                    {
                        Content = genre.Name,
                        Tag = genre.GenreId,
                        IsChecked = existingGenreIds.Contains(genre.GenreId)
                    };
                    genreCheckboxes.Add(cb);
                    genresPanel.Children.Add(cb);
                }
            }
            stack.Children.Add(genresPanel);

            var btn = new Button
            {
                Content = "Сохранить",
                Height = 30,
                Background = new SolidColorBrush(Color.FromRgb(30, 58, 95)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(0, 10, 0, 0)
            };
            btn.Click += (s, args) =>
            {
                if (string.IsNullOrEmpty(titleBox.Text)) { MessageBox.Show("Введите название!"); return; }
                using (var db = new ReadAndWriteDBEntities())
                {
                    var b = db.Books.Find(book.BookId);
                    if (b != null)
                    {
                        b.Title = titleBox.Text; b.Description = descBox.Text; b.TextContent = textBox.Text;
                        b.CoverPath = selectedCoverPath;

                        var oldGenres = db.BookGenres.Where(bg => bg.BookId == book.BookId);
                        db.BookGenres.RemoveRange(oldGenres);

                        foreach (var cb in genreCheckboxes)
                            if (cb.IsChecked == true)
                                db.BookGenres.Add(new BookGenres { BookId = book.BookId, GenreId = (int)cb.Tag });

                        db.SaveChanges();
                    }
                }
                window.Close();
                LoadMyBooks();
                MessageBox.Show("Книга обновлена!", "Успех");
            };
            stack.Children.Add(btn);

            window.Content = new ScrollViewer { Content = stack, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
            window.ShowDialog();
        }

        private void FrozenBooksBtn_Click(object sender, RoutedEventArgs e)
        {
            AuthorBooksPanel.Children.Clear();
            using (var db = new ReadAndWriteDBEntities())
            {
                var frozenBooks = db.Books.Where(b => b.AuthorId == CurrentUser.Id && b.IsFrozen).ToList();

                foreach (var book in frozenBooks)
                {
                    var card = CreateBookCard(book);

                    var appealBtn = new Button
                    {
                        Content = "📩 Оспорить",
                        FontSize = 11,
                        Background = new SolidColorBrush(Color.FromRgb(243, 156, 18)),
                        Foreground = Brushes.White,
                        BorderThickness = new Thickness(0),
                        Margin = new Thickness(10, 5, 10, 0),
                        Cursor = Cursors.Hand
                    };

                    appealBtn.Click += (s, args) =>
                    {
                        var reason = new FreezeAppealDialog();
                        reason.ShowDialog();
                        if (!string.IsNullOrEmpty(reason.Result))
                        {
                            using (var db2 = new ReadAndWriteDBEntities())
                            {
                                db2.UnfreezeRequests.Add(new UnfreezeRequests
                                {
                                    UserId = CurrentUser.Id,
                                    TargetType = "Book",
                                    TargetId = book.BookId,
                                    Reason = reason.Result,
                                    Status = "Pending",
                                    CreatedAt = System.DateTime.Now
                                });
                                db2.SaveChanges();
                            }
                            MessageBox.Show("Заявка на разморозку книги отправлена!", "Успех");
                        }
                    };

                    var panel = card.Child as StackPanel;
                    panel?.Children.Add(appealBtn);

                    AuthorBooksPanel.Children.Add(card);
                }

                if (frozenBooks.Count == 0)
                    AuthorBooksPanel.Children.Add(new TextBlock
                    {
                        Text = "Замороженных книг нет.",
                        FontSize = 16,
                        Foreground = Brushes.Gray,
                        Margin = new Thickness(10, 20, 0, 0)
                    });
            }
        }
    }
}
