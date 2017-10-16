namespace TBD
{
    public class QueryMethods
    {
        private const string COLUMN_NOME = "Nome";
        private const string COLUMN_FATURA = "FacturaID";
        private const string COLUMN_CLIENT = "ClientID";
        private const string COLUMN_MORADA = "Morada";

        public static string CreateRandomTransaction(string isolationLevel)
        {
            string tran;
            tran = "SET TRANSACTION ISOLATION LEVEL " + isolationLevel + " ";
            tran += "";
            // TODO

            return tran;
        }

        public static string GenerateUpdateTransaction(string isolationLevel, string fatura_id, string cliente_id, string nome, string morada)
        {
            string tran;
            tran = "UPDATE " + Config.DEFAULT_TABLENAME + " ";
            tran += "SET " + COLUMN_CLIENT + " = " + cliente_id + ", ";
            tran += "" + COLUMN_NOME + " = " + nome + ", ";
            tran += "" + COLUMN_MORADA + " = " + morada + " ";
            tran += "WHERE " + COLUMN_FATURA + " = " + fatura_id;

            return tran;
        }

        public static string GenerateDeleteTransaction(string isolationLevel, string fatura_id)
        {
            string tran;
            tran = "DELETE FROM " + Config.DEFAULT_TABLENAME + " ";
            tran += "WHERE " + COLUMN_FATURA + " = " + fatura_id;

            return tran;
        }

        public static string GenerateInsertTransaction(string isolationLevel, string fatura_id, string cliente_id, string nome, string morada)
        {
            string tran;
            tran = "INSERT INTO " + Config.DEFAULT_TABLENAME + "( " + COLUMN_FATURA + ", " + COLUMN_CLIENT + ", " + COLUMN_NOME + ", " + COLUMN_MORADA + ") ";
            tran += "VALUES (" + fatura_id + ", ";
            tran += "" + cliente_id + ", ";
            tran += "" + nome + ", ";
            tran += "" + morada + ")";

            return tran;
        }
    }
}