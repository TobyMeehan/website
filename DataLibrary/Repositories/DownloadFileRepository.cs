﻿using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TobyMeehan.Com.Data.CloudStorage;
using TobyMeehan.Com.Data.Models;
using TobyMeehan.Com.Data.Upload;
using TobyMeehan.Sql;

namespace TobyMeehan.Com.Data.Repositories
{
    public class DownloadFileRepository : IDownloadFileRepository
    {
        private readonly ISqlTable<DownloadFile> _table;
        private readonly ICloudStorage _storage;
        private readonly IConfiguration _configuration;

        public DownloadFileRepository(ISqlTable<DownloadFile> table, ICloudStorage storage, IConfiguration configuration)
        {
            _table = table;
            _storage = storage;
            _configuration = configuration;
        }

        public async Task<DownloadFile> AddAsync(string downloadId, string filename, Stream uploadStream, CancellationToken cancellationToken = default, IProgress<IUploadProgress> progress = null)
        {
            string bucket = _configuration.GetSection("DownloadStorageBucket").Value;

            string id = Guid.NewGuid().ToString();

            string url = await _storage.UploadFileAsync(uploadStream, bucket, id, cancellationToken, progress);

            await _table.InsertAsync(new
            {
                Id = id,
                DownloadID = downloadId,
                Filename = filename,
                Url = url
            });

            return (await _table.SelectByAsync(f => f.Id == id)).SingleOrDefault();
        }

        public async Task DeleteAsync(string id)
        {
            string bucket = _configuration.GetSection("DownloadStorageBucket").Value;

            await _storage.DeleteFileAsync(bucket, id);

            await _table.DeleteAsync(f => f.Id == id);
        }

        public async Task<IList<DownloadFile>> GetAsync()
        {
            return (await _table.SelectAsync()).ToList();
        }

        public async Task<IList<DownloadFile>> GetByDownloadAsync(string downloadId)
        {
            return (await _table.SelectByAsync(f => f.DownloadId == downloadId)).ToList();
        }

        public async Task<IList<DownloadFile>> GetByFilenameAsync(string filename)
        {
            return (await _table.SelectByAsync(f => f.Filename == filename)).ToList();
        }
    }
}
