﻿using System;
using Cake.Common.Tests.Fixtures.Tools.NuGet.Packer;
using Cake.Common.Tests.Properties;
using Cake.Common.Tools.NuGet;
using Cake.Common.Tools.NuGet.Pack;
using Cake.Core;
using Cake.Core.IO;
using Cake.Testing;
using Cake.Testing.Xunit;
using Xunit;

namespace Cake.Common.Tests.Unit.Tools.NuGet.Pack
{
    public sealed class NuGetPackerTests
    {
        public sealed class ThePackMethod
        {
            public sealed class WithNuSpec
            {
                [Fact]
                public void Should_Throw_If_Nuspec_File_Path_Is_Null()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();
                    fixture.NuSpecFilePath = null;

                    // When
                    var result = Record.Exception(() => fixture.Run());

                    // Then
                    Assert.IsArgumentNullException(result, "nuspecFilePath");
                }

                [Fact]
                public void Should_Throw_If_Settings_Is_Null()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();
                    fixture.Settings = null;

                    // When
                    var result = Record.Exception(() => fixture.Run());

                    // Then
                    Assert.IsArgumentNullException(result, "settings");
                }

                [Fact]
                public void Should_Throw_If_NuGet_Executable_Was_Not_Found()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();
                    fixture.GivenDefaultToolDoNotExist();

                    // When
                    var result = Record.Exception(() => fixture.Run());

                    // Then
                    Assert.IsCakeException(result, "NuGet: Could not locate executable.");
                }

                [Theory]
                [InlineData("/bin/nuget/nuget.exe", "/bin/nuget/nuget.exe")]
                [InlineData("./tools/nuget/nuget.exe", "/Working/tools/nuget/nuget.exe")]
                public void Should_Use_NuGet_Executable_From_Tool_Path_If_Provided(string toolPath, string expected)
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();
                    fixture.Settings.ToolPath = toolPath;
                    fixture.GivenSettingsToolPathExist();

                    // When
                    var result = fixture.Run();

