using System;
using System.Collections.Generic;
using System.Linq;

namespace DbCombineWpf
{
    class TablesCompareExtensions
    {



        public static bool isNull(string originalString)
        {

            if (string.IsNullOrWhiteSpace(originalString))
            {
                return true;
            }
            return false;
        }

        public static string nameTrim(string originalString)
        {
            string result;

            if (!string.IsNullOrWhiteSpace(originalString))
            {
                char[] charsToTrim = {' '};
                result = originalString.Trim(charsToTrim);
            }
            else
            {
                result = originalString;
            }

            return result;
        }



        public static bool isDuplicateInstit(Institution instit, Dictionary<int, Institution> bzTable)
        {
            string name = instit.name;
            string kurzName = instit.kurzname;
            bool found = isDuplicateInstit(name, kurzName, bzTable);
            return found;

        }


        //is dupe - nur alarm wenn 

        public static bool isDuplicateInstit(string name, string kurzName, Dictionary<int, Institution> bzTable)
        {



            var originalLongNameFound = bzTable.Values.Where(x => String.Equals(nameTrim(x.name), nameTrim(name), StringComparison.OrdinalIgnoreCase)).ToList();

            var originalShortNameFound = bzTable.Values.Where(x => String.Equals(nameTrim(x.kurzname), nameTrim(kurzName), StringComparison.OrdinalIgnoreCase)).ToList();



            if (originalShortNameFound.Any() && originalLongNameFound.Any())
            {
                //extract method, vor qchangedinstit.add immer aufrufen
                if (originalShortNameFound[0].id == originalLongNameFound[0].id)
                {
                    //qChangedInstit.Add(qItemID, originalShortNameFound[0].id);
                    //nein, kein duplikat
                    return true;
                    //andere instit - bei update gibts fehler
                    //kein update/insert, sondern das erstbeste in shortname nehmen
                    //FEHLER kann
                }
                
            }
            //ja, ist duplikat
            //true = ja, würde fehler beim insert geben
            return false;
        }

        public static bool isExistantInstit(Institution instit, Dictionary<int, Institution> bzTable)


        {
            //instit to update

            string name = instit.name;
            string kurzName = instit.kurzname;

            List<Institution> originalLongNameFound =
    bzTable.Values.Where(x => String.Equals(x.name, name, StringComparison.OrdinalIgnoreCase)).ToList();
            //todo
            List<Institution> originalShortNameFound =
                bzTable.Values.Where(x => String.Equals(x.kurzname, kurzName, StringComparison.OrdinalIgnoreCase))
                    .ToList();


            if (originalShortNameFound.Any() || originalLongNameFound.Any())
            {
                    return true;
                
            }
            return false;
        }




        public static Institution returnBestInstitMatch(List<Institution> institList, string name)
        {
            Institution matchedInstit = null;
            int? lv = null;

            foreach (var m in institList)
            {
                //muss nicht zwingend in liste sein!
                int distance = LevenshteinDistance.Compute(m.name, name);

                if (!lv.HasValue || distance < lv)
                {
                    lv = distance;
                    matchedInstit = m;
                }



            }

            return matchedInstit;
        }
    }
}