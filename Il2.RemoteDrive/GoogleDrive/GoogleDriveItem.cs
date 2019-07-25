﻿using System;

namespace Il2.RemoteDrive.GoogleDrive
{
    public class GoogleDriveItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsFolder => MimeType == "application/vnd.google-apps.folder";
        public string MimeType { get; set; }
        public string DownloadLink { get; set; }
        public string BrowserViewLink { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? ModifiedTime { get; set; }

        public GoogleDriveItem(string id)
        {
            Id = id;
        }

        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Name)}: {Name}, {nameof(IsFolder)}: {IsFolder}, {nameof(MimeType)}: {MimeType}, {nameof(CreatedTime)}: {CreatedTime}, {nameof(ModifiedTime)}: {ModifiedTime}";
        }
    }
}