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
        static Dictionary<string, int> model_year = new Dictionary<string, int>();
        static Dictionary<string, int> origin = new Dictionary<string, int>();
        static Dictionary<string, int> brand = new Dictionary<string, int>();
        static Dictionary<string, int> model = new Dictionary<string, int>();
        static Dictionary<string, int> type = new Dictionary<string, int>();

        /*
         * Categorical Data: model_year, origin, Brand, Model, Type
         * Numerical Data: mpg, cylinders, displacement, horsepower, weight, acceleration
         * Order of columns
         * 0:  id integer NOT NULL,
         * 1:  mpg real,
         * 2:  cylinders integer,
         * 3:  displacement real,
         * 4:  horsepower real,
         * 5:  weight real,
         * 6:  acceleration real,
         * 7:  model_year integer,
         * 8:  origin integer,
         * 9:  brand text,
         * 10: model text,
         * 11: type text
         */

        // Possible choice to make cylinders and origin Categorical?

        static void Main(string[] args)
        {
           
            SQLiteConnection.CreateFile("Cars.sqlite");

            string connectionString = "Data Source=Cars.sqlite;Version=3;";
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionString);

            m_dbConnection.Open();

            CreateDatabase(m_dbConnection);
            ReadDatabase(m_dbConnection);

            m_dbConnection.Close();

            //DebugReadDictionary(model_year);
            //DebugReadDictionary(origin);
            //DebugReadDictionary(brand);
            //DebugReadDictionary(model);
            //DebugReadDictionary(type);

            Console.ReadKey();
        }

        private static void CreateDatabase(SQLiteConnection m_dbConnection)
        {
            string sql = File.ReadAllText("..\\..\\Database\\autompg.sql");
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
        }

        private static void ReadDatabase(SQLiteConnection m_dbConnection)
        {
            string sql = "SELECT * FROM autompg";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                FillDictionary(model_year, reader[7].ToString());
                FillDictionary(origin, reader[8].ToString());
                FillDictionary(brand, reader[9].ToString());
                FillDictionary(model, reader[10].ToString());
                FillDictionary(type, reader[11].ToString());
            }

        }

        private static void FillDictionary(Dictionary<string, int> attribute, string value)
        {
            if(attribute.ContainsKey(value))
            {
                attribute[value]++;
            }
            else
            {
                attribute.Add(value, 1);
            }
        }

        #region Debug Methods
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

        private static void DebugReadDictionary(Dictionary<string, int> attribute)
        {
            foreach(KeyValuePair<string, int> value in attribute)
            {
                Console.WriteLine(value.ToString());
            }
        }

        #endregion

    }
}
