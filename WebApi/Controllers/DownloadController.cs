﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DataAccessLibrary.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DownloadController : AuthenticatedControllerBase
    {
        private readonly IUserProcessor _userProcessor;
        private readonly IDownloadProcessor _downloadProcessor;
        private readonly IMapper _mapper;

        public DownloadController(IUserProcessor userProcessor, IDownloadProcessor downloadProcessor, IMapper mapper) : base (userProcessor, mapper)
        {
            _userProcessor = userProcessor;
            _downloadProcessor = downloadProcessor;
            _mapper = mapper;
        }

        [Route("/{id}")]
        public async Task<ActionResult<Download>> Get(string id)
        {
            Download download = _mapper.Map<Download>(await _downloadProcessor.GetDownloadById(id));

            if (download == null)
            {
                return NotFound();
            }
            
            if (!download.Authors.Any(u => u.Id == UserId))
            {
                return Forbid();
            }

            return download;
        }

        public async Task<ActionResult<Download>> Post(Download download)
        {
            download.Authors.Clear();
            download.Authors.Add(await GetUser());
            download.Updated = DateTime.Now;

            return _mapper.Map<Download>(await _downloadProcessor.CreateDownload(_mapper.Map<DataAccessLibrary.Models.Download>(download)));
        }

        public async Task<IActionResult> Put(Download download)
        {
            if (!await _downloadProcessor.IsAuthor(download.Id, UserId))
            {
                return Forbid();
            }

            await _downloadProcessor.UpdateDownload(_mapper.Map<DataAccessLibrary.Models.Download>(download));

            return Ok();
        }

        [Route("/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            if (!await _downloadProcessor.IsAuthor(id, UserId))
            {
                return Forbid();
            }

            await _downloadProcessor.DeleteDownload(id);

            return Ok();
        }
    }
}