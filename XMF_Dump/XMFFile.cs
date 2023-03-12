// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMF_Dump
{
    public class XMFFile
    {
        public int version;

        public readonly List<SampleRegistry> sampleRegistry = new();

        public readonly List<byte> sectionIndexes = new();

        public int sampleCount;

        public int sectionCount;

        // Not sure what these are, one per sample.
        public readonly List<byte> controlFlags = new();

        public readonly List<InstructionSection> instructionSections = new();

        public void LoadFrom(BinaryReader reader)
        {
            version = reader.ReadByte();

            // load in the sample registry

            for (int i = 0; i < 256; i++)
            {
                var regEntry = new SampleRegistry();
                regEntry.LoadFrom(reader);
                if (regEntry.Length != 0)
                {
                    sampleRegistry.Add(regEntry);
                }
            }

            // loadin the section indexes
            // a section can be reused via this indirection
            bool endMarkerFound = false;
            for (int i = 0; i < 256; i++)
            {
                var b = reader.ReadByte();
                if (!endMarkerFound)
                {
                    if (b != 255)
                    {
                        sectionIndexes.Add(b);
                    }
                    else
                    {
                        endMarkerFound = true;
                    }
                }
            }

            sampleCount = reader.ReadByte() + 1;
            
            sectionCount = reader.ReadByte() + 1;

            for (int i = 0; i < sampleCount; i++)
            {
                controlFlags.Add(reader.ReadByte());
            }

            for (int sectionIndex = 0; sectionIndex < sectionCount; sectionIndex++)
            {
                var section = new InstructionSection();
                instructionSections.Add(section);

                for (int i = 0; i < 64; i++)
                {
                    var row = new InstructionRow();
                    section.rows.Add(row);

                    for (int j = 0; j < sampleCount; j++)
                    {
                        var instr = new Instruction();
                        row.columns.Add(instr);

                        instr.LoadFrom(reader);
                    }
                }
            }

            foreach (var regEntry in sampleRegistry)
            {
                reader.Read(regEntry.sampleBytes);
            }
        }
    }

    public class SampleRegistry
    {
        public long offset1;
        public long offset2;
        public long offset3;
        public long offset4;

        public long Length { get { return offset4 - offset3;  } }

        public byte param0;
        public byte param1;

        public ushort frequency;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public byte[] sampleBytes;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public void LoadFrom(BinaryReader br)
        {
            offset1 = Read3Bytes(br);
            offset2 = Read3Bytes(br);
            offset3 = Read3Bytes(br);
            offset4 = Read3Bytes(br);
            param0 = br.ReadByte();
            param1 = br.ReadByte();
            frequency = br.ReadUInt16();
            sampleBytes = new byte[Length];
        }

        private int Read3Bytes(BinaryReader br)
        {
            return br.ReadUInt16() + br.ReadByte() * 65536;
        }
    }

    public class InstructionSection
    {
        public readonly List<InstructionRow> rows = new();
    }

    public class InstructionRow
    {
        // per sample instructions
        public readonly List<Instruction> columns = new();
    }

    public class Instruction
    {
        public byte note;
        public byte sampleNumber;
        public byte func1;
        public byte func2;
        public byte func2_Param;
        public byte func1_Param;

        public void LoadFrom(BinaryReader br)
        {
            note = br.ReadByte();
            sampleNumber = br.ReadByte();
            func1 = br.ReadByte();
            func2 = br.ReadByte();
            func2_Param = br.ReadByte();
            func1_Param = br.ReadByte();
        }
    }

}
