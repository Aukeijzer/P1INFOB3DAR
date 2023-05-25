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

        // Categorical Data
        static Dictionary<string, TableTuple> cylinders = new Dictionary<string, TableTuple>();
        static Dictionary<string, TableTuple> model_year = new Dictionary<string, TableTuple>();
        static Dictionary<string, TableTuple> origin = new Dictionary<string, TableTuple>();
        static Dictionary<string, TableTuple> brand = new Dictionary<string, TableTuple>();
        static Dictionary<string, TableTuple> model = new Dictionary<string, TableTuple>();
        static Dictionary<string, TableTuple> type = new Dictionary<string, TableTuple>();
        // End - Categorical Data

        // Numerical Data

        // End - Numerical Data

        static void Main(string[] args)
        {
           
            SQLiteConnection.CreateFile("Cars.sqlite");

            string connectionString = "Data Source=Cars.sqlite;Version=3;";
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionString);

            m_dbConnection.Open();

            CreateDatabase(m_dbConnection);
            ReadDatabase(m_dbConnection);

            m_dbConnection.Close();

            ReadWorkload();
            //DebugReadDictionary(cylinders);
            //DebugReadDictionary(model_year);
            //DebugReadDictionary(origin);
            //DebugReadDictionary(brand);
            //DebugReadDictionary(model);
            DebugReadDictionary(type);

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
                FillDictionary(cylinders, reader[2].ToString());
                FillDictionary(model_year, reader[7].ToString());
                FillDictionary(origin, reader[8].ToString());
                FillDictionary(brand, reader[9].ToString());
                FillDictionary(model, reader[10].ToString());
                FillDictionary(type, reader[11].ToString());
            }

        }

        /// <summary>
        /// Create new tuples for attribute values and counts the Term Frequency
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        private static void FillDictionary(Dictionary<string, TableTuple> attribute, string value)
        {
            if(attribute.ContainsKey(value))
            {
                attribute[value].IncreaseTF();
            }
            else
            {
                attribute.Add(value, new TableTuple());
            }
        }

        private static void ReadWorkload()
        {
            string file = "..\\..\\Database\\workload.txt";

            IEnumerable<string> lines = File.ReadLines(file);

            int workloadID = 0;

            foreach (string line in lines.Skip(2))
            {
                if (String.IsNullOrEmpty(line)) break;

                int qfIndex = line.IndexOf("times");
                int qf = Int32.Parse(line.Substring(0, qfIndex - 1));

                int index = line.IndexOf("WHERE");
                string ceq = line.Substring(index + 6);
                string[] seperator = new string[] {" AND "};

                string[] queries = ceq.Split(seperator, StringSplitOptions.None);

                foreach(string subquery in queries)
                {
                    if (subquery.Contains(" IN "))
                    {
                        AddToSimilarity(workloadID, subquery);
                    }
                    else
                    {
                        AddToQueryFrequency(qf, subquery);
                    }

                }
                workloadID++;
            }

        }

        /// <summary>
        /// Adds the query frequency amount to a certain value
        /// </summary>
        /// <param name="qf"></param>
        /// <param name="query"></param>
        private static void AddToQueryFrequency(int qf, string query)
        {
            string[] seperator2 = new string[] { " = " };
            string[] equalityQuery = query.Split(seperator2, StringSplitOptions.None);
            string attribute = equalityQuery[0];
            string value = equalityQuery[1].Trim('\'');

            switch (attribute)
            {
                case "cylinders":
                    FillDictionaryQF(cylinders, value, qf);
                    break;
                case "model_year":
                    FillDictionaryQF(model_year, value, qf);
                    break;
                case "origin":
                    FillDictionaryQF(origin, value, qf);
                    break;
                case "brand":
                    FillDictionaryQF(brand, value, qf);
                    break;
                case "model":
                    FillDictionaryQF(model, value, qf);
                    break;
                case "type":
                    FillDictionaryQF(type, value, qf);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Fills QF value if key exists in dictionary
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <param name="qf"></param>
        private static void FillDictionaryQF(Dictionary<string, TableTuple> attribute, string value, int qf)
        {
            if (attribute.ContainsKey(value))
            {
                attribute[value].IncreaseQF(qf);
            }
        }

        /// <summary>
        /// Adds an ID to brand values if they are contained in a "brand IN" query
        /// </summary>
        /// <param name="workloadID"></param>
        /// <param name="query"></param>
        private static void AddToSimilarity(int workloadID, string query)
        {   
            int index = query.IndexOf("IN");
            string[] subqueries = query.Substring(index + 4).Split(')')[0].Split(',');

            if (query.Contains("brand"))
            {
                foreach (string brandQuery in subqueries)
                {
                    brand[brandQuery.Trim('\'')].AddToSet(workloadID);
                }
            }
            else if(query.Contains("type"))
            {
                foreach (string typeQuery in subqueries)
                {
                    type[typeQuery.Trim('\'')].AddToSet(workloadID);
                }
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

        private static void DebugReadDictionary(Dictionary<string, TableTuple> attribute)
        {
            foreach(KeyValuePair<string, TableTuple> value in attribute)
            {
                Console.WriteLine(value.ToString());
            }
        }

        #endregion

    }
}
