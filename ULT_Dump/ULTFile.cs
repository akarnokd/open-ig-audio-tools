﻿// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULT_Dump
{
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

        internal byte[] trackData;

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

            /*
            trackData = new byte[patterns * tracks * 64 * 6];
            br.Read(trackData);

            for (int i = 0; i < tracks; i++)
            {
                var sample = samples[i];
                sample.data = new byte[sample.Length];
                br.Read(sample.data);
            }
            */
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
        internal uint loopStart;
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
            loopStart = br.ReadUInt32();
            loopEnd = br.ReadInt32();
            sizeStart = br.ReadInt32();
            sizeEnd = br.ReadInt32();
            volume = br.ReadByte();
            bidiAndLoopFlags = br.ReadByte();
            frequency = br.ReadUInt16();
            finetuneSettings = br.ReadUInt16();
        }

    }
}
