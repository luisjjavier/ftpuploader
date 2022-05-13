using System.Net;
using FluentFTP;
using FTPUpload.Configuration;
using FTPUpload.Models;
using FTPUpload.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;

const string settingKey = "Settings";
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .Build();

var log = new LoggerConfiguration()
    .ReadFrom.Configuration(config)
    .CreateLogger();

string connectionString = config.GetConnectionString("default");
Settings settings = config.GetRequiredSection(settingKey).Get<Settings>();

var connection = new SqliteConnection(connectionString);
connection.Open();
var documentsRepository = new DocumentsRepository(connection);

try
{
    initDatabase(documentsRepository);

    var client = await buildFtpClient(settings);

    var listToUpload = getFilesToUpload(settings, log, documentsRepository);

    foreach (string file in listToUpload)
    {
        if (await tryToUploadTheFile(client, file, documentsRepository, log))
            continue;

    }
}
catch (Exception ex)
{
    log.Error(ex, "Ha ocurido un error en la aplicación");
}
finally
{
    await connection.CloseAsync();
}

void initDatabase(DocumentsRepository documentsRepository1)
{
    documentsRepository1.CreateTables();
}

async Task<FtpClient> buildFtpClient(Settings settings1)
{

    FtpClient ftpClient = new FtpClient();
    var credentials = new NetworkCredential(settings1.Username, settings1.Password);
    ftpClient.Credentials = credentials;
    ftpClient.Host = settings1.Host;
    await ftpClient.ConnectAsync();
    return ftpClient;
}

List<string> getFilesToUpload(Settings settings2, Logger logger, DocumentsRepository documentsRepository2)
{

    List<string> files = Directory.GetFiles(settings2.DirectoryToWatch).ToList();

    logger.Information($"Cantidad de archivos en el folder {files.Count}");

    var list = files.Where(path => !documentsRepository2.DocumentExists(Path.GetFileName(path), File.GetLastWriteTime(path))).ToList();

    logger.Information($"Cantidad de archivos para subir {list.Count}");
    return list;
}

async Task<bool> tryToUploadTheFile(FtpClient client, string localFilePath, DocumentsRepository documentsRepository3,
    Logger log1)
{

    string filename = Path.GetFileName(localFilePath);
    DateTime lastModefication = File.GetLastWriteTime(localFilePath);
    try
    {

        log.Information($"Subiendo archivo....{filename}");
        await client.UploadFileAsync(localFilePath, filename);
        documentsRepository3.SaveDocument(new Document
        {
            Filename = filename,
            UploadedDate = DateTimeOffset.UtcNow,
            LastModification = lastModefication
        });
        log.Information($"Subiendo completado.... {filename}");
    }
    catch (Exception e)
    {
        log.Information($"Subiendo completado.... {filename}");
        log1.Error(e, $"Ha ocurido un error subiendo el archivo {filename}");
        return true;
    }

    return false;
}



