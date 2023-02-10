using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                    List<DataReceivedEventArgs> log = new List<DataReceivedEventArgs>();
                    List<DataReceivedEventArgs> error = new List<DataReceivedEventArgs>();
                    foreach (var path in set.batPaths)
                    {
                        var name = Path.GetFileName(path);
                        var dir = Path.GetDirectoryName(path);
                        if (Path.GetFileName(name).ToLower().Contains("svnup"))
                        {
                            RunBat(name, dir, ref log, ref error);
                        }
                    }
                    var conflict = log.FindAll(item => item.Data.ToLower().StartsWith("c ")).Select(item=>item.Data).ToList();
                    foreach (var msg in conflict)
                        UnityEngine.Debug.LogError(msg);
                    conflict = conflict.Select(item => item.Substring(1).Trim()).ToList();
                    HashSet<string> svnRoots = new HashSet<string>();
                    foreach (var file in conflict)
                    {
                        var curRoot = Path.GetDirectoryName(file);
                        while (!Directory.Exists(Path.Join(curRoot,".svn")))
                        {
                            curRoot = Path.GetDirectoryName(curRoot);
                        }

                        if (!svnRoots.Contains(curRoot))
                            svnRoots.Add(curRoot);
                    }

                    if (svnRoots.Count > 0)
                    {
                        EditorUtility.DisplayDialog("Conflict", $"以下目录存在冲突", "OK");
                        foreach (var root in svnRoots)
                            Process.Start("explorer.exe", root);
                    }
                }
            }
        }
        private static void RunBat(string batFile, string workingDir, ref List<DataReceivedEventArgs> log,  ref List<DataReceivedEventArgs> error)
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
                    // proc.StartInfo.WorkingDirectory = workingDir;
                    proc.StartInfo.FileName =Path.Join(workingDir,batFile);
                    proc.StartInfo.UseShellExecute = false;
                    proc.StartInfo.RedirectStandardOutput = true;
                    proc.StartInfo.RedirectStandardError = true;
                    //proc.StartInfo.Arguments = args;
                    proc.StartInfo.CreateNoWindow = true;
                    //proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;//disable dos window


                    proc.Start();
                    proc.BeginOutputReadLine();
                    var list = log;
                    proc.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            UnityEngine.Debug.Log(e.Data);
                            list.Add(e);
                        }
                    });
                    proc.BeginErrorReadLine();
                    var argsList = error;
                    proc.ErrorDataReceived += new DataReceivedEventHandler((sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            UnityEngine.Debug.Log(e.Data);
                            argsList.Add(e);
                        }
                    });
                    Console.ReadLine();
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
