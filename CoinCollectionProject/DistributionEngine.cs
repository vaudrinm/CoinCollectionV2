namespace CoinCollection
{
	public class DistributionEngine
	{
		private Queue<CollectionItem> collectionItemQueue;
		private CollectionShareQueue collectionShareQueue;
        private DistributionMode distributionMode;

		public DistributionEngine(
			Queue<CollectionItem> collectionItemQueue,
			CollectionShareQueue collectionShareQueue,
			DistributionMode distributionMode)
		{
			if (collectionItemQueue == null) { throw new ArgumentNullException(nameof(collectionItemQueue)); }
			this.collectionItemQueue = collectionItemQueue;

            if (collectionShareQueue == null) { throw new ArgumentNullException(nameof(collectionShareQueue)); }
            this.collectionShareQueue = collectionShareQueue;

            this.distributionMode = distributionMode;
		}

		public void DistributeItems()
		{
            while (this.collectionItemQueue.TryDequeue(out CollectionItem collectionItem))
            {
                CollectionShare share = this.collectionShareQueue.DequeueShare();

                    share.AddItem(collectionItem.Clone());
                    collectionShareQueue.EnqueueShare(share);

                    // do not enqueue item as we pre-divide groups of items
            }
        }
	}
}

