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
        public int RawQueryFrequency;
        public List<int> SimilaritySet;
        public TableTuple()
        {
            this.TermFrequency = 1;
            this.RawQueryFrequency = 0;
            SimilaritySet = new List<int>();
        }

        public void IncreaseTF()
        {
            this.TermFrequency++;
        }

        public void IncreaseRQF(int QF)
        {
            this.RawQueryFrequency += QF;
        }

        public void AddToSet(int query)
        {
            this.SimilaritySet.Add(query);
        }

        public override string ToString()
        {
            string TFQF = String.Format("TF: {0}, QF: {1}, Set: (", TermFrequency, RawQueryFrequency);
            foreach(int i in SimilaritySet)
            {
                TFQF += i.ToString() + ", ";
            }
            TFQF += ")";
            return TFQF;
        }

    }
}
