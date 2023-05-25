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
         * Categorical Data: cylinders, model_year, origin, Brand, Model, Type
         * Numerical Data: mpg, displacement, horsepower, weight, acceleration
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
        static Dictionary<string, TableTuple> mpg = new Dictionary<string, TableTuple>();
        static Dictionary<string, TableTuple> displacement = new Dictionary<string, TableTuple>();
        static Dictionary<string, TableTuple> horsepower = new Dictionary<string, TableTuple>();
        static Dictionary<string, TableTuple> weight = new Dictionary<string, TableTuple>();
        static Dictionary<string, TableTuple> acceleration = new Dictionary<string, TableTuple>();
        // End - Numerical Data

        static Computations Comp = new Computations();
        static int totalTuples = 0;

        static void Main(string[] args)
        {
            //Create database file for autompg
            SQLiteConnection.CreateFile("Cars.sqlite");

            string connectionString = "Data Source=Cars.sqlite;Version=3;";
            SQLiteConnection m_dbConnection = new SQLiteConnection(connectionString);

            m_dbConnection.Open();

            //Create database and read data for dictionaries, mainly TF
            CreateDatabase(m_dbConnection);
            ReadDatabase(m_dbConnection);

            m_dbConnection.Close();

            //Fill dictionaries with more data regarding RQF and similarity sets
            ReadWorkload();

            //Create all INSERT statements for workload.txt
            InsertAllDictionaries();

            Console.WriteLine("Preprocessing finished!");
            Console.ReadKey();
        }

        #region Data Reading Methods

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
                FillDictionary(mpg, reader[1].ToString());
                FillDictionary(cylinders, reader[2].ToString());
                FillDictionary(displacement, reader[3].ToString());
                FillDictionary(horsepower, reader[4].ToString());
                FillDictionary(weight, reader[5].ToString());
                FillDictionary(acceleration, reader[6].ToString());
                FillDictionary(model_year, reader[7].ToString());
                FillDictionary(origin, reader[8].ToString());
                FillDictionary(brand, reader[9].ToString());
                FillDictionary(model, reader[10].ToString());
                FillDictionary(type, reader[11].ToString());
                totalTuples++;
            }

        }

        /// <summary>
        /// Read the workload file and fill data in the dictionaries
        /// </summary>
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

        #endregion 

        #region Data Modifying Methods

        /// <summary>
        /// Create new tuples for attribute values and counts the Term Frequency
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        private static void FillDictionary(Dictionary<string, TableTuple> attribute, string value)
        {
            if (attribute.ContainsKey(value))
            {
                attribute[value].IncreaseTF();
            }
            else
            {
                attribute.Add(value, new TableTuple());
            }
        }

        /// <summary>
        /// Fills QF value if key exists in dictionary
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="value"></param>
        /// <param name="rqf"></param>
        private static void FillDictionaryRQF(Dictionary<string, TableTuple> attribute, string value, int rqf)
        {
            if (attribute.ContainsKey(value))
            {
                attribute[value].IncreaseRQF(rqf);
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
                case "mpg":
                    FillDictionaryRQF(mpg, value, qf);
                    break;
                case "cylinders":
                    FillDictionaryRQF(cylinders, value, qf);
                    break;
                case "horsepower":
                    FillDictionaryRQF(horsepower, value, qf);
                    break;
                case "weight":
                    FillDictionaryRQF(weight, value, qf);
                    break;
                case "acceleration":
                    FillDictionaryRQF(acceleration, value, qf);
                    break;
                case "model_year":
                    FillDictionaryRQF(model_year, value, qf);
                    break;
                case "origin":
                    FillDictionaryRQF(origin, value, qf);
                    break;
                case "brand":
                    FillDictionaryRQF(brand, value, qf);
                    break;
                case "model":
                    FillDictionaryRQF(model, value, qf);
                    break;
                case "type":
                    FillDictionaryRQF(type, value, qf);
                    break;
                default:
                    break;
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

        #endregion

        #region Data Exporting Methods

        /// <summary>
        /// Resets the metaload file (if it exists)
        /// Create for every dictionary INSERT statements and inject them into metaload.txt
        /// </summary>
        private static void InsertAllDictionaries()
        {
            string filePath = "..\\..\\Database\\metaload.txt";
            try
            {
                File.Delete(filePath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            InsertValue(mpg, "mpg", false);
            InsertValue(cylinders, "cylinders", true);
            InsertValue(displacement, "displacement", false);
            InsertValue(horsepower, "horsepower", false);
            InsertValue(weight, "weight", false);
            InsertValue(acceleration, "acceleration", false);
            InsertValue(model_year, "model_year", true);
            InsertValue(origin, "origin", true);
            InsertValue(brand, "brand", true);
            InsertValue(model, "model", true);
            InsertValue(type, "type", true);
        }

        /// <summary>
        /// Create an INSERT statement string and insert into workload.txt
        /// </summary>
        /// <param name="attribute"></param>
        /// <param name="name"></param>
        /// <param name="categorical"></param>
        private static void InsertValue(Dictionary<string, TableTuple> attribute, string name, bool categorical)
        {
            List<double> attributeValues = new List<double>();
            List<int> RQFList = new List<int>();
            
            foreach(KeyValuePair<string, TableTuple> tuple in attribute)
            {
                for(int i = 0; i < tuple.Value.TermFrequency; i++)
                {
                    RQFList.Add(tuple.Value.RawQueryFrequency);
                    if(!categorical)
                        attributeValues.Add(Double.Parse(tuple.Key));
                }
            }

            int RQFMax = RQFList.Max();
            string filePath = "..\\..\\Database\\metaload.txt";

            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                foreach (KeyValuePair<string, TableTuple> tuple in attribute)
                {
                    TableTuple data = tuple.Value;
                    double IDF = 0;
                    double QF = Comp.QF(data.RawQueryFrequency, RQFMax);
                    string insert = "";

                    if (categorical)
                    {
                        IDF = Comp.IDFCategorical(totalTuples, data.TermFrequency);
                        string set = CreateSimilaritySetString(data);

                        insert = string.Format("INSERT INTO categorical_metadata VALUES ('{0}', {1}, {2}, {3}, "
                            + set + ")", name, tuple.Key, IDF, QF);
                    }
                    else
                    {
                        IDF = Comp.IDFNumerical(totalTuples, Double.Parse(tuple.Key), attributeValues);

                        insert = string.Format("INSERT INTO numerical_metadata VALUES ('{0}', {1}, {2}, {3})", name, tuple.Key, IDF, QF);
                    }


                    writer.WriteLine(insert);
                }
            }
        }

        /// <summary>
        /// Creates string for the Similarity Set
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static string CreateSimilaritySetString(TableTuple data)
        {
            string set = "(";
            for (int i = 0; i < data.SimilaritySet.Count; i++)
            {
                // do something with each item
                if ((i + 1) == data.SimilaritySet.Count)
                {
                    set += i;
                }
                else
                {
                    set += i + ", ";
                }
            }
            set += ")";
            return set;
        }

        #endregion

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
