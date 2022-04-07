//using Penguin.Collections;
//using Penguin.Collections.SerializationSettings;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.IO;
//using System.Threading;

//namespace Penguin.Vfs.Caches
//{
//    internal struct MemoryCache
//    {
//        public byte[] Data;
//        public ulong Key;
//    }

//    public class CachedFileManager
//    {
//        public const uint BLOCK_SIZE = 1_048_576;
//        public const uint CACHE_BLOCKS = 1000;
//        public const uint CACHE_SIZE = BLOCK_SIZE * CACHE_BLOCKS;
//        public const uint MEMORY_CACHE_BLOCKS = 100;
//        private const string CACHE_ROOT = "Cache\\Data";
//        private static DictionaryFile<ulong> BlockAllocationTable;
//        private static ListFile<ulong> OnDiskBlockAccess = new("BlockAllocationTable.list", new ULongSerialization(), false);
//        private static ConcurrentQueue<ulong> RetrievedBlockProcessingQueue = new();
//        private static ConcurrentDictionary<ulong, byte[]> BlocksToCache = new();
//        private static Stream CacheFileStream;
//        private static object CacheLock = new();
//        private static DictionaryFile<string, uint> IdDictionary;
//        private static int MemoryBlockPointer = 0;
//        private static MemoryCache[] MemoryCachedBlocks = new MemoryCache[MEMORY_CACHE_BLOCKS];
//        private static byte[] UsedBlocks = new byte[CACHE_BLOCKS];
//        private uint FileId;

//        private static int NextMemoryBlockPointer
//        {
//            get
//            {
//                lock (CacheLock)
//                {
//                    MemoryBlockPointer++;
//                    if (MemoryBlockPointer >= MEMORY_CACHE_BLOCKS)
//                    {
//                        MemoryBlockPointer = 0;
//                    }

//                    return MemoryBlockPointer;
//                }
//            }
//        }

//        static CachedFileManager()
//        {
//            if (!Directory.Exists(CACHE_ROOT))
//            {
//                _ = Directory.CreateDirectory(CACHE_ROOT);
//            }

//            IdDictionary = new DictionaryFile<string, uint>(Path.Combine(CACHE_ROOT, "Ids.dict"), new StringSerialization(), new UIntSerialization());
//            BlockAllocationTable = new DictionaryFile<ulong>(Path.Combine(CACHE_ROOT, "BlockAllocationTable.dict"), new ULongSerialization());
//            CacheFileStream = File.Open(Path.Combine(CACHE_ROOT, "Cache.dat"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);

//            foreach (KeyValuePair<ulong, ulong> kvp in BlockAllocationTable)
//            {
//                uint blockNumber = (uint)(kvp.Value / BLOCK_SIZE);
//                UsedBlocks[blockNumber] = 1;
//            }

//            CacheProcessor = new BackgroundWorker();
//            CacheProcessor.DoWork += CacheProcessor_DoWork;
//            CacheProcessor.RunWorkerAsync();
//        }

//        private static void CacheProcessor_DoWork(object sender, DoWorkEventArgs e)
//        {
//            do
//            {
//                try
//                {
//                    do
//                    {
//                        _ = CacheProcessEvent.WaitOne();

//                        List<ulong> TempList = new((int)(CACHE_BLOCKS * 5));
//                        HashSet<ulong> NewBlocks = new((int)(CACHE_BLOCKS * 5));
//                        for(int i = 0; i < CACHE_BLOCKS; i++)
//                        {
//                            TempList.Add(ulong.Parse(OnDiskBlockAccess[i]));
//                        }

//                        while (!RetrievedBlockProcessingQueue.IsEmpty)
//                        {
//                            while (RetrievedBlockProcessingQueue.TryDequeue(out ulong o))
//                            {
//                                bool f = false;
//                                for(int i = 0; i < TempList.Count; i++)
//                                {
//                                    if(i == 0)
//                                    {
//                                        if(ulong.Parse(OnDiskBlockAccess[0]) == o)
//                                        {
//                                            break;
//                                        }
//                                    } else
//                                    {
//                                        ulong t = TempList[i];
//                                        TempList[i] = 0;
//                                        TempList[i - 1] = t;
//                                        f= true;
//                                        break;
//                                    }
//                                }

//                                if(!f)
//                                {
//                                    TempList.Add(o);
//                                    _ = NewBlocks.Add(o);
//                                }
//                            }
//                        }

//                        int p = 0;

//                        for (; p < Math.Min(TempList.Count, CACHE_BLOCKS); p++)
//                        {
//                            OnDiskBlockAccess[p] = TempList[p].ToString();
//                        }

//                        for(; p < TempList.Count; p++)
//                        {
//                            if(BlockAllocationTable.Remove(TempList[p], out ulong pblock))
//                            {
//                                int iblock = (int)(pblock / BLOCK_SIZE);
//                                UsedBlocks[pblock] = 0;
//                            } else
//                            {
//                                _ = NewBlocks.Remove(TempList[p]);
//                            }
//                        }

