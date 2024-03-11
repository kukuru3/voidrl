using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using ScannerBtn = Scanner.Button;
using UnityBtn = UnityEngine.UI.Button;

namespace Scanner.Atomship {
    internal class StructureBrowserView : MonoBehaviour {
        const string folder = "Data\\Structures";
        [SerializeField] RectTransform container;
        [SerializeField] GameObject prefab;
        [SerializeField] ScannerBtn saveBtn;
        [SerializeField] ScannerBtn newBtn;


        private void Start() {
            saveBtn.Clicked += Save;
            newBtn.Clicked += () => { Save(); New(); };
            RepopulateFileList();
        }

        private void RepopulateFileList() {
            var files = EnumerateFiles();
            foreach (Transform child in container) Destroy(child.gameObject);
            foreach (var f in files) {
                var fi = new FileInfo(f);
                var n = Path.GetFileNameWithoutExtension(fi.Name);
                var gob = Instantiate(prefab, container);
                gob.GetComponentInChildren<TMPro.TMP_Text>().text = n;
                var btn = gob.GetComponentInChildren<UnityBtn>();
                btn.onClick.AddListener(() => {
                    var fn = fi.FullName;
                    SelectFile(fn);
                });
            }
        }

        string currentName = "";

        void Save() {
            var m = GetComponent<ModuleEditor>().CurrentModel;
            var blob = ModelSerializer.Serialize(m);
            var path = Path.Combine(folder, $"{currentName}.structure");
            File.WriteAllBytes(path, blob);
            RepopulateFileList();
        }

        void New() {
            currentName = FindAvailableFileName();
            GetComponent<ModuleEditor>().CreateNew();
            Save();
        }

        string FindAvailableFileName() {
            for (var i = 1; i < 100; i++) {
                var filename = $"new file {i}";
                var n = Path.Combine(folder, $"{filename}.structure");
                if (!File.Exists(n)) return filename;
            }
            throw new NotImplementedException("No more room");
        }

        private void SelectFile(string fullName) { 
            try { 
                var blob = File.ReadAllBytes(fullName);
                currentName = Path.GetFileNameWithoutExtension(fullName);
                var sm = ModelSerializer.Deserialize(blob);
                GetComponent<ModuleEditor>().Replace(sm);
            } catch (Exception e) {
                Debug.LogException(e);
            }
        }

        IEnumerable<string> EnumerateFiles() {
            var dd = Directory.GetCurrentDirectory();

            var finalPath = Path.Combine(dd, folder);

            var di = new DirectoryInfo(finalPath);

            var structFiles = di.EnumerateFiles("*.structure", SearchOption.AllDirectories);

            return structFiles.Select(f => f.FullName);
        }
    }
}
