using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace CoinCollection
{
    internal class Program
    {
        // 7 bins - bin id to collection hashset
        Dictionary<int, List<CollectionItem>> intIdToCollectionSetDictionary = new Dictionary<int, List<CollectionItem>>();

        // Queue for 7 bins/sets to draw lowest value bin
        //PriorityQueue<HashSet<CollectionItem>, HashSet<CollectionItem>> collectionSetPriorityQueue =
        //    new PriorityQueue<HashSet<CollectionItem>, HashSet<CollectionItem>>(new CollectionItemSetComparer());
        //PriorityQueue<HashSet<CollectionItem>, int> collectionSetPriorityQueue = new PriorityQueue<HashSet<CollectionItem>, int>();

        //Comparer<int>.Create((x, y) => y - x)

        //TPriority Priority
        static void Main(string[] args)
        {
            // TODO: pass in as argument
            ValueType selectedValueType = ValueType.Retail;
            int numberOfShares = 3; // TODO: 3 for testing, change to 7 for real data

            //Assembly? assembly = Assembly.GetExecutingAssembly();
            //string[]? ares = assembly.GetManifestResourceNames();

            //PriorityQueue<List<CollectionItem>, double> collectionSetPriorityQueue =
            //    new PriorityQueue<List<CollectionItem>, double>(Comparer<double>.Create((double x, double y) => x > y ? 1 : -1)); // sort ascending

            CollectionShareQueue collectionShareQueue = new CollectionShareQueue();

            List<CollectionItem> rawCollection = CollectionReader.RetrieveRawCollection(Constants.TestFilePath);
            List<CollectionItem> collection = CollectionReader.ComposeDiscreetItemList(rawCollection);

            //CollectionShare colShare = new CollectionShare(1);
            //int id = colShare.Id;
            Dictionary<int, CollectionShare> idToCollectionShareDict = new Dictionary<int, CollectionShare>();
            for (int shareNumber = 1; shareNumber <= numberOfShares; shareNumber++)
            {
                idToCollectionShareDict[shareNumber] = new CollectionShare(shareNumber, selectedValueType);
            }

            //// PrePop groups
            //foreach (CollectionItem collectionItem in collection)
            //{
            //    int preSortGroup = collectionItem.GroupPreSort;
            //    if (preSortGroup > 0)
            //    {
            //        idToCollectionShareDict[preSortGroup].AddItem(collectionItem);
            //    }
            //}

            //// PrePop groups and populate priority queue
            foreach (KeyValuePair<int, CollectionShare> pair in idToCollectionShareDict)
            {
                CollectionShare share = pair.Value;

                // Add items preselected for a particular share to the share
                var preselectedItems = collection.Where(item => item.GroupPreSort == share.Id);
                share.AddItems(preselectedItems.ToList());

                // remove these items from the pool of available collection items
                collection = collection.Except(preselectedItems).ToList();

                // add this share to the priority queue
                collectionShareQueue.EnqueueShare(share);
            }


            //// Sort remaining items
            // this sorts highest to lowest - flesh this out to control how we add (but this is prob the best way to do it..)
            Comparer<CollectionItem> collectionItemHighToLowSortComparer = Comparer<CollectionItem>.Create((CollectionItem x, CollectionItem y) => x > y ? -1 : 1);
            collection.Sort(collectionItemHighToLowSortComparer);
            
            List<CollectionItem> testCollection1 = new List<CollectionItem>()
            {
                new CollectionItem()
                {
                    Id = "testId1",
                    Name = "testName1",
                    UnitRetailValue = 10,
                    UnitWholesaleValue = 1,
                    Count = 1
                }
            };

            List<CollectionItem> testCollection2 = new List<CollectionItem>()
            {
                new CollectionItem()
                {
                    Id = "testId2",
                    Name = "testName2",
                    UnitRetailValue = 20,
                    UnitWholesaleValue = 2,
                    Count = 1
                }
            };

            // TEST: check CollectionItem compareto
            // TODO: remove from final
            Console.WriteLine(rawCollection[0] > rawCollection[1]);

            // TEST: check HashSet comparer (collection > rawCollection)
            // TODO: remove from final
            //collectionSetPriorityQueue.EnqueueCollectionItemSet(testCollection1);

            // TODO: push this into a DistributionEngine object
            // Distribute remaining items
            foreach (CollectionItem collectionItem in collection)
            {
                CollectionShare share = collectionShareQueue.DequeueShare();
                share.AddItem(collectionItem.Clone());
                collectionItem.Count = 0;
                collectionShareQueue.EnqueueShare(share);
            }

            // Sanity check 
            if (collectionShareQueue.Count() != numberOfShares || idToCollectionShareDict.Count != numberOfShares)
            {
                throw new Exception("Something has gone terribly wrong - you're missing/have too many shares!");
            }

            // Calculate
            // stockItems.Select(item => item.StockID).ToList();
            List<double> retailShareValues = idToCollectionShareDict.Values.Select(collectionShare => collectionShare.ComputeShareValue(ValueType.Retail)).ToList();
            double retailShareValueStandardDeviation = MathUtil.GetStandardDeviation(retailShareValues);

            List<double> wholesaleShareValues = idToCollectionShareDict.Values.Select(collectionShare => collectionShare.ComputeShareValue(ValueType.Retail)).ToList();
            double wholesaleShareValueStandardDeviation = MathUtil.GetStandardDeviation(wholesaleShareValues);

            // Print to csv:
            // TODO: determine file names from params, datetime, etc
            using (var writer = new StreamWriter("testResults.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords($"Retail Share Value Standard Deviation:, ${retailShareValueStandardDeviation}");
                csv.WriteRecords($"Wholesale Share Value Standard Deviation:, ${wholesaleShareValueStandardDeviation}");
                while (collectionShareQueue.Count() > 0)
                {
                    csv.WriteRecords(collectionShareQueue.DequeueShare().CollectionItemDict);
                }
            }

            Console.WriteLine("all done!");
        }
    }
}