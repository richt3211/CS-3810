using System;
using System.Collections.Generic;
namespace test
{

    public static class Program
    {
        static void Main(string[] args)
        {
            // Direct mapped with have an 8 byte block with 8 rows
            SimulateCache(8, 1, 12);

            // Full associative will have a 16 byte block with 5 ways
            SimulateCache(16, 5, 12);

            //Set Associate with have a 32 byte block with 2 ways and 1 row. 
            SimulateCache(32, 2, 12);
        }
        static void SimulateCache(int dataBlockSize, int waySize, double _cyclesOnMiss)
        {

            int DATA_BLOCK_SIZE = dataBlockSize;
            int WAY_SIZE = waySize;
            double cyclesOnMiss = _cyclesOnMiss;

            // total number of bits 860
            int totalBits = 860;

            // number of block bits is block size * 8
            int blockBits = DATA_BLOCK_SIZE * 8;

            // number of lru bits is log base 2 of number of ways
            int LRU_BITS = (int)Math.Log(WAY_SIZE, 2);

            // number of row bits is number of (block bits + number of lru bits) * number of ways
            int totalRowBits = (blockBits + LRU_BITS) * WAY_SIZE;

            // number of rows is 860//number of row bits
            int ROW_NUMBERS = (int)Math.Pow(2,(int)Math.Log((totalBits / totalRowBits), 2));

            // number of bits in an Entry is number of block bits + valid bit + number of tag bits + number of lru bits
            int OFFSET_BITS = (int)Math.Log(DATA_BLOCK_SIZE, 2);

            int LRU_SIZE = (int)Math.Pow(2, LRU_BITS);


            Entry[,] cache = InitializeCache(ROW_NUMBERS, WAY_SIZE);

            // int[] addr = new int[] { 4, 8, 20, 24, 28, 36, 44, 20, 28, 36, 40, 44, 68, 72, 92, 96, 100, 104, 108, 112, 100, 112, 116, 120, 128, 140 };
            int[] addr = new int[] { 16, 20, 24, 28, 32, 36, 60, 64, 56, 60, 64, 68, 56, 60, 64, 72, 76, 92, 96, 100, 104, 108, 112, 120, 124, 128, 144, 148 };

            // run the cache twice for actual simulation
            for (int x = 0; x < 2; x++)
            {
                int hits = 0;
                int misses = 0;
                // Loop through each address in array
                for (int i = 0; i < addr.Length; i++)
                {
                    // tag = address/ (2^offset bits * 2^row bits)
                    int tag = addr[i] / ((DATA_BLOCK_SIZE * ROW_NUMBERS));

                    // row = (address/2^offset bits) % 2^row bits)
                    int row = (addr[i] / DATA_BLOCK_SIZE) % ROW_NUMBERS;

                    // offset = address/ 2^offset bits
                    int offset = addr[i] % DATA_BLOCK_SIZE;
                    // int offset = addr[i] % 8;

                    if (x == 1)
                        System.Console.Write("Accessing address: " + addr[i].ToString() + "(tag " + tag.ToString() + ", row " + row.ToString() + ", offset " + offset.ToString() + "):");

                    bool hit = false;
                    // Loop through each way in the cache
                    for (int j = 0; j < WAY_SIZE; j++)
                    {
                        // if the tag matches at the current row number, it's a hit
                        if (cache[row, j].tag == tag)
                        {
                            hit = true;
                            hits++;
                            ReorderWays(cache, row, j, LRU_SIZE);
                            if (x == 1)
                            {
                                System.Console.Write("Hit from row " + row.ToString() + " Way " + j.ToString());
                                System.Console.WriteLine();
                            }
                            break;
                        }
                    }
                    // if it's not a hit
                    if (hit == false)
                    {
                        misses++;
                        // find the least recently used way and update the data
                        // the least recently used value is a 0. 
                        int wayNumber = 0;
                        for (int j = 0; j < WAY_SIZE; j++)
                        {
                            // if it's the least recently used way, or it hasn't been used
                            if (cache[row, j].LRU == 0 || cache[row, j].LRU == 1)
                            {
                                wayNumber = j;

                                // clear the data in the existing cache
                                cache[row, j].data.Clear();

                                // Add however many pieces of data that are specified in the data block
                                // I'm adding just the value of the address, which I'm counting as one byte
                                for (int k = 0; k < DATA_BLOCK_SIZE; k++)
                                {
                                    cache[row, j].data.Add(addr[i]);
                                }
                                // Update the tag 
                                cache[row, j].tag = tag;

                                // Update the order
                                ReorderWays(cache, row, j, LRU_SIZE);
                                break;
                            }
                        }
                        if (x == 1)
                        {
                            System.Console.Write("Miss - Cached to row " + row.ToString() + ": Way " + wayNumber.ToString());
                            System.Console.WriteLine();
                        }
                    }
                    // PrintContentsOfCache(ROW_NUMBERS, WAY_SIZE, cache);
                }
                if (x == 1)
                    CalculateCpi(cyclesOnMiss, DATA_BLOCK_SIZE, hits, misses, addr);
            }

        }

        static void ReorderWays(Entry[,] cache, int row, int index, int Lru_Size)
        {
            // set that way to the most recently used
            cache[row, index].LRU = Lru_Size;

            // Loop through the ways again
            for (int k = 0; k < Lru_Size; k++)
            {
                // if we are not already at the current way
                // and the way's value has not been set (it's value is 0)
                // decrement the value of the LRU> 
                if ((index != k) && cache[row, k].LRU != 0)
                {
                    cache[row, k].LRU -= 1;
                }
            }
        }
        static Entry[,] InitializeCache(int rowNumbers, int waySize)
        {
            Entry[,] cache = new Entry[rowNumbers, waySize];
            for (int i = 0; i < rowNumbers; i++)
            {
                for (int j = 0; j < waySize; j++)
                {
                    cache[i, j] = new Entry();
                }
            }
            return cache;
        }
        static void PrintContentsOfCache(int rowNumbers, int waySize, Entry[,] cache)
        {
            System.Console.WriteLine("Here are the contents of the cache");
            for (int m = 0; m < rowNumbers; m++)
            {
                for (int n = 0; n < waySize; n++)
                {
                    Console.Write("Row Number: " + m.ToString());
                    Console.Write("\tWay Number: " + n.ToString());
                    Console.Write("\tTag: " + cache[m, n].tag.ToString());
                    Console.Write("\tValid: " + cache[m, n].valid.ToString());
                    Console.Write("\tLRU: " + cache[m, n].LRU.ToString());
                    Console.Write("\tData: ");
                    foreach (int block in cache[m, n].data)
                    {
                        Console.Write(block.ToString() + ", ");
                    }
                    System.Console.WriteLine();
                }
            }
        }


        static void CalculateCpi(double cyclesOnMiss, int dataBlockSize, int hits, int misses, int[] addr)
        {
            double cpiOnHit = 1;
            double cpiOnMiss = cyclesOnMiss + dataBlockSize;
            double cpi = (hits * cpiOnHit + misses * cpiOnMiss) / addr.Length;
            System.Console.WriteLine("The cpi is " + cpi.ToString());
            System.Console.WriteLine();
            System.Console.WriteLine();
            System.Console.WriteLine();
        }
    }

    class Entry
    {
        public int tag;
        public List<int> data;
        public int valid;
        public int LRU;
        public Entry()
        {
            tag = -1;
            data = new List<int>();
            valid = -1;
            LRU = 0;
        }
        public void Add(int i)
        {
            this.data.Add(i);
        }
    }
}
