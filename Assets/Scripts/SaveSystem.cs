using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace LoD {
    public static class SaveSystem {
        private const string filename = "settings.dat";

        public static void SaveSettings(GameOptions data) {
            var formatter = new BinaryFormatter();
            var path = Path.Combine(Application.persistentDataPath, filename);

            using (var stream = new FileStream(path, FileMode.Create)) {
                formatter.Serialize(stream, data);
            }

            Debug.Log($"Saved to {path}.");
        }

        public static GameOptions LoadSettings() {
            var path = Path.Combine(Application.persistentDataPath, filename);
            if (File.Exists(path)) {
                var formatter = new BinaryFormatter();
                using (var stream = new FileStream(path, FileMode.Open)) {
                    return formatter.Deserialize(stream) as GameOptions;
                }
            }

            Debug.LogWarning($"No savefile found at {path}.");
            return new GameOptions();
        }
    }
}