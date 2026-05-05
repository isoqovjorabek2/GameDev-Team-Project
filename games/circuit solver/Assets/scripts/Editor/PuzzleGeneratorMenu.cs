#if UNITY_EDITOR
using System.IO;
using CircuitSolver.Data;
using CircuitSolver.Managers;
using UnityEditor;
using UnityEngine;

namespace CircuitSolver.EditorTools
{
    /// <summary>
    /// Editor menu that persists the ten default puzzles as .asset files
    /// under Assets/ScriptableObjects/Puzzles. Run it once and the runtime
    /// bootstrapper will pick those assets up from Resources/Puzzles
    /// (we also copy them into a Resources folder for convenience).
    /// </summary>
    public static class PuzzleGeneratorMenu
    {
        const string BaseDir = "Assets/ScriptableObjects/Puzzles";
        const string ResourcesDir = "Assets/Resources/Puzzles";

        [MenuItem("Circuit Solver/Generate Default Puzzles")]
        public static void Generate()
        {
            Directory.CreateDirectory(BaseDir);
            Directory.CreateDirectory(ResourcesDir);

            var puzzles = DefaultPuzzleLibrary.BuildAll();
            foreach (var p in puzzles)
            {
                string name = $"Puzzle_{p.puzzleId:00}_{SafeName(p.title)}.asset";
                string path = $"{BaseDir}/{name}";
                var existing = AssetDatabase.LoadAssetAtPath<CircuitPuzzle>(path);
                if (existing != null)
                {
                    EditorUtility.CopySerialized(p, existing);
                    EditorUtility.SetDirty(existing);
                    Object.DestroyImmediate(p);
                }
                else
                {
                    AssetDatabase.CreateAsset(p, path);
                }
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Copy references into Resources so the runtime bootstrapper
            // can Resources.LoadAll("Puzzles") without manual wiring.
            foreach (var p in AssetDatabase.FindAssets("t:CircuitPuzzle", new[] { BaseDir }))
            {
                var src = AssetDatabase.GUIDToAssetPath(p);
                var dst = $"{ResourcesDir}/{Path.GetFileName(src)}";
                AssetDatabase.CopyAsset(src, dst);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Circuit Solver: generated {puzzles.Count} puzzles under {BaseDir}");
        }

        [MenuItem("Circuit Solver/Open Puzzle Folder")]
        public static void OpenFolder()
        {
            EditorUtility.RevealInFinder(BaseDir);
        }

        static string SafeName(string input)
        {
            var clean = input.Replace(" ", "_");
            foreach (var ch in Path.GetInvalidFileNameChars())
                clean = clean.Replace(ch, '_');
            return clean;
        }
    }
}
#endif
