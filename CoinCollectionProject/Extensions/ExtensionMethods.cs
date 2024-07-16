using System;
using System.Collections.Generic;

namespace CoinCollection
{
    public static class ExtensionMethods
    {
        public static double CollectionItemSetValue(this List<CollectionItem> col)
        {
            double setValue = 0;
            bool compareByRetailvalue = CollectionItem.ValueComparisonType == ValueType.Retail;
            foreach (CollectionItem item in col)
            {
                setValue += compareByRetailvalue ? item.RetailValue : item.WholesaleValue;
            }
            return setValue;
        }

        // new function on 
        public static void EnqueueCollectionItemSet(this PriorityQueue<List<CollectionItem>, double> queue, List<CollectionItem> collectionItemList)
        {
            queue.Enqueue(collectionItemList, collectionItemList.CollectionItemSetValue());
        }
    }
}
