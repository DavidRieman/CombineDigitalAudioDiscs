using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TagLibFile = TagLib.File;

namespace CombineDigitalAudioDiscs
{
    public class Program
    {
        private static string dir;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting...");
            dir = args.Length > 0 ? args[0] : string.Empty;
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

            var files = FindFiles();
            if (!files.Any())
            {
                Console.WriteLine("Did not find any WMA or M4A files in that directory. Aborting.");
                return;
            }
            Console.WriteLine($"Found {files.Count()} applicable files. Processing...");

            // First fix any File Name issues, so we can query the new files list and save to the right place when done with the Tags fixes.
            if (Fixers.FixFileNameBasicReplacements(files))
            {
                files = FindFiles();
            }

            if (Fixers.FixFileNamesWithExtraNumbersBeforeChapters(files))
            {
                files = FindFiles();
            }

            var tagFiles = BuildTagFileList(files);
            Console.WriteLine($"Prepared tags for {tagFiles.Count} files.");
            Console.WriteLine("Checking for fixes that can be automatically applied.");
            Console.WriteLine("(No changes will be saved until the end, after a final confirmation prompt.)");
            bool haveChanges = false;

            Console.WriteLine();
            haveChanges = Fixers.FixDiskMarkersOnAlbumNames(tagFiles) || haveChanges;

            Console.WriteLine();
            haveChanges = Fixers.FixTitlesStartingWithNumbers(tagFiles) || haveChanges;

            Console.WriteLine();
            haveChanges = Fixers.FixTitlesStartingWithCh(tagFiles) || haveChanges;

            Console.WriteLine();
            haveChanges = Fixers.FixTitlesWithSingleDigitChapters(tagFiles) || haveChanges;

            Console.WriteLine();
            haveChanges = Fixers.FixTrackNumbers(tagFiles) || haveChanges;

            if (!haveChanges)
            {
                Console.WriteLine();
                Console.WriteLine("No changes were accepted. Done.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("Done applying fixes. Save all the chosen changes? [y/N]");
            if (Console.ReadKey().Key != ConsoleKey.Y)
            {
                Console.WriteLine("No changes will be saved to the files. Done.");
                return;
            }

            Console.WriteLine();
            SaveChanges(tagFiles);
        }

        private static void SaveChanges(List<TagLibFile> tagFiles)
        {
            Console.Write("Rewriting");
            tagFiles.ForEach(tagFile =>
            {
                tagFile.Save();
                Console.Write(".");
            });
            Console.WriteLine();
            Console.WriteLine("Done!");
        }

        private static List<TagLibFile> BuildTagFileList(IEnumerable<string> files)
        {
            return (from file in files
                    let tag = TagLibFile.Create(file)
                    orderby tag.Tag.Title ascending
                    select tag).ToList();
        }

        private static IEnumerable<string> GetFilesByExtensions(params string[] extensions)
        {
            foreach (var extension in extensions)
            {
                foreach (var file in Directory.GetFiles(dir, extension))
                {
                    yield return file;
                }
            }
        }

        private static IEnumerable<string> FindFiles()
        {
            return GetFilesByExtensions("*.wma", "*.m4a");
        }
    }
}
