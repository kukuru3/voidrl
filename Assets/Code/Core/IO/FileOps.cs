using System.IO;
using UnityEngine;

namespace Core.IO {

    public static class FileOps {
        public static string GetDataFolder() {
            #if UNITY_EDITOR
            var di = new DirectoryInfo(Application.dataPath);
            return Path.Combine(di.Parent.FullName, "Data");
            #else
            return Path.Combine(Application.dataPath, "Data");
            #endif
        }

        public static string GetFile(params string[] filepath) {
            var p2 = Path.Combine(filepath);
            var p = Path.Combine(GetDataFolder(), p2);
            return p;
        }
    }
    
}