                    // Then
                    Assert.Equal(expected, result.Path.FullPath);
                }

                [WindowsTheory]
                [InlineData("C:/nuget/nuget.exe", "C:/nuget/nuget.exe")]
                public void Should_Use_NuGet_Executable_From_Tool_Path_If_Provided_On_Windows(string toolPath, string expected)
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();
                    fixture.Settings.ToolPath = toolPath;
                    fixture.GivenSettingsToolPathExist();

                    // When
                    var result = fixture.Run();

                    // Then
                    Assert.Equal(expected, result.Path.FullPath);
                }

                [Fact]
                public void Should_Find_NuGet_Executable_If_Tool_Path_Not_Provided()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();

                    // When
                    var result = fixture.Run();

                    // Then
                    Assert.Equal("/Working/tools/NuGet.exe", result.Path.FullPath);
                }

                [Fact]
                public void Should_Throw_If_Process_Was_Not_Started()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();
                    fixture.GivenProcessCannotStart();

                    // When
                    var result = Record.Exception(() => fixture.Run());

                    // Then
                    Assert.IsCakeException(result, "NuGet: Process was not started.");
                }

                [Fact]
                public void Should_Throw_If_Process_Has_A_Non_Zero_Exit_Code()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();
                    fixture.GivenProcessExitsWithCode(1);

                    // When
                    var result = Record.Exception(() => fixture.Run());

                    // Then
                    Assert.IsCakeException(result, "NuGet: Process returned an error.");
                }

                [Fact]
                public void Should_Delete_Transformed_Nuspec()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();

                    // When
                    fixture.Run();

                    // Then
                    Assert.False(fixture.FileSystem.Exist((FilePath) "/Working/existing.temp.nuspec"));
                }

                [Fact]
                public void Should_Throw_If_Nuspec_Do_Not_Exist()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();
                    fixture.NuSpecFilePath = "./nonexisting.nuspec";

                    // When
                    var result = Record.Exception(() => fixture.Run());

                    // Then
                    Assert.IsCakeException(result, "Could not find nuspec file '/Working/nonexisting.nuspec'.");
                }

                [Fact]
                public void Should_Throw_If_Temporary_Nuspec_Already_Exist()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();
                    fixture.GivenTemporaryNuSpecAlreadyExist();

                    // When
                    var result = Record.Exception(() => fixture.Run());

                    // Then
                    Assert.IsCakeException(result,
                        "Could not create the nuspec file '/Working/existing.temp.nuspec' since it already exist.");
                }

                [Fact]
                public void Should_Add_Version_To_Arguments_If_Not_Null()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();
                    fixture.Settings.Version = "1.0.0";

                    // When
                    var result = fixture.Run();

                    // Then
                    Assert.Equal("pack -Version \"1.0.0\" \"/Working/existing.temp.nuspec\"", result.Args);
                }

                [Fact]
                public void Should_Add_Base_Path_To_Arguments_If_Not_Null()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();
                    fixture.Settings.BasePath = "./build";

                    // When
                    var result = fixture.Run();

                    // Then
                    Assert.Equal("pack -BasePath \"/Working/build\" " +
                                 "\"/Working/existing.temp.nuspec\"", result.Args);
                }

                [Fact]
                public void Should_Add_Output_Directory_To_Arguments_If_Not_Null()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();
                    fixture.Settings.OutputDirectory = "./build/output";

                    // When
                    var result = fixture.Run();

                    // Then
                    Assert.Equal("pack -OutputDirectory \"/Working/build/output\" " +
                                 "\"/Working/existing.temp.nuspec\"", result.Args);
                }

                [Fact]
                public void Should_Add_No_Package_Analysis_Flag_To_Arguments_If_Set()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();
                    fixture.Settings.NoPackageAnalysis = true;

                    // When
                    var result = fixture.Run();

                    // Then
                    Assert.Equal("pack \"/Working/existing.temp.nuspec\" -NoPackageAnalysis", result.Args);
                }

                [Fact]
                public void Should_Add_Symbols_Flag_To_Arguments_If_Set()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();
                    fixture.Settings.Symbols = true;

                    // When
                    var result = fixture.Run();

                    // Then
                    Assert.Equal("pack \"/Working/existing.temp.nuspec\" -Symbols", result.Args);
                }

                [Theory]
                [InlineData(NuGetVerbosity.Detailed, "pack \"/Working/existing.temp.nuspec\" -Verbosity detailed")]
                [InlineData(NuGetVerbosity.Normal, "pack \"/Working/existing.temp.nuspec\" -Verbosity normal")]
                [InlineData(NuGetVerbosity.Quiet, "pack \"/Working/existing.temp.nuspec\" -Verbosity quiet")]
                public void Should_Add_Verbosity_To_Arguments_If_Set(NuGetVerbosity verbosity, string expected)
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();
                    fixture.Settings.Verbosity = verbosity;

                    // When
                    var result = fixture.Run();

                    // Then
                    Assert.Equal(expected, result.Args);
                }

                [Fact]
                public void Should_Add_Metadata_Element_To_Nuspec_If_Missing()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();
                    fixture.WithNuSpecXml(Resources.Nuspec_NoMetadataElement);

                    fixture.Settings.Id = "The ID";
                    fixture.Settings.Version = "The version";
                    fixture.Settings.Title = "The title";
                    fixture.Settings.Authors = new[] { "Author #1", "Author #2" };
                    fixture.Settings.Owners = new[] { "Owner #1", "Owner #2" };
                    fixture.Settings.Description = "The description";
                    fixture.Settings.Summary = "The summary";
                    fixture.Settings.LicenseUrl = new Uri("https://license.com");
                    fixture.Settings.ProjectUrl = new Uri("https://project.com");
                    fixture.Settings.IconUrl = new Uri("https://icon.com");
                    fixture.Settings.RequireLicenseAcceptance = true;
                    fixture.Settings.Copyright = "The copyright";
                    fixture.Settings.ReleaseNotes = new[] { "Line #1", "Line #2", "Line #3" };
                    fixture.Settings.Tags = new[] { "Tag1", "Tag2", "Tag3" };

                    // When
                    var result = fixture.Run();

                    // Then
                    Assert.Equal(
                        Resources.Nuspec_Metadata.NormalizeLineEndings(),
                        result.NuspecContent.NormalizeLineEndings());
                }

                [Fact]
                public void Should_Replace_Template_Tokens_In_Nuspec()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();

                    fixture.Settings.Id = "The ID";
                    fixture.Settings.Version = "The version";
                    fixture.Settings.Title = "The title";
                    fixture.Settings.Authors = new[] { "Author #1", "Author #2" };
                    fixture.Settings.Owners = new[] { "Owner #1", "Owner #2" };
                    fixture.Settings.Description = "The description";
                    fixture.Settings.Summary = "The summary";
                    fixture.Settings.LicenseUrl = new Uri("https://license.com");
                    fixture.Settings.ProjectUrl = new Uri("https://project.com");
                    fixture.Settings.IconUrl = new Uri("https://icon.com");
                    fixture.Settings.RequireLicenseAcceptance = true;
                    fixture.Settings.Copyright = "The copyright";
                    fixture.Settings.ReleaseNotes = new[] { "Line #1", "Line #2", "Line #3" };
                    fixture.Settings.Tags = new[] { "Tag1", "Tag2", "Tag3" };

                    // When
                    var result = fixture.Run();

                    // Then
                    Assert.Equal(
                        Resources.Nuspec_Metadata.NormalizeLineEndings(),
                        result.NuspecContent.NormalizeLineEndings());
                }

                [Fact]
                public void Should_Replace_Template_Tokens_In_Nuspec_Without_Namespaces()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();
                    fixture.WithNuSpecXml(Resources.Nuspec_NoMetadataValues_WithoutNamespaces);

                    fixture.Settings.Id = "The ID";
                    fixture.Settings.Version = "The version";
                    fixture.Settings.Title = "The title";
                    fixture.Settings.Authors = new[] { "Author #1", "Author #2" };
                    fixture.Settings.Owners = new[] { "Owner #1", "Owner #2" };
                    fixture.Settings.Description = "The description";
                    fixture.Settings.Summary = "The summary";
                    fixture.Settings.LicenseUrl = new Uri("https://license.com");
                    fixture.Settings.ProjectUrl = new Uri("https://project.com");
                    fixture.Settings.IconUrl = new Uri("https://icon.com");
                    fixture.Settings.RequireLicenseAcceptance = true;
                    fixture.Settings.Copyright = "The copyright";
                    fixture.Settings.ReleaseNotes = new[] { "Line #1", "Line #2", "Line #3" };
                    fixture.Settings.Tags = new[] { "Tag1", "Tag2", "Tag3" };

                    // When
                    var result = fixture.Run();

                    // Then
                    Assert.Equal(
                        Resources.Nuspec_Metadata_WithoutNamespaces.NormalizeLineEndings(),
                        result.NuspecContent.NormalizeLineEndings());
                }

                [Fact]
                public void Should_Replace_Template_Tokens_In_Nuspec_With_Files()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();

                    fixture.Settings.Id = "The ID";
                    fixture.Settings.Version = "The version";
                    fixture.Settings.Title = "The title";
                    fixture.Settings.Authors = new[] { "Author #1", "Author #2" };
                    fixture.Settings.Owners = new[] { "Owner #1", "Owner #2" };
                    fixture.Settings.Description = "The description";
                    fixture.Settings.Summary = "The summary";
                    fixture.Settings.LicenseUrl = new Uri("https://license.com");
                    fixture.Settings.ProjectUrl = new Uri("https://project.com");
                    fixture.Settings.IconUrl = new Uri("https://icon.com");
                    fixture.Settings.RequireLicenseAcceptance = true;
                    fixture.Settings.Copyright = "The copyright";
                    fixture.Settings.ReleaseNotes = new[] { "Line #1", "Line #2", "Line #3" };
                    fixture.Settings.Tags = new[] { "Tag1", "Tag2", "Tag3" };
                    fixture.Settings.Files = new[]
                    {
                        new NuSpecContent { Source = "Cake.Core.dll", Target = "lib/net45" },
                        new NuSpecContent { Source = "Cake.Core.xml", Target = "lib/net45" },
                        new NuSpecContent { Source = "Cake.Core.pdb", Target = "lib/net45" },
                        new NuSpecContent { Source = "LICENSE" }
                    };

                    // When
                    var result = fixture.Run();

                    // Then
                    Assert.Equal(
                        Resources.Nuspec_Metadata.NormalizeLineEndings(),
                        result.NuspecContent.NormalizeLineEndings());
                }

                [Fact]
                public void Should_Replace_Template_Tokens_In_Nuspec_With_Files_Without_Namespaces()
                {
                    // Given
                    var fixture = new NuGetPackerWithNuSpecFixture();
                    fixture.WithNuSpecXml(Resources.Nuspec_NoMetadataValues_WithoutNamespaces);

                    fixture.Settings.Id = "The ID";
                    fixture.Settings.Version = "The version";
                    fixture.Settings.Title = "The title";
                    fixture.Settings.Authors = new[] { "Author #1", "Author #2" };
                    fixture.Settings.Owners = new[] { "Owner #1", "Owner #2" };
                    fixture.Settings.Description = "The description";
                    fixture.Settings.Summary = "The summary";
                    fixture.Settings.LicenseUrl = new Uri("https://license.com");
                    fixture.Settings.ProjectUrl = new Uri("https://project.com");
                    fixture.Settings.IconUrl = new Uri("https://icon.com");
                    fixture.Settings.RequireLicenseAcceptance = true;
                    fixture.Settings.Copyright = "The copyright";
                    fixture.Settings.ReleaseNotes = new[] { "Line #1", "Line #2", "Line #3" };
                    fixture.Settings.Tags = new[] { "Tag1", "Tag2", "Tag3" };
                    fixture.Settings.Files = new[]
                    {
                        new NuSpecContent { Source = "Cake.Core.dll", Target = "lib/net45" },
                        new NuSpecContent { Source = "Cake.Core.xml", Target = "lib/net45" },
                        new NuSpecContent { Source = "Cake.Core.pdb", Target = "lib/net45" },
                        new NuSpecContent { Source = "LICENSE" }
                    };

                    // When
                    var result = fixture.Run();

                    // Then
                    Assert.Equal(
                        Resources.Nuspec_Metadata_WithoutNamespaces.NormalizeLineEndings(),
                        result.NuspecContent.NormalizeLineEndings());
                }
            }

            public sealed class WithoutNuSpec
            {
                [Fact]
                public void Should_Pack_If_Sufficient_Settings_Specified()
                {
                    // Given
                    var fixture = new NuGetPackerWithoutNuSpecFixture();
                    fixture.Settings.OutputDirectory = "/Working/";
                    fixture.Settings.Id = "nonexisting";
                    fixture.Settings.Version = "1.0.0";
                    fixture.Settings.Description = "The description";
                    fixture.Settings.Authors = new[] { "Author #1", "Author #2" };
                    fixture.Settings.Files = new[]
                    {
                        new NuSpecContent { Source = "LICENSE" }
                    };

                    // When
                    var result = fixture.Run();

                    // Then
                    Assert.Equal("pack -Version \"1.0.0\" -OutputDirectory \"/Working\" " +
                                 "\"/Working/nonexisting.temp.nuspec\"", result.Args);
                }

                [Fact]
                public void Should_Throw_If_OutputDirectory_Setting_Not_Specified()
                {
                    // Given
                    var fixture = new NuGetPackerWithoutNuSpecFixture();

                    // When
                    var result = Record.Exception(() => fixture.Run());

                    // Then
                    Assert.IsCakeException(result, "Required setting OutputDirectory not specified or doesn't exists.");
                }

                [Fact]
                public void Should_Throw_If_Id_Setting_Not_Specified()
                {
                    // Given
                    var fixture = new NuGetPackerWithoutNuSpecFixture();
                    fixture.Settings.OutputDirectory = "/Working/";

                    // When
                    var result = Record.Exception(() => fixture.Run());

                    // Then
                    Assert.IsCakeException(result, "Required setting Id not specified.");
                }

                [Fact]
                public void Should_Throw_If_Version_Setting_Not_Specified()
                {
                    // Given
                    var fixture = new NuGetPackerWithoutNuSpecFixture();
                    fixture.Settings.OutputDirectory = "/Working/";
                    fixture.Settings.Id = "nonexisting";

                    // When
                    var result = Record.Exception(() => fixture.Run());

                    // Then
                    Assert.IsCakeException(result, "Required setting Version not specified.");
                }

                [Fact]
                public void Should_Throw_If_Authors_Setting_Not_Specified()
                {
                    // Given
                    var fixture = new NuGetPackerWithoutNuSpecFixture();
                    fixture.Settings.OutputDirectory = "/Working/";
                    fixture.Settings.Id = "nonexisting";
                    fixture.Settings.Version = "1.0.0";

                    // When
                    var result = Record.Exception(() => fixture.Run());

                    // Then
                    Assert.IsCakeException(result, "Required setting Authors not specified.");
                }

                [Fact]
                public void Should_Throw_If_Description_Setting_Not_Specified()
                {
                    // Given
                    var fixture = new NuGetPackerWithoutNuSpecFixture();
                    fixture.Settings.OutputDirectory = "/Working/";
                    fixture.Settings.Id = "nonexisting";
                    fixture.Settings.Version = "1.0.0";
                    fixture.Settings.Authors = new[] { "Author #1", "Author #2" };

                    // When
                    var result = Record.Exception(() => fixture.Run());

                    // Then
                    Assert.IsCakeException(result, "Required setting Description not specified.");
                }

                [Fact]
                public void Should_Throw_If_Files_Setting_Not_Specified()
                {
                    // Given
                    var fixture = new NuGetPackerWithoutNuSpecFixture();
                    fixture.Settings.OutputDirectory = "/Working/";
                    fixture.Settings.Id = "nonexisting";
                    fixture.Settings.Version = "1.0.0";
                    fixture.Settings.Authors = new[] { "Author #1", "Author #2" };
                    fixture.Settings.Description = "The description";

                    // When
                    var result = Record.Exception(() => fixture.Run());

                    // Then
                    Assert.IsCakeException(result, "Required setting Files not specified.");
                }
            }
        }
    }
}