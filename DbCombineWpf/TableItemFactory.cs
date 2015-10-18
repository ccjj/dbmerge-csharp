using System;
using System.Linq;

namespace DbCombineWpf
{
    static class TableItemFactory
    {

        public static ITable CreateTableItem(DbTabellen tableName, int id, string prop1, string prop2, params string[] args)
        {

            if (tableName == DbTabellen.Institution)
            {
                Institution instit;
                if (args.Any())
                {
                    instit = new Institution(id, prop1, prop2)
                    {
                        typ = Convert.ToInt32(args[0])
                    };
                }
                else
                {
                    instit = new Institution(id, prop1, prop2);

                }
                return instit;
            }
            else if (tableName == DbTabellen.Person)
            {
                Person person;
                if (args.Any())
                {
                    person = new Person(id, prop1, prop2)
                    {
                        titel = args[0],
                        geschlecht = args[1],
                        institution = Convert.ToInt32(args[2]),
                        abteilung = args[3],
                        funktion = args[4],
                        strasse = args[5],
                        plz = args[6],
                        ort = args[7],
                        bundesland = Convert.ToInt32(args[8]),
                        land = args[9],
                        mobil = args[10],
                        telefon = args[11],
                        fax = args[12],
                        email = args[13],
                        internet = args[14],
                        titelVname = args[15],
                        titelNname = args[16]
                    };
                }
                else
                {
                    person = new Person(id, prop1, prop2);
                }
                return person;
            }
            else if (tableName == DbTabellen.Veranstaltung)
            {
                var veranstaltung = new Veranstaltung(id, prop1, prop2)
                {
                    beginn = Convert.ToDateTime(args[0]),
                    ende = Convert.ToDateTime(args[1]),
                    beschreibung = args[2],
                    typ = Convert.ToInt32(args[3])

                };
                return veranstaltung;

            }

            else if (tableName == DbTabellen.Teilnahme)
            {

                var teilnahme = new Teilnahme(id, prop1, prop2)
                {
                    veranstaltung = Convert.ToInt32(args[0]),
                    datum = Convert.ToDateTime(args[1]),
                    details = args[2],
                    person = Convert.ToInt32(args[3])
                };
                return teilnahme;
            }
            else
            {
                throw new NotImplementedException();
            }
        }




    }
}
