﻿// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace BootstrapBlazor.Components
{
    /// <summary>
    /// AutoComplete 组件基类
    /// </summary>
    public partial class AutoComplete
    {
        private bool _isLoading;
        private bool _isShown;
        private string? _lastFilterText;

        /// <summary>
        /// 获得 组件样式
        /// </summary>
        protected virtual string? ClassString => CssBuilder.Default("auto-complete")
            .AddClass("is-loading", _isLoading)
            .AddClass("is-complete", _isShown)
            .Build();

        /// <summary>
        /// 获得 最终候选数据源
        /// </summary>
        [NotNull]
        protected List<string>? FilterItems { get; private set; }

        /// <summary>
        /// 获得/设置 通过输入字符串获得匹配数据集合
        /// </summary>
        [Parameter]
        [NotNull]
        public IEnumerable<string>? Items { get; set; }

        /// <summary>
        /// 获得/设置 无匹配数据时显示提示信息 默认提示"无匹配数据"
        /// </summary>
        [Parameter]
        [NotNull]
        public string? NoDataTip { get; set; }

        /// <summary>
        /// 获得/设置 匹配数据时显示的数量
        /// </summary>
        [Parameter]
        [NotNull]
        public int? DisplayCount { get; set; }

        /// <summary>
        /// 防抖时间间隔单位毫秒 默认为 0 关闭防抖
        /// </summary>
        [Parameter]
        public int DebounceInterval { get; set; } = 0;

        /// <summary>
        /// 获得/设置 是否开启模糊查询，默认为 false
        /// </summary>
        [Parameter]
        public bool IsLikeMatch { get; set; } = false;

        /// <summary>
        /// 获得/设置 匹配时是否忽略大小写，默认为 true
        /// </summary>
        [Parameter]
        public bool IgnoreCase { get; set; } = true;

        /// <summary>
        /// 获得/设置 自定义集合过滤规则
        /// </summary>
        [Parameter]
        public Func<Task<IEnumerable<string>>>? CustomFilter { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Inject]
        [NotNull]
        private IStringLocalizer<AutoComplete>? Localizer { get; set; }

        private string _selectedItem = "";

        /// <summary>
        /// 
        /// </summary>
        protected ElementReference AutoCompleteElement { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected int? CurrentItemIndex { get; set; }

        /// <summary>
        /// OnInitialized 方法
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            NoDataTip ??= Localizer[nameof(NoDataTip)];
            PlaceHolder ??= Localizer[nameof(PlaceHolder)];
            Items ??= Enumerable.Empty<string>();
            FilterItems ??= new List<string>();
        }

        /// <summary>
        /// firstRender
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (firstRender)
            {
                if (DebounceInterval > 0)
                {
                    await JSRuntime.InvokeVoidAsync(AutoCompleteElement, "bb_debounce", DebounceInterval);
                }
            }

            if (CurrentItemIndex.HasValue)
            {
                await JSRuntime.InvokeVoidAsync(AutoCompleteElement, "bb_autoScrollItem", CurrentItemIndex.Value);
            }
        }

        /// <summary>
        /// OnBlur 方法
        /// </summary>
        protected void OnBlur()
        {
            _selectedItem = "";
            _isShown = false;
        }

        /// <summary>
        /// 鼠标点击候选项时回调此方法
        /// </summary>
        protected virtual Task OnClickItem(string val)
        {
            CurrentValue = val;
            return Task.CompletedTask;
        }

        /// <summary>
        /// 获得/设置 是否跳过 Enter 按键处理 默认 false
        /// </summary>
        protected bool SkipEnter { get; set; }

        /// <summary>
        /// 获得/设置 是否跳过 Esc 按键处理 默认 false
        /// </summary>
        protected bool SkipEsc { get; set; }

        /// <summary>
        /// OnKeyUp 方法
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual async Task OnKeyUp(KeyboardEventArgs args)
        {
            if (!_isLoading && _lastFilterText != CurrentValueAsString)
            {
                _isLoading = true;
                _lastFilterText = CurrentValueAsString;
                if (CustomFilter != null)
                {
                    var items = await CustomFilter();
                    FilterItems = items.ToList();
                }
                else
                {
                    var comparison = IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
                    var items = IsLikeMatch ?
                        Items.Where(s => s.Contains(CurrentValueAsString, comparison)) :
                        Items.Where(s => s.StartsWith(CurrentValueAsString, comparison));
                    FilterItems = DisplayCount == null ? items.ToList() : items.Take((int)DisplayCount).ToList();
                }
                _isLoading = false;
            }

            var source = FilterItems;
            if (source.Any())
            {
                _isShown = true;

                // 键盘向上选择
                if (_isShown && args.Key == "ArrowUp")
                {
                    var index = source.IndexOf(_selectedItem) - 1;
                    if (index < 0)
                    {
                        index = source.Count - 1;
                    }
                    _selectedItem = source[index];
                    CurrentItemIndex = index;
                }
                else if (_isShown && args.Key == "ArrowDown")
                {
                    var index = source.IndexOf(_selectedItem) + 1;
                    if (index > source.Count - 1)
                    {
                        index = 0;
                    }
                    _selectedItem = source[index];
                    CurrentItemIndex = index;
                }
                else if (args.Key == "Escape")
                {
                    OnBlur();
                    if (!SkipEsc && OnEscAsync != null)
                    {
                        await OnEscAsync(Value);
                    }
                }
                else if (args.Key == "Enter")
                {
                    if (!string.IsNullOrEmpty(_selectedItem))
                    {
                        CurrentValueAsString = _selectedItem;
                        OnBlur();
                        if (!SkipEnter && OnEnterAsync != null)
                        {
                            await OnEnterAsync(Value);
                        }
                    }
                }
            }
        }
    }
}
