﻿// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

using BootstrapBlazor.Localization.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace BootstrapBlazor.Components
{
    /// <summary>
    /// ValidateForm 组件类
    /// </summary>
    public partial class ValidateForm : IAsyncDisposable
    {
        /// <summary>
        /// A callback that will be invoked when the form is submitted and the
        /// <see cref="EditContext"/> is determined to be valid.
        /// </summary>
        [Parameter]
        [NotNull]
        public Func<EditContext, Task>? OnValidSubmit { get; set; }

        /// <summary>
        /// A callback that will be invoked when the form is submitted and the
        /// <see cref="EditContext"/> is determined to be invalid.
        /// </summary>
        [Parameter]
        [NotNull]
        public Func<EditContext, Task>? OnInvalidSubmit { get; set; }

        /// <summary>
        /// 获得/设置 是否验证所有字段 默认 false
        /// </summary>
        [Parameter]
        public bool ValidateAllProperties { get; set; }

        /// <summary>
        /// Specifies the top-level model object for the form. An edit context will
        /// be constructed for this model. If using this parameter, do not also supply
        /// a value for <see cref="EditContext"/>.
        /// </summary>
        [Parameter]
        [NotNull]
        public object? Model { get; set; }

        /// <summary>
        /// Specifies the content to be rendered inside this
        /// </summary>
        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// 获得/设置 是否获取必填项标记 默认为 true 显示
        /// </summary>
        [Parameter]
        public bool ShowRequiredMark { get; set; } = true;

        /// <summary>
        /// 获得/设置 是否显示验证表单内的 Label 默认为 null 未设置时默认显示
        /// </summary>
        [Parameter]
        public bool? ShowLabel { get; set; } = true;

        [Inject]
        [NotNull]
        private IOptions<JsonLocalizationOptions>? Options { get; set; }

        [Inject]
        [NotNull]
        private IStringLocalizerFactory? LocalizerFactory { get; set; }

        /// <summary>
        /// 验证组件缓存
        /// </summary>
        private ConcurrentDictionary<(string FieldName, Type ModelType), (FieldIdentifier FieldIdentifier, IValidateComponent ValidateComponent)> ValidatorCache { get; } = new();

        /// <summary>
        /// 添加数据验证组件到 EditForm 中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal void AddValidator((string FieldName, Type ModelType) key, (FieldIdentifier FieldIdentifier, IValidateComponent IValidateComponent) value)
        {
            ValidatorCache.AddOrUpdate(key, k => value, (k, v) => v = value);
        }

        /// <summary>
        /// 移除数据验证组件到 EditForm 中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal bool TryRemoveValidator((string FieldName, Type ModelType) key, [MaybeNullWhen(false)] out (FieldIdentifier FieldIdentifier, IValidateComponent IValidateComponent) value) => ValidatorCache.TryRemove(key, out value);

        /// <summary>
        /// 设置指定字段错误信息
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="errorMessage">错误描述信息，可为空，为空时查找资源文件</param>
        public void SetError<TModel>(Expression<Func<TModel, object?>> expression, string errorMessage)
        {
            if (expression.Body is UnaryExpression unary && unary.Operand is MemberExpression mem)
            {
                InternalSetError(mem, errorMessage);
            }
            else if (expression.Body is MemberExpression exp)
            {
                InternalSetError(exp, errorMessage);
            }
        }

        private void InternalSetError(MemberExpression exp, string errorMessage)
        {
            var fieldName = exp.Member.Name;
            var modelType = exp.Expression?.Type;
            var validator = ValidatorCache.FirstOrDefault(c => c.Key.ModelType == modelType && c.Key.FieldName == fieldName).Value.ValidateComponent;
            if (validator != null)
            {
                var results = new List<ValidationResult>
                    {
                        new ValidationResult(errorMessage, new string[] { fieldName })
                    };
                validator.ToggleMessage(results, true);
            }
        }

        /// <summary>
        /// 设置指定字段错误信息
        /// </summary>
        /// <param name="propertyName">字段名，可以使用多层，如 a.b.c</param>
        /// <param name="errorMessage">错误描述信息，可为空，为空时查找资源文件</param>
        public void SetError(string propertyName, string errorMessage)
        {
            if (TryGetModelField(propertyName, out var modelType, out var fieldName)
                && TryGetValidator(modelType, fieldName, out var validator))
            {
                if (validator != null)
                {
                    var results = new List<ValidationResult>
                    {
                        new ValidationResult(errorMessage, new string[] { fieldName })
                    };
                    validator.ToggleMessage(results, true);
                }
            }
        }

        private bool TryGetModelField(string propertyName, [MaybeNullWhen(false)] out Type modelType, [MaybeNullWhen(false)] out string fieldName)
        {
            var propNames = new ConcurrentQueue<string>(propertyName.Split('.'));
            var modelTypeInfo = Model.GetType();
            modelType = null;
            fieldName = null;
            while (propNames.TryDequeue(out var propName))
            {
                modelType = modelTypeInfo;
                fieldName = propName;
                var propertyInfo = modelType.GetProperties()
                    .Where(p => p.Name == propName)
                    .FirstOrDefault();
                if (propertyInfo == null)
                {
                    break;
                }
                var exp = Expression.Parameter(modelTypeInfo);
                var member = Expression.Property(exp, propertyInfo);
                modelTypeInfo = member.Type;
            }
            return propNames.IsEmpty;
        }

        private bool TryGetValidator(Type modelType, string fieldName, [MaybeNull] out IValidateComponent validator)
        {
            validator = ValidatorCache.FirstOrDefault(c => c.Key.ModelType == modelType && c.Key.FieldName == fieldName).Value.ValidateComponent;
            return validator != null;
        }

        private static bool IsPublic(PropertyInfo p) => p.GetMethod != null && p.SetMethod != null && p.GetMethod.IsPublic && p.SetMethod.IsPublic;

        /// <summary>
        /// EditModel 数据模型验证方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="results"></param>
        internal void ValidateObject(ValidationContext context, List<ValidationResult> results)
        {
            if (ValidateAllProperties)
            {
                ValidateProperty(context, results);
            }
            else
            {
                // 遍历所有可验证组件进行数据验证
                foreach (var key in ValidatorCache.Keys)
                {
                    // 验证 DataAnnotations
                    var validatorValue = ValidatorCache[key];
                    var validator = validatorValue.ValidateComponent;
                    var fieldIdentifier = validatorValue.FieldIdentifier;
                    if (!validator.IsDisabled && !validator.SkipValidate)
                    {
                        var messages = new List<ValidationResult>();
                        var pi = key.ModelType.GetProperties().Where(p => p.Name == key.FieldName).FirstOrDefault();
                        if (pi != null)
                        {
                            var propertyValidateContext = new ValidationContext(fieldIdentifier.Model)
                            {
                                MemberName = fieldIdentifier.FieldName,
                                DisplayName = fieldIdentifier.GetDisplayName()
                            };

                            // 设置其关联属性字段
                            var propertyValue = LambdaExtensions.GetPropertyValue(fieldIdentifier.Model, fieldIdentifier.FieldName);

                            Validate(validator, propertyValidateContext, messages, pi, propertyValue);
                        }
                        // 客户端提示
                        validator.ToggleMessage(messages, false);
                        results.AddRange(messages);
                    }
                }
            }
        }

        /// <summary>
        /// 通过表单内绑定的字段验证方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="results"></param>
        /// <param name="fieldIdentifier"></param>
        internal void ValidateField(ValidationContext context, List<ValidationResult> results, in FieldIdentifier fieldIdentifier)
        {
            if (ValidatorCache.TryGetValue((fieldIdentifier.FieldName, fieldIdentifier.Model.GetType()), out var v))
            {
                var validator = v.ValidateComponent;
                if (validator != null && !validator.IsDisabled && !validator.SkipValidate)
                {
                    var fieldName = fieldIdentifier.FieldName;
                    var pi = fieldIdentifier.Model.GetType().GetProperties().Where(p => p.Name == fieldName).FirstOrDefault();
                    if (pi != null)
                    {
                        var propertyValue = LambdaExtensions.GetPropertyValue(fieldIdentifier.Model, fieldIdentifier.FieldName);
                        Validate(validator, context, results, pi, propertyValue);
                    }

                    // 客户端提示
                    validator.ToggleMessage(results, true);
                }
            }
        }

        /// <summary>
        /// 通过属性设置的 DataAnnotation 进行数据验证
        /// </summary>
        /// <param name="value"></param>
        /// <param name="context"></param>
        /// <param name="results"></param>
        /// <param name="propertyInfo"></param>
        /// <param name="memberName"></param>
        private void ValidateDataAnnotations(object? value, ValidationContext context, ICollection<ValidationResult> results, PropertyInfo propertyInfo, string? memberName = null)
        {
            var rules = propertyInfo.GetCustomAttributes(true).OfType<ValidationAttribute>();
            var displayName = context.DisplayName;
            memberName ??= propertyInfo.Name;
            var attributeSpan = nameof(Attribute).AsSpan();
            foreach (var rule in rules)
            {
                var result = rule.GetValidationResult(value, context);
                if (result != null && result != ValidationResult.Success)
                {
                    // 查找 resx 资源文件中的 ErrorMessage
                    var ruleNameSpan = rule.GetType().Name.AsSpan();
                    var index = ruleNameSpan.IndexOf(attributeSpan, StringComparison.OrdinalIgnoreCase);
                    var ruleName = rule.GetType().Name.AsSpan().Slice(0, index);
                    var find = false;
                    if (!string.IsNullOrEmpty(rule.ErrorMessage))
                    {
                        var resxType = Options.Value.ResourceManagerStringLocalizerType;
                        if (resxType != null
                            && JsonStringLocalizerFactory.TryGetLocalizerString(
                                localizer: LocalizerFactory.Create(resxType),
                                key: rule.ErrorMessage, out var resx))
                        {
                            rule.ErrorMessage = resx;
                            find = true;
                        }
                    }

                    // 通过设置 ErrorMessage 检索
                    if (!context.ObjectType.Assembly.IsDynamic && !find && !string.IsNullOrEmpty(rule.ErrorMessage) && JsonStringLocalizerFactory.TryGetLocalizerString(
                            localizer: LocalizerFactory.Create(context.ObjectType),
                            key: rule.ErrorMessage, out var msg))
                    {
                        rule.ErrorMessage = msg;
                        find = true;
                    }

                    // 通过 Attribute 检索
                    if (!rule.GetType().Assembly.IsDynamic && !find && JsonStringLocalizerFactory.TryGetLocalizerString(
                        localizer: LocalizerFactory.Create(rule.GetType()),
                        key: nameof(rule.ErrorMessage), out msg))
                    {
                        rule.ErrorMessage = msg;
                        find = true;
                    }

                    // 通过 字段.规则名称 检索
                    if (!context.ObjectType.Assembly.IsDynamic && !find && JsonStringLocalizerFactory.TryGetLocalizerString(
                            localizer: LocalizerFactory.Create(context.ObjectType),
                            key: $"{memberName}.{ruleName.ToString()}", out msg))
                    {
                        rule.ErrorMessage = msg;
                        find = true;
                    }

                    if (!find)
                    {
                        rule.ErrorMessage = result.ErrorMessage;
                    }

                    var errorMessage = string.IsNullOrEmpty(rule.ErrorMessageResourceName)
                        ? rule.FormatErrorMessage(displayName ?? memberName)
                        : rule.ErrorMessage;
                    results.Add(new ValidationResult(errorMessage, new string[] { memberName }));
                }
            }
        }

        /// <summary>
        /// 验证整个模型时验证属性方法
        /// </summary>
        /// <param name="context"></param>
        /// <param name="results"></param>
        private void ValidateProperty(ValidationContext context, List<ValidationResult> results)
        {
            // 获得所有可写属性
            var properties = context.ObjectType.GetRuntimeProperties()
                .Where(p => IsPublic(p) && p.CanWrite && !p.GetIndexParameters().Any());
            foreach (var pi in properties)
            {
                // 设置其关联属性字段
                var propertyValue = LambdaExtensions.GetPropertyValue(context.ObjectInstance, pi.Name);

                // 检查当前值是否为 Class 不是 string 不是集合
                if (propertyValue != null && propertyValue is not string
                    && !propertyValue.GetType().IsAssignableTo(typeof(System.Collections.IEnumerable))
                    && propertyValue.GetType().IsClass)
                {
                    var fieldContext = new ValidationContext(propertyValue);
                    ValidateProperty(fieldContext, results);
                }
                else
                {
                    // 验证 DataAnnotations
                    var messages = new List<ValidationResult>();
                    var fieldIdentifier = new FieldIdentifier(context.ObjectInstance, pi.Name);
                    context.DisplayName = fieldIdentifier.GetDisplayName();
                    context.MemberName = fieldIdentifier.FieldName;

                    if (ValidatorCache.TryGetValue((fieldIdentifier.FieldName, fieldIdentifier.Model.GetType()), out var v) && v.ValidateComponent != null)
                    {
                        var validator = v.ValidateComponent;
                        if (!validator.IsDisabled && !validator.SkipValidate)
                        {
                            // 组件进行验证
                            Validate(validator, context, messages, pi, propertyValue);

                            // 客户端提示
                            validator.ToggleMessage(messages, true);
                        }
                    }
                    results.AddRange(messages);
                }
            }
        }

        private void Validate(IValidateComponent validator, ValidationContext context, List<ValidationResult> messages, PropertyInfo pi, object? propertyValue)
        {
            // 单独处理 Upload 组件
            if (validator is IUpload uploader)
            {
                // 处理多个上传文件
                uploader.UploadFiles.ForEach(file =>
                {
                    // 优先检查 File 流，如果没有检查 FileName
                    ValidateDataAnnotations((object?)file.File ?? file.FileName, context, messages, pi, file.ValidateId);
                });
            }
            else
            {
                ValidateDataAnnotations(propertyValue, context, messages, pi);
                if (messages.Count == 0)
                {
                    // 自定义验证组件
                    validator.ValidateProperty(propertyValue, context, messages);
                }
            }
        }

        private List<Button> AsyncSubmitButtons { get; set; } = new List<Button>();

        /// <summary>
        /// 注册提交按钮
        /// </summary>
        /// <param name="button"></param>
        internal void RegisterAsyncSubmitButton(Button button)
        {
            AsyncSubmitButtons.Add(button);
        }

        private async Task OnValidSubmitForm(EditContext context)
        {
            if (OnValidSubmit != null)
            {
                var isAsync = AsyncSubmitButtons.Any();
                foreach (var b in AsyncSubmitButtons)
                {
                    b.TriggerAsync(true);
                }
                if (isAsync)
                {
                    await Task.Yield();
                }
                await OnValidSubmit(context);
                foreach (var b in AsyncSubmitButtons)
                {
                    b.TriggerAsync(false);
                }
            }
        }

        /// <summary>
        /// DisposeAsyncCore 方法
        /// </summary>
        /// <param name="disposing"></param>
        /// <returns></returns>
        protected virtual async ValueTask DisposeAsyncCore(bool disposing)
        {
            if (disposing)
            {
                await JSRuntime.InvokeVoidAsync(Id, "bb_form", "dispose");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore(true);
            GC.SuppressFinalize(this);
        }
    }
}
