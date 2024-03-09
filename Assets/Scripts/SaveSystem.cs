using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace LoD
{
    public class SaveSystem
    {
        public const string filename = "settings.dat";

        public static void SaveSettings(GameOptions data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            string path = Path.Combine(Application.persistentDataPath, filename);

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                formatter.Serialize(stream, data);
            }
            Debug.Log($"Saved to {path}.");
        }

        public static GameOptions LoadSettings()
        {
            string path = Path.Combine(Application.persistentDataPath, filename);
            if (File.Exists(path))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    return formatter.Deserialize(stream) as GameOptions;
                }
            }
            else
            {
                Debug.LogWarning($"No savefile found at {path}.");
                return new GameOptions();
            }
        }
    }
}