﻿using System.IO;
using Generator;
using NUnit.Framework;
using Simple1C.Impl.Helpers;
using Simple1C.Tests.Helpers;

namespace Simple1C.Tests
{
    public class CsProjUpdaterTest : TestBase
    {
        private string testDirectory;

        protected override void SetUp()
        {
            base.SetUp();
            testDirectory = PathHelpers.AppendBasePath("test-directory");
            if (Directory.Exists(testDirectory))
                Directory.Delete(testDirectory, true);
        }

        [Test]
        public void ReplaceFile()
        {
            const string csprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <ItemGroup>
    <Compile Include=""gen\f1.cs"" />
  </ItemGroup>
</Project>";
            var projectFilePath = WriteFile("test.csproj", csprojContent);
            WriteFile(@"gen\f2.cs", "");

            Update(projectFilePath);

            const string expectedCsprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <ItemGroup>
    <Compile Include=""gen\f2.cs"" />
  </ItemGroup>
</Project>";

            Assert.That(File.ReadAllText(projectFilePath), Is.EqualTo(expectedCsprojContent));
        }
        
        [Test]
        public void CanInsensitive()
        {
            const string csprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <ItemGroup>
    <Compile Include=""gen\f1.cs"" />
  </ItemGroup>
</Project>";
            var projectFilePath = WriteFile("test.csproj", csprojContent);
            WriteFile(@"gen\F1.cs", "");

            Update(projectFilePath);

            const string expectedCsprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <ItemGroup>
    <Compile Include=""gen\f1.cs"" />
  </ItemGroup>
</Project>";

            Assert.That(File.ReadAllText(projectFilePath), Is.EqualTo(expectedCsprojContent));
        }
        
        [Test]
        public void RemoveFile()
        {
            const string csprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <ItemGroup>
    <Compile Include=""gen\f1.cs"" />
  </ItemGroup>
</Project>";
            var projectFilePath = WriteFile("test.csproj", csprojContent);

            Update(projectFilePath);

            const string expectedCsprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <ItemGroup />
</Project>";

            Assert.That(File.ReadAllText(projectFilePath), Is.EqualTo(expectedCsprojContent));
        }
        
        [Test]
        public void AddNestedFile()
        {
            const string csprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <ItemGroup />
</Project>";
            WriteFile(@"gen\n1\n2\test.cs", "");
            var projectFilePath = WriteFile(@"test.csproj", csprojContent);

            Update(projectFilePath);

            const string expectedCsprojContent = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <ItemGroup>
    <Compile Include=""gen\n1\n2\test.cs"" />
  </ItemGroup>
</Project>";

            Assert.That(File.ReadAllText(projectFilePath), Is.EqualTo(expectedCsprojContent));
        }

        private void Update(string projectFilePath)
        {
            var updater = new CsProjectFileUpdater(projectFilePath, Path.Combine(testDirectory, "gen"));
            updater.Update();
        }

        private string WriteFile(string path, string content)
        {
            var fullPath = Path.Combine(testDirectory, path);
            var directoryPath = PathHelpers.GetDirectoryName(fullPath);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            File.WriteAllText(fullPath, content);
            return fullPath;
        }
    }
}