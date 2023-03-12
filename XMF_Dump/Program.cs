// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using XMF_Dump;

foreach (var file in Directory.EnumerateFiles(Directory.GetCurrentDirectory()))
{
    if (Path.GetExtension(file).ToUpper() == ".XMF")
    {
        ProcessFile(file);
    }
}

void ProcessFile(string filename)
{
    Console.WriteLine(Path.GetFileName(filename));

    using var reader = new BinaryReader(new FileStream(filename, FileMode.Open));

    var xmf = new XMFFile();
    xmf.LoadFrom(reader);

    Console.WriteLine("  Sample Entries: " + xmf.sampleRegistry.Count);
    Console.WriteLine("  Sample Count (for music): " + xmf.sampleCount);
    Console.WriteLine("  Section count: " + xmf.sectionCount);
    Console.WriteLine("  Sections played: " + xmf.sectionIndexes.Count);
    Console.WriteLine("  Sample Registry:");
    int i = 0;
    foreach (var regEntry in xmf.sampleRegistry)
    {

        Console.WriteLine(string.Format("    [{0:000}] Length: {1}, P1: {2}, P2: {3}, Frequency: {4}",
            i + 1, regEntry.Length, regEntry.param0, regEntry.param1, regEntry.frequency));
        i++;
    }
    Console.WriteLine();
}