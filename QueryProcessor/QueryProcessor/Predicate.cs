namespace QueryProcessor
{
public struct Predicate
    {
        public Predicate(QueryType query, string value)
        {
            this.Query = query;
            this.Value = value;
        }
        public QueryType Query;
        public string Value;
        public override string ToString()
        {
            return $"{Type2String(Query)} = {Value}";
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
        public static string Type2String(QueryType type)
        {
            switch (type)
            {
                case QueryType.Cylinders:
                    return "cylinders";
                case QueryType.Modelyear:
                    return "model_year";
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
        public static bool IsNumerical(QueryType query)
        {
            return QueryType.Origin < query && query < QueryType.Brand;
        }
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