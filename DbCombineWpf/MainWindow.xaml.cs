using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;

namespace DbCombineWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string bDB = @"....mdb";
        const string qDB = @"....mdb";
        
        readonly string bzSource;
        readonly string qSource;

        const string bzHome = @"C:\....mdb";
        const string qHome = @"C:\....mdb";
        
        private DataTable bzInstitTbl;
        private DataTable qInstitTbl;
        private DataTable duplicates;



        //geänderte instit-liste
        MultiValueDictionary<DateTime, Institution> bzInstitDate = new MultiValueDictionary<DateTime, Institution>();



        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(bDB) && File.Exists(qDB))
            {
                bzSource = bDB;
                qSource = qDB;
            }
            else if (File.Exists(bzHome) && File.Exists(qHome))
            {
                bzSource = bzHome;
                qSource = qHome;
            }
            else
            {
                throw new DirectoryNotFoundException();
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            var bzInstit = new DbSelTable(bzSource, DbTabellen.Institution);
            var qInstit = new DbSelTable(qSource, DbTabellen.Institution);
            var iLog = new List<string>();
            var pLog = new List<string>();
            var tLog = new List<string>();
            var vLog = new List<string>();

            #region alter table datetime


            string addId = "ALTER TABLE Institutionstyp ADD PRIMARY KEY (ID);";
            string qInstitType = "ALTER TABLE Institutionstyp ADD COLUMN Geändert DATETIME;";

            string bzInstitType = "ALTER TABLE Institutionstyp ADD COLUMN Dummy1 TEXT(1), Dummy2 TEXT(1),Dummy3 TEXT(1),Dummy4 TEXT(1),Dummy5 TEXT(1),Dummy6 TEXT(1);";

            string addInstitTypes = "ALTER TABLE Institutionstyp ADD COLUMN Universität TEXT(50), Hochschule TEXT(50),Firma TEXT(50),Sonstige TEXT(50)";


            #endregion


            #region instit

            var bzAddDates = new FillListWithDates(bzInstit, DbTabellen.Institution);
            var qAddDates = new FillListWithDates(qInstit, DbTabellen.Institution);



            Dictionary<int, Institution> bzInstitDateList =
                bzAddDates.InstitList.ToDictionary(item => item.Key, item => (Institution)item.Value);


            Dictionary<int, Institution> qInstitDateList =
                qAddDates.InstitList.ToDictionary(item => item.Key, item => (Institution)item.Value);

            foreach (var q in qInstitDateList.Keys)
            {
                qInstitDateList[q].id = q;
            }

            foreach (var bz in bzInstitDateList.Keys)
            {
                bzInstitDateList[bz].id = bz;
            }

            var InstitTuple = TablesCompare.CompareInstit(bzInstitDateList, qInstitDateList);
            int bzInstitMaxId = InstitTuple.Item1;
            Dictionary<int, int> qOldNewIdsInstit = InstitTuple.Item2;
            //old//new
            Dictionary<int, int> bzInsert = InstitTuple.Item3;
            Dictionary<int, int> bzUpdate = InstitTuple.Item4;

            foreach (var institToUpdate in bzUpdate)
            {
                int oldID = institToUpdate.Key;
                int newID = institToUpdate.Value;
                Institution institToReplaceInBz = qInstitDateList[newID];
                institToReplaceInBz.id = oldID;
                var oldInstit = bzInstitDateList[oldID];
                bzInstit.updateInstit(institToReplaceInBz);
                iLog.Add("Update: alt Instit: " + oldInstit.name + ", ID: " + oldID + ", neu Instit: " + institToReplaceInBz.name + ", ID: " + newID);

            }

            foreach (var institToInsert in bzInsert)
            {
                int oldID = institToInsert.Key;
                int newID = institToInsert.Value;
                Institution institToInsertInBz = qInstitDateList[oldID];
                institToInsertInBz.id = oldID;
                bzInstit.insertInstit(institToInsertInBz);
                iLog.Add("Insert: " + institToInsertInBz.name + ", ID: " + institToInsertInBz.id);
            }
            WriteLog.Write(iLog, "institutionen");


      
            bzInstitDateList.Count();
  
            #endregion

            #region person

            bzInstit.tableName = DbTabellen.Person;
            //var bzPersons = bzInstit.getTableList();
            qInstit.tableName = DbTabellen.Person;
            //var qPersons = qInstit.getTableList();


            var bzPersonAddDates = new FillListWithDates(bzInstit, DbTabellen.Person);
            var qPersonAddDates = new FillListWithDates(qInstit, DbTabellen.Person);



            Dictionary<int, Person> bzPersonDateList =
                bzPersonAddDates.InstitList.ToDictionary(item => item.Key, item => (Person)item.Value);


            Dictionary<int, Person> qPersonDateList =
                qPersonAddDates.InstitList.ToDictionary(item => item.Key, item => (Person)item.Value);

            foreach (var q in qPersonDateList.Keys)
            {
                qPersonDateList[q].id = q;
            }

            foreach (var bz in bzPersonDateList.Keys)
            {
                bzPersonDateList[bz].id = bz;
            }
            //int bzvorher = bzInstitDateList.Count();
            //int lastmaxId = bzInstitDateList.Keys.Max();

            //ids der instit in personen ändern
            foreach (int i in qOldNewIdsInstit.Keys)
            {

                var oldInstitIds = qPersonDateList.Values.Where(x => x.institution == i).ToList();
                foreach (var item in oldInstitIds)
                {
                    if (item.editiert == false)
                    {
                        item.institution = qOldNewIdsInstit[i];
                        item.editiert = true;
                    }
                }
            }
            var PersonTuple = TablesCompare.ComparePerson(bzPersonDateList, qPersonDateList);
            int bzPersonMaxId = PersonTuple.Item1;
            Dictionary<int, int> qOldNewIdsPerson = PersonTuple.Item2;

            foreach (int i in qOldNewIdsPerson.Keys)
            {
                if (qOldNewIdsPerson[i] > bzPersonMaxId)
                {
                    //insert in bz
                    bzInstit.insertPerson(qPersonDateList[i]);
                    pLog.Add("Insert: " + qPersonDateList[i].name + ", alte ID: " + qPersonDateList[i].id);
                    //bzInstit.insertPerson(bzPersonDateList[qOldNewIdsPerson[i]]);
                    
                }
                else
                {

                    pLog.Add("Update: alt Person: " + qPersonDateList[i].name + ", alte ID: " + qPersonDateList[i].id);
                    bzInstit.updatePerson(bzPersonDateList[qOldNewIdsPerson[i]]);
                    //update bz
                    pLog.Add("Update: alt Instit: " + bzPersonDateList[qOldNewIdsPerson[i]].name + ", ID: " + bzPersonDateList[qOldNewIdsPerson[i]].id + ", neu Instit: " + qPersonDateList[i].name + ", ID: " + qPersonDateList[i].id);
                }
                WriteLog.Write(pLog, "personen");
            }


       

  

            bzPersonDateList.Count();
            //foreach int i in qchangedInstit.keys
            // if qchangedInstit[i] > bzMAX
            //then insert in bz
            //else update

            //danach
            //foreach int i in qchangedInstit.keys
            //update in qPersons-recordSet, nicht in qdb!!!!
            //update persons set person.institution = qchangedInstit[i] where person.institution = i

            //danach in veranstaltungen, personen in q updaten, danach insert
            #endregion

            #region veranstaltung

            var qOldNewVeranstaltung = new Dictionary<int, int>();
            var bzVeranstaltung = bzInstit.getTableList(DbTabellen.Veranstaltung);
            var qVeranstaltung = qInstit.getTableList(DbTabellen.Veranstaltung);
            int bzVeranstaltungMaxId = bzVeranstaltung.Max(x => x.Key);

            foreach (var veranstaltung in qVeranstaltung.Keys)
            {
                qVeranstaltung[veranstaltung].id = veranstaltung;
            }
            int bzVeranstaltungCounter = bzVeranstaltungMaxId;

            foreach (var veranstaltung in qVeranstaltung)
            {
                bzVeranstaltungCounter += 1;
                qOldNewVeranstaltung.Add(veranstaltung.Value.id, bzVeranstaltungCounter);
                veranstaltung.Value.id = bzVeranstaltungCounter;
                bzInstit.insertVeranstaltung(qVeranstaltung[veranstaltung.Key] as Veranstaltung);
                //insert in db? zuerst personen updaten
            }

            #endregion



            #region teilnahme

            var bzTeilnahme = bzInstit.getTableList(DbTabellen.Teilnahme)
                .ToDictionary(item => item.Key, item => (Teilnahme) item.Value);
            int bzTeilnahmeMaxId = bzTeilnahme.Max(x => x.Key);
            var qTeilnahme = qInstit.getTableList(DbTabellen.Teilnahme)
                .ToDictionary(item => item.Key, item => (Teilnahme) item.Value);

            

            foreach (int i in qOldNewIdsInstit.Keys)
            {

                var oldInstitIds = qPersonDateList.Values.Where(x => x.institution == i).ToList();
                foreach (var item in oldInstitIds)
                {
                    if (item.editiert == false)
                    {
                        item.institution = qOldNewIdsInstit[i];
                        item.editiert = true;
                    }
                }
            }

            //zwei int, int dicts für veranstaltung, personen? old/new

            foreach (var tn in qTeilnahme.Values)
            {
                if (tn.editiert == false)
                {    ////qOldNewVeranstaltung
                    //value new des qoldperson-array mit key von altem index
                    //update person, dann update id
                    tn.veranstaltung = qOldNewVeranstaltung[tn.veranstaltung];
                    tn.person = qOldNewIdsPerson[tn.person]; //update person

                    tn.editiert = true;
                }
                bzInstit.insertTeilnahme(tn);


            }

            #endregion


        }

   

        private static bool RemoveOutdatedClusters(List<ClusterItem> clusterItemList, List<ClusterItem> clusterByDates)
        { //return 0 wenn durchlaufen, ansonsten i - 1
            //in args i
            //rekursiv?
            var datesCopy = new List<ClusterItem>(clusterByDates);
            for (var i = datesCopy.Count() - 1; i >= 0; i--)
            {
                var dItem = datesCopy.ElementAt(i);
                var cItem = clusterItemList.ElementAt(i);
                
                if (cItem.Cluster.First() == dItem.Cluster.First())
                {

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


        public void AddIfNotExist(List<ClusterItem> cList, ClusterItem clusterToInsert)
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
                        //wird nicht eingefügt, weil datum älter?
                        //muss man schleife zu ende laufen lassen?
                        return;
                        //continue;
                    }

                }

                //}

            }
            foreach (int delKey in toRemoveIndices)
            {
                cList.RemoveAt(delKey);
            }
            cList.Add(clusterToInsert);


        }

        public void OrderByDate(List<ClusterItem> cList)
        {
            //todo erstelle kopie
            for (var i = 0; i < cList.Count; i++)
            {

                var clusterItem = cList.ElementAt(i);

                //wenn datum größer(neuer, schlecht) ist als das datum ein element drüber
                if (clusterItem.Datum > cList.ElementAt(i + 1).Datum)
                {
                    cList.Remove(cList.ElementAt(i));
                }

            }

        }
    }







}
