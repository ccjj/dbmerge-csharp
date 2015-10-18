using System;
using System.Collections.Generic;
using System.Linq;

namespace DbCombineWpf
{
    internal class TablesCompare
    {
        private static readonly string[] personFields =
        {
            "strasse", "plz", "ort",
            "institution", "abteilung", "funktion", "email", "telefon", "mobil", "fax", "titelVname"
        };

        private Dictionary<string, string> ShortList = new Dictionary<string, string>
        {
            {"FH", "Fachhochschule"},
            {"HS", "Hochschule"},
            {"H", "Hochschule"},
            {"Uni", "Universität"},
            {"U", "Universität"},
            {"FHS", "Fachhochschule"},
            {"TH", "Technische Hochschule"},
            {"HDM", "Hochschule der Medien"},
            {"KH", "Kirchliche Hochschule"},
            {"MH", "Musikhochschule"},
            {"HH", "TODO"},
            {"LH", "TODO"},
            {"DH", "TODO"}
        };


        public static Tuple<int, Dictionary<int, int>> ComparePerson(Dictionary<int, Person> bzTable,
            Dictionary<int, Person> qTable)
        {
            var qChangedPerson = new Dictionary<int, int>();
            int bzTableCounter = bzTable.Keys.Max();
            int bzMax = bzTableCounter;


            for (int i = 0; i < qTable.Count(); i++)
            {
                Person qPerson = qTable.ElementAt(i).Value;
                DateTime qDatum = qPerson.änderungsDatum;
                string qName = qPerson.name;
                string qVorName = qPerson.vorname;
                int qItemID = qPerson.id;

                List<KeyValuePair<int, Person>> foundPersons =
                    (from n in bzTable where n.Value.name.Equals(qName) && n.Value.vorname.Equals(qVorName) select n)
                        .ToList();

                if (foundPersons.Count() > 1)
                {
                    int? matchedProperties = null;
                    Person matchedPerson = null;

                    for (int m = 0; m < foundPersons.Count(); m++)
                    {
                        Person foundPerson = foundPersons.ElementAt(m).Value;
                        int tempMatchedProperties = CompareProperties(foundPerson, qPerson);

                        if (tempMatchedProperties > matchedProperties || matchedProperties == null)
                        {
                            //date wie berücksichtigen?
                            matchedProperties = tempMatchedProperties;
                            matchedPerson = foundPerson;
                        }
                    }
                    //date-check
                    //TODO matched-prop > 2 
                    //person in bz neuer
                    if (matchedPerson.änderungsDatum >= qDatum)
                    {
                        qChangedPerson.Add(qItemID, foundPersons.ElementAt(0).Value.id);
                      

                    }
                    else //person in q neuer
                    {
                        int tempID = matchedPerson.id;
                        bzTable[tempID] = qTable.ElementAt(i).Value;
                        bzTable[tempID].id = tempID;
                        bzTable[tempID].editiert = true;
                        qChangedPerson.Add(qItemID, tempID);
                       
                    }
                }
                else if (foundPersons.Count() == 1)
                {
                    if (foundPersons.ElementAt(0).Value.änderungsDatum >= qDatum)
                    {
                        qChangedPerson.Add(qItemID, foundPersons.ElementAt(0).Value.id);
                    }
                    else
                    {
                        int tempID = foundPersons.ElementAt(0).Value.id;
                        bzTable[tempID] = qTable.ElementAt(i).Value;
                        bzTable[tempID].id = tempID;
                        bzTable[tempID].editiert = true;
                        qChangedPerson.Add(qItemID, tempID);
                    }
                }
                else //found == 0
                {
                    bzTableCounter += 1;
                    qChangedPerson.Add(qItemID, bzTableCounter);
                }
            }

            var tuple = new Tuple<int, Dictionary<int, int>>(bzMax, qChangedPerson);
            return tuple;
        }


        private static int CompareProperties(Person bzPerson, Person qPerson)
        {
            int matchedCounter = 0;

            foreach (string s in personFields)
            {
                if (bzPerson[s] == qPerson[s])
                    matchedCounter += 1;
            }

            return matchedCounter;
        }


