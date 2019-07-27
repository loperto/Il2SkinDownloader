using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Download;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Http;
using Google.Apis.Requests;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using File = Google.Apis.Drive.v3.Data.File;

namespace Il2.RemoteDrive.GoogleDrive
{
    public class GoogleDrive
    {
        private readonly string appName;
        private DriveService service;
        private IEnumerable<string> Scopes { get; } = new List<string> { DriveService.Scope.Drive };


        public GoogleDrive(string appName)
        {
            this.appName = appName;
        }

        public void Connect(string jsonPath)
        {
            var json = System.IO.File.ReadAllText(jsonPath);
            var credentials = GoogleCredential.FromJson(json).CreateScoped(Scopes);
            CreateDrive(credentials);
        }

        public async Task Connect(string idClient, string secret, CancellationToken cancellationToken)
        {
            var credentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = idClient,
                ClientSecret = secret
            }, Scopes, appName, cancellationToken, new FileDataStore("credentials.json"));

            CreateDrive(credentials);
        }

        private void CreateDrive(IConfigurableHttpClientInitializer credentials)
        {
            service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials,
                ApplicationName = appName,
            });
        }

        public async Task Upload(string filePath, string folderId = null)
        {
            var fileInfo = new FileInfo(filePath);
            var fileMetadata = new File
            {
                Name = fileInfo.Name,
                Parents = !string.IsNullOrWhiteSpace(folderId) ? new List<string>
                {
                    folderId,
                } : null,
            };
            FilesResource.CreateMediaUpload uploadRequest;
            using (var stream = new FileStream(fileInfo.FullName, FileMode.Open))
            {
                uploadRequest = service.Files.Create(fileMetadata, stream, MimeTypeMap.GetMimeType(fileInfo.Extension));
                uploadRequest.Fields = "id";
                await uploadRequest.UploadAsync();
            }
            var file = uploadRequest.ResponseBody;
            Console.WriteLine("File ID: " + file.Id);
        }

        public async Task CreateFolder(string folderName, string parentId = null)
        {
            var fileMetadata = new File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = !string.IsNullOrWhiteSpace(parentId) ? new List<string> { parentId } : null,
            };
            var request = service.Files.Create(fileMetadata);
            request.Fields = "id";
            await request.ExecuteAsync();
        }

        public async Task MoveFile(string fileId, string folderId)
        {
            var getRequest = service.Files.Get(fileId);
            getRequest.Fields = "parents";
            var file = getRequest.Execute();
            var previousParents = string.Join(",", file.Parents);
            var updateRequest = service.Files.Update(new File(), fileId);
            updateRequest.Fields = "id, parents";
            updateRequest.AddParents = folderId;
            updateRequest.RemoveParents = previousParents;
            await updateRequest.ExecuteAsync();
        }

        public async Task Download(string fileId, string downloadPath)
        {
            var request = service.Files.Get(fileId);
            using (var stream = new FileStream(downloadPath, FileMode.Create))
            {
                request.MediaDownloader.ProgressChanged +=
                    progress =>
                    {
                        switch (progress.Status)
                        {
                            case DownloadStatus.Downloading:
                                {
                                    Console.WriteLine(progress.BytesDownloaded);
                                    break;
                                }
                            case DownloadStatus.Completed:
                                {
                                    Console.WriteLine("Download complete.");
                                    break;
                                }
                            case DownloadStatus.Failed:
                                {
                                    Console.WriteLine("Download failed.");
                                    break;
                                }
                            case DownloadStatus.NotStarted:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    };

                await request.DownloadAsync(stream);
            }
        }

        public async Task<IReadOnlyList<GoogleDriveItem>> GetFiles(CancellationToken token)
        {
            var listRequest = service.Files.List();
            listRequest.PageSize = 20;
            listRequest.Q = "trashed=false";
            listRequest.Fields = "nextPageToken, files(id, name,mimeType,webContentLink,webViewLink,modifiedTime,createdTime)";
            string pageToken = null;
            var result = new List<GoogleDriveItem>();
            do
            {
                listRequest.PageToken = pageToken;
                var driveData = await listRequest.ExecuteAsync(token);
                pageToken = driveData.NextPageToken;
                var files = driveData.Files;
                if (files == null || files.Count <= 0) continue;
                foreach (var file in files)
                {
                    result.Add(new GoogleDriveItem(file.Id)
                    {
                        Name = file.Name,
                        MimeType = file.MimeType,
                        BrowserViewLink = file.WebViewLink,
                        DownloadLink = file.WebContentLink,
                        ModifiedTime = file.ModifiedTime,
                        CreatedTime = file.CreatedTime,
                    });
                }
            } while (pageToken != null);

            return result;
        }

        public async Task Share(string itemId, string userEmail)
        {
            var batch = new BatchRequest(service);

            var userPermission = new Permission
            {
                Type = "user",
                Role = "writer",
                EmailAddress = userEmail,
            };

            var request = service.Permissions.Create(userPermission, itemId);
            request.Fields = "id";
            batch.Queue(request, (BatchRequest.OnResponse<Permission>)((permission, error, index, message) =>
            {
                if (error != null)
                {
                    Console.WriteLine(error.Message);
                }
                else
                {
                    Console.WriteLine("Permission ID: " + permission.Id);
                }
            }));
            await batch.ExecuteAsync();
        }
    }
}
