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

        Console.WriteLine(string.Format("    [{0:000}] Length: {1,6}, P1: {2,3}, Control: {3}, Frequency: {4}",
            i + 1, regEntry.Length, regEntry.volume, regEntry.GetVoiceControlFlagsStr(), regEntry.frequency));
        i++;
    }
    Console.WriteLine();

    using StreamWriter writer = new StreamWriter(filename + "_Tracks.txt");

    int rowCounter = 1;
    foreach (var trackId in xmf.sectionIndexes)
    {
        var section = xmf.instructionSections[trackId];

        foreach (var row in section.rows)
        {
            writer.Write(string.Format("[{0:0000}]  |  ", rowCounter));
            foreach (var instr in row.columns)
            {
                if (instr.IsEmpty)
                {
                    writer.Write("                          |  ");
                }
                else
                {
                    writer.Write(string.Format("{0:000} {1:000} [{2:X2}({3:X2}), {4:X2}({5:X2})]  |  ",
                        instr.note, instr.sampleNumber, instr.func1, instr.func1_Param, instr.func2, instr.func2_Param));
                }
            }
            writer.WriteLine();
            rowCounter++;
        }

        writer.WriteLine();

        var set = new HashSet<string>();
        var set2 = new HashSet<string>();
        foreach (var s in xmf.instructionSections)
        {
            foreach (var r in s.rows)
            {
                foreach (var instr in r.columns)
                {
                    if (instr.func1 == 16)
                    {
                        set.Add(string.Format("{0:X2} - {1:X2}", instr.func1, instr.func1_Param));
                    }
                    if (instr.func2 == 16)
                    {
                        set2.Add(string.Format("{0:X2} - {1:X2}", instr.func2, instr.func2_Param));
                    }
                }
            }
        }

        
        writer.WriteLine("--");

        foreach (var st in set)
        {
            writer.WriteLine(st);
        }

        writer.WriteLine("--");

        foreach (var st in set2)
        {
            writer.WriteLine(st);
        }
        writer.WriteLine("--");
    }
}