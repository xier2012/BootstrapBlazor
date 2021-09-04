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
    /// ComingSoonPage 组件
    /// </summary>
    public partial class ComingSoonPage
    {
        /// <summary>
        /// 获得/设置 IJSRuntime 实例
        /// </summary>
        [Inject]
        [NotNull]
        protected IJSRuntime? JSRuntime { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Inject]
        [NotNull]
        private IStringLocalizer<ComingSoonPage>? Localizer { get; set; }

        /// <summary>
        /// 获得/设置 无匹配数据时显示提示信息 默认提示"无匹配数据"
        /// </summary>
        [Parameter]
        [NotNull]
        public string? NoDataTip { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected ElementReference ComingSoonPageElement { get; set; }

        /// <summary>
        /// OnInitialized 方法
        /// </summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            NoDataTip ??= Localizer[nameof(NoDataTip)];
        }

        /// <summary>
        /// firstRender
        /// </summary>
        /// <param name="firstRender"></param>
        /// <returns></returns>
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);

            if (!firstRender)
            {
                await JSRuntime.InvokeVoidAsync(ComingSoonPageElement, "bb_comingsoonpage");
            }
        }
         
    }
}
