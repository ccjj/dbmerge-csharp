using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DbCombineWpf
{


    /// <summary>
    /// Sets the Person-object, inheriting ITable. 
    /// </summary>

    public class Person : ITable
    {
        public int id { get; set; }
        public string vorname { get; set; }
        public string name { get; set; }
        public string titel { get; set; }
        public string geschlecht { get; set; }
        public int institution { get; set; }
        public string abteilung { get; set; }
        public string funktion { get; set; }
        public string strasse { get; set; }
        public string plz { get; set; }
        public string ort { get; set; }
        public int bundesland { get; set; }
        public string land { get; set; }
        public string mobil { get; set; }
        public string telefon { get; set; }
        public string fax { get; set; }
        public string email { get; set; }
        public string internet { get; set; }
        public string titelVname { get; set; }
        public string titelNname { get; set; }
        public DateTime änderungsDatum { get; set; }
        public bool editiert { get; set; }



        public Person(int id, string vorname, string name)
        {
            this.id = id;
            this.vorname = vorname;
            this.name = name;
        }


        public Person(int id, DateTime date, string vorname, string name)
        {
            this.id = id;
            this.änderungsDatum = date;
            this.vorname = vorname;
            this.name = name;

        }

        public Object this[string propertyName]
        {
            get
            {
                // probably faster without reflection:
                // like:  return Properties.Settings.Default.PropertyValues[propertyName] 
                // instead of the following
                Type myType = typeof(Person);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                //return myPropInfo.GetValue(this, null);
                return myPropInfo.GetValue(this, null);
            }
            private set
            {
                Type myType = typeof(Person);
                PropertyInfo myPropInfo = myType.GetProperty(propertyName);
                myPropInfo.SetValue(this, value, null);

            }
        }

    }


    /// <summary>
    /// Sets the Veranstaltung-object, inheriting ITable
    /// </summary>
    public class Veranstaltung : ITable
    {
        public int id { get; set; }
        public DateTime änderungsDatum { get; set; } //nicht benutzt
        public bool editiert { get; set; }
        public string titel { get; set; }
        public string ort { get; set; }
        public DateTime beginn { get; set; }
        public DateTime ende { get; set; }
        public string beschreibung { get; set; }
        public int typ { get; set; }
        public Veranstaltung(int id, string titel, string ort)
        {
            this.id = id;
            this.titel = titel;
            this.ort = ort;
        }
    }

    /// <summary>
    /// Sets the Teilnahme-object, inheriting ITable
    /// </summary>
    public class Teilnahme : ITable
    {

        public int id { get; set; }
        public int person { get; set; }
        public string tnStatus { get; set; }
        public string tnFunktion { get; set; }
        public DateTime änderungsDatum { get; set; }
        public DateTime datum { get; set; } //datum der veranstaltung
        public bool editiert { get; set; }
        public int veranstaltung { get; set; }
        public string details { get; set; }
        public Teilnahme(int id, string tnStatus, string tnFunktion)
        {
            this.id = id;
            this.tnStatus = tnStatus;
            this.tnFunktion = tnFunktion;

        }
    }

    /// <summary>
    /// Sets the Institution-object, inheriting ITable
    /// </summary>
    public class Institution : ITable
    {
        public int id { get; set; }
        public string kurzname { get; set; }
        public string name { get; set; }
        public DateTime änderungsDatum { get; set; }
        public bool editiert { get; set; }
        public bool aktuell { get; set; }
        public int typ { get; set; }

        public Institution(int id, string kurzname, string name)
        {
            this.id = id;
            this.kurzname = kurzname;
            this.name = name;
            aktuell = true;
        }


        public Institution(int id, DateTime date, string kurzname, string name)
        {
            this.id = id;
            this.änderungsDatum = date;
            this.kurzname = kurzname;
            this.name = name;
            aktuell = true;

        }
    }

    interface ITable
    {
        int id { get; set; }
        DateTime änderungsDatum { get; set; }
        bool editiert { get; set; }
    }

    public class ClusterItem
    {
        public List<int> Cluster { get; set; }
        public DateTime Datum { get; set; }

        public ClusterItem(List<int> _cluster, DateTime _date)
        {
            Cluster = _cluster;
            Datum = _date;
        }

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

    /// <summary>
    /// Helper for sorting ClusterItem-Objects
    /// </summary>
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

}
