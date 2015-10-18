using System.Data;

namespace DbCombineWpf
{

    class DbSelTable : DbOperations
    {
        public DbSelTable(string filepath, DbTabellen tbl)
            : base(filepath, tbl)
        {
        }

        public DataTable DataTable { get; private set; }

        protected override void DoSQL()
        {
            DataTable = new DataTable();
            _dataAdapter.Fill(DataTable);
        }


        public DataTable GetTable()
        {
            return DataTable;
        }

        public int countRows(DataTable dTable)
        {
            return dTable.Rows.Count;
        }




    }
}
