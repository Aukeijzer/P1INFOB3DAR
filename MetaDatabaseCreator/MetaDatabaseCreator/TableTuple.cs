using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetaDatabaseCreator
{
    public class TableTuple
    {
        public int TermFrequency;
        public int QueryFrequency;
        public List<int> SimilaritySet;
        public TableTuple()
        {
            this.TermFrequency = 1;
            this.QueryFrequency = 0;
            SimilaritySet = new List<int>();
        }

        public void IncreaseTF()
        {
            this.TermFrequency++;
        }

        public void IncreaseQF(int QF)
        {
            this.QueryFrequency += QF;
        }

        public void AddToSet(int query)
        {
            this.SimilaritySet.Add(query);
        }

        public override string ToString()
        {
            string TFQF = String.Format("TF: {0}, QF: {1}, Set: (", TermFrequency, QueryFrequency);
            foreach(int i in SimilaritySet)
            {
                TFQF += i.ToString() + ", ";
            }
            TFQF += ")";
            return TFQF;
        }

    }
}
