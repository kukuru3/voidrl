using System.Collections.Generic;
using System.Diagnostics;

namespace Void.Scripting {
    public static class ScriptAPI {
        static ScriptHost scriptHost;

        static public ScriptHost Host { get; private set; }

        static public void InitializePython() {
            scriptHost = new ScriptHost();
            scriptHost.PrepareScope();
            scriptHost.ExposeAssemblyToScripts("Void");

            var apiDir = K3.Paths.GetFolderOf("ScriptAPI");

            var directory = K3.Paths.GetFolderOf("Scripts");
            scriptHost.AddSearchPath(apiDir);
            scriptHost.LoadScriptFilesFromDirectory(apiDir);

            scriptHost.LoadScriptFilesFromDirectory(directory);
        }

        static List<MessagePump> pumps = new();

        static public MessagePump UIPump { get; private set; }

        static public void Register(MessagePump pump) {
            pumps.Add(pump);
            if (pump.id == "ui") UIPump = pump;
        }

        static public MessagePump GetMessagePump(string pumpID) {
            foreach (var p in pumps) 
                if (p.id == pumpID) 
                    return p;
            return null;
        }
    }

    public class MessagePump {
        public readonly string id;

        public delegate void GenericMessage(string id, object payload);

        public event GenericMessage OnMessage;
        public MessagePump(string id) {
            this.id = id;
        }

        // K3.Collections.Multidict<string, object> subscribers= new();

        public void Trigger(string msg_id, object payload = null) {
            OnMessage?.Invoke(msg_id, payload);
        }

        [UnityEditor.MenuItem("Void/Execute UI event test")]
        static void __TEST() {
            ScriptAPI.UIPump.Trigger("construct_options");
        }
    }
}
