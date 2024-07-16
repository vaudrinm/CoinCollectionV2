using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading.Task;

namespace CoinCollection
{
    public class CollectionShareQueue 
    {
        private PriorityQueue<CollectionShare, double> collectionSharePriorityQueue;
        private Comparer<double> ascendingSortComparer = Comparer<double>.Create((double x, double y) => x > y ? 1 : -1);

        public CollectionShareQueue()
        {
            this.collectionSharePriorityQueue = new PriorityQueue<CollectionShare, double>(ascendingSortComparer); 
        }

        public void EnqueueShare(CollectionShare collectionShare)
        {
            this.collectionSharePriorityQueue.Enqueue(collectionShare, collectionShare.ShareValue);
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
