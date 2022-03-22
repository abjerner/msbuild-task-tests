using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MyCustomTask
{
    public class MergeAppSettings : Microsoft.Build.Utilities.Task
    {
        public string Name { get; set; }
        public string Schema { get; set; }

        public override bool Execute()
        {

            if (string.IsNullOrWhiteSpace(Schema))
            {
                Log.LogMessage(MessageImportance.High, "No schema file specified.");
                return false;
            }

            string cwd = Directory.GetCurrentDirectory();
            
            string path1 = Path.Combine(cwd, "umbraco", "config", "appsettings-schema.json");
            string path2 = Path.Combine(cwd, Schema.TrimStart('/'));

            if (!File.Exists(path1))
            {
                Log.LogMessage(MessageImportance.High, "File '/umbraco/appsettings-schema.json' could not be found.");
                return false;
            }

            if (!File.Exists(path1))
            {
                Log.LogMessage(MessageImportance.High, $"File '{Schema}' could not be found.");
                return false;
            }


            uSync(new string[] { Name, path2, path1 });


            Log.LogMessage(MessageImportance.High, "Aloha");
            Log.LogMessage(MessageImportance.High, cwd);
            Log.LogMessage(MessageImportance.High, path1);
            Log.LogMessage(MessageImportance.High, path2);
            return true;
        }

        public static void uSync(string[] args)
        {


            var name = args[0];
            var source = Path.GetFullPath(args[1]);
            var target = Path.GetFullPath(args[2]);

            if (!File.Exists(source))
            {
                Console.WriteLine($"Cannot find source '{source}'");
                return;
            }

            if (!File.Exists(target))
            {
                Console.WriteLine($"Cannot find target '{target}'");
                return;
            }


            var json = JsonConvert.DeserializeObject<JToken>(File.ReadAllText(target));

            var properties = json["properties"];
            if (properties != null)
            {

                if (properties[name] != null)
                {
                    Console.WriteLine($"Property '{name}' already in schema");
                    return;
                }

                var refString = GetRefString(name, source, target);

                properties[name] = new JObject();
                properties[name]["$ref"] = refString;

                Console.WriteLine($">> Added '{refString}' to Json");
            }

            File.WriteAllText(target.Replace(".json", ".copy.json"), JsonConvert.SerializeObject(json, Formatting.Indented));
            Console.WriteLine($">> Saved {target}");


        }

        static string GetRefString(string name, string source, string target)
        {
            // Can't use 'GetRelativePath' method due to framework version
            //string relative = Path.GetRelativePath(source, source).Replace(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            string[] b = Path.GetDirectoryName(target).Split(Path.DirectorySeparatorChar);
            string[] c = source.Split(Path.DirectorySeparatorChar);

            string relative = null;

            for (int i = 0; i < Math.Min(c.Length, b.Length); i++)
            {

                if (b[i] != c[i]) break;

                relative = string.Empty;
                for (var j = 0; j < b.Length - i - 1; j++)
                {
                    relative += "../";
                }

                relative += string.Join("/", c.Skip(i + 1));

            }

            //Console.WriteLine();
            //Console.WriteLine(string.Join("/", b));
            //Console.WriteLine(string.Join("/", c));
            //Console.WriteLine(relative);
            //Console.WriteLine();

            return $"{relative}#/definitions/{name}";

        }

    }
}
