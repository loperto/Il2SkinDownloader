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

namespace IL2SkinDownloader.Core.GoogleDrive
{
    public static class GoogleDriveHelpers
    {
        public static GoogleDriveItem Convert(this File file)
        {
            return new GoogleDriveItem(file.Id)
            {
                Name = file.Name,
                BrowserViewLink = file.WebViewLink,
                DownloadLink = file.WebContentLink,
                ModifiedTime = file.ModifiedTime,
                CreatedTime = file.CreatedTime.Value,
                Parents = file.Parents,
                Size = file.Size ?? 0,
            };
        }
    }
    public class GoogleDriveWrapper
    {
        private DriveService _service;
        private IEnumerable<string> Scopes { get; } = new List<string> { DriveService.Scope.Drive };

        public void Connect(string jsonPath, string appName)
        {
            var json = System.IO.File.ReadAllText(jsonPath);
            var credentials = GoogleCredential.FromJson(json).CreateScoped(Scopes);
            CreateDrive(credentials, appName);
        }

        public async Task Connect(string idClient, string secret, string appName, CancellationToken cancellationToken)
        {
            var credentials = await GoogleWebAuthorizationBroker.AuthorizeAsync(new ClientSecrets
            {
                ClientId = idClient,
                ClientSecret = secret
            }, Scopes, appName, cancellationToken, new FileDataStore("credentials.json"));

            CreateDrive(credentials, appName);
        }

        private void CreateDrive(IConfigurableHttpClientInitializer credentials, string appName)
        {
            _service = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials,
                ApplicationName = appName,
            });
        }

        public async Task GetInfos()
        {
            var request = _service.About.Get();
            request.Fields = "user,storageQuota";
            var test = await request.ExecuteAsync();
            Console.WriteLine($"Limit:{test.StorageQuota.Limit} Usage:{test.StorageQuota.Usage} UsageInDrive:{test.StorageQuota.UsageInDrive} Trash:{test.StorageQuota.UsageInDriveTrash} {test.User.DisplayName}");
        }

        public async Task<GoogleDriveItem> UploadAsync(string filePath, string folderId = null)
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
                uploadRequest = _service.Files.Create(fileMetadata, stream, MimeTypeMap.GetMimeType(fileInfo.Extension));
                uploadRequest.Fields = "id,name,webContentLink,modifiedTime,createdTime,parents,size";
                await uploadRequest.UploadAsync();
            }

            return uploadRequest.ResponseBody.Convert();
        }



        public async Task<GoogleDriveDirectory> CreateFolderAsync(string folderName, string parentId = null)
        {
            var fileMetadata = new File
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder",
                Parents = !string.IsNullOrWhiteSpace(parentId) ? new List<string> { parentId } : null,
            };
            var request = _service.Files.Create(fileMetadata);
            request.Fields = "id,name";
            var file = await request.ExecuteAsync();
            return new GoogleDriveDirectory
            {
                Id = file.Id,
                Name = file.Name,
            };
        }

        public async Task MoveFileAsync(string fileId, string folderId)
        {
            var getRequest = _service.Files.Get(fileId);
            getRequest.Fields = "parents";
            var file = getRequest.Execute();
            var previousParents = string.Join(",", file.Parents);
            var updateRequest = _service.Files.Update(new File(), fileId);
            updateRequest.Fields = "id, parents";
            updateRequest.AddParents = folderId;
            updateRequest.RemoveParents = previousParents;
            await updateRequest.ExecuteAsync();
        }

        public async Task DownloadAsync(string fileId, string downloadPath, Action<long> onProgress = null, Action onCompleted = null)
        {
            var request = _service.Files.Get(fileId);
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
                                    onProgress?.Invoke(progress.BytesDownloaded);
                                    break;
                                }
                            case DownloadStatus.Completed:
                                {
                                    Console.WriteLine("DownloadAsync complete.");
                                    onCompleted?.Invoke();
                                    break;
                                }
                            case DownloadStatus.Failed:
                                {
                                    Console.WriteLine("DownloadAsync failed.");
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


        public async Task<IList<Permission>> GetFilePermissions(string fileId)
        {
            var permissionRequest = _service.Permissions.List(fileId);
            permissionRequest.Fields = "permissions(emailAddress,id,kind,role,type)";
            var list = await permissionRequest.ExecuteAsync();
            return list.Permissions;
        }
        public async Task<IReadOnlyList<GoogleDriveItem>> GetFilesAsync(CancellationToken token)
        {
            var listRequest = _service.Files.List();
            listRequest.PageSize = 20;
            listRequest.Q = "trashed=false and mimeType != 'application/vnd.google-apps.folder'";
            listRequest.Fields = "nextPageToken, files(id,name,webContentLink,modifiedTime,createdTime,parents,size)";
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
                    result.Add(file.Convert());
                }
            } while (pageToken != null);

            return result;
        }

        public async Task<GoogleDriveItem> GetFile(string fileId)
        {
            var listRequest = _service.Files.Get(fileId);
            listRequest.Fields = "files(id,name,webContentLink,modifiedTime,createdTime,parents,size)";
            var file = await listRequest.ExecuteAsync();
            return file.Convert();
        }

        public async Task<IReadOnlyList<GoogleDriveItem>> GetFilesInDirectoryAsync(string directory, CancellationToken token)
        {
            var listRequest = _service.Files.List();
            listRequest.PageSize = 20;
            listRequest.Q = $"trashed=false and mimeType != 'application/vnd.google-apps.folder' and '{directory}' in parents";
            listRequest.Fields = "nextPageToken, files(id, name,webContentLink,modifiedTime,createdTime,parents,size)";
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
                    result.Add(file.Convert());
                }
            } while (pageToken != null);

            return result;
        }

        public async Task<IReadOnlyList<GoogleDriveDirectory>> GetFoldersAsync()
        {
            var listRequest = _service.Files.List();
            listRequest.PageSize = 20;
            listRequest.Q = "trashed=false and mimeType = 'application/vnd.google-apps.folder'";
            listRequest.Fields = "nextPageToken, files(id, name)";
            string pageToken = null;
            var result = new List<GoogleDriveDirectory>();
            do
            {
                listRequest.PageToken = pageToken;
                var driveData = await listRequest.ExecuteAsync();
                pageToken = driveData.NextPageToken;
                var files = driveData.Files;
                if (files == null || files.Count <= 0) continue;
                foreach (var file in files)
                {
                    result.Add(new GoogleDriveDirectory
                    {
                        Id = file.Id,
                        Name = file.Name,
                    });
                }
            } while (pageToken != null);

            return result;
        }

        public async Task ShareAsync(string itemId, string userEmail)
        {
            var batch = new BatchRequest(_service);

            var userPermission = new Permission
            {
                Type = "user",
                Role = "reader",
                EmailAddress = userEmail,
            };

            var request = _service.Permissions.Create(userPermission, itemId);
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


        public async Task DeleteAsync(string itemId)
        {
            var request = _service.Files.Delete(itemId);
            request.Fields = "id";
            await request.ExecuteAsync();
        }
    }
}
