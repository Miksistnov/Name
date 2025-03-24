using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Name
{
    public class Database
    {
        private readonly string _con;
        public Database(string con)
        {
            _con = con;
        }
        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_con);
        }
    }
}
