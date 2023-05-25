using System;
using System.Collections.Generic;
using System.Linq;

namespace QueryProcessor.Properties
{
    public static class Interface
    {
        public static void Run()
        {
            bool running = true;
            while (running)
            {
                Console.WriteLine("Enter a query:");
                string query = Console.ReadLine();
                (bool valid, var predicates, int k) = ProcessQuery(query);
                if (query == "exit")
                    running = false;
                else if (valid)
                {
                    Console.WriteLine("correct");
                }
                else
                    Console.WriteLine("Invalid query");
            }

            Console.WriteLine("Exiting...");
        }
        
        private static (bool,List<Predicate>,int) ProcessQuery(string query)
        {
            //checking if semicolon is present
            if (query.Length>0 && query[query.Length-1]==';')
                query = query.Substring(0, query.Length - 1);
            else return default;
            
            //splitting query into tokens
            string[] tokens = query.ToLower().Split(',');
            int k = 10;
            List<Predicate> predicates = new List<Predicate>();
            
            //parsing tokens into predicates
            foreach (string token in tokens)
            {
                string[] expression = token.Split('=');
                if (expression.Length != 2)
                    return default;
                
                Predicate predicate = MkPredicate(expression[0].Trim(), expression[1].Trim());
                if (predicate.Query == QueryType.Invalid)
                    return default;
                
                if (predicate.Query == QueryType.Kvalue)
                    k = int.Parse(predicate.value);
                else
                    predicates.Add(predicate);
            }

            return (true,predicates,k);
        }
        public static Predicate invalidPredicate = new Predicate(QueryType.Invalid, "");
        public static Predicate MkPredicate(string s1, string s2)
        {
            QueryType type = String2Type(s1);
            //integer
            if (type <= QueryType.Origin && int.TryParse(s2, out int i))
                    return new Predicate(type, s2);
            //real
            if(type<=QueryType.Acceleration && double.TryParse(s2, out double d))
                    return new Predicate(type, s2);
            //text
            if(type<=QueryType.Type)
                return new Predicate(type, s2);
            
            //invalid type
            return invalidPredicate;
        }
        public static QueryType String2Type(string s)
        {
            switch (s)
            {
                case "cylinders":
                    return QueryType.Cylinders;
                case "modelyear":
                    return QueryType.Modelyear;
                case "origin":
                    return QueryType.Origin;
                case "mpg":
                    return QueryType.Mpg;
                case "displacement":
                    return QueryType.Displacement;
                case "horsepower":
                    return QueryType.Horsepower;
                case "weight":
                    return QueryType.Weight;
                case "acceleration":
                    return QueryType.Acceleration;
                case "brand":
                    return QueryType.Brand;
                case "model":
                    return QueryType.Model;
                case "type":
                    return QueryType.Type;
                case "k":
                    return QueryType.Kvalue;
            }

            return QueryType.Invalid;
        }
    }
    public struct Predicate
    {
        public Predicate(QueryType query, string value)
        {
            this.Query = query;
            this.value = value;
        }
        public QueryType Query;
        public string value;
    }
    public enum QueryType
    {
        //integer
        Kvalue,
        Cylinders,
        Modelyear,
        Origin,
        //real
        Mpg,
        Displacement,
        Horsepower,
        Weight,
        Acceleration,
        //text
        Brand,
        Model,
        Type,
        //misc
        Invalid
    }
}