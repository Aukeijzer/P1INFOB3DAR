using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Data.Common;
using System.IO;

namespace MetaDatabaseCreator
{
    internal class Program
    {

        static void Main(string[] args)
        {
            SQLiteConnection.CreateFile("Cars.sqlite");

            string connectionString = "Data Source=Cars.sqlite;Version=3;";
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionString);
            m_dbConnection.Open();

            CreateDatabase(m_dbConnection);
            DebugReadDatabase(m_dbConnection);

            m_dbConnection.Close();

            Console.ReadKey();
        }

        // Categorical Data: Brand, Model, Type
        // Numerical Data: mpg, cylinders, displacement, horsepower, weight, acceleration, model_year, origin

        // Possible choice to make cylinders and origin Categorical?

        private static void CreateDatabase(SQLiteConnection m_dbConnection)
        {
            string sql = File.ReadAllText("..\\..\\Database\\autompg.sql");
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }

        private static void DebugReadDatabase(SQLiteConnection m_dbConnection)
        {
            string sql = "SELECT * FROM autompg";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                Console.WriteLine(reader[0].ToString() + " "
                               + reader[1].ToString() + " "
                               + reader[2].ToString() + " "
                               + reader[3].ToString() + " "
                               + reader[4].ToString() + " "
                               + reader[5].ToString() + " "
                               + reader[6].ToString() + " "
                               + reader[7].ToString() + " "
                               + reader[8].ToString() + " "
                               + reader[9].ToString() + " "
                               + reader[10].ToString() + " "
                               + reader[11].ToString());
            }
        }
    }
}
