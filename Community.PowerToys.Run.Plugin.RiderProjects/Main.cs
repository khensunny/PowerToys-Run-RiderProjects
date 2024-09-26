// Copyright (c) Mpho Jele. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Xml.Linq;
using Community.PowerToys.Run.Plugin.RiderProjects.Helpers;
using Newtonsoft.Json.Linq;
using Wox.Infrastructure;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.RiderProjects
{
    /// <summary>
    /// Main class of this plugin that implement all used interfaces.
    /// </summary>
    public class Main : IPlugin, IContextMenu, IDisposable
    {
        private readonly RiderProjectsService _riderProjectsService = new();

        /// <summary>
        /// Gets the ID of the plugin.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static string PluginID => "62D86F0E2E9D45BFA0E067CD14694EC2";

        /// <summary>
        /// Gets the name of the plugin.
        /// </summary>
        public string Name => "RiderProjects";

        /// <summary>
        /// Gets the description of the plugin.
        /// </summary>
        public string Description => "Opens projects previously opened in JetBrains Rider";

        private bool Disposed { get; set; }

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
        {
            var instances = _riderProjectsService.GetInstances();
            var search = query.Search;

            var results = new List<Result>();
            var instanceType = GetType();

            foreach (var instance in instances)
            {
                // TODO: Check if the config directory hasn't been set to something else in idea.properties
                var productInfoPath = Path.Combine(instance.Path, "../../product-info.json");
                if (!File.Exists(productInfoPath))
                {
                    continue;
                }

                var productInfo = JObject.Parse(File.ReadAllText(productInfoPath));
                var dataDirectoryName = productInfo["dataDirectoryName"]?.ToString();
                if (string.IsNullOrEmpty(dataDirectoryName))
                {
                    continue;
                }

                var appDataRoaming = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var recentOpenedProjectsFile = appDataRoaming +
                    $"/JetBrains/{dataDirectoryName}/options/recentSolutions.xml";
                if (!File.Exists(recentOpenedProjectsFile))
                {
                    continue;
                }

                var recentProjects = XDocument.Load(recentOpenedProjectsFile);
                foreach (var project in recentProjects.Descendants("entry"))
                {
                    var projectPath = (string)project.Attribute("key");

                    projectPath = projectPath?.Replace(
                        "$USER_HOME$", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
                    Log.Info("project path: " + projectPath, instanceType);
                    var lastOpened = DateTimeOffset.UtcNow.UtcDateTime.ToLocalTime();

                    var metaInfo = project.Descendants("RecentProjectMetaInfo").First();
                    var title =
                        (string)metaInfo.Attribute("frameTitle") ??
                        (string)metaInfo.Attribute("displayName") ??
                        projectPath ??
                        "Unknown";
                    Log.Info("title: " + title, instanceType);

                    var matchResult = StringMatcher.FuzzySearch(search, title);
                    if (!string.IsNullOrWhiteSpace(search) && matchResult.Score <= 0)
                    {
                        continue;
                    }

                    foreach (var option in project.Descendants("option"))
                    {
                        if ((string)option.Attribute("name") != "projectOpenTimestamp")
                        {
                            continue;
                        }

                        if (long.TryParse((string)option.Attribute("value"), out var lastOpenedTimestamp))
                        {
                            lastOpened = DateTimeOffset.FromUnixTimeMilliseconds(lastOpenedTimestamp)
                                .UtcDateTime.ToLocalTime();
                        }
                    }

                    var subtitle =
                        $"{instance.Presentation}: {projectPath}, Last Opened: {lastOpened:yyyy-MM-dd HH:mm}";
                    results.Add(new Result
                    {
                        QueryTextDisplay = search,
                        IcoPath = instance.Path,
                        Title = title,
                        SubTitle = $"{instance.Presentation}: {projectPath}, Last Opened: {lastOpened:yyyy-MM-dd HH:mm}",
                        ToolTipData = new ToolTipData(title, subtitle),
                        ContextData = new CodeContainer(projectPath, instance.Path, lastOpened),
                        Action = _ =>
                        {
                            Helper.OpenInShell(instance.Path, $"\"{projectPath}\"");
                            return true;
                        },
                    });
                }
            }

            Log.Info($"results count: {results.Count}", instanceType);

            return results.OrderByDescending(x => ((CodeContainer)x.ContextData).LastOpened).ToList();
        }

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            _riderProjectsService.InitInstances();
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/> (shown on the right side of the result).
        /// </summary>
        /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
        /// <returns>A list context menu entries.</returns>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (selectedResult.ContextData is not CodeContainer container)
            {
                return [];
            }

            return
            [
                new()
                {
                    Title = "Run as administrator (Ctrl+Shift+Enter)",
                    Glyph = "\xE7EF",
                    FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                    AcceleratorKey = Key.Enter,
                    AcceleratorModifiers = ModifierKeys.Control | ModifierKeys.Shift,
                    PluginName = Name,
                    Action = _ =>
                    {
                        Helper.OpenInShell(
                            container.InstancePath,
                            $"\"{container.FullPath}\"",
                            runAs: Helper.ShellRunAsType.Administrator);
                        return true;
                    },
                },

                new()
                {
                    Title = "Open containing folder (Ctrl+Shift+E)",
                    Glyph = "\xE838",
                    FontFamily = "Segoe Fluent Icons,Segoe MDL2 Assets",
                    AcceleratorKey = Key.E,
                    AcceleratorModifiers = ModifierKeys.Control | ModifierKeys.Shift,
                    PluginName = Name,
                    Action = _ =>
                    {
                        Helper.OpenInShell(Path.GetDirectoryName(container.FullPath));
                        return true;
                    },
                }

            ];
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events form the plugin itself.
        /// </summary>
        /// <param name="disposing">Indicate that the plugin is disposed.</param>
        private void Dispose(bool disposing)
        {
            if (Disposed || !disposing)
            {
                return;
            }

            Disposed = true;
        }

        // TODO: Move this class to a separate file
        private class CodeContainer(string fullPath, string instancePath, DateTime lastOpened)
        {
            public string FullPath { get; } = fullPath;

            public string InstancePath { get; } = instancePath;

            public DateTime LastOpened { get; } = lastOpened;
        }
    }
}
