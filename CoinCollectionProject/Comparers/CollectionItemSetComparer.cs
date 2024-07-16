using System;
using System.Collections.Generic;

namespace CoinCollection
{
    public class CollectionItemSetComparer : Comparer<List<CollectionItem>>
    {
        public override int Compare(List<CollectionItem>? set1, List<CollectionItem>? set2)
        {
            // Sets are equal if they're both null
            if (set1 == null && set2 == null) { return 0; }

            // Sets are unequal if just one is null
            if (set1 == null)
            {
                return -1;
            }

            if (set2 == null)
            {
                return 1;
            }

            return set1.CollectionItemSetValue().CompareTo(set2.CollectionItemSetValue());
        }

        // TODO: sort by id before producing hashcode
        public int GetHashCode(HashSet<CollectionItem> set)
        {
            string code = string.Empty;

            foreach (CollectionItem item in set)
            {
                code += item.Id + "," + item.Name + ";";
            }
            return code.GetHashCode();
        }
    }

    class TitleComparer : IComparer<string>
    {
        public int Compare(string titleA, string titleB)
        {
            var titleAIsFancy = titleA.Equals("sir", StringComparison.InvariantCultureIgnoreCase);
            var titleBIsFancy = titleB.Equals("sir", StringComparison.InvariantCultureIgnoreCase);


            if (titleAIsFancy == titleBIsFancy) //If both are fancy (Or both are not fancy, return 0 as they are equal)
            {
                return 0;
            }
            else if (titleAIsFancy) //Otherwise if A is fancy (And therefore B is not), then return -1
            {
                return -1;
            }
            else //Otherwise it must be that B is fancy (And A is not), so return 1
            {
                return 1;
            }
        }
    }
}

