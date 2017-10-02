namespace TBD
{
    public class QueryMethods
    {
        public static string CreateRandomTransaction(string isolationLevel)
        {
            string tran;
            tran = "SET TRANSACTION ISOLATION LEVEL " + isolationLevel + " ";
            tran += ""; 
            // TODO

            return tran;
        }

        public static string CreateTransaction(string isolationLevel, string query)
        {
            string tran;
            tran = "SET TRANSACTION ISOLATION LEVEL " + isolationLevel + " ";
            tran += "BEGIN TRAN ";
            tran += query + " ";
            tran += "COMMIT;";
            // TODO : ROLLBACK?

            return tran;
        }
    }
}
