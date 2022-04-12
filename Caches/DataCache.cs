using Penguin.Collections;
using Penguin.Collections.SerializationSettings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;

namespace Penguin.Vfs.Caches
{
    public static class DataCache
    {
        public const uint CNT_DSK_BLK = 1000;
        public const uint CNT_MEM_BLK = 100;
        public const uint LEN_BLK = 1_048_576;
        private const string CACHE_ROOT = "Cache\\Data";
        private static readonly Stream CACHE_DISK;
        private static readonly object CacheLock = new();

        //Make this not fucking auto flush
        private static readonly DictionaryFile<ulong> DataPositions;

        private static readonly Stream MEM_CACHE_DISK;
        private static readonly byte[][] CACHE_MEMORY = new byte[CNT_MEM_BLK][];
        private static readonly ConcurrentQueue<ulong> CACHE_REQUESTS = new();
        private static readonly List<byte[]> CACHE_TEMP = new();
        private static readonly AutoResetEvent GATE_TEMP_CACHE = new(false);
        private static readonly DictionaryFile<string, uint> IdDictionary;
        private static readonly BackgroundWorker TempCacheWorker;
        public static string AppRoot => System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, System.AppDomain.CurrentDomain.RelativeSearchPath ?? "");
        public static string CacheDirectory => Path.Combine(AppRoot, CACHE_ROOT);

        static DataCache()
        {
            if (!Directory.Exists(CacheDirectory))
            {
                _ = Directory.CreateDirectory(CacheDirectory);
            }

            CACHE_DISK = File.Open(Path.Combine(CacheDirectory, "BlockAllocationTable.dat"), FileMode.OpenOrCreate);
            MEM_CACHE_DISK = File.Open(Path.Combine(CacheDirectory, "BlockAllocationTable.mem"), FileMode.OpenOrCreate);
            IdDictionary = new(Path.Combine(CacheDirectory, "Ids.dict"), new StringSerialization(), new UIntSerialization());
            DataPositions = new(Path.Combine(CacheDirectory, "BlockAllocationTable.dict"), new ULongSerialization(), false);

            TempCacheWorker = new BackgroundWorker();
            TempCacheWorker.DoWork += TempCacheWorker_DoWork;
            TempCacheWorker.RunWorkerAsync();

            _ = MEM_CACHE_DISK.Seek(0, SeekOrigin.Begin);

            int c = 0;

            while (MEM_CACHE_DISK.Position < MEM_CACHE_DISK.Length)
            {
                CACHE_MEMORY[c] = new byte[LEN_BLK];
                _ = MEM_CACHE_DISK.Read(CACHE_MEMORY[c], 0, (int)LEN_BLK);
                c++;
            }
        }

        public static ulong GetBlockAllocationKey(uint id, uint blockNumber) => ((ulong)id << 32) | blockNumber;

        public static byte[] ReadBlock(string filePath, uint blockNumber, Stream sourceFile) => ReadBlock(GetFileId(filePath), blockNumber, sourceFile);

        public static byte[] ReadBlock(uint fileId, uint blockNumber, Stream sourceFile)
        {
            ulong key = GetBlockAllocationKey(fileId, blockNumber);
            long sourcepos = sourceFile.Position;
            lock (CacheLock)
            {
                try
                {
                    byte[] rawBlock = new byte[LEN_BLK];

                    if (DataPositions.TryGetValue(key, out ulong cacheLocation))
                    {
                        rawBlock = RetrieveBlock(cacheLocation);
                    }
                    else
                    {
                        _ = sourceFile.Seek(blockNumber * LEN_BLK, SeekOrigin.Begin);

                        _ = sourceFile.Read(rawBlock, 0, rawBlock.Length);

                        _ = QueueBlock(key, rawBlock);
                    }

                    return rawBlock;
                }
                finally
                {
                    CACHE_REQUESTS.Enqueue(key);
                    _ = GATE_TEMP_CACHE.Set();
                    sourceFile.Position = sourcepos;
                }
            }
        }

        public static void StoreBlock(ulong cacheLocation, byte[] data, bool skipTemp = true)
        {
            if (cacheLocation < CNT_MEM_BLK)
            {
                CACHE_MEMORY[cacheLocation] = data;
                _ = MEM_CACHE_DISK.Seek((long)(cacheLocation * LEN_BLK), SeekOrigin.Begin);
                MEM_CACHE_DISK.Write(data);
                return;
            }

            cacheLocation -= CNT_MEM_BLK;

            if (cacheLocation < CNT_DSK_BLK)
            {
                WriteBlockToDisk((uint)cacheLocation, data);
                return;
            }

            if (skipTemp)
            {
                return;
            }

            cacheLocation -= CNT_DSK_BLK;

            CACHE_TEMP[(int)cacheLocation] = data;
        }

