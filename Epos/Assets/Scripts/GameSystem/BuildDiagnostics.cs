using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace GameSystem
{
    public sealed class BuildDiagnostics : MonoBehaviour
    {
        private const int MaxOverlayLines = 8;
        private static readonly Queue<string> OverlayLines = new();
        private static string _logPath;
        private static StreamWriter _writer;
        private static BuildDiagnostics _instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            if (_instance != null)
                return;

            var gameObj = new GameObject(nameof(BuildDiagnostics));
            DontDestroyOnLoad(gameObj);
            _instance = gameObj.AddComponent<BuildDiagnostics>();

            _logPath = Path.Combine(Application.persistentDataPath, "epos-runtime.log");
            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);
                _writer = new StreamWriter(_logPath, false, Encoding.UTF8) { AutoFlush = true };
                WriteLine($"Started {Application.productName} {Application.version}");
                WriteLine($"Unity {Application.unityVersion}");
                WriteLine($"Platform {Application.platform}");
                WriteLine($"DataPath {Application.dataPath}");
                WriteLine($"PersistentDataPath {Application.persistentDataPath}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"BuildDiagnostics failed to open log file: {e.Message}");
            }

            Application.logMessageReceived += OnLogMessageReceived;
        }

        private static void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            WriteLine($"[{type}] {condition}");
            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                WriteLine(stackTrace);
                EnqueueOverlay(condition);
            }
        }

        public static void Log(string message)
        {
            WriteLine($"[Diagnostics] {message}");
        }

        private static void WriteLine(string message)
        {
            var line = $"{DateTime.Now:HH:mm:ss.fff} {message}";
            _writer?.WriteLine(line);
        }

        private static void EnqueueOverlay(string message)
        {
            OverlayLines.Enqueue(message);
            while (OverlayLines.Count > MaxOverlayLines)
                OverlayLines.Dequeue();
        }

        private void OnGUI()
        {
            if (!Debug.isDebugBuild || OverlayLines.Count == 0)
                return;

            const int width = 980;
            GUILayout.BeginArea(new Rect(20f, 20f, width, 260f), GUI.skin.box);
            GUILayout.Label($"Runtime log: {_logPath}");
            foreach (var line in OverlayLines)
                GUILayout.Label(line);
            GUILayout.EndArea();
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            _writer?.Dispose();
            _writer = null;
        }
    }
}
