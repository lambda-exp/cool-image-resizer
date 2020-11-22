using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using CommandLine;

namespace cool_image_resizer
{
    class Options
    {
        [Option('o', "postfixorig", Required = true, HelpText = "postfix of image file names to be changed. (ex: @2x.png)")]
        public string PostfixOrig { get; set; }

        [Option('d', "postfixdest", Required = true, HelpText = "postfix of destination image file names. (ex: @1x.png)")]
        public string PostFixDest { get; set; }

        [Option('s', "scale", Required = true, HelpText = "target scale. (ex: 0.5)")]
        public double Scale { get; set; }

        [Option('r', "replace", Required = false, Default = false, HelpText = "all original files will be deleted if 'true'.")]
        public bool Replace { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    Console.WriteLine("Resizing start...");
                    var root = Directory.GetCurrentDirectory();
                    Resize(root, options.PostfixOrig, options.PostFixDest, options.Scale, options.Replace);
                })
                .WithNotParsed(error =>
                {
                    Console.WriteLine(error.ToString());
                });
        }
        private static void Resize(string path, string postfixOrig, string postfixDest, double scale, bool deleteOrig)
        {
            Console.WriteLine($"Resize : {path}");

            var dirs = Directory.GetDirectories(path);
            Console.WriteLine($"  Directories: {dirs.Length}");

            var files = Directory.GetFiles(path, $"*{postfixOrig}");
            Console.WriteLine($"  Files: {files.Length}");

            foreach (var file in files)
            {
                using (var fileOrig = File.OpenRead(file))
                {
                    var image = Image.Load(fileOrig);
                    image.Mutate(p => p.Resize((int)(image.Width * scale), (int)(image.Height * scale)));

                    var filenameDest = $"{file.Replace(postfixOrig, postfixDest)}";
                    using var fileDest = File.OpenWrite(filenameDest);
                    image.SaveAsPng(fileDest);
                    Console.WriteLine($"    Image Resized: filenameDest");
                }

                if (deleteOrig)
                {
                    File.Delete(file);
                }
            }

            foreach (var dir in dirs) Resize(dir, postfixOrig, postfixDest, scale, deleteOrig); // resize recursively
        }
    }
}