//                        foreach(ulong newBlock in NewBlocks)
//                        {
//                            lock (CacheLock)
//                            {
//                                if (BlocksToCache.TryRemove(newBlock, out byte[] value))
//                                {
//                                    _ = TryCacheData(newBlock, value);
//                                }
//                            }
//                        }
//                    } while (true);
//                } catch(Exception) { }
//            } while (true);

//        }

//        private static BackgroundWorker CacheProcessor = new();
//        private static AutoResetEvent CacheProcessEvent = new(true);

//        public CachedFileManager(string path) : this(GetId(path))
//        {
//        }

//        public CachedFileManager(uint id)
//        {
//            this.FileId = id;
//        }

//        public static ulong GetBlockAllocationKey(uint id, uint blockNumber) => ((ulong)id << 32) | blockNumber;

//        public static uint GetBlockNumber(long position) => (uint)(position / BLOCK_SIZE);

//        public static uint GetPosition(uint blockNumber) => blockNumber * BLOCK_SIZE;
//        public static void EnqueueDataCache(uint fileId, uint fileBlockNumber, byte[] data, int? length = null)
//        {
//            ulong key = GetBlockAllocationKey(fileId, fileBlockNumber);

//            lock(CacheLock)
//            {
//                _ = BlocksToCache.TryAdd(key, data);
//                RetrievedBlockProcessingQueue.Enqueue(key);
//            }

//            _ = CacheProcessEvent.Set();
//        }
//        private static bool TryCacheData(ulong key, byte[] data, int? length = null)
//        {
//            if (!length.HasValue)
//            {
//                length = data.Length;
//            }

//            lock (CacheLock)
//            {
//                uint freeBlock;

//                for (freeBlock = 0; freeBlock < CACHE_BLOCKS; freeBlock++)
//                {
//                    if (UsedBlocks[freeBlock] == 0)
//                    {
//                        break;
//                    }
//                }

//                if (freeBlock == CACHE_BLOCKS)
//                {
//                    return false;
//                }

//                UsedBlocks[freeBlock] = 1;

//                long freePos = GetPosition(freeBlock) + (BLOCK_SIZE - length.Value);

//                BlockAllocationTable.Add(key, (ulong)freePos);

//                CacheFileStream.Position = freePos;

//                CacheFileStream.Write(data, 0, length.Value);

//                MemoryCachedBlocks[NextMemoryBlockPointer] = new MemoryCache()
//                {
//                    Key = key,
//                    Data = data
//                };

//                return true;
//            }
//        }

//        public static bool TryGetBlock(string filePath, uint blockNumber, out byte[] data) => TryGetBlock(GetId(filePath), blockNumber, out data);

//        public static bool TryGetBlock(uint fileId, uint fileBlockNumber, out byte[] data)
//        {
//            ulong key = GetBlockAllocationKey(fileId, fileBlockNumber);

//            lock (CacheLock)
//            {
//                for (int i = 0; i < MEMORY_CACHE_BLOCKS; i++)
//                {
//                    int p = NextMemoryBlockPointer;

//                    if (MemoryCachedBlocks[p].Key == key)
//                    {
//                        data = MemoryCachedBlocks[p].Data;
//                        return true;
//                    }
//                }
//            }

//            if(BlocksToCache.TryGetValue(key, out data))
//            {
//                return true;
//            }

//            if (!BlockAllocationTable.TryGetValue(key, out ulong pos))
//            {
//                data = Array.Empty<byte>();
//                return false;
//            }

//            RetrievedBlockProcessingQueue.Enqueue(key);

//            lock (CacheLock)
//            {
//                CacheFileStream.Position = (long)pos;

//                ulong nextBlock = pos + (BLOCK_SIZE - (pos % BLOCK_SIZE));

//                long thisBlockLength = (long)(nextBlock - pos);
//                data = new byte[thisBlockLength];
//                _ = CacheFileStream.Read(data, 0, (int)thisBlockLength);

//                MemoryCachedBlocks[NextMemoryBlockPointer] = new MemoryCache()
//                {
//                    Key = key,
//                    Data = data
//                };
//            }

//            return true;
//        }

//        public void Flush()
//        {
//            lock (CacheLock)
//            {
//                CacheFileStream?.Flush();
//            }
//        }

//        public void EnqueueDataCache(byte[] data, uint fileBlockNumber, int? length = null) => EnqueueDataCache(this.FileId, fileBlockNumber, data, length);

//        public bool TryGetBlock(uint blockNumber, out byte[] data) => TryGetBlock(this.FileId, blockNumber, out data);

//        internal static uint GetId(string filePath)
//        {
//            lock (CacheLock)
//            {
//                if (!IdDictionary.TryGetValue(filePath, out uint v))
//                {
//                    uint count = (uint)IdDictionary.Count + 1;

//                    IdDictionary.Add(filePath, count);

//                    return count;
//                }

//                return v;
//            }
//        }
//    }
//}