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
            _datab = new Database("Server=192.168.200.13;Database=Libraryes;Uid=student;Pwd=student;");
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
                var command = new MySqlCommand("SELECT Id, CONCAT(FirstName, ' ', LastName) AS FullName FROM Authors", con);
                var reader = command.ExecuteReader();
                
                while(reader.Read()) 
                {
                    _authors.Add(new Author
                    {
                        Id = reader.GetInt32("Id"),
                        FirstName = reader.GetString("FirstName"),
                        Patronymic = reader.IsDBNull(reader.GetOrdinal("Patronymic")) ? string.Empty : reader.GetString("Patonymic"),
                        LastName = reader.GetString("LastName"),
                        FullName = $"{reader.GetString("LastName")} {reader.GetString("FirstName")}" + $"{(reader.IsDBNull(reader.GetOrdinal("Patronymic")) ? string.Empty : reader.GetString("Patonymic"))}"
                    });
                }
                AuthorComboBox.ItemsSource = _authors;
            }
        }
        private void AddAuthor_Click(object sender, EventArgs e)
        {
            var addAuthorWindow = new AddAuthorWindow();
            if(addAuthorWindow.ShowDialog() == true)
            {
                LoadAuthors();
            }
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
                    command.Parameters.AddWithValue("@YearPublished", YearPublishedBox);
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
