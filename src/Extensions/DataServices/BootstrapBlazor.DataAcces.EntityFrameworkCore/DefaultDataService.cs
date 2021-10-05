﻿// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

using BootstrapBlazor.Components;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BootstrapBlazor.DataAcces.EntityFrameworkCore
{
    /// <summary>
    /// Entity Framework ORM 的 IDataService 接口实现
    /// </summary>
    internal class DefaultDataService<TModel> : DataServiceBase<TModel>, IEntityFrameworkCoreDataService where TModel : class, new()
    {
        private readonly DbContext _db;
        private TModel? Model { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public DefaultDataService(Func<IEntityFrameworkCoreDataService, DbContext> dbContextResolve)
        {
            _db = dbContextResolve(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public override Task<bool> AddAsync(TModel model)
        {
            Model = model;
            return base.AddAsync(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task CancelAsync()
        {
            if (Model != null)
            {
                if (_db.Entry(Model).IsKeySet)
                {
                    _db.Entry(Model).State = EntityState.Unchanged;
                }
                else
                {
                    _db.Entry(Model).State = EntityState.Detached;
                }
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task EditAsync(object model)
        {
            Model = model as TModel;
            return Task.CompletedTask;
        }

        /// <summary>
        /// 删除方法
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public override async Task<bool> DeleteAsync(IEnumerable<TModel> models)
        {
            _db.RemoveRange(models);
            await _db.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 保存方法
        /// </summary>
        /// <param name="model"></param>
        /// <param name="changedType"></param>
        /// <returns></returns>
        public override async Task<bool> SaveAsync(TModel model, ItemChangedType changedType)
        {
            if (_db.Entry(model).IsKeySet)
            {
                _db.Update(model);
            }
            else
            {
                await _db.AddAsync(model);
            }

            await _db.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 查询方法
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public override Task<QueryData<TModel>> QueryAsync(QueryPageOptions option)
        {
            // 处理过滤与快捷搜索框逻辑
            var query = _db.Set<TModel>()
                .Where(option.Searchs.GetFilterLambda<TModel>(FilterLogic.Or), option.Searchs.Any())
                .Where(option.Filters.GetFilterLambda<TModel>(), option.Filters.Any())
                .Sort(option.SortName!, option.SortOrder, !string.IsNullOrEmpty(option.SortName))
                .Count(out var count)
                .Page((option.PageIndex - 1) * option.PageItems, option.PageItems);

            // 注意：未处理搜索，此处设置 IsSearched=true 后会导致高级搜索按钮高亮
            var ret = new QueryData<TModel>()
            {
                TotalCount = count,
                Items = query,
                IsSorted = true,
                IsFiltered = true
            };
            return Task.FromResult(ret);
        }
    }
}
