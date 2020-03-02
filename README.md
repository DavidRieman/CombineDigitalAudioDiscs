# CombineDigitalAudioDiscs

## Summary
This is a simple console program which helps rewrite certain key tags (starting with the "Album" and "Track" tags) to help get multiple-disc compilations to be treated as a single large album.  Mostly this is useful for combining audio book discs.

Basically, it will remove "[Disc ##]" from any "Album" tags in a folder, and re-number the "Track" tags into a single sequence.

This code can also serve as a nice kick-start example to build other automated audio tag editing.

## Why?
In the digital age, when you have media which originally spanned multiple physical discs, does it really matter that it did?  If your digital location has enough storage space, why should it inherit limitations caused by limited physical media storage space?
I don't know about you, but I'm not a fan of a media player requiring you to interrupt your activity (such as driving) to force it to "switch albums" into the correct, 23rd "disc" of your audio book. I just want it to keep playing the book. For example, my car is great at remembering where we left off (even when playing against a USB drive), but has absolutely no way to configure it to flow from one "disc" to the next when it has finished a "disc".

In actuality, we'd often much rather that large grouped compilations of audio would be treated as a "single album".  Unfortunately, luck may vary from one media player's CD-ripping capabilities to the next, and it's hard to find software that will automate _precicely_ what is needed to enable this scenario. You'd probably be facing a LOT of fiddly manipulation left to be done to both trick those discs as being one album while also getting the tracks to play in the correct order across various media-playing scenarios. (Generally media players don't give you an option to play in track-alphabetical-order over track-number-order, let alone also flowing correctly between multiple discs.)

In my case, using this tool and process, we can load our car up with large audio books (like the whole Harry Potter collection) onto a single USB drive, and we're good to go for a long road trip. The only interaction we need with the media player is to select which book.

## Requirements
This tool _currently_ works on Windows with .WMA files. That's just where I've tested it so far, but it is built on .NET Core against TagLibSharp, and seems like it should be trivial for this to work on other operating systems and against additional file types like MP3.
If you try this out for additional scenarios, I'd love to hear about it. Feel free to use the GitHub "Issues" section for results and similar discussions. Contributions are welcome.

## Process
This tool assumes you are already manually ripping CDs that you own as the source material. If you follow this process, you should end up with a set of files that most media players will acknowledge to be a single album that just has lots of tracks, and should play them in order.
* If you haven't yet, build CombineDigitalAudioDiscs to have a CombineDigitalAudioDiscs.exe.
* Rip each CD to your hard drive.
* Locate where your ripping process placed new folders for each disc.
* In each disc folder, view the "Title" attribute of the files.
* Ensure the "Title" attribute is in an ascending order. For example, a ripped audio book track may come out like "Chapter 01C: Did A Thing" where the "0" was an important aspect of getting all the tracks to be sortable by Title.
* If the ripping process failed to assign sensible, ordered Titles to all the tracks, you'll have to manually fix any that weren't good.
* You may as well drop anything like "[Disc 01]" from the first disc folder.
* Move all the files from all the other folders into that folder, and delete the empty folders.
* This may be a good opportunity to collapse any information you want to be static across all the files; For example on Windows 10 you can select all the files, right-click, properties, details, and ensure things you expect to be on ALL the files like "Album artist", "Genre", "Year" and such receive an identical string. Apply any desired changes and close.
* Run CombineDigitalAudioDiscs.exe with the first argument being the "quoted full file path of the combined audio folder", OR, run it with no arguments and you will be able to paste in that full path.
* Follow the prompts. This is one more chance to verify the file order should translate into new Track numbers before committing to rewriting tag information for all the files.
* If you plan to use software like Windows Media Player library or iTunes to further manage and get the files onto specific devices (like phones), you may need to remove the old versions and re-import the files there to pick up the changes efficiently. Else if you're just getting them onto a USB drive for consumption by some consumer audio player (like a car USB port), then just copy the folder onto your USB and test it out.

## Questions
Why is this a console program?
* In short, I didn't want to have a GUI and risk getting confused with the myriad of truly-horrible tag editing software and tag-editing modes out there already. I also want to keep the code really short, so you can read the full program in a quick sitting, and decide if you trust it to do what you need.

Can we add (new scenario) to this?
* Does it align well with the goals of getting audio-books and the like to register as a single album? The tool has one simple purpose and doesn't need much to accomplish that, and I don't want it to bloat out into lots of other purposes. Feel free to use the "Issues" section to discuss either way though.
