using System;
using System.Collections.Generic;
using System.Linq;
namespace QueryProcessor
{
    public static class Interface
    {
        public static void Run()
        {
            Processor processor = new Processor();
            bool running = true;
            while (running)
            {
                Console.WriteLine("Enter a query:");
                string query = Console.ReadLine();
                (bool valid, var predicates, int k) = ParseQuery(query);
                if (query == "exit")
                    running = false;
                else if (valid)
                {
                    var result = processor.FindTopK(predicates, k);
                    foreach (string s in result)
                        Console.WriteLine(s);
                }
                else
                    Console.WriteLine("Invalid query");
            }

            Console.WriteLine("Exiting...");
        }
        
        private static (bool,List<Predicate>,int) ParseQuery(string query)
        {
            //checking if semicolon is present
            if (query.Length>0 && query[query.Length-1]==';')
                query = query.Substring(0, query.Length - 1);
            else return default;
            
            //splitting query into tokens
            string[] tokens = query.ToLower().Split(',');
            int k = 10;
            var queries = new List<QueryType>();
            List<Predicate> predicates = new List<Predicate>();
            
            //parsing tokens into predicates
            foreach (string token in tokens)
            {
                string[] expression = token.Split('=');
                if (expression.Length != 2)
                    return default;
                
                Predicate predicate = MkPredicate(expression[0].Trim(), expression[1].Trim());

                //invalid attribute
                if (predicate.Query == QueryType.Invalid)
                    return default;
                
                //duplicate attribute
                if (queries.Contains(predicate.Query))
                    return default;
                queries.Add(predicate.Query);

                if (predicate.Query == QueryType.Kvalue)
                    k = int.Parse(predicate.Value);
                else
                    predicates.Add(predicate);
            }

            return (true,predicates,k);
        }
        public static Predicate invalidPredicate = new Predicate(QueryType.Invalid, "");
        public static Predicate MkPredicate(string s1, string s2)
        {
            QueryType type = Predicate.String2Type(s1);
            //integer
            if (type <= QueryType.Origin && int.TryParse(s2, out int i))
                    return new Predicate(type, s2);
            //real
            if (type <= QueryType.Acceleration && type > QueryType.Origin && double.TryParse(s2, out double d))
                return new Predicate(type, s2);
            //text
            if (type <= QueryType.Type && type > QueryType.Acceleration)
            {
                if (s2.StartsWith("\"") && s2.EndsWith("\""))
                    return new Predicate(type, s2.Substring(1, s2.Length - 2));
            }
            
            //invalid type
            return invalidPredicate;
        }
    }
}