// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

using BootstrapBlazor.Components;
using BootstrapBlazor.Shared.Common;
using BootstrapBlazor.Shared.Pages.Components;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BootstrapBlazor.Shared.Pages
{
    /// <summary>
    ///
    /// </summary>
    public sealed partial class Geolocations
    {
 
        private string? status { get; set; }
        private Geolocationitem? geolocations { get; set; }

        private Task OnResult(Geolocationitem geolocations)
        {
            this.geolocations = geolocations;
            StateHasChanged();
            return Task.CompletedTask;
        }

        private Task OnUpdateStatus(string status)
        {
            this.status = status;
            StateHasChanged();
            return Task.CompletedTask;
        }


        /// <summary>
        /// 获得属性方法
        /// </summary>
        /// <returns></returns>
        private IEnumerable<AttributeItem> GetAttributes() => new AttributeItem[]
        {
            // TODO: 移动到数据库中
            new AttributeItem() {
                Name = "ShowLabel",
                Description = "是否显示前置标签",
                Type = "bool",
                ValueList = "true|false",
                DefaultValue = "true"
            }
        };
    }
}
