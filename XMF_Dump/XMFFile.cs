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
        /// <summary>
        /// Where should the playback start relative to
        /// <see cref="startOffset">startOffset</see>.
        /// </summary>
        public long playbackShift;
        /// <summary>
        /// Shift the <see cref="startOffset">startOffset</see>.
        /// </summary>
        public long startShift;
        /// <summary>
        /// Where the sample is loaded into the memory.
        /// </summary>
        public long startOffset;
        /// <summary>
        /// Where the sample ends in memory.
        /// </summary>
        public long endOffsetPlus1;

        public long Length { get { return endOffsetPlus1 - startOffset;  } }

        public byte param0;

        /// <summary>
        /// Voice control flags:
        /// <see cref="GUS_Voice_Control_Flags"/>
        /// </summary>
        public byte voiceControlFlags;

        public ushort frequency;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public byte[] sampleBytes;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public void LoadFrom(BinaryReader br)
        {
            playbackShift = Read3Bytes(br);
            startShift = Read3Bytes(br);
            startOffset = Read3Bytes(br);
            endOffsetPlus1 = Read3Bytes(br);
            param0 = br.ReadByte();
            voiceControlFlags = br.ReadByte();
            frequency = br.ReadUInt16();
            sampleBytes = new byte[Length];
        }

        private int Read3Bytes(BinaryReader br)
        {
            return br.ReadUInt16() + br.ReadByte() * 65536;
        }

        public string GetVoiceControlFlagsStr()
        {
            List<string> list = new();
            /*
            if ((voiceControlFlags & (byte)GUS_Voice_Control_Flags.Stop_Voice) != 0)
            {
                list.Add("Stop Voice");
            }
            if ((voiceControlFlags & (byte)GUS_Voice_Control_Flags.Voice_Stopped) != 0)
            {
                list.Add("Voice Stopped");
            }
            */
            if ((voiceControlFlags & (byte)GUS_Voice_Control_Flags.Voice_Data_Type_16_bit) != 0)
            {
                list.Add("16 Bit");
            }
            else
            {
                list.Add(" 8 Bit");
            }
            if ((voiceControlFlags & (byte)GUS_Voice_Control_Flags.Voice_Loop_Enable) != 0)
            {
                list.Add("Loop");
            }
            else
            {
                list.Add("Once");
            }

            if ((voiceControlFlags & (byte)GUS_Voice_Control_Flags.Voice_Bi_Directional_Enable) != 0)
            {
                list.Add("BiDi Playback");
            }
            else
            {
                list.Add("Forward Playback");
            }
            if ((voiceControlFlags & (byte)GUS_Voice_Control_Flags.Voice_Playback_Direction) != 0)
            {
                list.Add("Decreasing");
            }
            else
            {
                list.Add("Increasing");
            }

            return string.Join("|", list);
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

        public bool IsEmpty
        {
            get
            {
                return note == 0 && sampleNumber == 0 
                    && func1 == 0 && func1_Param == 0
                    && func2 == 0 && func2_Param == 0;
            }
        }
    }

}
