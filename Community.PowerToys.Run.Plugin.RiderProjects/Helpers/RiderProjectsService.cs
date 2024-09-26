// Copyright (c) Mpho Jele. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.ObjectModel;
using JetBrains.Rider.PathLocator;

namespace Community.PowerToys.Run.Plugin.RiderProjects.Helpers;

public class RiderProjectsService
{
    private RiderPathLocator.RiderInfo[] _instances;
    private IRiderLocatorEnvironment _locatorEnvironment;
    private RiderPathLocator _riderPathLocator;

    public void InitInstances()
    {
        _locatorEnvironment = new RiderLocatorEnvironment();
        _riderPathLocator = new RiderPathLocator(_locatorEnvironment);
        _instances = _riderPathLocator.GetAllRiderPaths();
    }

    public ReadOnlyCollection<RiderPathLocator.RiderInfo> GetInstances()
    {
        if (_instances == null)
        {
            InitInstances();
        }

        return new(_instances!);
    }
}
