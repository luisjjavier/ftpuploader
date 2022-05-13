using FTPUpload.Models;
using Microsoft.Data.Sqlite;

namespace FTPUpload.Repositories
{
    internal sealed class DocumentsRepository
    {
        private readonly SqliteConnection _connection;
        public DocumentsRepository(SqliteConnection connection)
        {   
            _connection = connection;
        }

        public void CreateTables()
        {
            SqliteCommand command = _connection.CreateCommand();
            command.CommandText = @"CREATE TABLE IF NOT EXISTS Documents (filename VARCHAR(255),
                lastModification DATETIME not null, 
                uploaded DATETIMEOFFSET not null );
            Create index IF NOT EXISTS IDX_FILENAME_LASTMODIFICATION on  Documents  (filename,lastModification); ";
            command.ExecuteNonQuery();
        }

        public void SaveDocument(Document document)
        {
            SqliteCommand command = _connection.CreateCommand();
            command.CommandText = @$"
            INSERT INTO Documents
                (filename, uploaded,lastModification) VALUES('{document.Filename}', '{document.UploadedDate}', '{document.LastModification}');
            ";
            command.ExecuteNonQuery();
        }

        public bool DocumentExists(string filename,DateTime lastModification)
        {
            SqliteCommand command = _connection.CreateCommand();

            command.CommandText = $"select 1 from Documents where filename  = '{filename}' and lastModification = '{lastModification}'";
            var result = Convert.ToBoolean(command.ExecuteScalar());

            return result;
        }
    }
}
