// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BootstrapBlazor.Components
{

    /// <summary>
    /// 定位数据类
    /// </summary>
    public class Geolocations
    {
        /// <summary>
        /// 纬度
        /// </summary>
        /// <returns></returns>
        public decimal Latitude { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        /// <returns></returns>
        public decimal Logitude { get; set; }

        /// <summary>
        /// 准确度(米)<para></para>
        /// 将以m指定维度和经度值与实际位置的差距，置信度为95%.
        /// </summary>
        public int Accuracy { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public TimeSpan Timestamp { get; set; }
    }
}
