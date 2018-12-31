﻿// Copyright 2018 Maintainers of NUKE.
// Distributed under the MIT License.
// https://github.com/nuke-build/nuke/blob/master/LICENSE

using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Nuke.Common.Tools.MSBuild;
using Nuke.Platform;

namespace Nuke.Common.Tests
{
    public class MSBuildParseProjectTest
    {
        [WindowsFact]
        public void Test()
        {
            var rootDirectory = Constants.TryGetRootDirectoryFrom(Directory.GetCurrentDirectory()).NotNull("rootDirectory != null");
            var projectFile = rootDirectory / "src" / "Nuke.Common" / "Nuke.Common.csproj";

            var project = MSBuildTasks.MSBuildParseProject(projectFile, x => x.SetProperty("TargetFramework", "netstandard2.0"));
            project.IsSdkProject.Should().BeTrue();
            project.Properties["Configuration"].Should().Be("Debug");
            project.Properties["TargetFramework"].Should().Be("netstandard2.0");

            project = MSBuildTasks.MSBuildParseProject(projectFile, x => x.SetProperty("TargetFramework", "net461").SetConfiguration("Release"));
            project.Properties["Configuration"].Should().Be("Release");
            project.Properties["TargetFramework"].Should().Be("net461");
        }
    }
}