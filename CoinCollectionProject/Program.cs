using CsvHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace CoinCollection
{
    internal class Program
    {
        // args: ValueType PrePop(bool) ShareCount(int) FileName
        static void Main(string[] args)
        {
            if (!Enum.TryParse(args[0], out ValueType selectedValueType)) { throw new ArgumentException("Unable to determine ValueType"); }
            if (!Enum.TryParse(args[1], out DistributionMode distributionMode)) { throw new ArgumentException("Unable to determine DistributionMode"); }
            if (!bool.TryParse(args[2], out bool shouldPrePopGroups)) { throw new ArgumentException("Unable to determine shouldPrePopGroups"); }
            if (!bool.TryParse(args[3], out bool oneEach)) { throw new ArgumentException("Unable to determine oneEach"); }
            if (!int.TryParse(args[4], out int shareCount)) { throw new ArgumentException("Unable to determine shareCount"); }
            string fileName = args[5];

            int numberOfShares = shareCount;
            int argsCount = args.Length;

            Dictionary<int, CollectionShare> idToCollectionShareDict = new Dictionary<int, CollectionShare>();
            for (int shareNumber = 1; shareNumber <= numberOfShares; shareNumber++)
            {
                idToCollectionShareDict[shareNumber] = new CollectionShare(shareNumber, selectedValueType);
            }

            List<CollectionItem> rawCollection = CollectionReader.RetrieveRawCollection($"{Constants.FilePath}{fileName}");

            double initialRawRetailValue = rawCollection.Where(item => !item.IsSummary).Select(item => item.RetailValue).Sum();
            double initialRawWholesaleValue = rawCollection.Where(item => !item.IsSummary).Select(item => item.WholesaleValue).Sum();

            // give 1 of 7+ count items to each share before distribution
            if (oneEach)
            {
                List<CollectionItem> takeOneCollection = rawCollection.Where(item => item.Count >= numberOfShares).ToList();
                foreach (CollectionItem item in takeOneCollection)
                {
                    foreach (KeyValuePair<int, CollectionShare> pair in idToCollectionShareDict)
                    {
                        CollectionShare share = pair.Value;

                        share.AddItem(item.CloneSingle());
                    }

                    // reduce count by number of shares
                    item.Count -= shareCount;
                }
            }

            List<CollectionItem> collection = CollectionReader.ComposeDiscreteItemList(rawCollection, distributionMode);

            CollectionShareQueue collectionShareQueue = new CollectionShareQueue(selectedValueType);

            //// PrePop groups and populate priority queue
            if (shouldPrePopGroups)
            {
                foreach (KeyValuePair<int, CollectionShare> pair in idToCollectionShareDict)
                {
                    CollectionShare share = pair.Value;

                    // Add items preselected for a particular share to the share
                    List<CollectionItem> preselectedItemsList = collection.Where(item => item.GroupPreSort == share.Id).ToList();
                    share.AddItems(preselectedItemsList);

                    // remove items pre-selected for a particular share from the pool
                    // of available collection items
                    collection = collection.Where(item => item.GroupPreSort != share.Id).ToList();

                    // add this share to the priority queue
                    collectionShareQueue.EnqueueShare(share);
                }
            }

            //// Sort remaining items
            // this sorts highest to lowest in order to distribute higher value items first
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
            double retailShareValueStandardDeviation = retailShareValues.StandardDeviation();

            List<double> wholesaleShareValues = idToCollectionShareDict.Values.Select(collectionShare => collectionShare.ComputeShareValue(ValueType.Wholesale)).ToList();
            double wholesaleShareValueStandardDeviation = wholesaleShareValues.StandardDeviation();

            // .Substring(0, inputText.LastIndexOf("bin/Debug/net6.0"));
            string dir = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.LastIndexOf("bin/Debug/net6.0"));//.Trim("bin/Debug/net6.0");

            // move 
            CollectionShareQueue collectionShareResultsQueue = new CollectionShareQueue(selectedValueType);
            while (collectionShareQueue.Count() > 0)
            {
                collectionShareResultsQueue.EnqueueResultShare(collectionShareQueue.DequeueShare());
            }

            // Print to csv:
            // TODO: determine file names from params, datetime, etc
            string outputFileName = string.Join("-", args);
            using (var writer = new StreamWriter($"{dir}{outputFileName}-Results.csv"))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                double postDistributionRetailValue = 0.0;
                double postDistributionWholesaleValue = 0.0;

                //csv.WriteComment($"Retail Share Value Standard Deviation:, {retailShareValueStandardDeviation}");
                WriteCsvField(csv, "Cmd Args", string.Join(" ", args));
                WriteCsvField(csv, "Share Retail Value Standard Deviation", $"{retailShareValueStandardDeviation}");
                WriteCsvField(csv, "Share Wholesale Value Standard Deviation", $"{wholesaleShareValueStandardDeviation}");
                //csv.WriteRecords($"Wholesale Share Value Standard Deviation:, {wholesaleShareValueStandardDeviation}");

                while (collectionShareResultsQueue.Count() > 0)
                {
                    csv.NextRecord();

                    CollectionShare collectionShare = collectionShareResultsQueue.DequeueShare();
                    WriteCsvField(csv, "Share ID", collectionShare.Id.ToString());

                    double shareRetailValue = collectionShare.RetailShareValue;
                    double shareWholesaleValue = collectionShare.WholesaleShareValue;
                    postDistributionRetailValue += shareRetailValue;
                    postDistributionWholesaleValue += shareWholesaleValue;

                    WriteCsvField(csv, "Retail Value", $"${string.Format("{0:N2}", shareRetailValue)}");
                    WriteCsvField(csv, "Wholesale Value", $"${string.Format("{0:N2}", shareWholesaleValue)}");

                    csv.WriteRecords(collectionShare.CollectionItemDict);

                    csv.NextRecord();
                }

                // validate total share values
                if (Math.Round(initialRawRetailValue,2) != Math.Round(postDistributionRetailValue,2)) { throw new Exception("Result retail value does not equal initial raw retail value"); }
                if (Math.Round(initialRawWholesaleValue,2) != Math.Round(postDistributionWholesaleValue,2)) { throw new Exception("Result wholesale value does not equal initial raw wholesale value"); }
            }

            Console.WriteLine("all done!");
        }

        private static void WriteCsvField(CsvWriter csv, string key, string value)
        {
            csv.WriteField($"{key}:   {value}");
            csv.NextRecord();
        }
    }
}