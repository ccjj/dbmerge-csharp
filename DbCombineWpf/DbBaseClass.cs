using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;

namespace DbCombineWpf
{
    internal abstract class DbOperations
    {

        protected OleDbDataAdapter _dataAdapter;
        protected string _connectionString;
        public DbTabellen tableName { get; set; }

        protected bool IsConnected()
        {
            using (var connection = new OleDbConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    return true;
                }
                catch (OleDbException)
                {
                    return false;
                }
            }
        }

        protected DbOperations(string filepath, DbTabellen tbl)
        {

            //_dbconnection
            _connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filepath;
            tableName = tbl;
        }




        public void insertTeilnahme(Teilnahme teilnahme)
        {
            string tnStatus = teilnahme.tnStatus;
            string tnFunktion = teilnahme.tnFunktion;
            string datum = teilnahme.datum.ToString();
            string veranstaltung = teilnahme.veranstaltung.ToString();
            string details = teilnahme.details;
            string person = teilnahme.person.ToString();


            var pFields = new Dictionary<string, string>();
            pFields.Add("person", person);
            pFields.Add("teilnahmestatus", tnStatus);
            pFields.Add("teilnahmefunktion", tnFunktion);
            pFields.Add("datum", datum);
            pFields.Add("veranstaltung", veranstaltung);
            pFields.Add("details", details);

            var command = TableInsertCommandWithParams(pFields, "Teilnahme");
            ExecCommand(command);
        }



