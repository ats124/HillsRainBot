using System;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace HillsRainBotFunctions
{
    public static class CollectHillsCameraPictures
    {
        private static readonly CloudBlobClient blobClient;
        private static readonly HttpClient httpClient;

        static CollectHillsCameraPictures()
        {
            httpClient = new HttpClient();
            var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["HillsCameraPicturesStorage"]);
            blobClient = storageAccount.CreateCloudBlobClient();
        }

        [FunctionName("CollectHillsCameraPictures")]
        public static async Task Run([TimerTrigger("0 * * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info($"C# Timer trigger function executed at: {DateTimeExtensions.JapanNow}");

            var nowScheduleDateTime = myTimer.ScheduleStatus.Next.ConvertToJapanTime();

            // �q���Y�J�����摜���_�E�����[�h
            byte[] picture;
            using (var stream = await httpClient.GetStreamAsync(ConfigurationManager.AppSettings["HillsCameraPictureUrl"]))
            using (var memStream = new MemoryStream())
            {
                await stream.CopyToAsync(memStream);
                picture = memStream.ToArray();
            }

            // �摜�`�F�b�N(100�o�C�g���� SOI�Ŏn�܂���EOI�ŏI����ĂȂ����NG)
            if (picture.Length < 100 || picture[0] != 0xFF || picture[1] != 0xD8 || picture[picture.Length - 2] != 0xFF || picture[picture.Length - 1] != 0xD9)
            {
                log.Error("�j������JPEG�t�@�C����JPEG�t�@�C���ł͂���܂���");
                return;
            }

            // Blob�R���e�i
            var blobContainer = blobClient.GetContainerReference(ConfigurationManager.AppSettings["HillsCameraPicturesBlobContainer"]);
            await blobContainer.CreateIfNotExistsAsync();

            // Blob�ɕۑ�
            var blobName = $"{nowScheduleDateTime:yyyy}/{nowScheduleDateTime:yyyyMM}/{nowScheduleDateTime:yyyyMMdd}/{nowScheduleDateTime:yyyyMMddHHmmss}.jpg";
            var blob = blobContainer.GetBlockBlobReference(blobName);
            await blob.UploadFromByteArrayAsync(picture, 0, picture.Length);

            log.Info($"Blob�ۑ����� {blobName}");
        }
    }
}
