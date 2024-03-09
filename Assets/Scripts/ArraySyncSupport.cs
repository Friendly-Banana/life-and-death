using System.IO;
using Mirror;

namespace LoD
{
    public static class ArraySyncSupport
    {
        public static void Write2DArray<T>(this NetworkWriter writer, T[][] array)
        {
            if (array is null)
            {
                writer.WriteInt(-1);
                return;
            }
            writer.WriteInt(array.Length);
            for (int i = 0; i < array.Length; i++)
                writer.WriteArray<T>(array[i]);
        }

        public static T[][] Read2DArray<T>(this NetworkReader reader)
        {
            int length = reader.ReadInt();

            //  we write -1 for null
            if (length < 0)
                return null;

            // this assumes that a reader for T reads at least 1 bytes
            // we can't know the exact size of T because it could have a user created reader
            // NOTE: don't add to length as it could overflow if value is int.max
            if (length > reader.Length - reader.Position)
            {
                throw new EndOfStreamException($"Received array that is too large: {length}");
            }

            T[][] result = new T[length][];
            for (int i = 0; i < length; i++)
            {
                result[i] = reader.ReadArray<T>();
            }
            return result;
        }
        
        public static void Write2DArray(this NetworkWriter writer, uint[][] array)
        {
            if (array is null)
            {
                writer.WriteInt(-1);
                return;
            }
            writer.WriteInt(array.Length);
            for (int i = 0; i < array.Length; i++)
                writer.WriteArray<uint>(array[i]);
        }

        public static uint[][] Read2DArray(this NetworkReader reader)
        {
            int length = reader.ReadInt();
            //  we write -1 for null
            if (length < 0)
                return null;

            // this assumes that a reader for T reads at least 1 bytes
            // we can't know the exact size of T because it could have a user created reader
            // NOTE: don't add to length as it could overflow if value is int.max
            if (length > reader.Length - reader.Position)
            {
                throw new EndOfStreamException($"Received array that is too large: {length}");
            }

            uint[][] result = new uint[length][];
            for (int i = 0; i < length; i++)
            {
                result[i] = reader.ReadArray<uint>();
            }
            return result;
        }
    }
}
