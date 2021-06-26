namespace bgTeam.ProjectTemplate
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using bgTeam.ProjectTemplate.FileGenerators;

    internal class ProjectGenerator
    {
        private readonly string _resPath;

        public ProjectGenerator(string name, string resPath)
        {
            Name = name;
            _resPath = resPath;

            var dir = new DirectoryInfo(Path.Combine(_resPath, Name));

            if (dir.Exists)
            {
                dir.Delete(true);
            }

            dir.Create();
        }

        public string Name { get; set; }

        public string Output { get; private set; }

        public void ProjectFile(
            (string Name, string Version)[] nugets,
            string sdk = "Microsoft.NET.Sdk",
            string type = "Library",
            string[] projects = null,
            bool configs = false)
        {
            var result = new StringBuilder(File.ReadAllText("./Resourse/Templates/project.txt"));

            result.Replace("$sdk$", sdk);
            result.Replace("$type$", type);

#if NETCOREAPP3_1
            result.Replace("$framework$", "netcoreapp3.1");
#else
            result.Replace("$framework$", "net5.0");
#endif

            var builder = new StringBuilder();

            builder.AppendLine("  <ItemGroup>");
            foreach (var item in nugets)
            {
                builder.AppendLine($"    <PackageReference Include=\"{item.Name}\" Version=\"{item.Version}\" />");
            }

            builder.AppendLine("  </ItemGroup>");

            result.Replace("$nugets$", builder.ToString());

            builder.Clear();

            if (projects != null)
            {
                builder.AppendLine("  <ItemGroup>");

                foreach (var item in projects)
                {
                    builder.AppendLine($"    <ProjectReference Include=\"..\\..\\src\\{item}\\{item}.csproj\" />");
                }

                builder.AppendLine("  </ItemGroup>");

                result.Replace("$projects$", builder.ToString());

                builder.Clear();
            }
            else
            {
                result.Replace("$projects$", string.Empty);
            }

            if (configs)
            {
                result.Replace("$configs$", File.ReadAllText("./Resourse/Templates/configs.txt"));
            }
            else
            {
                result.Replace("$configs$", string.Empty);
            }

            Output = $"{Name}/{Name}.csproj";

            GeneratorHelper.WriteAllText(Path.Combine(_resPath, Output), result.ToString());
        }

        public void ClassTemplateFile(string name, string pathTemp, string[] folderPath = null, IList<(string Key, string Value)> replist = null)
        {
            if (folderPath == null)
            {
                folderPath = Array.Empty<string>();
            }

            var result = new StringBuilder(File.ReadAllText($"./Resourse/TemplatesClass/{pathTemp}.txt"));

            var pathList = new List<string>
            {
                Name,
            };
            pathList.AddRange(folderPath);

            result.Replace("$namespace$", string.Join(".", pathList));

            if (replist != null && replist.Count > 0)
            {
                foreach (var item in replist)
                {
                    result.Replace(item.Key, item.Value);
                }
            }

            GeneratorHelper.WriteAllText($"{_resPath}/{Name}/{string.Join("/", folderPath)}/{name}.cs", result.ToString());
        }

        public void JsonTemplateFile(string name, string pathTemp, string[] folderPath = null, IList<(string Key, string Value)> replist = null)
        {
            if (folderPath == null)
            {
                folderPath = Array.Empty<string>();
            }

            var result = new StringBuilder(File.ReadAllText($"./Resourse/TemplatesClass/{pathTemp}.json"));

            if (replist != null && replist.Count > 0)
            {
                foreach (var item in replist)
                {
                    result.Replace(item.Key, item.Value);
                }
            }

            var filePath = $"{_resPath}{Path.DirectorySeparatorChar}{Name}{Path.DirectorySeparatorChar}{string.Join($"{Path.DirectorySeparatorChar}", folderPath)}{Path.DirectorySeparatorChar}{name}.json";
            GeneratorHelper.WriteAllText(filePath, result.ToString());
        }

        public void Folder(string folder)
        {
            Directory.CreateDirectory($"{_resPath}{Path.DirectorySeparatorChar}{Name}{Path.DirectorySeparatorChar}{folder}");
        }
    }
}