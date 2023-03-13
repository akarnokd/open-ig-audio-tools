// Copyright (c) David Karnok, 2023
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMF_Dump
{
    /// <summary>
    /// Gravis Ultrasound Programming API.
    /// 
    /// http://archive.gamedev.net/archive/reference/articles/article448.html
    /// 
    /// Helps me understand the IO instructions I see in INTRO.EXE
    /// </summary>
    internal class GUS_API
    {
        /// <summary>
        /// Where the card's IO ports start.
        /// </summary>
        internal int port;

        /// <summary>
        /// Get a byte from the GUS internal memory.
        /// </summary>
        /// <param name="offset">The offset into the memory, 24 bits</param>
        /// <returns></returns>
        internal byte GUSPeek(int offset)
        {
            OutB(port + 0x103, 0x43);
            OutW(port + 0x104, offset & 0xFFFF);
            OutB(port + 0x103, 0x44);
            OutB(port + 0x105, offset >> 16);

            return InB(port + 0x107);
        }

        internal void GUSPoke(int offset, int value)
        {
            OutB(port + 0x103, 0x43);
            OutW(port + 0x104, offset & 0xFFFF);
            OutB(port + 0x103, 0x44);
            OutB(port + 0x105, offset >> 16);
            OutB(port + 0x107, value);
        }

        internal bool GUSProbe()
        {
            OutB(port + 0x103, 0x4C);
            OutB(port + 0x105, 0x00);
            GUSDelay();
            GUSDelay();
            OutB(port + 0x103, 0x4C);
            OutB(port + 0x105, 0x01);

            GUSPoke(0, 0xAA);
            GUSPoke(0x100, 0x55);
            return GUSPeek(0) == 0xAA;
        }

        internal void GUSFind()
        {
            for (int i = 0x210; i <= 0x280; i += 0x10)
            {
                port = i;
                if (GUSProbe())
                {
                    break;
                }
            }
        }

        internal bool GUSDetected()
        {
            return port >= 0x210 && port < 0x280;
        }

        /// <summary>
        /// Find out how much RAM is there, in bytes.
        /// 
        /// Should be eiter 256k, 512k, 768k or 1MB.
        /// </summary>
        /// <returns></returns>
        internal int GUSFindMem()
        {
            if (GUSSetAndGet(0x4_0000, 0xAA) != 0xAA)
            {
                return 0x4_0000;
            }
            if (GUSSetAndGet(0x8_0000, 0xAA) != 0xAA)
            {
                return 0x8_0000;
            }
            if (GUSSetAndGet(0xC_0000, 0xAA) != 0xAA)
            {
                return 0xC_0000;
            }
            return 0x10_0000;
        }

        internal byte GUSSetAndGet(int offset, int value)
        {
            GUSPoke(offset, value);
            return GUSPeek(offset);
        }

        /// <summary>
        /// Reads port 0x300 7 times
        /// </summary>
        internal void GUSDelay()
        {
            InB(0x300);
            InB(0x300);
            InB(0x300);
            InB(0x300);

            InB(0x300);
            InB(0x300);
            InB(0x300);
        }

        internal void GUSReset()
        {
            OutB(port + 0x103, 0x4C);
            OutB(port + 0x105, 0x01);
            GUSDelay();

            OutB(port + 0x103, 0x4C);
            OutB(port + 0x105, 0x07);

            OutB(port + 0x103, 0x0E);
            OutB(port + 0x105, 0xCE); // 14 OR $0C0 ??
        }

        /// <summary>
        /// Set the volume of a voice (channel)
        /// </summary>
        /// <param name="voice"></param>
        /// <param name="volume">Volume in log scale, 16 bits</param>
        internal void GUSSetVolume(int voice, int volume)
        {
            OutB(port + 0x102, voice);
            OutB(port + 0x102, voice);
            OutB(port + 0x102, voice);
            OutB(port + 0x103, 0x09);
            OutW(port + 0x104, volume);
        }

        /// <summary>
        /// Set the balance (stereo pan) of a voice/channel
        /// </summary>
        /// <param name="voice"></param>
        /// <param name="balance">0 - left, 7 - middle, 15 - right</param>
        internal void GUSSetBalance(int voice, int balance)
        {
            OutB(port + 0x102, voice);
            OutB(port + 0x102, voice);
            OutB(port + 0x102, voice);
            OutB(port + 0x103, 0x0C);
            OutB(port + 0x105, balance);
        }

        /// <summary>
        /// Set the frequency of a voice/channel
        /// </summary>
        /// <param name="voice"></param>
        /// <param name="frequency">The frequency, 16 bits</param>
        internal void GUSSetFrequency(int voice, int frequency)
        {
            OutB(port + 0x102, voice);
            OutB(port + 0x102, voice);
            OutB(port + 0x102, voice);
            OutB(port + 0x103, 0x0C);
            OutW(port + 0x104, frequency);
        }

        /// <summary>
        /// Play the sample addressed by vbegin,
        /// and parts between vstart and vend offsets.
        /// </summary>
        /// <param name="voice"></param>
        /// <param name="mode"></param>
        /// <param name="vbegin"></param>
        /// <param name="vstart"></param>
        /// <param name="vend"></param>
        internal void GUSPlayVoice(int voice, int mode, int vbegin, int vstart, int vend)
        {
            OutB(port + 0x102, voice);
            OutB(port + 0x102, voice);
            OutB(port + 0x102, voice); // ???

            OutB(port + 0x103, 0x0A);
            OutW(port + 0x104, (vbegin >> 7) & 8191);
            OutB(port + 0x103, 0x0A);
            OutW(port + 0x104, (vbegin & 0x7F) >> 8);

            OutB(port + 0x103, 0x02);
            OutW(port + 0x104, (vstart >> 7) & 8191);
            OutB(port + 0x103, 0x03);
            OutW(port + 0x104, (vstart & 0x7F) >> 8);

            OutB(port + 0x103, 0x04);
            OutW(port + 0x104, (vend >> 7) & 8191);
            OutB(port + 0x103, 0x05);
            OutW(port + 0x104, (vend & 0x7F) >> 8);

            OutB(port + 0x103, 0x00);
            OutB(port + 0x105, mode);

            // Example indicates these have to be also issued
            // otherwise the card doesn't play sound

            OutB(port + 0x000, 0x01);
            OutB(port + 0x103, 0x4C);
            OutB(port + 0x105, 0x03);
        }

        // Helper methods denoting port operations (do nothing, just so the examples compile).

        internal byte InB(int port) { return 0; }
        internal ushort InW(int port) { return 0; }

        /// <summary>
        /// Output a byte value (lower 8 bits of the int).
        /// </summary>
        /// <param name="port"></param>
        /// <param name="value"></param>
        internal void OutB(int port, int value) { }

        /// <summary>
        /// Output an ushort value (lower 16 bits of the int)
        /// </summary>
        /// <param name="port"></param>
        /// <param name="value"></param>
        internal void OutW(int port, int value) { }
    }
}
