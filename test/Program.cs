using System;
using System.Collections.Generic;
namespace test
{

    public static class Program
    {
        static void Main(string[] args)
        {
            // Direct mapped with have an 8 byte block with 8 rows
            // SimulateCache(64, 1, 12, true);

            // Full associative will have a 16 byte block with 6 ways
            // SimulateCache(64, 1, 12, false);

            //Set Associate with have a 16 byte block with 2 ways and 2 rows. 
            SimulateCache(64, 1, 12, true);
        }
        static void SimulateCache(int dataBlockSize, int waySize, double _cyclesOnMiss, bool hasRows)
        {

            int DATA_BLOCK_SIZE = dataBlockSize;
            System.Console.WriteLine("The Block Size is: " + DATA_BLOCK_SIZE.ToString());
            int WAY_SIZE = waySize;
            System.Console.WriteLine("The way size is: " + WAY_SIZE.ToString());
            double cyclesOnMiss = _cyclesOnMiss;
            System.Console.WriteLine("The number of cycles on miss is: " + cyclesOnMiss.ToString());

            // total number of bits 860
            int totalBits = 860;
            System.Console.WriteLine("Total number of bits is:" + totalBits.ToString());

            // number of block bits is block size * 8
            int blockBits = DATA_BLOCK_SIZE * 8;
            System.Console.WriteLine("Number of block bits: " + blockBits.ToString());

            // number of lru bits is log base 2 of number of ways
            int LRU_BITS = (int)Math.Log(WAY_SIZE, 2);
            System.Console.WriteLine("Number of Lru Bits is:" + LRU_BITS.ToString());

            // the size of the lru is 2^number of lru bits
            int LRU_SIZE = (int)Math.Pow(2, LRU_BITS);
            System.Console.WriteLine("The LRU size is: " + LRU_SIZE.ToString());


            // number of row bits is number of (block bits + number of lru bits) * number of ways
            int estimatedRowBits = (blockBits + LRU_BITS + 1) * WAY_SIZE;
            System.Console.WriteLine("Number of estimated row bits is: " + estimatedRowBits.ToString());

            // number of rows is 860//number of row bits
            int ROW_NUMBERS;
            int addressBits;
            if (hasRows)
            {
                ROW_NUMBERS = (int)Math.Pow(2, (int)Math.Log((totalBits / estimatedRowBits), 2));
                addressBits = (int)Math.Log(ROW_NUMBERS, 2);
                System.Console.WriteLine("Number of rows is: " + ROW_NUMBERS.ToString());
                System.Console.WriteLine("Number of address row bits is " + addressBits.ToString());
            }
            else
            {
                ROW_NUMBERS = 1;
                addressBits = 0;
                System.Console.WriteLine("Number of rows is: " + ROW_NUMBERS.ToString());
                System.Console.WriteLine("Number of address row bits is " + addressBits.ToString());

            }

            // number of bits in an Entry is number of block bits + valid bit + number of tag bits + number of lru bits
            int OFFSET_BITS = (int)Math.Log(DATA_BLOCK_SIZE, 2);
            System.Console.WriteLine("Number of offset bits is: " + OFFSET_BITS.ToString());

            // the number of tag bits is given by total number of bits - number of row bits - number of offset bits
            int tagBits =  16 - addressBits - OFFSET_BITS;
            System.Console.WriteLine("The number of tag bits is " + tagBits.ToString());

            // the total number of row bits is the tag bits + data block bits + valid bit + lru bits * way size
            int totalRowBits = (tagBits + blockBits + 1 + LRU_BITS) * WAY_SIZE * ROW_NUMBERS;
            System.Console.WriteLine("The total number of row bits is " + totalRowBits.ToString());


            Entry[,] cache = InitializeCache(ROW_NUMBERS, WAY_SIZE);


            int[] addr = new int[] { 4, 8, 20, 24, 28, 36, 44, 20, 28, 36, 40, 44, 68, 72, 92, 96, 100, 104, 108, 112, 100, 112, 116, 120, 128, 140 };
            // int[] addr = new int[] { 16, 20, 24, 28, 32, 36, 60, 64, 56, 60, 64, 68, 56, 60, 64, 72, 76, 92, 96, 100, 104, 108, 112, 120, 124, 128, 144, 148 };

            
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
                    {
                        // direct mapped
                        System.Console.Write("Accessing address: " + addr[i].ToString() + "(tag " + tag.ToString() + ", row " + row.ToString() + ", offset " + offset.ToString() + "):");
                    }



                    bool hit = false;
                    // Loop through each way in the cache
                    for (int j = 0; j < WAY_SIZE; j++)
                    {
                        // if the tag matches at the current row number, it's a hit
                        if (cache[row, j].tag == tag)
                        {
                            hit = true;
                            hits++;
                            if (x == 1)
                            {
                                // fully  associative
                                // System.Console.Write("Accessing address: " + addr[i].ToString() + "(tag " + tag.ToString() + ", row " + j.ToString() + ", offset " + offset.ToString() + "):");
                                // System.Console.Write("Hit from row " + j.ToString());

                                // direct mapped
                                System.Console.Write("Hit from row " + row.ToString());

                                // set associative
                                Console.Write(" Way " + j.ToString());

                                System.Console.WriteLine();
                            }
                            ReorderWays(cache, row, j, LRU_SIZE);
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
                            // fully associative
                            // System.Console.Write("Accessing address: " + addr[i].ToString() + "(tag " + tag.ToString() + ", row " + wayNumber.ToString() + ", offset " + offset.ToString() + "):");
                            // System.Console.Write("Miss - Cached to row " + wayNumber.ToString());

                            // direct mapped and set associative
                            System.Console.Write("Miss - Cached to row " + row.ToString());

                            // set associative
                            Console.Write(": Way " + wayNumber.ToString());
                            System.Console.WriteLine();
                        }
                    }
                    if (x == 1)
                    {
                        // PrintContentsOfCache(ROW_NUMBERS, WAY_SIZE, cache);
                    }
                }
                if (x == 1)
                {
                    CalculateCpi(cyclesOnMiss, DATA_BLOCK_SIZE, hits, misses, addr);
                    PrintContentsOfCache(ROW_NUMBERS, WAY_SIZE, cache);
                }
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
            // set associative
            Console.Write("Row \t Way \t Tag \t Data \t Valid \t LRU \n");

