﻿using System.Text;

// MIT License
//
// Copyright (c) 2017 Jitbit, 2022 TakoTako contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace TekaTeka._3rdParty
{
    public class MurmurHash2
    {
        public static uint Hash(string data)
        {
            return Hash(Encoding.UTF8.GetBytes(data));
        }

        public static uint Hash(byte[] data)
        {
            return Hash(data, 0xc58f1a7a);
        }

        private const uint m = 0x5bd1e995;
        private const int r = 24;

        public static uint Hash(byte[] data, uint seed)
        {
            var length = data.Length;
            if (length == 0)
                return 0;
            var h = seed ^ (uint)length;
            var currentIndex = 0;
            while (length >= 4)
            {
                var k = (uint)(data[currentIndex++] | (data[currentIndex++] << 8) | (data[currentIndex++] << 16) |
                               (data[currentIndex++] << 24));
                k *= m;
                k ^= k >> r;
                k *= m;

                h *= m;
                h ^= k;
                length -= 4;
            }

            switch (length)
            {
            case 3:
                h ^= (ushort)(data[currentIndex++] | (data[currentIndex++] << 8));
                h ^= (uint)(data[currentIndex] << 16);
                h *= m;
                break;
            case 2:
                h ^= (ushort)(data[currentIndex++] | (data[currentIndex] << 8));
                h *= m;
                break;
            case 1:
                h ^= data[currentIndex];
                h *= m;
                break;
            }

            h ^= h >> 13;
            h *= m;
            h ^= h >> 15;

            return h;
        }
    }
}
