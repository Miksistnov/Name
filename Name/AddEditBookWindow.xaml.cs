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
using System.Windows.Shapes;
using MySqlConnector;

namespace Name
{
    /// <summary>
    /// Логика взаимодействия для AddEditBookWindow.xaml
    /// </summary>
    public partial class AddEditBookWindow : Window
    {
        private readonly Database _datab;
        private readonly Book _book;
        private List<Author> _authors;
        public AddEditBookWindow(Book book=null)
        {
            InitializeComponent();
            _datab = new Database("Server = 192.168.200.13; Database = Libraryes; Uid = student; Pwd = student");
            _book = book;
            LoadAuthors();
            if(book != null)
            {
                TitleBox.Text = book.Title;
                YearPublishedBox.Text = book.YearPublished.ToString();
                GenreBox.Text = book.Genre;
                IsAvailableCheckBox.IsChecked = book.IsAvailable;
                AuthorComboBox.SelectedValue = book.AuthorID;
            }
        }
        private void LoadAuthors()
        {
            _authors = new List<Author>();
            using (var con = _datab.GetConnection())
            {
                con.Open();
                var command = new MySqlCommand("SELECT Id, FirstName, COALESCE(Patronomic, '') as Patronomic, LastName FROM Authors ORDER BY LastName, FirstName", con);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                         var author = new Author
                        {
                            Id = reader.GetInt32("Id"),
                            FirstName = reader.GetString("FirstName"),
                            Patronomic = reader.GetString("Patronomic"),
                            LastName = reader.GetString("LastName"),
                            FullName = $"{reader.GetString("LastName")} {reader.GetString("FirstName")} {reader.GetString("Patronomic")}"
                        };
                        _authors.Add(author);
                    }
                }
            }
            Dispatcher.Invoke(() =>
            {
                AuthorComboBox.ItemsSource = _authors;
                AuthorComboBox.DisplayMemberPath = "FullName";
                AuthorComboBox.SelectedValuePath = "Id";
                if(_book != null)
                {
                    AuthorComboBox.SelectedValue = _book.AuthorID;
                }
            });
        }
        private void AddAuthor_Click(object sender, EventArgs e)
        {
            var addAuthorWindow = new AddAuthorWindow();
            if(addAuthorWindow.ShowDialog() == true)
            {
                LoadAuthors();
                AuthorComboBox.Focus();
                if(AuthorComboBox.Items.Count > 0)
                {
                    AuthorComboBox.SelectedIndex = AuthorComboBox.Items.Count - 1;
                    var scrollViewer = FindVisualChild<ScrollViewer>(AuthorComboBox);
                    scrollViewer?.ScrollToEnd();
                }
            }
        }
        private static T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for( int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if( child != null  && child is T)
                    return (T)child;
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TitleBox.Text) ||
                AuthorComboBox.SelectedValue == null ||
                string.IsNullOrEmpty(YearPublishedBox.Text) ||
                string.IsNullOrEmpty(GenreBox.Text))
            {
                MessageBox.Show("Все поля должны быть заполнены.");
                return;
            }
            if(!int.TryParse(YearPublishedBox.Text, out int year) || year > DateTime.Now.Year)
            {
                MessageBox.Show("Некорректный год публикации");
            }
            
            try
            {
                using (var con = _datab.GetConnection())
                {
                    con.Open();
                    MySqlCommand command;
                    if (_book == null)
                    {
                        command = new MySqlCommand("INSERT INTO Books (Title, AuthorID,YearPublished, Genre, IsAvailable) VALUES (@Title, @AuthorID, @YearPublished, @Genre, @IsAvailable)", con);
                    }
                    else
                    {
                        command = new MySqlCommand("UPDATE Books SET Title = @Title, AuthorID = @AuthorID, YearPublished = @YearPublished, Genre = @Genre, IsAvailable =  @IsAvailable WHERE Id = @Id", con);
                        command.Parameters.AddWithValue("@Id", _book.Id);
                    }
                    command.Parameters.AddWithValue("@Title", TitleBox.Text);
                    command.Parameters.AddWithValue("@AuthorID", AuthorComboBox.SelectedValue);
                    command.Parameters.AddWithValue("@YearPublished", year);
                    command.Parameters.AddWithValue("@Genre", GenreBox.Text);
                    command.Parameters.AddWithValue("@IsAvailable", IsAvailableCheckBox.IsChecked ?? false);
                    command.ExecuteNonQuery();
                }
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
            }
        }
    }
}
