// Copyright (c) Mpho Jele. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Runtime.InteropServices;

// using System.Text.Json;
// using System.Text.Json.Serialization;
using JetBrains.Rider.PathLocator;
using Newtonsoft.Json;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.RiderProjects.Helpers;

public class RiderLocatorEnvironment : IRiderLocatorEnvironment
{
    // private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    // {
    //     UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
    //     PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    // };
    public OS CurrentOS
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OS.Windows;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OS.Linux;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OS.MacOSX;
            }

            return OS.Other;
        }
    }

    public T FromJson<T>(string json)
    {
        // return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
        return JsonConvert.DeserializeObject<T>(json);
    }

    public void Info(string message, Exception e = null)
    {
        Log.Info(message, GetType());
    }

    public void Warn(string message, Exception e = null)
    {
        Log.Warn(message, GetType());
    }

    public void Error(string message, Exception e = null)
    {
        Log.Error(message, GetType());
    }

    public void Verbose(string message, Exception e = null)
    {
        Log.Info(message, GetType());
    }
}
