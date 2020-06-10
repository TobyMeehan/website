﻿using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TobyMeehan.Com.Data.Upload;

namespace TobyMeehan.Com.Data.CloudStorage
{
    public interface ICloudStorage
    {
        Task DeleteFileAsync(string bucket, string filename);
        Task<string> UploadFileAsync(Stream stream, string bucket, string objectName, string filename, CancellationToken cancellationToken = default, IProgress<IUploadProgress> progress = null);
        Task DownloadFileAsync(string bucket, string filename, Stream destination);
    }
}