USE master

DECLARE @DBName sysname
DECLARE @DataPath nvarchar(200)
DECLARE @DataFileName nvarchar(200)
DECLARE @LogPath nvarchar(200)
DECLARE @LogFileName nvarchar(200)

-- Nome e caminho para a BD
SET @DBName = 'MEI_TRAB1'
SET @DataPath = 'c:\MEI_TBD\' + @DBName   -- Alterar se necessário
SET @LogPath  = 'c:\MEI_TBD\' + @DBName   -- Alterar se necessário

-- Nome dos ficheiros de dados e log
SET @DataFileName = @DataPath +'\Trab1dat.mdf' 
SET @LogFileName  = @LogPath  +'\Trab1log.ldf' 

-- Criar as directorias (pastas)
SET NOCOUNT ON;
DECLARE @DirectoryExists int;

EXEC master.dbo.xp_fileexist @DataPath, @DirectoryExists OUT;
IF @DirectoryExists = 0
   EXEC master.sys.xp_create_subdir @DataPath;

EXEC master.dbo.xp_fileexist @LogPath, @DirectoryExists OUT;
IF @DirectoryExists = 0
   EXEC master.sys.xp_create_subdir @LogPath;
   

-- Criar a Base de dados
   
DECLARE @SQLString nvarchar(max)

SET @SQLString = 'CREATE DATABASE ' + @DBName +
  ' ON 
   ( NAME = ''Trab1_dat'',
      FILENAME ='''+ @DataFileName + ''',      
      SIZE = 10,
      MAXSIZE = 50,
      FILEGROWTH = 5 )
   LOG ON
   ( NAME = ''Trab1_log'',
     FILENAME ='''+ @LogFileName + ''',
     SIZE = 5MB,
     MAXSIZE = 25MB,
     FILEGROWTH = 5MB )'

 SET NOCOUNT OFF;

/*
IF ( EXISTS( SELECT * FROM [dbo].[sysdatabases] Where name = '@DBName') )
Begin
  DROP DATABASE @DBName
end
*/


-------------------------------------------------------------------------------
--
-- Se não existir a BD então vamos criá-la...
-- (if not exists the database then create them)
--
IF (NOT EXISTS( SELECT * FROM [dbo].[sysdatabases] Where name = @DBName) )
Begin
  exec(@SQLString)
end


-- Criar as tabelas na base de dados recém-criada

-------------------------------------------------------------------------------
-- Criar as tabelas
-- (create the database tables)
-------------------------------------------------------------------------------

-- Tabela de Facturas

SET @SQLString = 
   'CREATE TABLE Factura (                               -- Não está em 3FN!!!
	  FacturaID int NOT NULL CHECK (FacturaID >= 1),                   
	  ClienteID int NOT NULL CHECK (ClienteID >= 1),                   
      Nome nvarchar(30) NOT NULL,                        -- Nome cliente
      Morada nvarchar(30) NOT NULL  DEFAULT ''Covilhã'',   -- Morada cliente
    
    CONSTRAINT PK_Factura PRIMARY KEY (FacturaID) -- Chave primária
   )'


SET @SQLString = 'USE '+ @DBName + 
                  ' if not exists (select * from dbo.sysobjects  where id = object_id(N''[dbo].[Factura]''))  begin '+ 
				      @SQLString +' end'

EXEC ( @SQLString)    


-- Linhas da factura

SET @SQLString = 
   'CREATE TABLE FactLinha (                        -- Não está em 3FN!!!
	  FacturaID int NOT NULL,
	  ProdutoID int NOT NULL,
	  
	  Designacao nvarchar (50) NOT NULL ,                                  -- Designação produto            
	  Preco decimal(10,2) NOT NULL  DEFAULT 10.0   CHECK (Preco >= 0.0),
	  Qtd decimal(10,2) NOT NULL  DEFAULT 1.0   CHECK (Qtd >= 0.0),         -- Qtd produto
	  
	  
	  
	  CONSTRAINT PK_FactLinha
	    PRIMARY KEY (FacturaID, ProdutoID),           -- constraint type: primary key
	  
	  
	  CONSTRAINT FK_FacturaID FOREIGN KEY (FacturaID) 
	     REFERENCES Factura(FacturaID)
	     ON UPDATE CASCADE 
	     ON DELETE NO ACTION
  )' 
SET @SQLString = 'USE '+ @DBName + 
                  ' if not exists (select * from dbo.sysobjects  where id = object_id(N''[dbo].[FactLinha]''))  begin '+ 
				      @SQLString +' end'

EXEC ( @SQLString)    
  

-- Tabela de Log
  
-- https://docs.microsoft.com/en-us/sql/t-sql/statements/create-table-transact-sql-identity-property
SET @SQLString = 
  'CREATE TABLE LogOperations (
    NumReg int IDENTITY(1,1),       -- Autoincremente
	EventType char(1),              -- I, U, D (Insert, Update, Delete)
	
    -- Dados existentes  (Para o update e o Delete)
    FactId_Old int,
    ClientID_Old int,
    Nome_Old nvarchar(30),
    Morada_Old nvarchar(30),
	
    -- Novos dados (usado no Insert e no Update)
    FactId_New int,
    ClientID_New int,
    Nome_New nvarchar(30),
    Morada_New nvarchar(30),
	
	-- Dados sobre o utilizador e posto de trabalho
    UserID nvarchar(30) NOT NULL DEFAULT USER_NAME(), 
    TerminalD      nvarchar(30) NOT NULL  DEFAULT HOST_ID(),
	TerminalName   nvarchar(30) NOT NULL  DEFAULT HOST_NAME(),
	
	-- Quanto tempo demorou a operação
    StartTime datetime NOT NULL,                   -- Início da operação
    EndTime datetime NOT NULL DEFAULT GetDate(),   -- Fim da operação
   
    CONSTRAINT PK_LogOperations PRIMARY KEY (NumReg)
  )'
  
SET @SQLString = 'USE '+ @DBName + 
                  ' if not exists (select * from dbo.sysobjects  where id = object_id(N''[dbo].[LogOperations]''))  begin '+ 
				      @SQLString +' end'

EXEC ( @SQLString)    
  