        private static OleDbCommand TableInsertCommandWithParams(Dictionary<string, string> pFields, string tableName)
        {


            List<string> intValues = new List<string>() {
                "id",
                "person",
                "veranstaltung",
                "institution",
                "bundesland",
                "typ"
            };
            List<string> dateValues = new List<string>()
            {
                "änderungsDatum",
                "datum",
                "beginn",
                "ende"
            };



            

            OleDbCommand command = new OleDbCommand();
           

            var sqlBuilder = new StringBuilder();
            string insertStart = "INSERT INTO " + tableName + " ";
            var keyBuilder = new StringBuilder();
            var valueBuilder = new StringBuilder();
            OleDbParameterCollection paramCollection = command.Parameters;


            for (int n = 0; n < pFields.Count; n++)
            {
                //INSERT INTO table_name (column1,column2,column3,...) VALUES (value1,value2,value3,...);
                string seperator;
                if (n != pFields.Count - 1)
                {
                    seperator = ", ";
                }
                else
                {
                    seperator = " ";
                }

                var field = pFields.ElementAt(n);
                keyBuilder.Append(field.Key + seperator);

                //paramCollection.AddWithValue(field.Value, field.Value);

                if (TablesCompareExtensions.nameTrim(field.Value) == "" || string.IsNullOrWhiteSpace(field.Value) || field.Value == "0")
                {
                    // DBNull.Value
                    paramCollection.Add(new OleDbParameter(field.Value, OleDbType.VarWChar)).Value = DBNull.Value;

                }


                else if (intValues.Any(s => s.Equals(field.Key, StringComparison.OrdinalIgnoreCase)))
                {//IST int
                    paramCollection.Add(new OleDbParameter(field.Value, OleDbType.Integer)).Value = field.Value;
                }
                else if (dateValues.Any(s => s.Equals(field.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    //IST datum
                    paramCollection.Add(new OleDbParameter(field.Value, OleDbType.DBTimeStamp)).Value = field.Value;
                }
                else
                { //IST string

                    paramCollection.Add(new OleDbParameter(field.Value, OleDbType.VarWChar)).Value = field.Value;
                }



                valueBuilder.Append(" ? " + seperator);
            }
            
            sqlBuilder.Append(insertStart + "(" + keyBuilder + ")" + " VALUES (" + valueBuilder + ");");
            command.CommandText = sqlBuilder.ToString();
 
            return command;


        }

        private static string TableInsertCommand(Dictionary<string, string> pFields, string tableName)
        {


            List<string> intValues = new List<string>() {
                "id",
                "person",
                "veranstaltung",
                "institution",
                "bundesland",
                "typ"
            };
            List<string> dateValues = new List<string>()
            {
                "änderungsDatum",
                "datum",
                "beginn",
                "ende"
            };


            string cmd;
            var sqlBuilder = new StringBuilder();
            string insertStart = "INSERT INTO " + tableName + " ";
            var keyBuilder = new StringBuilder();
            var valueBuilder = new StringBuilder();


            for (int n = 0; n < pFields.Count; n++)
            {
                string seperator;
                if (n != pFields.Count - 1)
                {
                    seperator = ", ";
                }
                else
                {
                    seperator = " ";
                }

                var field = pFields.ElementAt(n);
                keyBuilder.Append(field.Key + seperator);

                string fieldValue;

                if (!intValues.Any(s => s.Equals(field.Key, StringComparison.OrdinalIgnoreCase)))
                {//ja, feld ist KEIN int, anführungszeichen
                    fieldValue = "'" + field.Value + "'";
                }
                else
                {
                    fieldValue = field.Value;
                }

                valueBuilder.Append(fieldValue + seperator);
            }

            sqlBuilder.Append(insertStart + "(" + keyBuilder + ")" + " VALUES (" + valueBuilder + ");");
            cmd = sqlBuilder.ToString();
            return cmd;
        }

        private static OleDbCommand TableUpdateCommandWithId(Object obj, string tableName)
        {
            var pFields = new Dictionary<string, string>();
            string id;
            var command = new OleDbCommand();
            OleDbParameterCollection paramCollection = command.Parameters;

            List<string> intValues = new List<string>() {
                "id",
                "person",
                "veranstaltung",
                "institution",
                "bundesland",
                "typ"
            };
            List<string> dateValues = new List<string>()
            {
                "änderungsDatum",
                "datum",
                "beginn",
                "ende",
                "Geändert"
            };


            if (tableName == "Institution")
            {
                var institution = obj as Institution;
                id = institution.id.ToString();
                string name = institution.name;
                string kurzname = institution.kurzname;
                string typ = institution.typ.ToString();
                string änderungsDatum = institution.änderungsDatum.ToString();

                pFields.Add("name", name);
                pFields.Add("kurzname", kurzname);
                pFields.Add("typ", typ);
                pFields.Add("Geändert", änderungsDatum);
            }
            else if (tableName == "Person")
            {
                var person = obj as Person;
                //TODO namen mit db-namen angleichen (plz=?=
                id = person.id.ToString(); //int
                string vorname = person.vorname;
                string name = person.name;
                string titel = person.titel;
                string geschlecht = person.geschlecht;
                string institution = person.institution.ToString(); //int
                string abteilung = person.abteilung;
                string funktion = person.funktion;
                string strasse = person.strasse;
                string plz = person.plz;
                string ort = person.ort;
                string bundesland = person.bundesland.ToString(); //int
                string land = person.land;
                string mobil = person.mobil;
                string telefon = person.telefon;
                string fax = person.fax;
                string email = person.email;
                string internet = person.internet;
                string titelVname = person.titelVname;
                string titelNname = person.titelNname;
                string änderungsDatum = person.änderungsDatum.ToString();


                
                //pFields.Add("id", id);
                pFields.Add("vorname", vorname);
                pFields.Add("name", name);
                pFields.Add("titel", titel);
                pFields.Add("geschlecht", geschlecht);
                pFields.Add("institution", institution);
                pFields.Add("abteilung", abteilung);
                pFields.Add("funktion", funktion);
                pFields.Add("straße", strasse);
                pFields.Add("plz", plz);
                pFields.Add("ort", ort);
                pFields.Add("bundesland", bundesland);
                pFields.Add("land", land);
                pFields.Add("mobil", mobil);
                pFields.Add("telefon", telefon);
                pFields.Add("fax", fax);
                pFields.Add("email", email);
                pFields.Add("internet", internet);
                pFields.Add("titelVname", titelVname);
                pFields.Add("titelNname", titelNname);
                pFields.Add("Geändert", änderungsDatum);
            }
            else
            {
                throw new NotImplementedException("tabelle nicht gefunden");
            }

            
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append("UPDATE "+ tableName + " SET ");



            for (int n = 0; n < pFields.Count; n++)
            {
                var field = pFields.ElementAt(n);
                sqlBuilder.Append(field.Key + "= ? ");
                if (n != pFields.Count - 1)
                {
                    sqlBuilder.Append(", ");
                }
                else
                {
                    sqlBuilder.Append(" WHERE " + tableName + ".Id = " + id + ";");
                }



                //SET column1=value1,column2=value2,...
                if (intValues.Any(s => s.Equals(field.Key, StringComparison.OrdinalIgnoreCase)))
                {//IST int

                    if (TablesCompareExtensions.isNull(field.Value) || field.Value == "0")
                    {
                        paramCollection.Add(new OleDbParameter(field.Value, OleDbType.Integer)).Value = DBNull.Value;

                    }
                    else
                    {
                        paramCollection.Add(new OleDbParameter(field.Value, OleDbType.Integer)).Value = field.Value;
                    }
                }
                else if (dateValues.Any(s => s.Equals(field.Key, StringComparison.OrdinalIgnoreCase)))
                {
                    if (TablesCompareExtensions.isNull(field.Value) == true)
                    {
                        paramCollection.Add(new OleDbParameter(field.Value, OleDbType.DBTimeStamp)).Value = DBNull.Value;

                    }
                    else
                    {
                        //IST datum
                        paramCollection.Add(new OleDbParameter(field.Value, OleDbType.DBTimeStamp)).Value = field.Value;  
                    }

                }
                else //DBNull.Value
                { //IST string
                    if (TablesCompareExtensions.isNull(field.Value) == true)
                    {
                        paramCollection.Add(new OleDbParameter(field.Value, OleDbType.VarWChar)).Value = DBNull.Value;

                    }
                    else
                    {
                        paramCollection.Add(new OleDbParameter(field.Value, OleDbType.VarWChar)).Value = field.Value;
                    }
                }
            }

            command.CommandText = sqlBuilder.ToString();


            return command;
        }

        public void insertVeranstaltung(Veranstaltung veranstaltung)
        {
            string id = veranstaltung.id.ToString(); 
            string titel = veranstaltung.titel;
            string ort = veranstaltung.ort;
            string beginn = veranstaltung.beginn.ToString();
            string ende = veranstaltung.ende.ToString();
            string beschreibung = veranstaltung.beschreibung;
            string typ = veranstaltung.typ.ToString(); //todo typen anpassen

            var pFields = new Dictionary<string, string>();
            //pFields.Add("id", id);
            pFields.Add("titel", titel);
            pFields.Add("ort", ort);
            pFields.Add("beginn", beginn);
            pFields.Add("ende", ende);
            pFields.Add("beschreibung", beschreibung);
            pFields.Add("typ", typ);

            var command = TableInsertCommandWithParams(pFields, "Veranstaltung");
            ExecCommand(command);

        }

        public void insertInstit(Institution institution)
        {
            string id = institution.id.ToString();
            string name = institution.name;
            string kurzname = institution.kurzname;
            string typ = institution.typ.ToString();
            string änderungsDatum = institution.änderungsDatum.ToString();

            var pFields = new Dictionary<string, string>();
            pFields.Add("name", name);
            pFields.Add("kurzname", kurzname);
            pFields.Add("typ", typ);
            pFields.Add("Geändert", änderungsDatum);



            var command = TableInsertCommandWithParams(pFields, "Institution");
            ExecCommand(command);
        }




        public void updateInstit(Institution institution)
        {
            var cmd = TableUpdateCommandWithId(institution, "Institution");
            ExecCommand(cmd);
        }


        public void insertPerson(Person person)
        {
            
            string id = person.id.ToString(); //int
            string vorname = person.vorname;
            string name = person.name;
            string titel = person.titel;
            string geschlecht = person.geschlecht;
            string institution = person.institution.ToString(); //int
            string abteilung = person.abteilung;
            string funktion = person.funktion;
            string strasse = person.strasse;
            string plz = person.plz;
            string ort = person.ort;
            string bundesland = person.bundesland.ToString(); //int
            string land = person.land;
            string mobil = person.mobil;
            string telefon = person.telefon;
            string fax = person.fax;
            string email = person.email;
            string internet = person.internet;
            string titelVname = person.titelVname;
            string titelNname = person.titelNname;
            string änderungsDatum = person.änderungsDatum.ToString();


            var pFields = new Dictionary<string, string>();
            pFields.Add("vorname", vorname);
            pFields.Add("name", name);
            pFields.Add("titel", titel);
            pFields.Add("geschlecht", geschlecht);
            pFields.Add("institution", institution);
            pFields.Add("abteilung", abteilung);
            pFields.Add("funktion", funktion);
            pFields.Add("straße", strasse);
            pFields.Add("plz", plz);
            pFields.Add("ort", ort);
            pFields.Add("bundesland", bundesland);
            pFields.Add("land", land);
            pFields.Add("mobil", mobil);
            pFields.Add("telefon", telefon);
            pFields.Add("fax", fax);
            pFields.Add("email", email);
            pFields.Add("internet", internet);
            pFields.Add("titelVname", titelVname);
            pFields.Add("titelNname", titelNname);
            pFields.Add("Geändert", änderungsDatum);


            var command = TableInsertCommandWithParams(pFields, "Person");
            ExecCommand(command);
        }


        public void updatePerson(Person person)
        {
            var cmd = TableUpdateCommandWithId(person, "Person");
            ExecCommand(cmd);
        }


        public SortedList<int, ITable> getTableList(DbTabellen tblName)
        {
            #region tabellen-auswahl
            string operation;
            if (tblName == DbTabellen.Person)
            {
                operation = @"SELECT Person.[ID], Person.[Vorname], Person.[Name], Person.[Titel], Person.[Geschlecht], Person.[Institution], Person.[Abteilung], Person.[Funktion], Person.[Straße], Person.[PLZ], Person.[Ort], Person.[Bundesland], Person.[Land], Person.[Mobil], Person.[Telefon], Person.[Fax], Person.[EMail], Person.[Internet], Person.[TitelVName], Person.[TitelNName] FROM Person WHERE Person.[Name] IS NOT NULL AND Person.[Vorname] IS NOT NULL AND Person.[Geschlecht] IS NOT NULL ORDER BY Person.ID ASC;";
            }
            else if (tblName == DbTabellen.Institution)
            {
                operation = @"SELECT Institution.ID, Institution.Kurzname, Institution.Name, Institution.Typ FROM Institution ORDER BY Institution.ID ASC;";
                //mitgliedshs
            } else if(tblName == DbTabellen.Veranstaltung){
                operation = @"SELECT Veranstaltung.[ID], Veranstaltung.[Titel], Veranstaltung.[Ort], Veranstaltung.[Beginn], Veranstaltung.[Ende], Veranstaltung.[Beschreibung], Veranstaltung.[Typ] FROM Veranstaltung ORDER BY Veranstaltung.ID ASC;";
            } else if(tblName == DbTabellen.Teilnahme){

               operation = @"SELECT Teilnahme.ID, Teilnahme.Teilnahmestatus, Teilnahme.Teilnahmefunktion, Teilnahme.Veranstaltung, Teilnahme.Datum, Teilnahme.Details, Teilnahme.Person, Person.Geschlecht FROM  Teilnahme INNER JOIN Person ON Person.ID = Teilnahme.Person WHERE (Person.Name IS NOT NULL AND Person.Vorname IS NOT NULL AND Person.Geschlecht IS NOT NULL AND Person.ID IS NOT NULL);";

            }


            else
            {
                throw new NotImplementedException();
            }
            #endregion
            string[] args;
            var tableList = new SortedList<int, ITable>();
            using (var connection = new OleDbConnection(_connectionString))
            {
                OleDbCommand command = new OleDbCommand(operation, connection);
                connection.Open();
                using (OleDbDataReader dataReader = command.ExecuteReader())
                {
                    while (dataReader != null && dataReader.Read())
                    {
                        
                         if (tblName == DbTabellen.Person)
                        {
                            args = new string[17];
                            args[0] = dataReader[3] as string; //titel
                            //args[0] = dataReader.GetString(3); //titel
                            args[1] = dataReader[4] as string; //geschlecht
                            var instId = dataReader[5] as int? ?? default(int);
                            args[2] = instId.ToString();
                            //args[2] = dataReader.GetInt32(5).ToString(); //instit
                            args[3] = dataReader[6] as string; //abteilung
                            args[4] = dataReader[7] as string; //funktion
                            args[5] = dataReader[8] as string; //strasse
                            args[6] = dataReader[9] as string; //plz
                            args[7] = dataReader[10] as string; //ort
                            //args[8] = dataReader.GetInt32(11).ToString(); //bundesland
                            //sqlreader[indexAge] as int? ?? default(int);
                            var bland = dataReader[11] as int? ?? default(int);
                            args[8] = bland.ToString(); //bundesland
                            args[9] = dataReader[12] as string; //land
                            args[10] = dataReader[13] as string; //mobil
                            args[11] = dataReader[14] as string; //telefon
                            args[12] = dataReader[15] as string; //fax
                            args[13] = dataReader[16] as string; //email
                            args[14] = dataReader[17] as string; //internet
                            args[15] = dataReader[18] as string; //titelVname
                            args[16] = dataReader[19] as string; //titelNname
                        }
                         else if (tblName == DbTabellen.Institution)
                         {
                             args = new string[1];
                             args[0] = dataReader.GetInt32(3).ToString(); //Typ
                        }
                         else if (tblName == DbTabellen.Veranstaltung)
                         {
                             args = new string[4];
                             var beginn = dataReader[3] as DateTime? ?? default(DateTime);
                             args[0] = beginn.ToString();//beginn
                             var ende = dataReader[4] as DateTime? ?? default(DateTime);
                             args[1] = ende.ToString();//ende
                             args[2] = dataReader[5] as string;//beschreibung memo
                             var typ = dataReader[6] as int? ?? default(int);
                             args[3] = typ.ToString();//typ int
                             // todo
                         }
                         else if (tblName == DbTabellen.Teilnahme)
                         {//strin,g string, int
                             args = new string[4];
                             var veranstaltung = dataReader[3] as int? ?? default(int);
                             args[0] = veranstaltung.ToString(); //veranstaltung
                             var datum = dataReader[4] as DateTime? ?? default(DateTime);//datum
                             args[1] = datum.ToString();
                             args[2] = dataReader[5] as string;//details                         }
                             var personId = dataReader[6] as int? ?? default(int);
                             args[3] = personId.ToString(); //person
                         }
                         else
                         {
                             throw new NotImplementedException();
                         }
                         var tableItem = TableItemFactory.CreateTableItem(tblName, dataReader.GetInt32(0), dataReader.GetString(1), dataReader.GetString(2), args);

                        tableList.Add(dataReader.GetInt32(0), tableItem);

                    }
                }
                connection.Close();
            }
            return tableList;

        }

 


        public MultiValueDictionary<DateTime, ITable> getTableDateList(DbTabellen tblName)
        {


            #region tabellen-auswahl
            string operation;
            if (tblName == DbTabellen.Person)
            {
                operation = @"SELECT Person.ID, Person.Vorname, Person.Name, Veranstaltung.Beginn FROM Veranstaltung INNER JOIN (Person INNER JOIN Teilnahme ON Person.ID = Teilnahme.Person) ON Veranstaltung.ID = Teilnahme.Veranstaltung WHERE Person.[Name] IS NOT NULL AND Person.[Vorname] IS NOT NULL AND Person.[Geschlecht] IS NOT NULL AND Person.[ID] IS NOT NULL ORDER BY Veranstaltung.Beginn ASC;";
            }
            else if (tblName == DbTabellen.Institution)
            {
                operation = @"SELECT Institution.ID, Institution.Kurzname, Institution.Name, Veranstaltung.Beginn
FROM Veranstaltung INNER JOIN ((Institution RIGHT JOIN Person ON Institution.ID = Person.Institution) INNER JOIN Teilnahme ON Person.ID = Teilnahme.Person) ON Veranstaltung.ID = Teilnahme.Veranstaltung WHERE Institution.ID IS NOT NULL ORDER BY Veranstaltung.Beginn ASC;";
            }
            else
            {
                throw new NotImplementedException();
            }
            #endregion

            

           

            var institList = new MultiValueDictionary<DateTime, ITable>();

            using (var connection = new OleDbConnection(_connectionString))
            {
                OleDbCommand command = new OleDbCommand(operation, connection);
                connection.Open();
                using (OleDbDataReader dataReader = command.ExecuteReader())
                {

                    while (dataReader != null && dataReader.Read())
                    {
                       var tableItem = TableItemFactory.CreateTableItem(tableName, dataReader.GetInt32(0), dataReader.GetString(1), dataReader.GetString(2));
                        institList.Add(dataReader.GetDateTime(3), tableItem);


                    }

                }
                connection.Close();
            }
            return institList;
        }

        public void ExecCommand(OleDbCommand cmd)
        {
            using (var connection = new OleDbConnection(_connectionString))
            {

                connection.Open();
                cmd.Connection = connection;
                cmd.ExecuteNonQuery();
                //DoSQL();
                connection.Close();


            }

        }


        public void ExecSql(string operation)
        {
            using (var connection = new OleDbConnection(_connectionString))
            {
  
                    connection.Open();
                    OleDbCommand command = new OleDbCommand(operation, connection);
                    command.ExecuteNonQuery();
                    connection.Close();
                

            }

        }


        protected virtual void DoSQL()
        {

        }


    }



    enum DbTabellen { Person, Institution, Veranstaltung, Teilnahme };
}