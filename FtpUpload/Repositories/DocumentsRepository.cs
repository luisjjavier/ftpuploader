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
            command.CommandText = @"CREATE TABLE IF NOT EXISTS documentos (filename VARCHAR(255) Unique primary key, uploaded DATETIMEOFFSET not null );";
            command.ExecuteNonQuery();
        }

        public void SaveDocument(Document document)
        {
            SqliteCommand command = _connection.CreateCommand();
            command.CommandText = @$"
            INSERT INTO documentos
                (filename, uploaded) VALUES('{document.Filename}', '{document.UploadedDate}');
            ";
            command.ExecuteNonQuery();
        }

        public bool DocumentExists(string filename)
        {
            SqliteCommand command = _connection.CreateCommand();

            command.CommandText = $"select 1 from documentos where filename  = '{filename}'";
            var result = Convert.ToBoolean(command.ExecuteScalar());

            return result;
        }
    }
}
