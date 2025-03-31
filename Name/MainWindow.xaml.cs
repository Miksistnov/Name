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
using System.Data;

namespace Name
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Database _datab;
        private List<Book> _allBooks;
        public MainWindow()
        {
            InitializeComponent();
            _datab = new Database("Server = 192.168.200.13; Database = Libraryes; Uid = student; Pwd = student");
            LoadBooks();
        }
        private void LoadBooks()
        {
            using(var conn = _datab.GetConnection())
            {
                conn.Open();
                var command = new MySqlCommand("SELECT Books.Id, Books.Title, Books.YearPublished, Books.Genre, Books.IsAvailable, Concat(Authors.FirstName, ' ', Authors.LastName) AS AuthorName FROM Books JOIN Authors ON Books.AuthorID = Authors.Id", conn);
                var adapter = new MySqlDataAdapter(command);
                var dataSet = new DataSet();
                adapter.Fill(dataSet);
                _allBooks= dataSet.Tables[0].AsEnumerable().Select(row => new Book
                {
                    Id = row.Field<int>("Id"),
                    Title = row.Field<string>("Title"),
                    YearPublished=row.Field<int>("YearPublished"),
                    Genre = row.Field<string>("Genre"),
                    IsAvailable=row.Field<bool>("IsAvailable"),
                    AuthorName= row.Field<string>("AuthorName")
                }).ToList();
                BooksGrid.ItemsSource = _allBooks;
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
        private void SearchBooks(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                BooksGrid.ItemsSource = _allBooks;
                return;
            }
            
            searchText = searchText.ToLower();

            var SearchType = (SearchTypeComboBox.SelectedItem as ComboBoxItem)?.Tag.ToString();
            IEnumerable<Book> query = _allBooks;
            switch (SearchType)
            {
                case "Title":
                    query = _allBooks.Where(book => book.Title.ToLower().Contains(searchText));
                    break;
                case "Author":
                    query = _allBooks.Where(book => book.AuthorName.ToLower().Contains(searchText));
                    break;
                default: 
                    query = _allBooks.Where(book => book.Title.ToLower().Contains(searchText) || book.AuthorName.ToLower().Contains(searchText));
                    break;
            }
            BooksGrid.ItemsSource = query.ToList();
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchBooks(SearchTextBox.Text);
        }

        private void SearchTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
