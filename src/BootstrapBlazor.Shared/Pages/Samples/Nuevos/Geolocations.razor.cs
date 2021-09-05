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
    /// Geolocation 地理定位/移动距离追踪
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
            new AttributeItem()
            {
                Name = "ButtonGetLocationText",
                Description = "获取位置按钮文字",
                Type = "string",
                ValueList = " - ",
                DefaultValue = "获取位置"
            },
            new AttributeItem()
            {
                Name = "ButtonWatchPositionText",
                Description = "获取移动距离追踪按钮文字",
                Type = "string",
                ValueList = " - ",
                DefaultValue = "移动距离追踪"
            },
            new AttributeItem() {
                Name = "ShowButtons",
                Description = "是否显示默认按钮界面",
                Type = "bool",
                ValueList = "true|false",
                DefaultValue = "true"
            },
            new AttributeItem()
            {
                Name = "OnResult",
                Description = "定位完成回调方法",
                Type = "Func<Geolocationitem, Task>",
                ValueList = " - ",
                DefaultValue = " - "
            }, 
            new AttributeItem()
            {
                Name = "OnUpdateStatus",
                Description = "状态更新回调方法",
                Type = "Func<string, Task>",
                ValueList = " - ",
                DefaultValue = " - "
            },
        };
    }
}
