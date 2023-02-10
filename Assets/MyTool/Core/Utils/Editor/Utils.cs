using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MhyTool
{
    public class Utils
    {
        public static string FormatPath(string path)
        {
            path = path.Replace("/", "\\");
            if (Application.platform == RuntimePlatform.OSXEditor)
                path = path.Replace("\\", "/");
            return path;
        }
        [MenuItem("Assets/MyTool/Copy Full Path", false)]
        public static void FullPath()
        {
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            var root = Application.dataPath.Remove(Application.dataPath.Length - 6);
            GUIUtility.systemCopyBuffer = Path.Join(root, path);
        }

        public static void CreateScriptObj<T>(string path) where T:ScriptableObject
        {
            var name = typeof(T).Name;
            var indx = "";
            var fullName = Path.Join(FormatPath(path), $"{name}{indx}.asset");

            while (File.Exists(fullName))
            {
                indx = ((string.IsNullOrEmpty(indx) ? "0" : indx).ToInt32()+1).ToString();
                fullName = Path.Join(FormatPath(path), $"{name}{indx}.asset");
            }
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<T>(), fullName);
            AssetDatabase.Refresh();
        }

        public static List<T> GetScriptObj<T>() where T : ScriptableObject
        {
            var filter = $"t:{typeof(T).FullName}";
            var guids = UnityEditor.AssetDatabase.FindAssets(filter);
            return guids.Select(guid =>
                UnityEditor.AssetDatabase.LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid))).ToList();
        }
    }
}