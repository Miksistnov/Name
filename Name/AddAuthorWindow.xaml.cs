using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
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
    /// Логика взаимодействия для AddAuthorWindow.xaml
    /// </summary>
    public partial class AddAuthorWindow : Window
    {
        private readonly Database _datab;
        public AddAuthorWindow()
        {
            InitializeComponent();
            _datab = new Database("Server = 192.168.200.13; Database = Libraryes; Uid = student; Pwd = student");
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(FirstNameBox.Text) ||
                string.IsNullOrEmpty(LastNameBox.Text) ||
                BirthdayPicker.SelectedDate == null)
            {
                MessageBox.Show("Все поля должны быть заполнены.");
                return;
            }
            if (BirthdayPicker.SelectedDate > DateTime.Now)
            {
                MessageBox.Show("Год рождения должен быть меньше текущего.");
                return;
            }

            using (var con = _datab.GetConnection())
            {
                con.Open();
                var command = new MySqlCommand("INSERT INTO Authors (FirstName,Patronomic, LastName,Birthday) VAlUES (@FirstName, @Patronomic, @LastName, @Birthday)", con);
                command.Parameters.AddWithValue("@FirstName", FirstNameBox.Text);
                command.Parameters.AddWithValue("@Patronomic", PatronomycBox.Text);
                command.Parameters.AddWithValue("@LastName", LastNameBox.Text);
                command.Parameters.AddWithValue("@Birthday", BirthdayPicker.SelectedDate);
                command.ExecuteNonQuery();
                var getIdc = new MySqlCommand("SELECT LAST_INSERT_ID()", con);
                int newAuthorId = Convert.ToInt32(getIdc.ExecuteScalar());
                DialogResult = true;
                var newAuthor = new Author
                {
                    Id = newAuthorId,
                    FirstName = FirstNameBox.Text,
                    LastName = LastNameBox.Text,
                    Patronomic = PatronomycBox.Text ?? string.Empty,
                    Birthday = BirthdayPicker.SelectedDate.Value,
                    FullName = $"{LastNameBox.Text} {FirstNameBox.Text} {PatronomycBox.Text}"
                };
                this.Tag = newAuthor;
            }
            Close();
        }
        
    }
}
