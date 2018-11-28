using System;
using System.Collections.Generic;
namespace test
{

    public static class Program
    {
        static void Main(string[] args)
        {

            // SimulateDirectMapped();
            SimulateSetAssociative();
        }
        static void SimulateSetAssociative()
        {
            // the set associative will have an 8 byte data block with 2 rows and 4 ways. 
            // Bit address outline: Tag -> 15-4, Row -> 3, Offset -> 2,0
            // Cache outline: Tag -> 12 bits, Data -> 8 bytes, Valid -> 1 bit, LRU -> 2 bits
            // Tag computation: address/(8*2)
            // Row computation: (address/8)%2
            // Offset computation: address%8


            int DATA_BLOCK_SIZE = 16;
            int WAY_SIZE = 1;
            double cyclesOnMiss = 12;

            // total number of bits 860
            int totalBits = 860;

            // number of block bits is block size * 8
            int blockBits = DATA_BLOCK_SIZE * 8;

            // number of lru bits is number of ways//2 + 1 if number of ways//2 isn't 0. 
            int LRU_BITS = 0;
            LRU_BITS = (int)Math.Log(WAY_SIZE, 2);

            // number of row bits is number of (block bits + number of lru bits) * number of ways
            int totalRowBits = (blockBits + LRU_BITS) * WAY_SIZE;

            // number of rows is 860//number of row bits
            int ROW_NUMBERS = (totalBits / totalRowBits) / 2 + 1;

            // number of bits in an entry is number of block bits + valid bit + number of tag bits + number of lru bits
            int OFFSET_BITS = (int)Math.Log(DATA_BLOCK_SIZE, 2);




            int LRU_SIZE = (int)Math.Pow(2, LRU_BITS);

            SetEntry[,] cache = new SetEntry[ROW_NUMBERS, WAY_SIZE];
            for (int i = 0; i < ROW_NUMBERS; i++)
            {
                for (int j = 0; j < WAY_SIZE; j++)
                {
                    cache[i, j] = new SetEntry();
                }
            }
            // int[] addr = new int[] { 4, 8, 20, 24, 28, 36, 44, 20, 28, 36, 40, 44, 68, 72, 92, 96, 100, 104, 108, 112, 100, 112, 116, 120, 128, 140 };
            int[] addr = new int[] { 16, 20, 24, 28, 32, 36, 60, 64, 56, 60, 64, 68, 56, 60, 64, 72, 76, 92, 96, 100, 104, 108, 112, 120, 124, 128, 144, 148};
            // Loop through each address in array
            for (int x = 0; x < 2; x++)
            {
                int hits = 0;
                int misses = 0;
                for (int i = 0; i < addr.Length; i++)
                {
                    // tag = address/ (2^offset bits * 2^row bits)
                    int tag = addr[i] / ((DATA_BLOCK_SIZE * ROW_NUMBERS));

                    // row = (address/2^offset bits) % 2^row bits)
                    int row = (addr[i] / DATA_BLOCK_SIZE) % ROW_NUMBERS;

                    // offset = address/ 2^offset bits
                    int offset = addr[i] % DATA_BLOCK_SIZE;
                    // int offset = addr[i] % 8;

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
                            System.Console.Write("Hit from row " + row.ToString() + " Way " + j.ToString());
                            System.Console.WriteLine();
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

                                // System.Console.WriteLine("The tag is " + cache[row,j].tag.ToString());

                                // Update the tag 
                                cache[row, j].tag = tag;

                                // Update the order
                                ReorderWays(cache, row, j, LRU_SIZE);
                                break;
                            }
                        }
                        System.Console.Write("Miss - Cached to row " + row.ToString() + ": Way " + wayNumber.ToString());
                        System.Console.WriteLine();


                    }
                    System.Console.WriteLine("Here are the contents of the cache");
                    for (int m = 0; m < ROW_NUMBERS; m++)
                    {
                        for (int n = 0; n < WAY_SIZE; n++)
                        {
                            Console.Write("Row Number: " + m.ToString());
                            Console.Write("\tWay Number: " + n.ToString());
                            Console.Write("\tTag: " + cache[m, n].tag.ToString());
                            Console.Write("\tValid: " + cache[m, n].valid.ToString());
                            Console.Write("\tLRU: " + cache[m, n].LRU.ToString());
                            Console.Write("\tData: ");
                            foreach (int block in cache[m,n].data)
                            {
                                Console.Write(block.ToString() + ", ");
                            }
                            System.Console.WriteLine();
                        }
                    }
                }
                double cpiOnHit = 1;

