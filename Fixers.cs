using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TagLibFile = TagLib.File;

namespace CombineDigitalAudioDiscs
{
    public static class Fixers
    {
        public static bool FixFileNameBasicReplacements(IEnumerable<string> files)
        {
            var fileNameChanges = from file in files
                                  let basicFile = Path.GetFileNameWithoutExtension(file)
                                  let possibleReplacement = basicFile.Replace("Ch.", "Chapter ").Replace("  ", " ").Trim()
                                  where basicFile != possibleReplacement
                                  select new { file, newFile = Path.Combine(Path.GetDirectoryName(file), possibleReplacement + Path.GetExtension(file)) };
            if (!fileNameChanges.Any())
            {
                Console.Write("Verified no file names need basic replacement fix-ups.");
                return false;
            }

            Console.WriteLine("The following automatic File Name changes are available:");
            foreach (var change in fileNameChanges)
            {
                Console.WriteLine($"{Path.GetFileName(change.file)} => {Path.GetFileName(change.newFile)}");
            }

            Console.WriteLine($"Carefully check that the above changes are desired!");
            Console.WriteLine($"Rewrite file names as shown above? [y/N]");
            if (Console.ReadKey().Key != ConsoleKey.Y)
            {
                Console.WriteLine("Skipping this fix.");
                return false;
            }

            foreach (var change in fileNameChanges)
            {
                File.Move(change.file, change.newFile);
            }

            return true;
        }

        public static bool FixFileNamesWithExtraNumbersBeforeChapters(IEnumerable<string> files)
        {
            var fixes = from file in files
                        let basicFile = Path.GetFileNameWithoutExtension(file)
                        where basicFile.Length > 8 &&
                              char.IsNumber(basicFile[0]) &&
                              (char.IsNumber(basicFile[1]) || char.IsWhiteSpace(basicFile[1]) || basicFile[1] == '-') &&
                              (basicFile.Contains(" Chapter ") || basicFile.Contains(" Ch."))
                        let dir = Path.GetDirectoryName(file)
                        let ext = Path.GetExtension(file)
                        let indexChapter = basicFile.IndexOf(" Chapter ")
                        let indexCh = basicFile.IndexOf(" Ch.")
                        let indexDrop = indexChapter >= 0 ? indexChapter : indexCh
                        select new { file, newFile = Path.Combine(dir, (basicFile[indexDrop..]).Trim() + ext) };
            if (!fixes.Any())
            {
                Console.WriteLine("Verified that File Names do not have the Extra Front Number pattern (like ## Chapter ##).");
                return false;
            }

            Console.WriteLine($"These File Names seem to use an Extra Front Number pattern that sometimes occurs when burning the disc,");
            Console.WriteLine($"but can be problematic for name-based sorting compared to the Chapter ##A pattern.");
            Console.WriteLine($"Check if the following renames should occur:");
            foreach (var fix in fixes)
            {
                Console.WriteLine($"{Path.GetFileName(fix.file)} => {Path.GetFileName(fix.newFile)}");
            }

            Console.WriteLine($"Carefully check whether the above changes should be applied!");
            Console.WriteLine($"Rewrite file names as described above? [y/N]");
            if (Console.ReadKey().Key != ConsoleKey.Y)
            {
                Console.WriteLine("Skipping this fix.");
                return false;
            }

            foreach (var fix in fixes)
            {
                File.Move(fix.file, fix.newFile);
            }

            return true;
        }

        public static bool FixTrackNumbers(List<TagLibFile> tagFiles)
        {
            // First find out if any files have a track number that does not seem fixed yet.
            bool anyTrackNumberOrderGap = false;
            for (int i = 0; i < tagFiles.Count; i++)
            {
                if (tagFiles[i].Tag.Track != i + 1)
                {
                    anyTrackNumberOrderGap = true;
                    break;
                }
            }

            if (!anyTrackNumberOrderGap)
            {
                Console.WriteLine("Verified that Track numbers seem tightly incremental already.");
                return false;
            }

            Console.WriteLine($"The Track numbers are not tightly incremental, and can be automatically condensed.");
            Console.WriteLine($"Check that these files are listed in the intended track order:");
            Console.WriteLine($"{"FILE NAME",-45} = #   = TITLE TAG");
            tagFiles.ForEach(tagFile =>
            {
                var fileName = Path.GetFileNameWithoutExtension(tagFile.Name);
                Console.WriteLine($"{fileName,-45} = {tagFile.Tag.Track,-3} = {tagFile.Tag.Title}");
            });

            Console.WriteLine($"Carefully check that the ORDER of the above is correct!");
            Console.WriteLine($"Rewrite Track numbers as 1 through {tagFiles.Count}, in the displayed order? [y/N]");
            if (Console.ReadKey().Key != ConsoleKey.Y)
            {
                Console.WriteLine("Skipping this fix.");
                return false;
            }

            // Set the new track number, and print what is changing.
            for (int i = 0; i < tagFiles.Count; i++)
            {
                var tagFile = tagFiles[i];
                var fileName = Path.GetFileNameWithoutExtension(tagFile.Name);
                Console.WriteLine($"{fileName,-45} = {tagFile.Tag.Track,-3} = {tagFile.Tag.Title}");
                tagFile.Tag.Track = (uint)i + 1;
            }
            return true;
        }

