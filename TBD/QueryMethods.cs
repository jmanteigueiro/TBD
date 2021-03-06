﻿using TBD.Entities;

namespace TBD
{
    public class QueryMethods
    {
        private const string COLUMN_NOME = "Nome";
        private const string COLUMN_FATURA = "FacturaID";
        private const string COLUMN_CLIENT = "ClienteID";
        private const string COLUMN_MORADA = "Morada";

        private const string COLUMN_SEC_FATURAID = "FacturaID";
        private const string COLUMN_SEC_PRODID = "ProdutoID";
        private const string COLUMN_SEC_DESIGNACAO = "Designacao";
        private const string COLUMN_SEC_PRECO = "Preco";
        private const string COLUMN_SEC_QTD = "Qtd";

        private const string COLUMN_LOG_EVENT_TYPE = "EventType";
        private const string COLUMN_LOG_FATURA_ID_OLD = "FactId_Old";
        private const string COLUMN_LOG_CLIENT_ID_OLD = "ClientID_Old";
        private const string COLUMN_LOG_NOME_OLD = "Nome_Old";
        private const string COLUMN_LOG_MORADA_OLD = "Morada_Old";
        private const string COLUMN_LOG_FATURA_ID = "FactId_New";
        private const string COLUMN_LOG_CLIENT_ID = "ClientID_New";
        private const string COLUMN_LOG_NOME = "Nome_NEW";
        private const string COLUMN_LOG_MORADA = "Morada_New";
        private const string COLUMN_LOG_USER_ID = "UserID";
        private const string COLUMN_LOG_TERMINAL_ID = "TerminalID";
        private const string COLUMN_LOG_TERMINAL_NAME = "TerminalName";
        private const string COLUMN_LOG_START_TIME = "StartTime";
        private const string COLUMN_LOG_END_TIME = "EndTime";

        public static string GenerateUpdateTransaction(int idToUpdate, int cliente_id, string nome, string morada)
        {

            string tran;

            tran = " DECLARE @dateBegin DATETIME; ";
            tran += " DECLARE @dateEnd DATETIME; ";
            tran += " SET @dateBegin = CONVERT( VARCHAR, GETDATE(), 121); ";
            tran += " DECLARE @clienteOldID INT;";
            tran += " DECLARE @nomeOld nvarchar(30);";
            tran += " DECLARE @moradaOld nvarchar(30);";
            tran += " SELECT @clienteOldID = " + COLUMN_CLIENT +
                ", @nomeOld = " + COLUMN_NOME + ", @moradaOld = " + COLUMN_MORADA + " FROM " + Config.DEFAULT_TABLENAME +
                " WHERE " + COLUMN_FATURA + " = " + idToUpdate + ";";

            //tran += " DECLARE @lastFacturaID INT; ";
            //tran += " SET @lastFacturaID = (SELECT TOP(1) FacturaID FROM Factura ORDER BY FacturaID DESC); ";
            tran += " BEGIN TRANSACTION; ";
            tran += "UPDATE " + Config.DEFAULT_TABLENAME +
                " SET " + COLUMN_NOME + " = '" + nome + "', " + COLUMN_MORADA + " = '" + morada + "', " + COLUMN_CLIENT + " = " + cliente_id + " ";
            tran += "WHERE " + COLUMN_FATURA + " = " + idToUpdate + ";";

            tran += " COMMIT; ";
            tran += " SET @dateEnd = CONVERT( VARCHAR, GETDATE(), 121); ";

            tran += "INSERT INTO " + Config.DEFAULT_LOGTABLENAME + " ";
            tran += "(" + COLUMN_LOG_EVENT_TYPE + ", " + COLUMN_LOG_FATURA_ID +
                ", " + COLUMN_LOG_CLIENT_ID + ", " + COLUMN_LOG_NOME + ", " + COLUMN_LOG_MORADA +
                ", " + COLUMN_LOG_FATURA_ID_OLD + ", " + COLUMN_LOG_CLIENT_ID_OLD + ", " + COLUMN_LOG_NOME_OLD + ", " + COLUMN_LOG_MORADA_OLD + ", " + COLUMN_LOG_START_TIME + ", " + COLUMN_LOG_END_TIME + ") ";
            tran += "VALUES ('U', "+idToUpdate+", "+cliente_id+", '"+nome+ "', '" + morada + "', " + idToUpdate + ", @clienteOldID, @nomeOld, @moradaOld, @dateBegin, @dateEnd);";

            return tran;
        }

