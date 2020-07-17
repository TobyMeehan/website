﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TobyMeehan.Com.Data.Models;
using TobyMeehan.Sql;

namespace TobyMeehan.Com.Data.Repositories
{
    public class SqlCommentRepository : ICommentRepository
    {
        private readonly ISqlTable<Comment> _sqlTable;

        public SqlCommentRepository(ISqlTable<Comment> sqlTable)
        {
            _sqlTable = sqlTable;
        }

        public Task AddAsync(string entityId, string userId, string content)
        {
            string id = Guid.NewGuid().ToString();

            return _sqlTable.InsertAsync(new
            {
                Id = id,
                EntityId = entityId,
                UserId = userId,
                Content = content
            });
        }

        public async Task<IList<Comment>> GetByEntityAsync(string entityId)
        {
            return (await _sqlTable.SelectByAsync(c => c.EntityId == entityId)).ToList();
        }

        public async Task<Comment> GetByIdAsync(string id)
        {
            return (await _sqlTable.SelectByAsync(c => c.Id == id)).Single();
        }
    }
}