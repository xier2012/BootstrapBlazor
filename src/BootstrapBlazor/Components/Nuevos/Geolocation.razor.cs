// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace BootstrapBlazor.Components
{
    /// <summary>
    /// Geolocation 组件基类
    /// </summary>
    public partial class Geolocation
    {
        /// <summary>
        /// 获得/设置 IJSRuntime 实例
        /// </summary>
        [Inject]
        [NotNull]
        protected IJSRuntime? JSRuntime { get; set; }

        /// <summary>
        /// 获得/设置 定位
        /// </summary>
        [Parameter]
        [NotNull]
        public string? GeolocationInfo { get; set; }

        /// <summary>
        /// 获得/设置 无匹配数据时显示提示信息 默认提示"无匹配数据"
        /// </summary>
        [Parameter]
        [NotNull]
        public string? NoDataTip { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Inject]
        [NotNull]
        private IStringLocalizer<Geolocation>? Localizer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected ElementReference GeolocationElement { get; set; }

        /// <summary>
        /// OnInitialized 方法
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            NoDataTip ??= Localizer[nameof(NoDataTip)];
        }

        /// <summary>
        /// 获取定位
        /// </summary>
        protected virtual async Task GetLocation()
        {
            await JSRuntime.InvokeVoidAsync(GeolocationElement, "bb_getLocation");
        }

        /// <summary>
        /// 持续定位
        /// </summary>
        protected virtual async Task WatchPosition()
        {
            await JSRuntime.InvokeVoidAsync(GeolocationElement, "bb_getLocation",null,true);
        }

    }
}
