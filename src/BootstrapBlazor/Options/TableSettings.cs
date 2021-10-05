﻿// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

using Microsoft.AspNetCore.Components;

namespace BootstrapBlazor.Components
{
    /// <summary>
    /// 
    /// </summary>
    public class TableSettings
    {
        /// <summary>
        /// 获得/设置 复选框宽度 默认 36
        /// </summary>
        public int CheckboxColumnWidth { get; set; } = 36;

        /// <summary>
        /// 获得/设置 明细行 Row Header 宽度 默认 24
        /// </summary>
        public int DetailColumnWidth { get; set; } = 24;

        /// <summary>
        /// 获得/设置 显示文字的复选框列宽度 默认 80
        /// </summary>
        public int ShowCheckboxTextColumnWidth { get; set; } = 80;

        /// <summary>
        /// 获得/设置 行号列宽度 默认 60
        /// </summary>
        public int LineNoColumnWidth { get; set; } = 60;
    }
}
