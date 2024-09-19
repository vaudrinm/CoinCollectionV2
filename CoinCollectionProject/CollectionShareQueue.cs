using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoinCollection
{
    public class CollectionShareQueue 
    {
        private PriorityQueue<CollectionShare, double> collectionSharePriorityQueue;
        private Comparer<double> ascendingSortComparer = Comparer<double>.Create((double x, double y) => x > y ? 1 : -1);
        private ValueType valueType;

        public CollectionShareQueue(ValueType valueType)
        {
            this.valueType = valueType;
            this.collectionSharePriorityQueue = new PriorityQueue<CollectionShare, double>(ascendingSortComparer); 
        }

        public void EnqueueShare(CollectionShare collectionShare)
        {
            this.collectionSharePriorityQueue.Enqueue(collectionShare,
                valueType == ValueType.Retail
                ? collectionShare.RetailShareValue
                : collectionShare.WholesaleShareValue);
        }

        public void EnqueueResultShare(CollectionShare collectionShare)
        {
            this.collectionSharePriorityQueue.Enqueue(collectionShare,collectionShare.Id);
        }

        public CollectionShare DequeueShare()
        {
            return this.collectionSharePriorityQueue.Dequeue();
        }

        public int Count()
        {
            return this.collectionSharePriorityQueue.Count;
        }
    }
}
