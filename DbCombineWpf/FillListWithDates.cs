using System;
using System.Collections.Generic;
using System.Linq;

namespace DbCombineWpf
{
    class FillListWithDates
    {

        public SortedList<int, ITable> InstitList { get; set; }

            public FillListWithDates(DbSelTable dbClass, DbTabellen tableName){


            var institWithDates = dbClass.getTableDateList(tableName);
            var totalInstit = dbClass.getTableList(tableName);
            var clusterItemList = new List<ClusterItem>();
            
            foreach (var k in institWithDates.Keys)
            {
                foreach (var v in institWithDates[k])
                {
                    totalInstit[v.id].änderungsDatum = k;
                    //suche nach id in totalinstit, update

                }

                int clusterCounter = 0;
                var tempClusterList = new MultiValueDictionary<DateTime, int>();
                for (int i = 0; i < totalInstit.Count; i++)
                {

                    var actualDate = totalInstit.ElementAt(i).Value.änderungsDatum;
                    if (totalInstit.ElementAt(i).Value.änderungsDatum == k)
                    {
                        clusterCounter += 1;
                        tempClusterList.Add(k, totalInstit.ElementAt(i).Key);

                    }
                    else
                    {
                        if (clusterCounter > 6)
                        {
                           var tempCluster = new List<int>();
                            DateTime clusterDate = tempClusterList.ElementAt(0).Key;



                            foreach (var value in tempClusterList[clusterDate])
                            {
                                tempCluster.Add(value);

                            }

                            
                            AddIfNotExist(clusterItemList, new ClusterItem(tempCluster, clusterDate));
                        }
                        tempClusterList.Clear();
                        clusterCounter = 0;
                    }
                }
            }
            //duplicate? inrange funktion

            var dates = new LinkedList<DateTime>();
            foreach (var s in clusterItemList)
            {
                dates.AddFirst(s.Datum);

            }
            var result = clusterItemList.OrderBy(p => p.Cluster.First()).ThenBy(p => p.Datum).ToList();


            var asd = result.Count();
            var maxTableDates = new List<ITable>();

            for (int i = 0; i < institWithDates.Keys.Count(); i++)
            {
                var institDate = institWithDates.ElementAt(i).Key;
                var institValue = institWithDates[institDate];

                foreach (var instit in institValue)
                {
                    instit.änderungsDatum = institDate;
                    //check maxinstit, ob id existiert, wenn ja,
                    //dann datum ändern wenn größer
                    var tryFind = maxTableDates.Find(x => x.id == instit.id);
                    if (tryFind == null)
                    { //instit nicht gefunden, add
                        maxTableDates.Add(instit);
                    }
                    else
                    {   //instit gefunden, datums check, wenn neuer ändern
                        if (tryFind.änderungsDatum < instit.änderungsDatum)
                        {
                            tryFind.änderungsDatum = instit.änderungsDatum;
                        }

                    }

                    
                }



            }

            maxTableDates.Count();

            var clusterByDates = new List<ClusterItem>(clusterItemList);
            clusterByDates.Sort(new ClusterDateSort());
            clusterByDates.Reverse();
            clusterItemList.Sort(new ClusterSort());
            clusterItemList.Reverse();
            clusterItemList.Count();


            clusterItemList.Count();
            clusterItemList.Reverse();

            #region institListe komplett mit datum eintragen

            foreach (var instit in totalInstit.Keys)
            {
                var tryFind = maxTableDates.Find(x => x.id == instit);
                if (tryFind != null)
                {
                    //instit nicht gefunden, add
                    totalInstit[instit].änderungsDatum = tryFind.änderungsDatum;
                }
                else
                {
                    // aus clusterItemList
                    //clusterItemList
                    //TODO

                    int instit1 = instit;
                    var lastDate =
                        clusterItemList.LastOrDefault(x => x.Cluster.Min() <= instit1);
                    if (lastDate != null) { 
                        //nicht vorm ersten cluster
                    totalInstit[instit].änderungsDatum = lastDate.Datum;
                    }
 
                    else
                    {
                        totalInstit[instit].änderungsDatum = clusterItemList.First().Datum;
                    }
                }
            }
            #endregion
            InstitList = totalInstit;
            }

            #region private methods
            private static bool RemoveOutdatedClusters(List<ClusterItem> clusterItemList, List<ClusterItem> clusterByDates)
            { //return 0 wenn durchlaufen, ansonsten i - 1
                //in args i
                var datesCopy = new List<ClusterItem>(clusterByDates);
                for (var i = datesCopy.Count() - 1; i >= 0; i--)
                {
                    var dItem = datesCopy.ElementAt(i);
                    var cItem = clusterItemList.ElementAt(i);

                    //njet. cluster suchen mit dem datum, danach vergleichen
                    if (cItem.Cluster.First() == dItem.Cluster.First())
                    {
                    //datum durchgehen. wenn position des datums nicht der position des items, del
                    }
                    else
                    {
                        clusterItemList.RemoveAll(x => x.Cluster.First() == dItem.Cluster.First());

                        clusterByDates.Remove(clusterByDates.ElementAt(i));
                        return true;
                    }
                    if (i == 0)
                        return false;

                }
                return true;
            }

            private void AddIfNotExist(List<ClusterItem> cList, ClusterItem clusterToInsert)
            {
                var toRemoveIndices = new List<int>();
                #region if empty, insert

                if (cList.Count == 0)
                {
                    cList.Add(clusterToInsert);
                    return;
                }

                #endregion

                var cListCopy = new List<ClusterItem>(cList);


                for (int i = 0; i < cListCopy.Count; i++)
                {
                    ClusterItem clusterItem = cList.ElementAt(i);
                    var intersect = clusterItem.Cluster.Intersect(clusterToInsert.Cluster);
                    if (intersect.Any())
                    {

                        if (cList[i].Datum > clusterToInsert.Datum)
                        {

                            toRemoveIndices.Add(i);
                        }
                        else
                        {
                            return;
                        }

                    }


                }
                foreach (int delKey in toRemoveIndices)
                {
                    cList.RemoveAt(delKey);
                }
                cList.Add(clusterToInsert);
            }

            class ClusterSort : IComparer<ClusterItem>
            {
                public int Compare(ClusterItem x1, ClusterItem x2)
                {

                    int x1Int = x1.Cluster.First();
                    int x2Int = x2.Cluster.First();

                    if (x1Int > x2Int)
                        return 1;
                    if (x1Int < x2Int)
                        return -1;
                    if (x1Int == x2Int)
                    {
                        if (x1.Datum > x2.Datum)
                        {
                            return 1;

                        }
                        else if (x1.Datum < x2.Datum)
                        {
                            return -1;
                        }
                    }
                    return 0;
                }

            }

            class ClusterDateSort : IComparer<ClusterItem>
            {
                public int Compare(ClusterItem x1, ClusterItem x2)
                {

                    var x1Int = x1.Datum;
                    var x2Int = x2.Datum;

                    if (x1Int > x2Int)
                        return 1;
                    if (x1Int < x2Int)
                        return -1;
                    if (x1Int == x2Int)
                    {
                        if (x1.Cluster.First() > x2.Cluster.First())
                        {
                            return 1;

                        }
                        else if (x1.Cluster.First() < x2.Cluster.First())
                        {
                            return -1;
                        }
                    }
                    return 0;
                }

            }
            #endregion


    }
}
