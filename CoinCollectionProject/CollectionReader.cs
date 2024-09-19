using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CoinCollection
{
    public static class CollectionReader
    {
        public static List<CollectionItem> RetrieveRawCollection(string collectionFilePath)
        {
            Assembly? assembly = Assembly.GetExecutingAssembly();
            List<CollectionItem> collectionData = new List<CollectionItem>();

            using (Stream manifestStream = assembly.GetManifestResourceStream(collectionFilePath))
            using (StreamReader collectionItemsStream = new StreamReader(manifestStream))
            {
                string? line;
                string[] row = new string[11];
                while ((line = collectionItemsStream.ReadLine()) != null)
                {
                    if (!(line.StartsWith(",") || line.StartsWith("ID")))
                    {
                        // Id,Name,RetailValue,WholesaleValue,Year,Count,UnitWholesaleValue,RawWholesaleValue,Description,IsSummary,IsUnique,Container,Bag,GroupPreSort
                        row = line.Split(',');
                        CollectionItem rawItem = new CollectionItem
                        {
                            Id = row[0],
                            Name = row[1],
                            //RetailValue = double.Parse(row[2]),
                            //WholesaleValue = double.Parse(row[3]),
                            Year = row[4],
                            Count = int.Parse(row[5]),
                            UnitRetailValue = double.Parse(row[6]),
                            UnitWholesaleValue = double.Parse(row[7]),
                            Description = row[8],
                            IsSummary = bool.Parse(row[9]),
                            IsUnique = bool.Parse(row[10]),
                            GroupPreSort = int.TryParse(row[13], out int groupSort) ? groupSort : -1
                        };

                        if (!rawItem.IsSummary)
                        {
                            collectionData.Add(rawItem);
                        }
                    }
                }

                collectionItemsStream.Close();
                manifestStream.Close();
            }

            return collectionData;
        }

        // Likely needs retooling
        // Intended to split summary items into individual items (as opposed to
        // distributing based on equivalent subdivisions
        // TODO: improve to bucket more flexibly as desired
        public static List<CollectionItem> ComposeDiscreteItemList(List<CollectionItem> rawCollection, DistributionMode distributionMode)
        {
            List<CollectionItem> collectionData = new List<CollectionItem>();
            foreach (CollectionItem rawItem in rawCollection)
            {
                if (rawItem.IsSummary)
                {
                    throw new ArgumentException($"{nameof(rawCollection)} should not contain summary items");
                }

                // if the rawItem count is == 1 or if we're distrubuting by original items as captured
                // in the date file, simply add the item to the overall collection
                if (rawItem.Count == 1 || distributionMode == DistributionMode.WholeItems)
                {
                    collectionData.Add(rawItem);
                }
                else
                {
                    //// split this item up into individual pieces

                    // for the number of count
                    for (int intSubIndex = 1; intSubIndex <= rawItem.Count; intSubIndex++)
                    {
                        // compose the subIndex
                        double subIndex = intSubIndex;
                        do
                        {
                            subIndex /= 10;
                        } while (subIndex >= 1);

                        collectionData.Add(new CollectionItem
                        {
                            Id = rawItem.Id,
                            SubId = rawItem.Id + subIndex,
                            Name = rawItem.Name,
                            //RetailValue = rawItem.RetailValue / rawItem.Count,
                            //WholesaleValue = rawItem.WholesaleValue / rawItem.Count,
                            UnitRetailValue = rawItem.UnitRetailValue,
                            UnitWholesaleValue = rawItem.UnitWholesaleValue,
                            Year = rawItem.Year,
                            Count = 1,
                            Description = rawItem.Description,
                            IsSummary = rawItem.IsSummary,
                            IsUnique = rawItem.IsUnique,
                            GroupPreSort = rawItem.GroupPreSort
                        });
                    }
                }
            }

            return collectionData;
        }
    }
}
