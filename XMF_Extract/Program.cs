// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System.IO;
using System.Text;

Console.WriteLine("Open-IG XMF Converter");

int defaultFrequency = 22050;
int found = 0;
foreach (var file in Directory.EnumerateFiles(Directory.GetCurrentDirectory())) {
    if (Path.GetExtension(file).ToUpper() == ".XMF")
    {
        found++;
        Console.WriteLine("  Processing " + Path.GetFileName(file));

        var bytes = File.ReadAllBytes(file);

        var samples = bytes[0x1101] + 1;
        var tracks = bytes[0x1102] + 1;

        var sampleOffset = samples * tracks * 0x180 + samples + 0x1103;

        Console.WriteLine("    Samples: " + samples + ", Tracks: " + tracks);

        var sampleRepositoryOffset = 7;
        for (int i = 0; i < 256; i++)
        {
            var start = Get3Bytes(bytes, sampleRepositoryOffset);
            var end = Get3Bytes(bytes, sampleRepositoryOffset + 3);

            var len = end - start;
            if (len > 0)
            {
                var istr = string.Format("{0:000}.wav", i + 1);
                Console.WriteLine("       Sample " + istr + ", Offset: + " + string.Format("{0:X6}", sampleOffset) + ", Length: " + len);
                SaveAsWav(file + "_" + istr, bytes, sampleOffset, len, defaultFrequency);
            }
            else
            {
                break;
            }

            sampleRepositoryOffset += 16;
            sampleOffset += len;
        }
    }
}

if (found == 0)
{
    Console.WriteLine("No files with extension .XMF found.");
}

int Get3Bytes(byte[] buffer, int offset)
{
    return (buffer[offset] & 0xFF) + (buffer[offset + 1] & 0xFF) * 256 + (buffer[offset + 2] & 0xFF) * 65536;
}

void SaveAsWav(string filename, byte[] buffer, int offset, int len, int frequency)
{
    // Wav needs to be even sized.
    var outputLen = len;
    if ((outputLen & 1) != 0)
    {
        outputLen++;
    }
    
    using BinaryWriter writer = new(new FileStream(filename, FileMode.OpenOrCreate));

    writer.Write(Encoding.ASCII.GetBytes("RIFF"));
    writer.Write(outputLen + 36);
    writer.Write(Encoding.ASCII.GetBytes("WAVE"));
    writer.Write(Encoding.ASCII.GetBytes("fmt "));
    writer.Write(16);
    writer.Write((short)1); // audio format
    writer.Write((short)1); // channels
    writer.Write(frequency); // sample rate
    writer.Write(frequency); // byte rate
    writer.Write((short)1); // block alignment
    writer.Write((short)8); // bytes per sample
    writer.Write(Encoding.ASCII.GetBytes("data"));
    
    // turn signed PCM into unsigned PCM
    for (int i = offset; i < offset + len; i++)
    {
        writer.Write((byte)(((sbyte)buffer[i] + 128) & 0xFF));
    }

    // file must be even length
    if (len != outputLen)
    {
        writer.Write((byte)128);
    }
}