        public static string GenerateDeleteTransaction(int idToDelete)
        {
            string tran;

            tran = " DECLARE @dateBegin DATETIME; ";
            tran += " DECLARE @dateEnd DATETIME; ";
            tran += " SET @dateBegin = CONVERT( VARCHAR, GETDATE(), 121); ";
            tran += " DECLARE @facturaID INT;";
            tran += " DECLARE @clienteID INT;";
            tran += " DECLARE @nome nvarchar(30);";
            tran += " DECLARE @morada nvarchar(30);";
            tran += " SELECT @facturaID = " + COLUMN_FATURA + ", @clienteID = " + COLUMN_CLIENT +
                ", @nome = " + COLUMN_NOME + ", @morada = " + COLUMN_MORADA + " FROM " + Config.DEFAULT_TABLENAME +
                " WHERE " + COLUMN_FATURA + " = " + idToDelete + ";";

            //delete fact linhas
            tran += " BEGIN TRANSACTION; ";
            tran += "DELETE FROM " + Config.DEFAULT_SECONDARYTABLENAME + " ";
            tran += "WHERE " + COLUMN_SEC_FATURAID + " = " + idToDelete;


            tran += "DELETE FROM " + Config.DEFAULT_TABLENAME + " ";
            tran += "WHERE " + COLUMN_FATURA + " = " + idToDelete;
            
            tran += " COMMIT; ";
            tran += " SET @dateEnd = CONVERT( VARCHAR, GETDATE(), 121); ";

            tran += "INSERT INTO " + Config.DEFAULT_LOGTABLENAME + " ";
            tran += "(" + COLUMN_LOG_EVENT_TYPE + ", " + COLUMN_LOG_FATURA_ID +
                ", " + COLUMN_LOG_CLIENT_ID + ", " + COLUMN_LOG_NOME + ", " + COLUMN_LOG_MORADA +
                ", " + COLUMN_LOG_START_TIME + ", " + COLUMN_LOG_END_TIME + ") ";
            tran += "VALUES ('D', @facturaID, @clienteID, @nome, @morada, @dateBegin, @dateEnd);";

            return tran;
        }

        public static string GenerateInsertTransaction(int clienteID, string nome, string morada, int qtd_prods, string[] prods, string[] prods_price, int[] prods_qtd, int[] prods_id)
        {
            string tran;

            tran = " DECLARE @dateBegin DATETIME; ";
            tran += " DECLARE @dateEnd DATETIME; ";
            tran += " SET @dateBegin = CONVERT( VARCHAR, GETDATE(), 121); ";
            tran += " DECLARE @facturaID INT;";
            tran += " SELECT TOP(1) @facturaID = " + COLUMN_FATURA + " FROM " + Config.DEFAULT_TABLENAME +
                " ORDER BY " + COLUMN_FATURA + " DESC; IF @facturaID is NULL SET @facturaID = 0;";

            tran += " BEGIN TRANSACTION; ";
            tran += "INSERT INTO " + Config.DEFAULT_TABLENAME + "(" + COLUMN_FATURA + ", " + COLUMN_CLIENT + ", " + COLUMN_NOME + ", " + COLUMN_MORADA + 
                ") VALUES(@facturaID+1, '" + clienteID + "', '" + nome + "', '" + morada + "');";


            //Fact Linhas
            for(int j=0; j<qtd_prods; j++)
            {
                tran += "INSERT INTO " + Config.DEFAULT_SECONDARYTABLENAME + "(" + COLUMN_SEC_FATURAID + ", " + COLUMN_SEC_PRODID + ", " + COLUMN_SEC_DESIGNACAO + ", " + COLUMN_SEC_PRECO + ", " + COLUMN_SEC_QTD + ") ";
                tran += "VALUES(@facturaID+1, '" + prods_id[j] + "', '" + prods[j] + "', '" + prods_price[j] + "', '" + prods_qtd[j] + "');";
            }



            tran += " COMMIT; ";
            tran += " SET @dateEnd = CONVERT( VARCHAR, GETDATE(), 121); ";

            tran += "INSERT INTO " + Config.DEFAULT_LOGTABLENAME + " ";
            tran += "(" + COLUMN_LOG_EVENT_TYPE + ", " + COLUMN_LOG_FATURA_ID_OLD + ", " + COLUMN_LOG_CLIENT_ID_OLD +
                ", " + COLUMN_LOG_NOME_OLD + ", " + COLUMN_LOG_MORADA_OLD +
                ", " + COLUMN_LOG_START_TIME + ", " + COLUMN_LOG_END_TIME + ") ";
            tran += "VALUES ('I', @facturaID+1, '"+clienteID+"', '"+nome+"', '"+morada+"', @dateBegin, @dateEnd);";

            return tran;
        }
    }
}