using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Linq;
using MySqlConnector;

namespace Name
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Database _datab;
        public MainWindow()
        {
            InitializeComponent();
            _datab = new Database("Server=192.168.200.13;Database=Libraryes;Uid=student;Pwd=student;");
            LoadBooks();
        }
        private void LoadBooks()
        {
            using(var conn = _datab.GetConnection())
            {
                conn.Open();
                var command = new MySqlCommand("SELECT Books.Id, Books.Title, Books.YearPublished, Books.Genre, Books.IsAvailable, Concat(Authors.FirstName, ' ', Authors.LastName) AS AuthorName FROM Books JOIN Authors ON Books.AuthorID = Authors.Id", conn);
                var reader = command.ExecuteReader();
                var books = new List<Book>();
                while (reader.Read())
                {
                    books.Add(new Book
                    {
                        Id = reader.GetInt32("Id"),
                        Title = reader.GetString("Title"),
                        YearPublished = reader.GetInt32("YearPublished"),
                        Genre = reader.GetString("Genre"),
                        IsAvailable = reader.GetBoolean("IsAvailable"),
                        AuthorName = reader.GetString("AuthorName"),
                    });
                }
                BooksGrid.ItemsSource = books;
            }
        }
        private void AddBook_Click(object sender, RoutedEventArgs e)
        {
            var addBookWindow = new AddEditBookWindow();
            addBookWindow.ShowDialog();
            LoadBooks();
        }
        private void EditBook_Click(Object sender, RoutedEventArgs e)
        {
            var selectedBook = BooksGrid.SelectedItem as Book;
            if (selectedBook != null)
            {
                var editBookWindow = new AddEditBookWindow(selectedBook);
                editBookWindow.ShowDialog();
                LoadBooks();
            } 
        }
        private void DeleteBook_Click(Object sender, RoutedEventArgs e)
        {
            var selectedBook = BooksGrid.SelectedItem as Book;
            if (selectedBook != null)
            {
                var result = MessageBox.Show("Вы уверены, что хотите удалить эту книгу?", "Подтверждение удаления", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    using(var conn = _datab.GetConnection())
                    {
                        conn.Open();
                        var command = new MySqlCommand("DELETE FROM Books WHERE Id = @Id", conn);
                        command.Parameters.AddWithValue("@Id", selectedBook.Id);
                        command.ExecuteNonQuery();
                    }
                    LoadBooks();
                }
            }
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = SearchBox.Text.ToLower();
            var books = BooksGrid.ItemsSource as List<Book>;
            if (books != null)
            {
                BooksGrid.ItemsSource= books.Where(b => b.Title.ToLower().Contains(searchText) || b.AuthorName.ToLower().Contains(searchText)).ToList();
            }
        }
    }
}
