﻿// Copyright (c) Argo Zhang (argo@163.com). All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Website: https://www.blazor.zone or https://argozhang.github.io/

using BootstrapBlazor.Localization.Json;
using BootstrapBlazor.Shared.Extensions;
using Microsoft.Extensions.Options;

namespace BootstrapBlazor.Shared.Services;

class CodeSnippetService
{
    private HttpClient Client { get; set; }

    private string ServerUrl { get; set; }

    private bool IsDevelopment { get; }

    private string ContentRootPath { get; }

    private ICacheManager CacheManager { get; set; }

    private JsonLocalizationOptions LocalizerOptions { get; }

    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="client"></param>
    /// <param name="cacheManager"></param>
    /// <param name="options"></param>
    /// <param name="localizerOptions"></param>
    public CodeSnippetService(
        HttpClient client,
        ICacheManager cacheManager,
        IOptionsMonitor<WebsiteOptions> options,
        IOptionsMonitor<JsonLocalizationOptions> localizerOptions)
    {
        LocalizerOptions = localizerOptions.CurrentValue;

        CacheManager = cacheManager;
        Client = client;
        Client.Timeout = TimeSpan.FromSeconds(5);
        Client.BaseAddress = new Uri(options.CurrentValue.RepositoryUrl);

        IsDevelopment = options.CurrentValue.IsDevelopment;
        ContentRootPath = options.CurrentValue.ContentRootPath;
        ServerUrl = options.CurrentValue.ServerUrl;
    }

    /// <summary>
    /// 获得示例源码方法
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetCodeAsync(string codeFile, string? blockTitle)
    {
        var content = "";
        try
        {
            var payload = await GetContentFromFile(codeFile);

            if (blockTitle != null)
            {
                // 生成资源文件
                content = CacheManager.GetCode(codeFile, blockTitle, entry =>
                {
                    payload = Filter(payload);

                    entry.SlidingExpiration = TimeSpan.FromMinutes(10);
                    return payload;
                });
            }
            else
            {
                content = payload;
            }
        }
        catch (Exception ex) { content = $"Error: {ex.Message}"; }
        return content;

        string Filter(string content)
        {
            var beginFlag = "<DemoBlock ";
            var endFlag = "</DemoBlock>";
            if (!string.IsNullOrEmpty(blockTitle))
            {
                var findStrings = new string[] { $"Name=\"{blockTitle}\"", $"Title=\"{blockTitle}\"" };
                var endLength = endFlag.Length;
                while (content.Length > 0)
                {
                    var star = content.IndexOf(beginFlag);
                    if (star == -1)
                    {
                        break;
                    }

                    var length = content.IndexOf(endFlag);
                    if (length == -1)
                    {
                        break;
                    }

                    var seg = content[star..(length + endLength)];
                    if (seg.IndexOf(findStrings[0]) > -1 || seg.IndexOf(findStrings[1]) > -1)
                    {
                        var lineFlag = "\n";
                        var seqStar = seg.IndexOf(lineFlag);
                        var end = seg.IndexOf(endFlag);
                        var data = seg[seqStar..end];
                        content = data.Replace("\n    ", "\n").TrimStart('\n');
                        break;
                    }
                    else
                    {
                        content = content[(length + endLength)..];
                    }
                }
            }
            TrimTips();
            return content;

            void TrimTips()
            {
                beginFlag = "<Tips>";
                endFlag = $"</Tips>{Environment.NewLine}";
                var endLength = endFlag.Length;
                var star = content.IndexOf(beginFlag);
                if (star > -1)
                {
                    var length = content.IndexOf(endFlag);
                    if (length > -1)
                    {
                        content = $"{content[..star]}{content[(length + endLength)..]}";
                    }
                }
            }
        }
    }

    private Task<string> GetContentFromFile(string codeFile) => CacheManager.GetContentFromFileAsync(codeFile, async entry =>
    {
        var payload = "";

        if (IsDevelopment)
        {
            payload = await ReadFileTextAsync(codeFile);
        }
        else
        {
            if (OperatingSystem.IsBrowser())
            {
                Client.BaseAddress = new Uri($"{ServerUrl}/api/");
                payload = await Client.GetStringAsync($"Code?fileName={codeFile}");
            }
            else
            {
                payload = await Client.GetStringAsync(codeFile);
            }
        }
        if (Path.GetExtension(codeFile) == ".razor")
        {
            // 将资源文件信息替换
            CacheManager.GetLocalizedStrings(codeFile, LocalizerOptions).ToList().ForEach(ReplacePayload);
            payload = payload.Replace("@@", "@")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">");
        }
        return payload;

        void ReplacePayload(LocalizedString l)
        {
            payload = payload.Replace($"@(((MarkupString)Localizer[\"{l.Name}\"].Value).ToString())", l.Value)
                .Replace($"@((MarkupString)Localizer[\"{l.Name}\"].Value)", l.Value)
                .Replace($"@Localizer[\"{l.Name}\"]", l.Value);
        }
    });

    private async Task<string> ReadFileTextAsync(string codeFile)
    {
        var payload = "";
        var paths = new string[] { "..", "BootstrapBlazor.Shared", "Samples" };
        var folder = Path.Combine(ContentRootPath, string.Join(Path.DirectorySeparatorChar, paths));
        var file = Path.Combine(folder, codeFile);
        if (File.Exists(file))
        {
            payload = await File.ReadAllTextAsync(file);
        }
        return payload;
    }
}
