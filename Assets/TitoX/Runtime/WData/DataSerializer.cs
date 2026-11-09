using System;
using MemoryPack;
using UnityEngine;

namespace WData
{
    public static class DataSerializer
    {
        public static byte[] Serialize<T>(T data) where T : class
        {
            try
            {
                return MemoryPackSerializer.Serialize(data);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        public static T Deserialize<T>(byte[] data) where T : class
        {
            try
            {
                return MemoryPackSerializer.Deserialize<T>(data);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }
    }
}