namespace bgTeam.ProjectTemplate
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using bgTeam.Extensions;
    using bgTeam.ProjectTemplate.FileGenerators;

    internal class SolutionGenerator
    {
        public void Generate(string @namespace, string name, SolutionSettings settings)
        {
            var folder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "result");

            if (Directory.Exists(folder))
            {
                Console.WriteLine($"Dropping existing directory {folder}{GeneratorHelper.NewLine}");
                Directory.Delete(folder, true);
            }

            Console.WriteLine($"Generating project in {folder}{GeneratorHelper.NewLine}");

            var nameSln = string.Join('.', new[] { @namespace, name }.Where(x => x != null));

            var projects = GenerateProjects(nameSln, folder, "src", settings);

            var file = ProjectsFile(projects);

            File.WriteAllText($"{folder}{GeneratorHelper.Separator}{nameSln}.sln", file);

            GeneratorHelper.CopyFile("./Resourse/LICENSE", $"{folder}/LICENSE");
            GeneratorHelper.CopyFile("./Resourse/README.md", $"{folder}/README.md");
            GeneratorHelper.CopyFile("./Resourse/.gitignore", $"{folder}/.gitignore");
            File.WriteAllText($"{folder}/coverage.bat", File.ReadAllText("./Resourse/coverage.bat").Replace("$namespace$", @namespace));

            foreach (var item in Directory.EnumerateFiles("./Resourse/shared", "*", SearchOption.AllDirectories))
            {
                var dest = $"{folder}/{Path.GetRelativePath("./Resourse/", item)}";
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                GeneratorHelper.CopyFile(item, dest);
                Console.WriteLine($"Generating project in {folder}");
            }

            foreach (var item in Directory.EnumerateFiles("./Resourse/lint", "*", SearchOption.AllDirectories))
            {
                var dest = $"{folder}/{Path.GetRelativePath("./Resourse/", item)}";
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                GeneratorHelper.CopyFile(item, dest);
            }
        }

        private IEnumerable<ProjectInfoItem> GenerateProjects(string name, string folder, string path, SolutionSettings settings)
        {
            var result = new List<ProjectInfoItem>();
            var fullPath = $"{folder}{GeneratorHelper.Separator}{path}";

            var fapps = new ProjectInfoItem("Apps", $"Apps", ProjectTypeEnum.Folder);
            var fmain = new ProjectInfoItem("Main", $"Main", ProjectTypeEnum.Folder);
            var fshared = new ProjectInfoItem("Shared", $"Shared", ProjectTypeEnum.Folder);
            var fsrv = new ProjectInfoItem("Services", $"Services", ProjectTypeEnum.Folder);
            var ftest = new ProjectInfoItem("Tests", $"Tests", ProjectTypeEnum.Folder);

            var fconfigs = new ProjectInfoItem("configs", $"configs", ProjectTypeEnum.Folder)
            {
                Description =
    @"ProjectSection(SolutionItems) = preProject
		shared\configs\connectionStrings.Development.json = shared\configs\connectionStrings.Development.json
		shared\configs\connectionStrings.Production.json = shared\configs\connectionStrings.Production.json
		shared\configs\serilog.Development.json = shared\configs\serilog.Development.json
		shared\configs\serilog.Production.json = shared\configs\serilog.Production.json
	EndProjectSection",
            };

            var p1 = new ProjectGenerator($"{name}.Common", fullPath);
            p1.ProjectFile(new[] { ("bgTeam.Core", settings.BgTeamVersion) });
            p1.Folder("Impl");
            p1.ClassTemplateFile("ITestService", $"Common{GeneratorHelper.Separator}ITestService");
            p1.ClassTemplateFile("TestService", $"Common{GeneratorHelper.Separator}Impl{GeneratorHelper.Separator}TestService", new[] { "Impl" });

            if (settings.IsWeb)
            {
                p1.Folder("Exceptions");
                p1.ClassTemplateFile("BadRequestException", $"Common{GeneratorHelper.Separator}BadRequestException", new[] { "Exceptions" });
            }

            var fp1 = new ProjectInfoItem(p1.Name, $"{path}{GeneratorHelper.Separator}{p1.Output}");

            var p2 = new ProjectGenerator($"{name}.DataAccess", fullPath);
            p2.ProjectFile(
                new[]
                {
                    ("bgTeam.Core", settings.BgTeamVersion),
                    ("bgTeam.DataAccess", settings.BgTeamVersion),
                    ("Microsoft.EntityFrameworkCore", settings.EntityFrameworkCoreVersion),
                    ("Microsoft.EntityFrameworkCore.Proxies", settings.EntityFrameworkCoreVersion),
                    ("Npgsql.EntityFrameworkCore.PostgreSQL", settings.EntityFrameworkCoreNpgsqlVersion),
                    ("Microsoft.AspNetCore.Identity.EntityFrameworkCore", settings.EntityFrameworkCoreNpgsqlVersion),
                }, projects: new[] { $"{name}.Domain" });
            p2.Folder("Impl");
            p2.ClassTemplateFile("TestQuery", $"DataAccess{GeneratorHelper.Separator}TestQuery", replist: new List<(string, string)> { ("$prj$", name) });
            p2.ClassTemplateFile("TestQueryContext", $"DataAccess{GeneratorHelper.Separator}TestQueryContext");
            p2.ClassTemplateFile("IEntityFrameworkRepository", $"DataAccess{GeneratorHelper.Separator}IEntityFrameworkRepository", replist: new List<(string, string)> { ("$prj$", name) });
            p2.ClassTemplateFile("IQueryLibrary", $"DataAccess{GeneratorHelper.Separator}IQueryLibrary");
            p2.ClassTemplateFile("ITransaction", $"DataAccess{GeneratorHelper.Separator}ITransaction");
            p2.ClassTemplateFile("AppDbContext", $"DataAccess{GeneratorHelper.Separator}Impl{GeneratorHelper.Separator}AppDbContext", new[] { "Impl" }, replist: new List<(string, string)> { ("$prj$", name) });
            p2.ClassTemplateFile("EntityFrameworkRepository", $"DataAccess{GeneratorHelper.Separator}Impl{GeneratorHelper.Separator}EntityFrameworkRepository", new[] { "Impl" }, replist: new List<(string, string)> { ("$prj$", name) });
            var fp2 = new ProjectInfoItem(p2.Name, $"{path}{GeneratorHelper.Separator}{p2.Output}");

            var p3 = new ProjectGenerator($"{name}.Domain", fullPath);
            p3.ProjectFile(new[]
            {
                ("bgTeam.Impl.Dapper", settings.BgTeamVersion),
                ("Microsoft.Extensions.Identity.Stores", settings.MicrosoftIdentityStoresVersion),
            });
            p3.Folder("Dto");
            p3.Folder("Entities");
            p3.Folder("UserEntity");
            p3.ClassTemplateFile("IEntity", $"Domain{GeneratorHelper.Separator}IEntity");
            p3.ClassTemplateFile("BaseEntity", $"Domain{GeneratorHelper.Separator}BaseEntity");
            p3.ClassTemplateFile("TestDto", $"Domain{GeneratorHelper.Separator}Dto{GeneratorHelper.Separator}TestDto", new[] { "Dto" });
            p3.ClassTemplateFile("Test", $"Domain{GeneratorHelper.Separator}Entities{GeneratorHelper.Separator}Test", new[] { "Entities" });
            p3.ClassTemplateFile("Role", $"Domain{GeneratorHelper.Separator}UserEntity{GeneratorHelper.Separator}Role", new[] { "UserEntity" });
            p3.ClassTemplateFile("RoleClaim", $"Domain{GeneratorHelper.Separator}UserEntity{GeneratorHelper.Separator}RoleClaim", new[] { "UserEntity" });
            p3.ClassTemplateFile("User", $"Domain{GeneratorHelper.Separator}UserEntity{GeneratorHelper.Separator}User", new[] { "UserEntity" });
            p3.ClassTemplateFile("UserClaim", $"Domain{GeneratorHelper.Separator}UserEntity{GeneratorHelper.Separator}UserClaim", new[] { "UserEntity" });
            p3.ClassTemplateFile("UserLogin", $"Domain{GeneratorHelper.Separator}UserEntity{GeneratorHelper.Separator}UserLogin", new[] { "UserEntity" });
            p3.ClassTemplateFile("UserRole", $"Domain{GeneratorHelper.Separator}UserEntity{GeneratorHelper.Separator}UserRole", new[] { "UserEntity" });
            p3.ClassTemplateFile("UserToken", $"Domain{GeneratorHelper.Separator}UserEntity{GeneratorHelper.Separator}UserToken", new[] { "UserEntity" });
            var fp3 = new ProjectInfoItem(p3.Name, $"{path}{GeneratorHelper.Separator}{p3.Output}");

            var p4 = new ProjectGenerator($"{name}.Story", fullPath);
            p4.ProjectFile(
                new[]
                {
                    ("bgTeam.Core", settings.BgTeamVersion),
                    ("bgTeam.DataAccess", settings.BgTeamVersion),
                }, projects: new[] { $"{name}.Domain" });
            p4.ClassTemplateFile("TestStory", $"Story{GeneratorHelper.Separator}TestStory", replist: new List<(string, string)> { ("$prj$", name) });
            p4.ClassTemplateFile("TestStoryContext", $"Story{GeneratorHelper.Separator}TestStoryContext");
            p4.ClassTemplateFile("IStoryLibrary", $"Story{GeneratorHelper.Separator}IStoryLibrary");
            var fp4 = new ProjectInfoItem(p4.Name, $"{path}{GeneratorHelper.Separator}{p4.Output}");

            var p5 = new ProjectGenerator($"{name}.Tests", $"{folder}{GeneratorHelper.Separator}tests");
            p5.ProjectFile(
                new[]
                {
                    ("bgTeam.Impl.MsSql", settings.BgTeamVersion),
                    ("Microsoft.NET.Test.Sdk", "16.7.1"),
                    ("xunit", "2.4.1"),
                    ("xunit.runner.visualstudio", "2.4.3"),
                }, projects: new[] { $"{name}.Story" },
                configs: true);
            p5.Folder("Common");
            p5.ClassTemplateFile("TestStoryTests", $"Tests{GeneratorHelper.Separator}TestStoryTests", replist: new List<(string, string)> { ("$prj$", name) });
            p5.ClassTemplateFile("FactoryTestService", $"Tests{GeneratorHelper.Separator}FactoryTestService", new[] { "Common" });
            p5.JsonTemplateFile("appsettings", $"Tests{GeneratorHelper.Separator}appsettings");
            p5.JsonTemplateFile("appsettings.Development", $"Web{GeneratorHelper.Separator}appsettings");
            p5.JsonTemplateFile("appsettings.Production", $"Web{GeneratorHelper.Separator}appsettings");
            var fp5 = new ProjectInfoItem(p5.Name, $"tests{GeneratorHelper.Separator}{p5.Output}", ProjectTypeEnum.Compile);

            fmain.AddChild(fp1);
            fmain.AddChild(fp2);
            fmain.AddChild(fp3);
            fmain.AddChild(fp4);
            ftest.AddChild(fp5);
            fshared.AddChild(fconfigs);

            result.Add(fapps);
            result.Add(fmain);
            result.Add(ftest);
            result.Add(fshared);
            result.Add(fsrv);
            result.Add(fconfigs);
            result.Add(fp1);
            result.Add(fp2);
            result.Add(fp3);
            result.Add(fp4);
            result.Add(fp5);

            if (settings.IsApp)
            {
                var p6 = new ProjectGenerator($"{name}.App", fullPath);
                p6.ProjectFile(
                    new[]
                    {
                        ("bgTeam.Core", settings.BgTeamVersion),
                        ("bgTeam.Impl.Dapper", settings.BgTeamVersion),
                        ("bgTeam.Impl.MsSql", settings.BgTeamVersion),
                        ("bgTeam.Impl.Serilog", settings.BgTeamVersion),
                        ("Scrutor", "3.2.2"),
                    }, type: "Exe",
                    projects: new[] { $"{name}.Story", $"{name}.DataAccess" },
                    configs: true);
                p6.ClassTemplateFile("AppSettings", $"App{GeneratorHelper.Separator}AppSettings");
                p6.ClassTemplateFile("AppIocConfigure", $"App{GeneratorHelper.Separator}AppIocConfigure", replist: new List<(string, string)> { ("$prj$", name) });
                p6.ClassTemplateFile("Program", $"App{GeneratorHelper.Separator}Program", replist: new List<(string, string)> { ("$prj$", name) });
                p6.ClassTemplateFile("Runner", $"App{GeneratorHelper.Separator}Runner", replist: new List<(string, string)> { ("$prj$", name) });
                p6.JsonTemplateFile("appsettings", $"App{GeneratorHelper.Separator}appsettings");
                p6.JsonTemplateFile("appsettings.Development", $"App{GeneratorHelper.Separator}appsettings");
                p6.JsonTemplateFile("appsettings.Production", $"App{GeneratorHelper.Separator}appsettings");
                p6.Folder("Properties");
                p6.JsonTemplateFile("launchSettings", $"App{GeneratorHelper.Separator}launchSettings", new[] { "Properties" }, new List<(string, string)> { ("$prj$", p6.Name) });
                var fp6 = new ProjectInfoItem(p6.Name, $"{path}{GeneratorHelper.Separator}{p6.Output}");
                //fmain.AddChild(fp6);
                fapps.AddChild(fp6);
                result.Add(fp6);
            }

            if (settings.IsWeb)
            {
                var p7 = new ProjectGenerator($"{name}.WebApp", fullPath);
                p7.ProjectFile(
                    new[]
                    {
                        ("bgTeam.Core", settings.BgTeamVersion),
                        ("bgTeam.Impl.Dapper", settings.BgTeamVersion),
                        ("bgTeam.Impl.MsSql", settings.BgTeamVersion),
                        ("Microsoft.AspNetCore.App", "2.2.8"),
                        ("Scrutor", "3.2.2"),
                    }, projects: new[] { $"{name}.Common", $"{name}.DataAccess", $"{name}.Story" });
                p7.ClassTemplateFile("AppSettings", $"App{GeneratorHelper.Separator}AppSettings");
                p7.ClassTemplateFile("AppIocConfigure", $"WebApp{GeneratorHelper.Separator}AppIocConfigure", replist: new List<(string, string)> { ("$prj$", name) });
                p7.ClassTemplateFile("AppMiddlewareException", $"WebApp{GeneratorHelper.Separator}AppMiddlewareException", replist: new List<(string, string)> { ("$prj$", name) });
                p7.Folder("Controllers");
                p7.ClassTemplateFile("HomeController", $"WebApp{GeneratorHelper.Separator}Controllers{GeneratorHelper.Separator}HomeController", new[] { "Controllers" }, new List<(string, string)> { ("$api-name$", $"{name} API") });
                p7.ClassTemplateFile("UserController", $"WebApp{GeneratorHelper.Separator}Controllers{GeneratorHelper.Separator}UserController", new[] { "Controllers" });
                var fp7 = new ProjectInfoItem(p7.Name, $"{path}{GeneratorHelper.Separator}{p7.Output}");
                fmain.AddChild(fp7);
                result.Add(fp7);

                var p8 = new ProjectGenerator($"{name}.Web", fullPath);
                p8.ProjectFile(
                    new[]
                    {
                        ("NSwag.AspNetCore", "13.8.2"),
                        ("Serilog.AspNetCore", "3.4.0"),
                        ("Microsoft.AspNetCore.Mvc.NewtonsoftJson", "3.1.7"),
                    },
                    "Microsoft.NET.Sdk.Web",
                    "Exe",
                    new[] { $"{name}.WebApp" },
                    true);
                p8.ClassTemplateFile("Program", $"Web{GeneratorHelper.Separator}Program", replist: new List<(string, string)> { ("$prj$", name) });
                p8.ClassTemplateFile("Startup", $"Web{GeneratorHelper.Separator}Startup", replist: new List<(string, string)> { ("$prj$", name), ("$api-name$", $"{name} API") });
                p8.JsonTemplateFile("appsettings", $"Web{GeneratorHelper.Separator}appsettings");
                p8.JsonTemplateFile("appsettings.Development", $"Web{GeneratorHelper.Separator}appsettings");
                p8.JsonTemplateFile("appsettings.Production", $"Web{GeneratorHelper.Separator}appsettings");
                p8.Folder("Properties");
                p8.JsonTemplateFile("launchSettings", $"Web{GeneratorHelper.Separator}launchSettings", new[] { "Properties" }, new List<(string, string)> { ("$prj$", p8.Name) });

                var fp8 = new ProjectInfoItem(p8.Name, $"{path}{GeneratorHelper.Separator}{p8.Output}");
                //fmain.AddChild(fp8);
                fapps.AddChild(fp8);
                result.Add(fp8);
            }

            var p9 = new ProjectGenerator($"{name}.StRunner", fullPath);
            p9.ProjectFile(
                new[]
                {
                    ("bgTeam.Core", settings.BgTeamVersion),
                    ("bgTeam.Queues", settings.BgTeamVersion),
                    ("bgTeam.StoryRunner", settings.BgTeamVersion),
                    ("Microsoft.AspNetCore", "2.2.0"),
                    ("Scrutor", "3.2.2"),
                },
                type: "Exe",
                projects: new[] { $"{name}.Story" },
                configs: true);
            p9.ClassTemplateFile("AppIocConfigure", $"StoryRunner{GeneratorHelper.Separator}AppIocConfigure", replist: new List<(string, string)> { ("$prj$", name) });
            p9.ClassTemplateFile("AppSettings", $"StoryRunner{GeneratorHelper.Separator}AppSettings");
            p9.ClassTemplateFile("Program", $"StoryRunner{GeneratorHelper.Separator}Program");
            p9.ClassTemplateFile("Runner", $"StoryRunner{GeneratorHelper.Separator}Runner");
            p9.JsonTemplateFile("appsettings", $"StoryRunner{GeneratorHelper.Separator}appsettings");
            p9.JsonTemplateFile("appsettings.Development", $"StoryRunner{GeneratorHelper.Separator}appsettings");
            p9.JsonTemplateFile("appsettings.Production", $"StoryRunner{GeneratorHelper.Separator}appsettings");
            var fp9 = new ProjectInfoItem(p9.Name, $"{path}{GeneratorHelper.Separator}{p9.Output}");
            fsrv.AddChild(fp9);
            result.Add(fp9);

            var p10 = new ProjectGenerator($"{name}.StScheduler", fullPath);
            p10.ProjectFile(
                new[]
                {
                    ("bgTeam.Core", settings.BgTeamVersion),
                    ("bgTeam.Queues", settings.BgTeamVersion),
                    ("bgTeam.DataAccess", settings.BgTeamVersion),
                    ("bgTeam.StoryRunnerScheduler", settings.BgTeamVersion),
                    ("Microsoft.AspNetCore", "2.2.0"),
                    ("Quartz", "3.3.2"),
                },
                type: "Exe",
                configs: true);
            p10.ClassTemplateFile("AppIocConfigure", $"StoryScheduler{GeneratorHelper.Separator}AppIocConfigure");
            p10.ClassTemplateFile("AppSettings", $"StoryScheduler{GeneratorHelper.Separator}AppSettings");
            p10.ClassTemplateFile("Program", $"StoryScheduler{GeneratorHelper.Separator}Program");
            p10.ClassTemplateFile("Runner", $"StoryScheduler{GeneratorHelper.Separator}Runner");
            p10.JsonTemplateFile("appsettings", $"StoryScheduler{GeneratorHelper.Separator}appsettings");
            p10.JsonTemplateFile("appsettings.Development", $"StoryScheduler{GeneratorHelper.Separator}appsettings");
            p10.JsonTemplateFile("appsettings.Production", $"StoryScheduler{GeneratorHelper.Separator}appsettings");
            p10.Folder("Configurations");
            p10.JsonTemplateFile("0_Examples", $"StoryScheduler{GeneratorHelper.Separator}0_Examples", new[] { "Configurations" });
            var fp10 = new ProjectInfoItem(p10.Name, $"{path}{GeneratorHelper.Separator}{p10.Output}");
            fsrv.AddChild(fp10);
            result.Add(fp10);

            return result;
        }

        private string ProjectsFile(IEnumerable<ProjectInfoItem> projects)
        {
            var str = new StringBuilder();

            str.AppendLine(
@"Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio Version 16
VisualStudioVersion = 16.0.30413.136
MinimumVisualStudioVersion = 10.0.40219.1");

            foreach (var prj in projects)
            {
                str.AppendLine($"Project(\"{{{prj.Type.GetDescription()}}}\") = \"{prj.Name}\", \"{prj.Path}\", \"{{{prj.Code}}}\"");

                if (!string.IsNullOrEmpty(prj.Description))
                {
                    str.AppendLine(prj.Description);
                }

                str.AppendLine("EndProject");
            }

            str.AppendLine("Global");
            str.AppendLine(
@"	GlobalSection(SolutionConfigurationPlatforms) = preSolution
		Debug|Any CPU = Debug|Any CPU
		Live|Any CPU = Live|Any CPU
		Release|Any CPU = Release|Any CPU
		Uat|Any CPU = Uat|Any CPU
	EndGlobalSection");

            str.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
            foreach (var prj in projects)
            {
                if (!prj.Build)
                {
                    continue;
                }

                str.AppendLine($"\t{{{prj.Code}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
                str.AppendLine($"\t{{{prj.Code}}}.Debug|Any CPU.Build.0 = Debug|Any CPU");
                str.AppendLine($"\t{{{prj.Code}}}.Live|Any CPU.ActiveCfg = Live|Any CPU");
                str.AppendLine($"\t{{{prj.Code}}}.Live|Any CPU.Build.0 = Live|Any CPU");
                str.AppendLine($"\t{{{prj.Code}}}.Release|Any CPU.ActiveCfg = Release|Any CPU");
                str.AppendLine($"\t{{{prj.Code}}}.Release|Any CPU.Build.0 = Release|Any CPU");
                str.AppendLine($"\t{{{prj.Code}}}.Uat|Any CPU.ActiveCfg = Uat|Any CPU");
                str.AppendLine($"\t{{{prj.Code}}}.Uat|Any CPU.Build.0 = Uat|Any CPU");
            }

            str.AppendLine("\tEndGlobalSection");

            str.AppendLine(
@"	GlobalSection(SolutionProperties) = preSolution
		HideSolutionNode = FALSE
	EndGlobalSection");

            str.AppendLine("\tGlobalSection(NestedProjects) = preSolution");
            foreach (var prj in projects)
            {
                foreach (var item in prj.ListChild)
                {
                    str.AppendLine($"\t\t{{{item}}} = {{{prj.Code}}}");
                }
            }

            str.AppendLine("\tEndGlobalSection");

            str.AppendLine(
@"    GlobalSection(ExtensibilityGlobals) = postSolution
		SolutionGuid = {1CEBAF23-BD02-4EA1-92B0-AF727714CF50}
	EndGlobalSection");

            str.AppendLine("EndGlobal");

            return str.ToString();
        }
    }
}
