// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULT_Dump
{
    /// <summary>
    /// https://fossies.org/linux/libxmp/src/loaders/ult_load.c
    /// </summary>
    internal class ULTFile
    {
        internal const string magicConst = "MAS_UTrack_";

        internal string magic;

        internal string version4Digit;

        internal const string versionUlt2_2 = "V004";

        internal string songTitle;

        internal readonly List<string> songTexts = new();

        internal readonly List<ULTSample> samples = new();

        internal readonly List<byte> patternOrders = new();

        internal int tracks;

        internal int patterns;

        internal readonly List<byte> trackPans = new();

        internal readonly List<byte> trackData = new();

        internal void SaveTo(BinaryWriter bw)
        {
            WriteChars(bw, magicConst.Length, magic);
            WriteChars(bw, 4, version4Digit);
            WriteChars(bw, 32, songTitle);
            bw.Write((byte)songTexts.Count);
            foreach (var st in songTexts)
            {
                WriteChars(bw, 32, st);
            }
            bw.Write((byte)samples.Count);
            foreach (var smp in samples)
            {
                smp.SaveTo(bw);
            }
            foreach (var po in patternOrders)
            {
                bw.Write(po);
            }
            bw.Write((byte)(tracks - 1));
            bw.Write((byte)(patterns - 1));
            foreach (var panPos in trackPans)
            {
                bw.Write(panPos);
            }

            foreach (var instr in trackData)
            {
                bw.Write(instr);
            }

            foreach (var smp in samples)
            {
                smp.SaveData(bw);
            }
        }

        internal void LoadFrom(BinaryReader br)
        {
            magic = ReadChars(br, magicConst.Length);
            version4Digit = ReadChars(br, 4);
            songTitle = ReadChars(br, 32);
            int songTextCount = br.ReadByte();
            for (int i = 0; i < songTextCount; i++)
            {
                songTexts.Add(ReadChars(br, 32));
            }
            int numSamples = br.ReadByte();

            for (int i = 0; i < numSamples; i++)
            {
                var sample = new ULTSample();
                sample.LoadFrom(br);
                samples.Add(sample);
            }
            
            for (int i = 0; i < 256; i++)
            {
                patternOrders.Add(br.ReadByte());
            }
            tracks = br.ReadByte() + 1;
            patterns = br.ReadByte() + 1;

            for (int i = 0; i < tracks; i++)
            {
                trackPans.Add(br.ReadByte());
            }

            var perTrackLength = patterns * 64 * 5;
            trackData.Capacity = perTrackLength * tracks;

            for (int i = 0; i < tracks; i++)
            {
                for (int j = 0; j < perTrackLength;)
                {
                    var b = br.ReadByte();
                    var cnt = 1;
                    if (b == 0xFC)
                    {
                        cnt = br.ReadByte();
                        b = br.ReadByte();
                    }
                    var f1 = br.ReadByte();
                    var f2 = br.ReadByte();
                    var f3 = br.ReadByte();
                    var f4 = br.ReadByte();

                    if (cnt == 0)
                    {
                        cnt++;
                    }

                    for (int k = 0; k < cnt; k++)
                    {
                        trackData.Add(b);
                        trackData.Add(f1);
                        trackData.Add(f2);
                        trackData.Add(f3);
                        trackData.Add(f4);
                    }
                    j += cnt * 5;
                }
            }

            foreach (var smp in samples)
            {
                smp.LoadData(br);
            }
        }

        internal void AddTrackData(byte note, byte instrument, byte f1, byte f2, byte f2Param, byte f1Param)
        {
            trackData.Add(note);
            trackData.Add(instrument);
            trackData.Add((byte)((f1 << 4) | (f2 & 0xF)));
            trackData.Add(f2Param);
            trackData.Add(f1Param);
        }

        internal static string ReadChars(BinaryReader br, int count)
        {
            StringBuilder sb = new();
            while (count-- != 0)
            {
                sb.Append((char)br.ReadByte());
            }
            return sb.ToString();
        }

        internal static void WriteChars(BinaryWriter bw, int count, string chars)
        {
            int n = Math.Min(count, chars.Length);
            for (int i = 0; i < n; i++)
            {
                bw.Write((byte)chars[i]);
            }

            for (int i = n; i < count; i++)
            {
                bw.Write((byte)0x20);
            }
        }
    }

    internal class ULTSample
    {
        internal string name;
        internal string dosName;
        internal int loopStart;
        internal int loopEnd;
        internal int sizeStart;
        internal int sizeEnd;
        internal byte volume;
        internal byte bidiAndLoopFlags;
        internal ushort finetuneSettings;
        internal ushort frequency;

        internal byte[] data;

        internal int Length
        {
            get
            {
                return sizeEnd - sizeStart;
            }
        }

        internal void LoadFrom(BinaryReader br)
        {
            name = ULTFile.ReadChars(br, 32);
            dosName = ULTFile.ReadChars(br, 12);
            loopStart = br.ReadInt32();
            loopEnd = br.ReadInt32();
            sizeStart = br.ReadInt32();
            sizeEnd = br.ReadInt32();
            volume = br.ReadByte();
            bidiAndLoopFlags = br.ReadByte();
            finetuneSettings = br.ReadUInt16();
            frequency = br.ReadUInt16();
        }

        internal void SaveTo(BinaryWriter bw)
        {
            ULTFile.WriteChars(bw, 32, name);
            ULTFile.WriteChars(bw, 12, dosName);
            bw.Write(loopStart);
            bw.Write(loopEnd);
            bw.Write(sizeStart);
            bw.Write(sizeEnd);
            bw.Write(volume);
            bw.Write(bidiAndLoopFlags);
            bw.Write(finetuneSettings);
            bw.Write(frequency);
        }

        internal void LoadData(BinaryReader br)
        {
            data = new byte[Length];
            br.Read(data);
        }

        internal void SaveData(BinaryWriter bw)
        {
            bw.Write(data);
        }
    }
}