                double cpiOnMiss = cyclesOnMiss + DATA_BLOCK_SIZE;
                double cpi = (hits * cpiOnHit + misses * cpiOnMiss) / addr.Length;
                System.Console.WriteLine("The cpi is " + cpi.ToString());
                System.Console.WriteLine();
                System.Console.WriteLine();
                System.Console.WriteLine();
            }
            // for (int i = 0; i < 2; i++)
            // {
            //     for (int j = 0; j < 4; j++)
            //     {
            //         Console.Write("Row Number: " + i.ToString());
            //         Console.Write("\tWay Number: " + j.ToString());
            //         Console.Write("\tTag: " + cache[i, j].tag.ToString());
            //         Console.Write("\tValid: " + cache[i, j].valid.ToString());
            //         Console.Write("\tLRU: " + cache[i, j].LRU.ToString());
            //         Console.Write("\tData: ");
            //         foreach (int block in cache[i,j].data)
            //         {
            //             Console.Write(block.ToString() + ", ");
            //         }
            //         System.Console.WriteLine();
            //     }
            // }
        }
        static void SimulateDirectMapped()
        {
            // the direct mapped has a 16 byte entry with 4 rows. 
            // Bit address outline: Tag -> 15-6, Row -> 5-4, Offset -> 3, 0
            // Tag computation: address/16*4
            // Row computation: address/16%4
            // Offset computation: address%16
            DirectEntry[] cache = new DirectEntry[4];
            cache[0] = new DirectEntry();
            cache[1] = new DirectEntry();
            cache[2] = new DirectEntry();
            cache[3] = new DirectEntry();
            int[] addr = new int[] { 4, 8, 20, 24, 28, 36, 44, 20, 28, 36, 40, 44, 68, 72, 92, 96, 100, 104, 108, 112, 100, 112, 116, 120, 128, 140 };
            for (int i = 0; i < addr.Length; i++)
            {
                int tag = addr[i] / 64;
                int row = (addr[i] / 16) % 4;
                int offset = addr[i] % 16;
                System.Console.Write("Accessing address: " + addr[i].ToString() + "(tag " + tag.ToString() + ", row " + row.ToString() + ", offset " + offset.ToString() + "):");
                if (cache[row].tag == tag)
                {
                    System.Console.Write("Hit from row " + row.ToString());
                    System.Console.WriteLine();
                }
                else
                {
                    System.Console.Write("Miss - Cached to row " + row.ToString());
                    cache[row].tag = tag;
                    cache[row].data.Clear();
                    cache[row].data.Add(i);
                    cache[row].data.Add(i);
                    cache[row].data.Add(i);
                    cache[row].data.Add(i);
                    System.Console.WriteLine();
                }
            }
        }
        static void ReorderWays(SetEntry[,] cache, int row, int index, int Lru_Size)
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
    }
    class DirectEntry
    {
        public int tag;
        public List<int> data;
        public int valid;
        public DirectEntry()
        {
            tag = -1;
            data = new List<int>();
            valid = -1;
        }
        public void Add(int i)
        {
            this.data.Add(i);
        }
    }
    class SetEntry
    {
        public int tag;
        public List<int> data;
        public int valid;
        public int LRU;
        public SetEntry()
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
