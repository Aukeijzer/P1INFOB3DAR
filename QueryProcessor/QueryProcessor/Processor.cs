﻿using System;
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
        /// <summary>
        /// Initializes db connection strings
        /// </summary>
        public Processor()
        {
            string mainConnectionString = $"Data Source={root}Cars.sqlite;Version=3;";
            main_dbConnection = new SQLiteConnection(mainConnectionString);
            
            string metaConnectionString = $"Data Source={root}CarsMeta.sqlite;Version=3;";
            meta_dbConnection = new SQLiteConnection(metaConnectionString);
            
            main_dbConnection.Open();
            Console.WriteLine(lookupID(2));
            meta_dbConnection.Close();
        }
        public List<string> FindTopK(List<Predicate> predicates, int k)
        {
            int n = 3; //TODO: CHANGE THIS
            scores = new (double, int)[n];

            main_dbConnection.Open();
            meta_dbConnection.Open();
            
            //For each predicate in the list of predicates update the scores
            foreach (var predicate in predicates)
            {
                //numerical
                if (QueryType.Origin < predicate.Query && predicate.Query < QueryType.Brand)
                    UpdateNumericalScores(predicate);
                //categorical
                else
                    UpdateCategoricalScores(predicate);
            }
            //For each attribute not in the list of predicates update the scores

            main_dbConnection.Close();
            meta_dbConnection.Close();
            
            //Sort scores ascending
            Array.Sort(scores);
            
            //Return the IDs of the top k
            List<String> result = new List<string>();
            for(int i = 0; i < k; i++)
                result.Add(lookupID(scores[scores.Length-1-i].Item2));
            return result;
        }

        private string lookupID(int id)
        {
            string sql = "SELECT * FROM autompg WHERE id = " + id;
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
            string type = Type2String(predicate.Query);
            double value = double.Parse(predicate.Value);
            int n = 3; //TODO: change this
            
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

        public double calcGuassian(double mean, double value, double h)
        {
            double exponent = -0.5 * Math.Pow((value - mean) / h, 2);
            return Math.Pow(Math.E, exponent);
        }
        public bool IsNumerical(QueryType query)
        {
            return QueryType.Origin < query && query < QueryType.Brand;
        }
        
        private void UpdateNumericalScores(Predicate predicate)
        {
            string type = Type2String(predicate.Query);
            double h = 1; //TODO: change this
            double idf = CalcIdf(predicate,h);
            double value = double.Parse(predicate.Value);
            
            string sql = "SELECT * FROM autompg ORDER BY id";
            SQLiteCommand command = new SQLiteCommand(sql, main_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            
            for (int i = 0; i < scores.Length; i++)
            {
                reader.Read();
                double entry = (double)reader[type];
                scores[i].Item1 += idf * calcGuassian(value, entry, h);
            }
        }

        private (double,double) lookUpIdfQf(Predicate predicate)
        {
            string sql = $"SELECT (idf,qf) FROM autompg " +
                         $"WHERE attribute = {Type2String(predicate.Query)} " +
                         $"AND value = {predicate.Value}";
            SQLiteCommand command = new SQLiteCommand(sql, meta_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            reader.Read();
            double idf = (double)reader["idf"];
            double qf = (double)reader["qf"];
            
            return (idf,qf);
        }
        private void UpdateCategoricalScores(Predicate predicate)
        {
            string type = Type2String(predicate.Query);
            (double idf, double qf) = lookUpIdfQf(predicate);
            string value = predicate.Value;
            
            string sql = "SELECT * FROM autompg ORDER BY id";
            SQLiteCommand command = new SQLiteCommand(sql, main_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            
            for (int i = 0; i < scores.Length; i++)
            {
                reader.Read();
                string entry = (string)reader[type];
                if (entry == value)
                    scores[i].Item1 += idf * qf;
            }
        }
        private double calculateSimilarity(string query, string entry, double idf, double qf)
        {
            if (query!=entry)
                return 0;
            else
                return idf * qf;
        }
        public static string Type2String(QueryType type)
        {
            switch (type)
            {
                case QueryType.Cylinders:
                    return "cylinders";
                case QueryType.Modelyear:
                    return "modelyear";
                case QueryType.Origin:
                    return "origin";
                case QueryType.Mpg :
                    return "mpg";
                case QueryType.Displacement:
                    return "displacement";
                case QueryType.Horsepower:
                    return "horsepower";
                case QueryType.Weight:
                    return "weight";
                case QueryType.Acceleration:
                    return "acceleration";
                case QueryType.Brand:
                    return "brand";
                case QueryType.Model:
                    return "model";
                case QueryType.Type:
                    return "type";
                default:
                    return null;
            }
        }
    }
}