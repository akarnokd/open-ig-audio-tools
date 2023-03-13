﻿// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0


using ULT_Dump;

DumpULTFile("MAIN3.ULT");

void DumpULTFile(string filename)
{
    var ultFile = new ULTFile();

    using BinaryReader br = new(new FileStream(filename, FileMode.Open));

    ultFile.LoadFrom(br);

    Console.WriteLine("Magic: " + ultFile.magic);
    Console.WriteLine("Version: " + ultFile.version4Digit);
    Console.WriteLine("Title: " + ultFile.songTitle);
    foreach (var str in ultFile.songTexts)
    {
        Console.WriteLine("  " + str); 
    }
    Console.WriteLine("# of samples: " + ultFile.samples.Count);

    int i = 1;
    foreach (var smp in ultFile.samples)
    {
        Console.WriteLine(string.Format("  [{0:000}] {1} | {2} | {3:X8} - {4:X8} | FT {5,6} | Freq {6,6}",
                i, smp.name, smp.dosName, smp.sizeStart, smp.sizeEnd, smp.finetuneSettings, smp.frequency
            ));

        i++;
    }

    Console.WriteLine("# tracks: " + ultFile.tracks);
    Console.WriteLine("# patterns: " + ultFile.patterns);
}
