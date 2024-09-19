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
        public double RetailShareValue { get; private set; }
        public double WholesaleShareValue { get; private set; }
        public ValueType ValueType { get; private set; }
        public Dictionary<string, CollectionItem> CollectionItemDict { get; private set; }

        public CollectionShare(int id, ValueType valueType)
        {
            this.Id = id;
            this.ValueType = valueType;
            this.RetailShareValue = 0.0;
            this.WholesaleShareValue = 0.0;
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
            this.RetailShareValue += collectionItem.RetailValue;
            this.WholesaleShareValue += collectionItem.WholesaleValue;

            if (this.CollectionItemDict.ContainsKey(collectionItem.Id))
            {
                CollectionItemDict[collectionItem.Id].Count += collectionItem.Count;
            }
            else
            {
                CollectionItemDict[collectionItem.Id] = collectionItem;
            }
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
