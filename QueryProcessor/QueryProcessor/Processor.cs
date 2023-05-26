using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace QueryProcessor
{
    public class Processor
    {
        static string root = "..//..//..//..//";
        private SQLiteConnection main_dbConnection;
        private SQLiteConnection meta_dbConnection;
        private (double, int)[] scores;
        private int n;
        /// <summary>
        /// Initializes db connection strings
        /// </summary>
        public Processor()
        {
            string mainConnectionString = $"Data Source={root}Cars.sqlite;Version=3;";
            main_dbConnection = new SQLiteConnection(mainConnectionString);
            
            string metaConnectionString = $"Data Source={root}CarsMeta.sqlite;Version=3;";
            meta_dbConnection = new SQLiteConnection(metaConnectionString);
            
        }
        public List<string> FindTopK(List<Predicate> predicates, int k)
        {
            main_dbConnection.Open();
            meta_dbConnection.Open();
            
            n = (int) calcN();
            scores = new (double, int)[n + 10];
            for(int i = 0;i<scores.Length;i++)
                scores[i] = (0, i);
            List<QueryType> queries = new List<QueryType>();
            //For each predicate in the list of predicates update the scores
            foreach (var predicate in predicates)
            {
                //numerical
                if (QueryType.Origin < predicate.Query && predicate.Query < QueryType.Brand)
                    UpdateNumericalScores(predicate);
                //categorical
                else
                    UpdateCategoricalScores(predicate);
                
                queries.Add(predicate.Query);
            }
            //For each attribute not in the list of predicates update the scores
            for (int i = 1; i < 12; i++)
            {
                if(!queries.Contains((QueryType)i))
                    UpdateQFScores((QueryType)i);
            }
            //Sort scores ascending
            Array.Sort(scores);
            
            //Return the IDs of the top k
            List<String> result = new List<string>();
            for(int i = 0; i < k; i++)
                result.Add(lookupID(scores[scores.Length-1-i].Item2));
            
            main_dbConnection.Close();
            meta_dbConnection.Close();
            
            return result;
        }

        private void UpdateQFScores(QueryType queryType)
        {
            string attribute = Predicate.Type2String(queryType);
            string sql = $"SELECT id,{attribute} FROM autompg";
            SQLiteCommand command = new SQLiteCommand(sql, main_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                long id = (long) reader["id"];
                string value, table;
                if (Predicate.IsNumerical(queryType))
                {
                    table = "numerical_metadata";
                    value = ((double) reader[attribute]).ToString();
                    value = value.Replace(',', '.');
                }
                else
                {
                    table = "categorical_metadata";
                    if (queryType >= QueryType.Brand)
                        value = (string)reader[attribute];
                    else value = ((long) reader[attribute]).ToString();
                    value = $"\"{value}\"";
                }

                
                string sql2 = $"SELECT qf FROM {table} WHERE attribute = \"{attribute}\" AND value = {value}";
                SQLiteCommand command2 = new SQLiteCommand(sql2, meta_dbConnection);
                SQLiteDataReader reader2 = command2.ExecuteReader();
                reader2.Read();
                double qf = (double) reader2["qf"];
                scores[id].Item1 += Math.Log(qf);
            }
        }

        private string lookupID(int id)
        {
            string sql = $"SELECT * FROM autompg WHERE id = {id}";
            SQLiteCommand command = new SQLiteCommand(sql, main_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            string result = "";
            for (int i = 0; i < 12; i++)
                result += reader[i] + " ";
            
            return result;
        }

        private double CalcIdf(Predicate predicate, double h)
        {
            string type = Predicate.Type2String(predicate.Query);
            double value = double.Parse(predicate.Value);
            
            string sql = $"SELECT {type} FROM autompg";
            SQLiteCommand command = new SQLiteCommand(sql, main_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            double sum = 0;
            while(reader.Read())
            {
                double entry = (double)reader[type];
                sum += calcGuassian(value, entry, h);
            }
            return Math.Log(n / sum);
        }

        private double calcGuassian(double mean, double value, double h)
        {
            double exponent = -0.5 * Math.Pow((value - mean) / h, 2);
            return Math.Pow(Math.E, exponent);
        }

        private long calcN()
        {
            string sql = $"SELECT COUNT(*) FROM autompg";
            SQLiteCommand command = new SQLiteCommand(sql, main_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            bool test = reader.Read();
            return (long)reader[0];
        }
        private double lookUpH(Predicate predicate)
        {
            string sql = $"SELECT h FROM numerical_metadata where value = {predicate.Value}";
            SQLiteCommand command = new SQLiteCommand(sql, meta_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            return (double)reader["h"];
        }

        private void UpdateNumericalScores(Predicate predicate)
        {
            string type = Predicate.Type2String(predicate.Query);
            double h = lookUpH(predicate);
            double idf = CalcIdf(predicate,h);
            double value = double.Parse(predicate.Value);
            
            string sql = "SELECT * FROM autompg ORDER BY id";
            SQLiteCommand command = new SQLiteCommand(sql, main_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                long id = (long)reader["id"];
                double entry = (double)reader[type];
                scores[id].Item1 += idf * calcGuassian(value, entry, h);
            }
        }

        private (double,double) lookUpIdfQf(Predicate predicate)
        {
            string sql = $"SELECT idf,qf FROM categorical_metadata " +
                         $"WHERE attribute = \"{Predicate.Type2String(predicate.Query)}\" " +
                         $"AND value = \"{predicate.Value}\"";
            SQLiteCommand command = new SQLiteCommand(sql, meta_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                double idf = (double)reader["idf"];
                double qf = (double)reader["qf"];
                return (idf, qf);
            }
            else return (0, 0);
            
        }
        private void UpdateCategoricalScores(Predicate predicate)
        {
            string type = Predicate.Type2String(predicate.Query);
            (double idf, double qf) = lookUpIdfQf(predicate);
            string value = predicate.Value;
            
            string sql = $"SELECT id,{type} FROM autompg";
            SQLiteCommand command = new SQLiteCommand(sql, main_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            
            while(reader.Read())
            {
                string entry = reader[type].ToString();
                long id = (long)reader["id"];
                if (entry == value)
                    scores[id].Item1 += idf * qf;
            }
        }
    }
}