using UnityEditor;
using UnityEngine;

namespace Scanner {

    public static class GameController {
        public static void ExitGame() {
            Application.Quit();
            #if UNITY_EDITOR
            EditorApplication.ExitPlaymode();
            #endif
        }

        public static void LaunchMenu() {

        }
    }
}