        internal static uint GetFileId(string filePath)
        {
            lock (CacheLock)
            {
                if (!IdDictionary.TryGetValue(filePath, out uint v))
                {
                    uint count = (uint)IdDictionary.Count + 1;

                    IdDictionary.Add(filePath, count);

                    return count;
                }

                return v;
            }
        }

        private static uint QueueBlock(ulong key, byte[] data)
        {
            uint newLocation = 0;

            lock (CacheLock)
            {
                newLocation = (uint)CACHE_TEMP.Count + CNT_DSK_BLK + CNT_MEM_BLK;
                CACHE_TEMP.Add(data);
                DataPositions.Add(key, newLocation);
            }

            return newLocation;
        }

        private static byte[] ReadBlockFromDisk(uint blockNumber)
        {
            byte[] toReturn = new byte[LEN_BLK];

            lock (CacheLock)
            {
                _ = CACHE_DISK.Seek(blockNumber * LEN_BLK, SeekOrigin.Begin);
                _ = CACHE_DISK.Read(toReturn, 0, toReturn.Length);
            }

            return toReturn;
        }

        private static byte[] RetrieveBlock(ulong cacheLocation)
        {
            if (cacheLocation < CNT_MEM_BLK)
            {
                return CACHE_MEMORY[cacheLocation];
            }

            cacheLocation -= CNT_MEM_BLK;

            if (cacheLocation < CNT_DSK_BLK)
            {
                return ReadBlockFromDisk((uint)cacheLocation);
            }

            cacheLocation -= CNT_DSK_BLK;

            return CACHE_TEMP[(int)cacheLocation];
        }

        private static void TempCacheWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            do
            {
                try
                {
                    do
                    {
                        _ = GATE_TEMP_CACHE.WaitOne();

                        Monitor.Enter(CacheLock);

                        ulong[] workingCacheIndex = new ulong[CNT_MEM_BLK + CNT_DSK_BLK + CACHE_TEMP.Count];

                        foreach (KeyValuePair<ulong, ulong> kvp in DataPositions)
                        {
                            workingCacheIndex[kvp.Value] = kvp.Key;
                        }

                        while (!CACHE_REQUESTS.IsEmpty)
                        {
                            if (CACHE_REQUESTS.TryDequeue(out ulong key))
                            {
                                int emptyIndex = -1;

                                for (int i = 0; i < workingCacheIndex.Length; i++)
                                {
                                    if (i == 0 && workingCacheIndex[i] == key)
                                    {
                                        break;
                                    }

                                    if (workingCacheIndex[i] == 0 && emptyIndex < 0)
                                    {
                                        emptyIndex = i;
                                    }

                                    if (workingCacheIndex[i] == key)
                                    {
                                        if (emptyIndex >= 0)
                                        {
                                            workingCacheIndex[emptyIndex] = key;
                                            workingCacheIndex[i] = 0;
                                            break;
                                        }
                                        else
                                        {
                                            ulong t = workingCacheIndex[i - 1];
                                            workingCacheIndex[i - 1] = key;
                                            workingCacheIndex[i] = t;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        Dictionary<ulong, byte[]> transformationCache = new();

                        bool dataMoved = false;

                        for (ulong i = 0; i < CNT_DSK_BLK + CNT_MEM_BLK; i++)
                        {
                            ulong currentKey = workingCacheIndex[i];

                            if (currentKey == 0)
                            {
                                continue;
                            }

                            if (DataPositions[currentKey] != i)
                            {
                                byte[] t = RetrieveBlock(DataPositions[currentKey]);

                                transformationCache.Add(currentKey, t);

                                dataMoved = true;
                            }
                        }

                        for (ulong i = 0; i < CNT_DSK_BLK + CNT_MEM_BLK; i++)
                        {
                            ulong currentKey = workingCacheIndex[i];

                            if (currentKey == 0)
                            {
                                continue;
                            }

                            if (transformationCache.TryGetValue(currentKey, out byte[] data))
                            {
                                StoreBlock(i, data);
                                DataPositions[currentKey] = i;
                            }
                        }

                        for (int i = (int)(CNT_DSK_BLK + CNT_MEM_BLK); i < workingCacheIndex.Length; i++)
                        {
                            ulong currentKey = workingCacheIndex[i];

                            if (currentKey == 0)
                            {
                                continue;
                            }

                            _ = DataPositions.Remove(currentKey);
                        }

                        if (dataMoved)
                        {
                            DataPositions.Flush();
                        }

                        CACHE_TEMP.Clear();

                        Monitor.Exit(CacheLock);
                    } while (true);
                }
                catch (Exception)
                {
                }
            } while (true);
        }

        private static void WriteBlockToDisk(uint blockNumber, byte[] data)
        {
            lock (CacheLock)
            {
                _ = CACHE_DISK.Seek(blockNumber * LEN_BLK, SeekOrigin.Begin);
                CACHE_DISK.Write(data);
            }
        }
    }
}