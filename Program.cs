using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureStorageMirror
{
    class Program
    {
        private const string ConnectionString = "DefaultEndpointsProtocol=https;AccountName=xxxx;AccountKey=yyyy;";

        static void Main(string[] args)
        {
            CreateContainer();


            var rootDir = "c:\\src\\msdevshow";// Directory.GetCurrentDirectory();
            var files = Directory.EnumerateFiles(rootDir, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                //TODO: this is dangerous maybe
                var relativePath = file.Replace(rootDir + "\\", "");
                
                var fileHash = GetFileHash(file);
                var blobHash = GetAzureFileHash(relativePath);

                if (fileHash != blobHash)
                {
                    Console.WriteLine("Uploading '{0}'", relativePath);
                    UploadFile(file, relativePath);
                    Console.WriteLine("Upload complete");
                }
            }

            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static string GetFileHash(string path)
        {
            var hasher = MD5.Create();
            var sb = new StringBuilder();
            
            using (var fs = File.OpenRead(path))
            {
                 return Convert.ToBase64String(hasher.ComputeHash(fs));
            }
        }

        private static string GetAzureFileHash(string path)
        {
            var container = GetContainer();
            var blob = container.GetBlockBlobReference(path);

            if (!blob.Exists())
            {
                return "";
            }

            var attr = blob.Properties;
            return attr.ContentMD5;
        }

        private static void UploadFile(string file, string relativePath)
        {
            var container = GetContainer();
            var blob = container.GetBlockBlobReference(relativePath);
            blob.UploadFromFile(file, FileMode.Open);
        }

        private static void CreateContainer()
        {
            var container = GetContainer();
            container.CreateIfNotExists();
        }

        private static CloudBlobContainer GetContainer()
        {
            var storageAccount = CloudStorageAccount.Parse(ConnectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("folder1");

            return container;
        }
    }
}
