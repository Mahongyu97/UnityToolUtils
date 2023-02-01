using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
namespace MhyTool
{
    public class BatRun
    {
        [MenuItem("Assets/MyTool/Create/BatRunSettings", false)]
        public static void CreateBatSetting()
        {
            var dir = AssetDatabase.GetAssetPath(Selection.activeObject);
            Utils.CreateScriptObj<BatRunSettings>(dir);
        }
        [MenuItem("MyTool/Run[SvnUp]")]
        private static void RunSvnUpdate()
        {
            var batSets = Utils.GetScriptObj<BatRunSettings>();
            foreach (var set in batSets)
            {
                if (set.batPaths != null)
                {
                    foreach (var path in set.batPaths)
                    {
                        var name = Path.GetFileName(path);
                        var dir = Path.GetDirectoryName(path);
                        if (Path.GetFileName(name).ToLower().Contains("svnup"))
                        {
                            RunBat(name, dir);
                        }
                    }
                }
            }
        }
        private static void RunBat(string batFile, string workingDir)
        {
            var path = Utils.FormatPath(Path.Join(workingDir,batFile));
            if (!System.IO.File.Exists(path))
            {
                Debug.LogError($"bat文件不存在：{path}");
            }
            else
            {
                System.Diagnostics.Process proc = null;
                try
                {
                    proc = new System.Diagnostics.Process();
                    proc.StartInfo.WorkingDirectory = workingDir;
                    proc.StartInfo.FileName = batFile;
                    //proc.StartInfo.Arguments = args;
                    //proc.StartInfo.CreateNoWindow = true;
                    //proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;//disable dos window
                    proc.Start();
                    proc.WaitForExit();
                    proc.Close();
                }
                catch (System.Exception ex)
                {
                    Debug.LogFormat("Exception Occurred :{0},{1}", ex.Message, ex.StackTrace.ToString());
                }
            }
        }
    }
}
