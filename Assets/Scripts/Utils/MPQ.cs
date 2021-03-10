﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace Utils
{

    public enum MQP_HASH_CHAR
    {
        Low,
        Upper
    }

    public static class MPQ
    {
        private static uint[] cryptTable = new uint[0x500];
        private static bool m_IsInited = false;
        private static void Init() {
            uint seed = 0x00100001;
            for (int index1 = 0; index1 < 0x100; ++index1) {
                int index2 = index1;
                for (int i = 0; i < 5; ++i, index2 += 0x100) {
                    seed = (seed * 125 + 3) % 0x2AAAAB;
                    uint temp1 = (seed & 0xFFFF) << 0x10;

                    seed = (seed * 125 + 3) % 0x2AAAAB;
                    uint temp2 = (seed & 0xFFFF);

                    cryptTable[index2] = (temp1 | temp2);
                }

            }
        }

        public static void Create() {
            if (m_IsInited)
                return;
            Init();
            m_IsInited = true;
        }

        public unsafe static uint HashString(string fileName) {
            if (fileName == null)
                return 0;

            uint ulHash = 0xf1e2d3c4;
            int len = fileName.Length;
            fixed (char* ptr = fileName) {
                char* ch = ptr;
                for (int i = 0; i < len; ++i) {
                    ulHash <<= 1;
                    ulHash += *(ch++);
                }
            }

            return ulHash;
        }

        private unsafe static uint HashString(string fileName, int hashType, MQP_HASH_CHAR hashCharType = MQP_HASH_CHAR.Upper) {
            if (fileName == null)
                return 0;

            Create();

            uint seed1 = 0x7FED7FED; uint seed2 = 0xEEEEEEEE;
            int len = fileName.Length;
            fixed (char* ptr = fileName) {
                char* pChar = ptr;
                for (int i = 0; i < len; ++i) {
                    char c = *(pChar++);
                    switch (hashCharType) {
                        case MQP_HASH_CHAR.Low: {
                                if (c >= 'A' && c <= 'Z')
                                    c = Char.ToLower(c);
                                break;
                            }
                        case MQP_HASH_CHAR.Upper: {
                                if (c >= 'a' && c <= 'z')
                                    c = Char.ToUpper(c);
                                break;
                            }
                    }
                    int ch = (int)c;
                    seed1 = cryptTable[(hashType << 8) + ch] ^ (seed1 + seed2);
                    seed2 = (uint)ch + seed1 + seed2 + (seed2 << 5) + 3;
                }
            }

            return seed1;
        }

        public static ulong GetFileNameHash(string fileName) {
            const int /*HASH_OFFSET = 0,*/ HASH_A = 1, HASH_B = 2;
            uint low = HashString(fileName, HASH_A);
            uint high = HashString(fileName, HASH_B);
            ulong ret = (high << 32) | low;
            return ret;
        }
    }
}