        public static bool FixDiskMarkersOnAlbumNames(List<TagLibFile> tagFiles)
        {
            Regex findDiscMarker = new Regex("\\[Disc \\d+\\]|\\(Disce \\d+\\)");
            var tagsContainingDiscNumberInAlbumName = from tagFile in tagFiles
                                                      let album = tagFile.Tag.Album
                                                      where findDiscMarker.IsMatch(album)
                                                      select tagFile;
            if (!tagsContainingDiscNumberInAlbumName.Any())
            {
                Console.WriteLine("Verified that Album names do not contain [Disc ##] pattern.");
                return false;
            }

            Console.WriteLine("At least one Album name may contain [Disc ##] markers. Remove these? [y/N]");
            if (Console.ReadKey().Key != ConsoleKey.Y)
            {
                Console.WriteLine($"Skipping this fix.");
                return false;
            }

            // Remove the [Disc ##] from all affected tag files.
            tagsContainingDiscNumberInAlbumName.ToList().ForEach(tagFile =>
            {
                var newAlbum = findDiscMarker.Replace(tagFile.Tag.Album, string.Empty).Trim();
                Console.WriteLine($"Change: tagFile.Tag.Album => newAlbum");
                tagFile.Tag.Album = newAlbum;
            });
            return true;
        }

        public static bool FixTitlesStartingWithNumbers(List<TagLibFile> tagFiles)
        {
            var tagsStartingWithNumbers = from tagFile in tagFiles
                                          let title = tagFile.Tag.Title
                                          let firstChar = title.ToCharArray()[0]
                                          where char.IsDigit(firstChar)
                                          select tagFile;
            if (!tagsStartingWithNumbers.Any())
            {
                Console.WriteLine("Verified that Titles do not start with numbers.");
                return false;
            }

            Console.WriteLine($"The following Titles start with numbers. Check if these are all actually meant to be \"Chapter ...\"");
            tagsStartingWithNumbers.ToList().ForEach(tagFile =>
            {
                var fileName = Path.GetFileNameWithoutExtension(tagFile.Name);
                var newTitle = $"Chapter {tagFile.Tag.Title}";
                Console.WriteLine($"{tagFile.Tag.Title} => {newTitle}");
            });

            Console.WriteLine("Accept those title changes? [y/N]");
            if (Console.ReadKey().Key != ConsoleKey.Y)
            {
                Console.WriteLine($"Skipping this fix.");
                return false;
            }

            tagsStartingWithNumbers.ToList().ForEach(tagFile =>
            {
                tagFile.Tag.Title = $"Chapter {tagFile.Tag.Title}";
            });
            return true;
        }

        public static bool FixTitlesStartingWithCh(List<TagLibFile> tagFiles)
        {
            var tagsStartingWithCh = from tagFile in tagFiles
                                     let title = tagFile.Tag.Title
                                     where title.StartsWith("Ch.")
                                     select tagFile;
            if (!tagsStartingWithCh.Any())
            {
                Console.WriteLine("Verified that Titles do not start with \"Ch. \"");
                return false;
            }

            Console.WriteLine($"The following Titles start with \"Ch. `\". Check if these are all actually meant to be \"Chapter ...\"");
            tagsStartingWithCh.ToList().ForEach(tagFile =>
            {
                var newTitle = ReplaceChWithChapter(tagFile.Tag.Title);
                Console.WriteLine($"{tagFile.Tag.Title} => {newTitle}");
            });

            Console.WriteLine("Accept those title changes? [y/N]");
            if (Console.ReadKey().Key != ConsoleKey.Y)
            {
                Console.WriteLine($"Skipping this fix.");
                return false;
            }

            tagsStartingWithCh.ToList().ForEach(tagFile =>
            {
                tagFile.Tag.Title = ReplaceChWithChapter(tagFile.Tag.Title);
            });
            return true;
        }

        public static bool FixTitlesWithSingleDigitChapters(List<TagLibFile> tagFiles)
        {
            var tagsWithSingleDigitChapters = from tagFile in tagFiles
                                              let title = tagFile.Tag.Title
                                              where title.Length > 8 && title.StartsWith("Chapter ")
                                              let firstChar = title[8]
                                              where char.IsDigit(firstChar) && (title.Length == 9 || !char.IsDigit(title[9]))
                                              select tagFile;
            if (!tagsWithSingleDigitChapters.Any())
            {
                Console.WriteLine("Verified that Titles do not have single digit chapter numbers.");
                return false;
            }

            Console.WriteLine($"The following Titles have single digit chapter numbers. Check if these changes should be applied:");
            tagsWithSingleDigitChapters.ToList().ForEach(tagFile =>
            {
                var newTitle = InsertZeroForChapter(tagFile.Tag.Title);
                Console.WriteLine($"{tagFile.Tag.Title} => {newTitle}");
            });

            Console.WriteLine("Accept those title changes? [y/N]");
            if (Console.ReadKey().Key != ConsoleKey.Y)
            {
                Console.WriteLine($"Skipping this fix.");
                return false;
            }

            tagsWithSingleDigitChapters.ToList().ForEach(tagFile =>
            {
                tagFile.Tag.Title = InsertZeroForChapter(tagFile.Tag.Title);
            });
            return true;
        }

        private static string ReplaceChWithChapter(string s)
        {
            return "Chapter " + s.Substring(3).Trim();
        }

        private static string InsertZeroForChapter(string s)
        {
            return "Chapter 0" + s.Substring(8).Trim();
        }
    }
}
