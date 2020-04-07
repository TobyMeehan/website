﻿using AutoMapper;
using BlazorInputFile;
using BlazorUI.Models;
using DataAccessLibrary.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorUI.Pages.Downloads
{
    public partial class Files : ComponentBase
    {
        [Inject] private IConfiguration configuration { get; set; }
        [Inject] private IDownloadProcessor downloadProcessor { get; set; }
        [Inject] private IMapper mapper { get; set; }
        [Inject] private AuthenticationStateProvider authenticationStateProvider { get; set; }
        [Inject] private IAuthorizationService authorizationService { get; set; }
        [Inject] private NavigationManager navigationManager { get; set; }
        [Inject] private EditDownloadState editDownloadState { get; set; }
        [Inject] private FileUploadState uploadState { get; set; }
        [Inject] private AlertState alertState { get; set; }

        [Parameter] public string Id { get; set; }

        private Download _download;
        private string _downloadHost;
        private AuthenticationState _context;

        private Alert _accessDeniedAlert = new Alert
        {
            Title = null,
            Body = "You do not have edit access for this download.",
            Context = AlertContext.Danger
        };

        protected override async Task OnInitializedAsync()
        {
            _context = await authenticationStateProvider.GetAuthenticationStateAsync();
            _download = await Task.Run(async () => mapper.Map<Download>(await downloadProcessor.GetDownloadById(Id)));
            _downloadHost = configuration.GetSection("DownloadHost").Value;

            editDownloadState.Title = _download.Title;
            editDownloadState.Id = _download.Id;

            uploadState.OnUploadComplete += async (filename) => await InvokeAsync(async () =>
            {
                _download.Files.Add(filename);
            });
        }

        private async Task FileUpload_Change(IFileListEntry[] files)
        {
            if ((await authorizationService.AuthorizeAsync(_context.User, _download, Authorization.Policies.EditDownload)).Succeeded)
            {
                var file = files.FirstOrDefault();

                await uploadState.UploadFile(file.Name, UploadFile(file));
            }
            else
            {
                alertState.Queue.Add(_accessDeniedAlert);

                navigationManager.NavigateTo($"/downloads/{Id}");
            }
        }

        private async Task<bool> UploadFile(IFileListEntry file)
        {
            const int maxSize = 209715200; // 200MB

            var ms = new MemoryStream();
            await file.Data.CopyToAsync(ms);

            //if (ms.Length > maxSize)
            //    return false;

            bool result = await downloadProcessor.TryAddFile(new DataAccessLibrary.Models.DownloadFileModel
            {
                DownloadId = _download.Id,
                Filename = file.Name
            }, ms);

            return result;
        }

        private async Task DeleteFile_Click(string filename)
        {
            if ((await authorizationService.AuthorizeAsync(_context.User, _download, Authorization.Policies.EditDownload)).Succeeded)
            {
                await downloadProcessor.DeleteFile(new DataAccessLibrary.Models.DownloadFileModel { DownloadId = _download.Id, Filename = filename });
                _download.Files.Remove(filename);
            }
            else
            {
                alertState.Queue.Add(_accessDeniedAlert);

                navigationManager.NavigateTo($"/downloads/{Id}");
            }
        }
    }
}
