// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.Diagnostics;
using System.Xml.Serialization;
using ULT_Dump;
using XMF_Dump;

foreach (var file in Directory.EnumerateFiles(Directory.GetCurrentDirectory()))
{
    if (Path.GetExtension(file).ToUpper() == ".XMF")
    {
        Convert(file);
    }
}

void Convert(string fileName)
{
    Console.Write("Converting " + Path.GetFileName(fileName));

    var xmf = new XMFFile();

    using (BinaryReader br = new BinaryReader(new FileStream(fileName, FileMode.Open)))
    {
        xmf.LoadFrom(br);
    }

    var ult = new ULTFile();
    ult.magic = ULTFile.magicConst;
    ult.version4Digit = ULTFile.versionUlt2_2;
    ult.songTitle = Path.GetFileName(fileName);
    ult.songTexts.Add("Converted with XMF_Convert");
    ult.songTexts.Add("Open Imperium Galactica Project");

    int i = 1;
    foreach (var xmfSample in xmf.sampleRegistry)
    {
        var ultSample = new ULTSample();
        ultSample.name = "Sample " + i;
        ultSample.dosName = "Sample" + i + ".RAW";
        ultSample.loopStart = xmfSample.playbackShift;
        ultSample.loopEnd = xmfSample.startShift;
        ultSample.sizeStart = xmfSample.startOffset;
        ultSample.sizeEnd = xmfSample.endOffsetPlus1;
        ultSample.volume = xmfSample.volume;
        ultSample.bidiAndLoopFlags = xmfSample.bidiLoop;
        ultSample.finetuneSettings = 0;
        ultSample.frequency = xmfSample.frequency;
        ultSample.data = xmfSample.data;

        ult.samples.Add(ultSample);
        i++;
    }
    for (int j = 0; j < xmf.sectionIndexes.Count; j++)
    {
        ult.patternOrders.Add(xmf.sectionIndexes[j]);
        ult.activePatterns++;
    }
    for (int j = xmf.sectionIndexes.Count; j < 256; j++)
    {
        ult.patternOrders.Add(0xFF);
    }
    ult.tracks = xmf.sampleCount;
    ult.patterns = xmf.sectionCount;

    foreach (var pan in xmf.trackPans)
    {
        ult.trackPans.Add(pan);
        if (ult.trackPans.Count == ult.tracks)
        {
            break;
        }
    }

    ult.ResetTrackData();

    int sectionIndex = 0;
    byte[] globalVolume = new byte[ult.tracks];

    foreach (var section in xmf.instructionSections)
    {
        int rowIndex = 0;
        foreach (var row in section.rows)
        {
            int track = 0;
            foreach (var instr in row.columns)
            {
                // Global volume setting is not supported by the ULT format
                // We'll try to pre-multiply the individual Set Volume commands.
                var f1 = instr.func1;
                var f1p = instr.func1_Param;
                if (instr.func1 == 0x10)
                {
                    globalVolume[track] = instr.func1_Param;
                    f1 = 0x00;
                    f1p = 0x00;
                }
                if (instr.func1 == 0x0C)
                {
                    f1p = (byte)(f1p * globalVolume[track] / 15d);
                }

                var f2 = instr.func2;
                var f2p = instr.func2_Param;
                if (instr.func2 == 0x10)
                {
                    globalVolume[track] = instr.func2_Param;
                    f2 = 0x00;
                    f2p = 0x00;
                }
                if (instr.func2 == 0x0C)
                {
                    f1p = (byte)(f2p * globalVolume[track] / 15d);
                }

                ult.SetTrackData(sectionIndex, rowIndex, track,
                    instr.note, instr.sampleNumber, f1, f1p, f2, f2p
                );
                track++;
            }
            rowIndex++;
        }
        sectionIndex++;
    }

    using (BinaryWriter bw = new BinaryWriter(new FileStream(fileName + ".ULT", FileMode.Create)))
    {
        ult.SaveTo(bw);
    }
    /*
    var beatsPerMinute = 80d;
    var beatsPerSection = 64;
    var speed = 6d;
    
    var time = ult.activePatterns * beatsPerSection / beatsPerMinute / speed;

    var mins = (int)time;
    var seconds = (int)((time - (int)time) * 60);

    Console.Write(" | {0:00}:{1:00} | {2} ", mins, seconds, ult.activePatterns * beatsPerSection);
    */

    Console.WriteLine();
}

void RemapInstr(byte xmfInstr, byte xmfArg, out byte ultInstr, out byte ultArg)
{
    // Fast Tracker II - Set Global Volume instruction
    if (xmfInstr == 0x10)
    {
        // Not available in Ultratracker v4 so we set the sample volume instead?
        ultInstr = 0x00;
        ultArg = 0x00; // xmfArg;
        return;
    }
    ultInstr = xmfInstr;
    ultArg = xmfArg;
}