        public static Tuple<int, Dictionary<int, int>, Dictionary<int, int>, Dictionary<int, int>> CompareInstit(Dictionary<int, Institution> bzTable,
            Dictionary<int, Institution> qTable) //tbltocompare = q, tabletoadd = bz
        {
            // IDetails anstatt Institution hardcoded?
            //todo temp list
            var bzNotChanged = new List<int>();
            //old/new
            var bzUpdate = new Dictionary<int, int>();
            var bzInsert = new Dictionary<int, int>();
            var foundInstit = new List<Institution>();
            var notfoundInstit = new List<Institution>();
            //int oldid, int newid (in bz)
            var qChangedInstit = new Dictionary<int, int>();
            int bzTableCounter = bzTable.Keys.Max();
            int bzMax = bzTableCounter;

            for (int i = 0; i < qTable.Count(); i++)
            {
                int qKey = qTable.ElementAt(i).Key;
                string kurzName = qTable[qKey].kurzname;
                string name = qTable[qKey].name;
                int qItemID = qTable[qKey].id;
                DateTime qTableDate = qTable[qKey].änderungsDatum;
                List<string> shortSplitString =
                    SpecialChars.removeSpecialChars(kurzName)
                        .Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();
                List<string> longSplitString =
                    SpecialChars.removeSpecialChars(name)
                        .Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();


                List<Institution> originalLongNameFound =
                    bzTable.Values.Where(x => String.Equals(TablesCompareExtensions.nameTrim(x.name), TablesCompareExtensions.nameTrim(name), StringComparison.OrdinalIgnoreCase)).ToList();
                //todo
                List<Institution> originalShortNameFound =
                    bzTable.Values.Where(x => String.Equals(TablesCompareExtensions.nameTrim(x.kurzname), TablesCompareExtensions.nameTrim(kurzName), StringComparison.OrdinalIgnoreCase))
                        .ToList();


                //methode, nimmt short- oder longname
                //

                //erster test, ob die beiden original-namen in bz schon vorhanden sind
                if (TablesCompareExtensions.isDuplicateInstit(name, kurzName, bzTable))
                {
                    //ja, kommt in beiden dbs vor && ist einzelner eintrag in bz
                    //if (kurzName == "Weihenstephan-Triesdorf H")
                    //{
                    //    MessageBox.Show("Weihenstephan-Triesdorf H");
                    //}

                    //MessageBox.Show(name + kurzName);
                    bzNotChanged.Add(qKey);
                    qChangedInstit.Add(qItemID,
                        originalShortNameFound.Any() ? originalShortNameFound[0].id : originalLongNameFound[0].id);

                    continue;
                }

                #region shortname original

                if (originalShortNameFound.Any())
                {
                    //datum kleiner und qinstit darf nicht in verschiedenen instit in bz vorhanden sein
                    // == instit von kurzname und langname müssen gleich sein bei update
                    //TODO doppelter dupe-test?
                    if (originalShortNameFound[0].änderungsDatum < qTableDate &&
                        TablesCompareExtensions.isDuplicateInstit(qTable[qKey], bzTable))
                    {
                        //weil es ein duplikat ist, kann es geupdatet werden
                        //test oben schon geleistet

                        int tempID = originalShortNameFound[0].id;
                        //ersetzt die instit aus bz mit der qinstit
                        bzTable[tempID] = qTable.ElementAt(i).Value;
                        //passt die q-id dem bz index an
                        bzTable[tempID].id = tempID;
                        bzTable[tempID].aktuell = false;

                        //bz  geupdatet
                        //old/new id
                        bzUpdate.Add(tempID, qKey);
                        qChangedInstit.Add(qItemID, originalShortNameFound[0].id);
                    }
                    else
                    {
                        //kein bz-update
                        qChangedInstit.Add(qItemID, originalShortNameFound[0].id);
                    }
                    continue;
                }

                #endregion

                #region longname original

                if (originalLongNameFound.Any())
                {
                    //s.o.
                    if (originalLongNameFound[0].änderungsDatum < qTableDate &&
                        TablesCompareExtensions.isDuplicateInstit(qTable[qKey], bzTable))
                    {
                        //weil es ein duplikat ist, kann es geupdatet werden
                        //test oben schon geleistet

                        int tempID = originalLongNameFound[0].id;
                        //ersetzt die instit aus bz mit der qinstit
                        bzTable[tempID] = qTable.ElementAt(i).Value;
                        //passt die q-id dem bz index an
                        bzTable[tempID].id = tempID;
                        bzTable[tempID].aktuell = false;

                        //bz  geupdatet
                        //old/new id
                        bzUpdate.Add(tempID, qKey);
                        qChangedInstit.Add(qItemID, originalLongNameFound[0].id);
                    }
                    else
                    {
                        //kein bz-update
                        qChangedInstit.Add(qItemID, originalLongNameFound[0].id);
                    }
                    continue;
                }

                #endregion

                List<Institution> shortSplitNameFound =
                    bzTable.Values.Where(
                        x => shortSplitString.All(k => x.kurzname.Contains(k, StringComparison.OrdinalIgnoreCase)))
                        .ToList();

                //List<Institution> longSplitNameFound =
                //    bzTable.Values.Where(
                //        x => longSplitString.All(k => x.name.Contains(k, StringComparison.OrdinalIgnoreCase))).ToList();


                if (shortSplitNameFound.Count() == 1)
                {
                    if (shortSplitNameFound[0].änderungsDatum < qTableDate &&
                       TablesCompareExtensions.isDuplicateInstit(qTable[qKey], bzTable))
                    {
                        //weil es ein duplikat ist, kann es geupdatet werden
                        //test oben schon geleistet

                        //isExistantInstit (Institution instit, Dictionary<int, Institution> bzTable)
                        

                        int tempID = shortSplitNameFound[0].id;
                        //ersetzt die instit aus bz mit der qinstit
                        bzTable[tempID] = qTable.ElementAt(i).Value;
                        //passt die q-id dem bz index an
                        bzTable[tempID].id = tempID;
                        bzTable[tempID].aktuell = false;

                        //bz  geupdatet
                        //old/new id
                        bzUpdate.Add(tempID, qKey);
                        qChangedInstit.Add(qItemID, shortSplitNameFound[0].id);
                    }
                    else
                    {
                        //kein bz-update
                        qChangedInstit.Add(qItemID, shortSplitNameFound[0].id);
                    }
                    continue;
                }

                List<Institution> longNameFound;
                if (!shortSplitNameFound.Any())
                    // keine einträge in shortSplitNameFound, daher ganze bztabelle nach langnamen durchsuchen
                {
                    longNameFound =
                        bzTable.Values.Where(
                            x => longSplitString.All(k => x.name.Contains(k, StringComparison.OrdinalIgnoreCase)))
                            .ToList();
                }
                else
                {
                    longNameFound =
                        shortSplitNameFound.Where(x => longSplitString.All(k => x.name.Contains(k))).ToList();
                }

                if (longNameFound.Count() == 1)
                {
                    if (longNameFound[0].änderungsDatum < qTableDate &&
   TablesCompareExtensions.isDuplicateInstit(qTable[qKey], bzTable))
                    {
                        //weil es ein duplikat ist, kann es geupdatet werden
                        //test oben schon geleistet

                        int tempID = longNameFound[0].id;
                        //ersetzt die instit aus bz mit der qinstit
                        bzTable[tempID] = qTable.ElementAt(i).Value;
                        //passt die q-id dem bz index an
                        bzTable[tempID].id = tempID;
                        bzTable[tempID].aktuell = false;

                        //bz  geupdatet
                        //old/new id
                        bzUpdate.Add(tempID, qKey);
                        qChangedInstit.Add(qItemID, longNameFound[0].id);
                    }
                    else
                    {
                        //kein bz-update
                        qChangedInstit.Add(qItemID, longNameFound[0].id);
                    }
                    continue;
                }
                else if (longNameFound.Count() > 1)
                {
                    Institution matchedInstit = null;
                    #region lev-distance
                    
                    int? lv = null;
                    foreach (var m in longNameFound)
                    {
                        
                        //muss nicht zwingend in liste sein!
                        int distance = LevenshteinDistance.Compute(m.name, name);
                        //enum-liste zum checken?

                        if (!lv.HasValue || distance < lv)
                        {
                            lv = distance;
                            matchedInstit = m;
                        }



                    }
                    #endregion




                    if (matchedInstit.änderungsDatum < qTableDate &&
   TablesCompareExtensions.isDuplicateInstit(qTable[qKey], bzTable))
                    {
                        //weil es ein duplikat ist, kann es geupdatet werden
                        //test oben schon geleistet

                        int tempID = matchedInstit.id;
                        //ersetzt die instit aus bz mit der qinstit
                        bzTable[tempID] = qTable.ElementAt(i).Value;
                        //passt die q-id dem bz index an
                        bzTable[tempID].id = tempID;
                        bzTable[tempID].aktuell = false;

                        //bz  geupdatet
                        //old/new id
                        bzUpdate.Add(tempID, qKey);
                        qChangedInstit.Add(qItemID, matchedInstit.id);
                    }
                    else
                    {
                        //kein bz-update
                        qChangedInstit.Add(qItemID, matchedInstit.id);
                    }


                }
                else
                {
                    //insert q am ende von bz
                    bzTableCounter += 1;
                    bzInsert.Add(qItemID, bzTableCounter);
                    qChangedInstit.Add(qItemID, bzTableCounter);
                }


                //add to insert list
            }


            //TODO besser : tuple insertIntoBz, UpdateInBz, bzAktueller bzw nichts geändert
            var tuple = new Tuple<int, Dictionary<int, int>, Dictionary<int, int>, Dictionary<int, int>>(bzMax, qChangedInstit, bzInsert, bzUpdate);
            return tuple;

            #region abgleich-test

            //var testList = new Dictionary<string, string>();
            //foreach (var i in qChangedInstit.Keys)
            //{
            //    int max = bzTable.Keys.Max();
            //    string newName;
            //    string oldName = "old: " + qTable[i].name + " : " + qTable[i].kurzname;
            //    if (qChangedInstit[i] < max)
            //    {
            //        newName = "new: " + bzTable[qChangedInstit[i]].name + " : " + bzTable[qChangedInstit[i]].kurzname;
            //    }
            //    else
            //    {
            //        newName = "not found: " + qTable[i].name + " : " + qTable[i].kurzname;
            //    }
            //    testList.Add(oldName, newName);
            //}

            //testList.Count(); 

            #endregion

            //qChangedInstit.Count();
        }

        private enum Abkürzungen
        {
            FH,
            HS,
            H,
            UNI,
            U,
            FHS,
            TH,
            HDM,
            KH,
            MH,
            HH,
            LH,
            DH
        };


        //oder besser instit?

        //true = gefunden, erstbestes geaddet
    }
}