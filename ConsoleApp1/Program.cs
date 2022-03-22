using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace ConsoleApp1
{
    internal class Program
    {

        public static string Schema = "/App_Plugins/uSync/config/appsettings-usync-schema.json";

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Directory.SetCurrentDirectory(@"D:\Testing\MyCustomTask\MyCustomPackage.cs");









            string cwd = Directory.GetCurrentDirectory();
            //string cwd = @"D:\Testing\MyCustomTask\MyCustomPackage.cs";


            if (string.IsNullOrWhiteSpace(Schema))
            {
                Console.Error.WriteLine("No schema file specified.");
                return;
            }


            string path1 = Path.Combine(cwd, "umbraco", "config", "appsettings-schema.json");
            string path2 = Path.Combine(cwd, Schema.TrimStart('/'));

            if (!File.Exists(path1))
            {
                //Log.LogMessage(MessageImportance.High, "File '/umbraco/appsettings-schema.json' could not be found.");
                Console.Error.WriteLine("File '/umbraco/appsettings-schema.json' could not be found.");
                return;
            }

            if (!File.Exists(path1))
            {
                //Log.LogMessage(MessageImportance.High, $"File '{Schema}' could not be found.");
                Console.Error.WriteLine($"File '{Schema}' could not be found.");
                return;
            }


            uSync(new string[] { "uSync", path2, path1 });




            Console.WriteLine("hello");

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

            for (int i = 0; i < Math.Min(c.Length, b.Length); i++) {

                if (b[i] != c[i]) break;

                relative = string.Empty;
                for (var j = 0; j < b.Length - i - 1; j++) {
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
