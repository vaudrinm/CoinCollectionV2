using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinCollection
{
    public class CollectionShare
    {
        public int Id { get; private set; }
        public double ShareValue { get; private set; }
        public ValueType ValueType { get; private set; }
        public Dictionary<string, CollectionItem> CollectionItemDict { get; private set; }

        public CollectionShare(int id, ValueType valueType)
        {
            this.Id = id;
            this.ValueType = valueType;
            this.ShareValue = 0.0;
            this.CollectionItemDict = new Dictionary<string, CollectionItem>();
        }

        public void AddItems(List<CollectionItem> items)
        {
            foreach (CollectionItem item in items)
            {
                this.AddItem(item);
            }
        }

        public void AddItem(CollectionItem collectionItem)
        {
            if (this.ValueType == ValueType.Retail)
            {
                this.ShareValue += collectionItem.RetailValue;
            }
            else
            {
                this.ShareValue += collectionItem.WholesaleValue;
            }

            if (this.CollectionItemDict.ContainsKey(collectionItem.Id))
            {
                CollectionItemDict[collectionItem.Id].Count += collectionItem.Count;
            }
            else
            {
                CollectionItemDict[collectionItem.Id] = collectionItem;
            }

            // TODO: add a temporary check here for some testing to confirm this is being added correctly under different conditions
            //       (in lieu of actual bug testing lol)
        }

        public double ComputeShareValue(ValueType valueType)
        {
            double shareValue = 0.0;
            foreach (CollectionItem item in this.CollectionItemDict.Values)
            {
                if (valueType == ValueType.Retail)
                {
                    shareValue += item.RetailValue;
                }
                else if (valueType == ValueType.Wholesale)
                {
                    shareValue += item.WholesaleValue;
                }
            }
            return shareValue;
        }
    }
}
