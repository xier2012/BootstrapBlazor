﻿// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

using BootstrapBlazor.Components;
using BootstrapBlazor.Shared.Pages.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace BootstrapBlazor.Shared.Pages.Table
{
    /// <summary>
    /// 
    /// </summary>
    partial class TablesExcel
    {
        [Inject]
        [NotNull]
        private IStringLocalizer<Foo>? Localizer { get; set; }

        [Inject]
        [NotNull]
        private IStringLocalizer<Tables>? TablesLocalizer { get; set; }

        [NotNull]
        private BlockLogger? Trace { get; set; }

        /// <summary>
        /// OnInitialized 方法
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            Items = Foo.GenerateFoo(Localizer);
        }

        // 绑定数据源代码
        private static IEnumerable<int> PageItemsSource => new int[] { 10, 20, 40 };

        [NotNull]
        private List<Foo>? Items { get; set; }

        private static readonly ConcurrentDictionary<Type, Func<IEnumerable<Foo>, string, SortOrder, IEnumerable<Foo>>> SortLambdaCache = new();

        private Task<QueryData<Foo>> OnQueryAsync(QueryPageOptions options)
        {
            IEnumerable<Foo> items = Items;

            // 过滤
            var isFiltered = false;
            if (options.Filters.Any())
            {
                items = items.Where(options.Filters.GetFilterFunc<Foo>());
                isFiltered = true;
            }

            // 排序
            var isSorted = false;
            if (!string.IsNullOrEmpty(options.SortName))
            {
                var invoker = SortLambdaCache.GetOrAdd(typeof(Foo), key => LambdaExtensions.GetSortLambda<Foo>().Compile());
                items = invoker(items, options.SortName, options.SortOrder);
                isSorted = true;
            }

            // 设置记录总数
            var total = items.Count();

            // 内存分页
            items = items.Skip((options.PageIndex - 1) * options.PageItems).Take(options.PageItems).ToList();

            return Task.FromResult(new QueryData<Foo>()
            {
                Items = items,
                TotalCount = total,
                IsSorted = isSorted,
                IsFiltered = isFiltered,
                IsSearch = true
            });
        }

        private Task<Foo> OnAddAsync()
        {
            // Excel 模式下新建要求更改数据源
            var foo = new Foo() { DateTime = DateTime.Now, Address = $"自定义地址  {DateTime.Now.Second}" };
            Items.Insert(0, foo);

            // 输出日志信息
            Trace.Log($"集合值变化通知 列: {Items.Count} - 类型: Add");
            return Task.FromResult(foo);
        }

        private Task<bool> OnSaveAsync(Foo item, ItemChangedType changedType)
        {
            // 对象已经更新
            // 输出日志信息
            Trace.Log($"单元格变化通知 类: Foo - 值: 单元格");
            return Task.FromResult(true);
        }

        private Task<bool> OnDeleteAsync(IEnumerable<Foo> items)
        {
            Items.RemoveAll(i => items.Contains(i));

            // 输出日志信息
            Trace.Log($"集合值变化通知 列: {Items.Count} - 类型: Delete");
            return Task.FromResult(true);
        }

        private ConcurrentDictionary<int, string> TitleCache { get; } = new();

        private string GetTitle(int id) => TitleCache.GetOrAdd(id, key => Foo.GetTitle());
    }
}
