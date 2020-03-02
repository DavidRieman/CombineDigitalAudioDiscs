using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace CombineDigitalAudioDiscs
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            string dir = args.Length > 0 ? args[0] : string.Empty;
            if (string.IsNullOrWhiteSpace(dir))
            {
                Console.WriteLine("No path argument was given; What directory are we fixing Track numbers for?");
                dir = Console.ReadLine();
            }

            if (!Directory.Exists(dir))
            {
                Console.WriteLine($"Directory does not exist: {dir}. Aborting.");
                return;
            }

            var files = Directory.GetFiles(dir, "*.wma");
            if (files.Length == 0)
            {
                Console.WriteLine("Did not find any WMA files in that directory. Aborting.");
                return;
            }
            Console.WriteLine($"Found {files.Length} applicable files. Processing...");

            var tagFiles = (from file in files
                            let tag = TagLib.File.Create(file)
                            orderby tag.Tag.Title ascending
                            select tag).ToList();
            Console.WriteLine($"Prepared tags for {tagFiles.Count} files. Check the order:");
            Console.WriteLine($"{"FILE NAME",-45} = #   = TITLE TAG");
            tagFiles.ForEach(tagFile =>
            {
                var fileName = Path.GetFileNameWithoutExtension(tagFile.Name);
                Console.WriteLine($"{fileName,-45} = {tagFile.Tag.Track,-3} = {tagFile.Tag.Title}");
            });

            Console.WriteLine($"Check that the ORDER of them is correct. Rewrite Track numbers as 1 through {tagFiles.Count}? [y/N]");
            if (Console.ReadKey().Key != ConsoleKey.Y)
            {
                Console.WriteLine("Aborting.");
                return;
            }

            Regex findDiscMarker = new Regex("\\[Disc \\d+\\]|\\(Disce \\d+\\)");
            var anyAlbumContainsDiscNumber = (from tagFile in tagFiles
                                              let album = tagFile.Tag.Album
                                              where findDiscMarker.IsMatch(album)
                                              select album).Any();
            bool removeDiscMarkers = false;
            if (anyAlbumContainsDiscNumber)
            {
                Console.WriteLine("At least one Album name may contain [Disc ##] markers. Remove these?");
                removeDiscMarkers = Console.ReadKey().Key == ConsoleKey.Y;
            }

            Console.WriteLine("Processing and rewriting:");
            uint i = 1;
            tagFiles.ForEach(tagFile =>
            {
                tagFile.Tag.Track = i;
                if (removeDiscMarkers)
                    tagFile.Tag.Album = findDiscMarker.Replace(tagFile.Tag.Album, string.Empty).Trim();
                tagFile.Save();
                i++;
                Console.Write(".");
            });
            Console.WriteLine();
            Console.WriteLine("Done!");
        }
    }
}
