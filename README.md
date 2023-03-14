# open-ig-audio-tools
C# tools to work with the original Imperium Galactica sound/music files

# XMF_Convert.exe

Converts all *.XMF* files in the current directory into an UltraTracker v2.2 *.ULT* format, playable by VLC, as `<music_file_name>.ULT`.

:warning: The XMF has a Fast Tracker II-specific command (0x10 - Set Global Volume) which I don't know how to emulate within the ULT as it lacks this feature. (In fact, its encoding scheme has no room for this command as each ULT command must be in 0x00 .. 0x0F). Most likely I'd have to find another modern format similar to ULT.

# XMF_Extract.exe

Extract the audio samples from all  *.XMF* files of the current directory into 22050Hz WAV files as `<music_file_name>_<counter>.wav`.

# XMF_Dump.exe

Extract the music instructions from all *.XMF* files of the current directory into `<music_file_name>_Tracks.txt`.
