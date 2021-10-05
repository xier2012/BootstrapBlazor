﻿// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

using System.Data;

namespace BootstrapBlazor.Components
{
    /// <summary>
    /// DataTable 动态类实例
    /// </summary>
    public class DataTableDynamicObject : DynamicObject
    {
        /// <summary>
        /// 获得/设置 DataRow 实例
        /// </summary>
        internal DataRow? Row { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public override object? GetValue(string propertyName)
        {
            object? ret = null;
            if (Row != null && Row.RowState != DataRowState.Deleted && Row.RowState != DataRowState.Detached && Row.Table.Columns.Contains(propertyName))
            {
                ret = Row[propertyName];
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public override void SetValue(string propertyName, object? value)
        {
            if (Row != null && Row.Table.Columns.Contains(propertyName))
            {
                Row[propertyName] = value;
            }
        }
    }
}
