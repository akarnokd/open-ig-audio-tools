// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMF_Converter
{
    public class XMFFile
    {
        public byte version;

        public readonly List<SampleRegistry> sampleRegistry = new();

        public readonly List<byte> sectionIndexes = new();

        public readonly List<InstructionSection> instructionSections = new();

        public void LoadFrom(BinaryReader reader)
        {
            version = reader.ReadByte();


        }
    }

    public class SampleRegistry
    {
        public long offset0;
        public long offset2;
        public long offset3;
        public long offset4;

        public long Length { get { return offset4 - offset3;  } }

        public byte param0;
        public byte param1;

        public ushort frequency;

        public byte[] sampleBytes;
    }

    public class InstructionSection
    {
        public readonly List<InstructionRow> rows = new();
    }

    public class InstructionRow
    {
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
    }

}