            // fully associative
            // Console.Write("Row \t Tag \t Data \t Valid \t LRU \n");

            // direct mapped
            // Console.Write("Row \t Tag \t Data \t Valid \n");



            for (int m = 0; m < rowNumbers; m++)
            {
                for (int n = 0; n < waySize; n++)
                {
                    // direct mapped
                    // Console.Write(m.ToString() + " \t" + cache[m, n].tag.ToString() + " \t"); 

                    // fully associative
                    // Console.Write(n.ToString() + " \t" + cache[m, n].tag.ToString() + " \t"); 

                    // set associative
                    Console.Write(m.ToString() + "\t" + n.ToString()  + " \t" + cache[m, n].tag.ToString() + " \t"); 


                    // fully associative and direct mapped
                    // foreach (int block in cache[m, n].data)
                    // {
                    //     Console.Write("-\t ");
                    // }

                    // set associative
                    Console.Write(cache[m,n].data.Count.ToString() + "\t");

                    if (cache[m, n].tag != -1)
                        cache[m, n].valid = 1;

                    // direct mapped
                    Console.Write(cache[m, n].valid.ToString() + "\t");

                    // fully associative and set associative
                    Console.Write(cache[m,n].LRU.ToString() + "\n"); 

                }
            }

            
        }


        static void CalculateCpi(double cyclesOnMiss, int dataBlockSize, int hits, int misses, int[] addr)
        {
            double cpiOnHit = 1;
            double cpiOnMiss = cyclesOnMiss + dataBlockSize;
            double cpi = (hits * cpiOnHit + misses * cpiOnMiss) / addr.Length;
            System.Console.WriteLine("Number of hits: " + hits.ToString());
            System.Console.WriteLine("Number of misses: " + misses.ToString());
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
