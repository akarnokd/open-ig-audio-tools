// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.Xml.Serialization;
using ULT_Dump;
using XMF_Dump;

Convert("MAIN1.XMF");

void Convert(string fileName)
{
    Console.WriteLine("Converting " + Path.GetFileName(fileName));

    var xmf = new XMFFile();

    using BinaryReader br = new BinaryReader(new FileStream(fileName, FileMode.Open));

    xmf.LoadFrom(br);

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
        ultSample.frequency = xmfSample.frequency;
        ultSample.data = xmfSample.data;
        i++;
    }
    for (int j = 0; j < xmf.sectionIndexes.Count; j++)
    {
        ult.patternOrders.Add(xmf.sectionIndexes[j]);
    }
    for (int j = xmf.sectionIndexes.Count; j < 256; j++)
    {
        ult.patternOrders.Add(0xFF);
    }
    ult.tracks = xmf.sampleCount;
    ult.patterns = xmf.sectionCount;

    foreach (var section in xmf.instructionSections)
    {
        foreach (var row in section.rows)
        {
            foreach (var instr in row.columns)
            {
                ult.AddTrackData(instr.note, instr.sampleNumber, instr.func1, instr.func2, instr.func2_Param, instr.func1_Param);
            }
        }
    }

    using BinaryWriter bw = new BinaryWriter(new FileStream(fileName + ".ULT", FileMode.Create));
    ult.SaveTo(bw);
}