using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
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
                        if (File.Exists(file))
                        {
                            var curRoot = Path.GetDirectoryName(file);
                            while (!Directory.Exists(Path.Join(curRoot,".svn")))
                            {
                                curRoot = Path.GetDirectoryName(curRoot);
                            }

                            if (!svnRoots.Contains(curRoot))
                                svnRoots.Add(curRoot);
                        }
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


        public static void Get(string url, Action<string> action)
        {
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.Timeout = 3000;

            Debug.LogError($"req {url}");
            if (req == null || req.GetResponse() == null)
                return;

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
            if (resp == null)
                return;

            using (Stream stream = resp.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    // Debug.LogError(reader.ReadToEnd());
                    action(reader.ReadToEnd());
                }
            }
        }

        [Serializable]
        public class ReplayDetail
        {
            public string replay_id;
            public int time;
            public string lua_ver;
            public string replay_ver;
            public long start_tick;
        }
        [Serializable]
        public class ReplayData
        {
            public List<ReplayDetail> replay_data_time = new List<ReplayDetail>();
        }

        const string  replayListUtl = "http://aquaman-battle.ingress-pre.flowgame.cn/replaysvr/public/getReplayTime?replay_time_cost=1";
        const string  replayDataUrl = "http://aquaman-battle.ingress-pre.flowgame.cn/replaysvr/public/getReplayDataPb?replay_id=";

        const string  prod_replayListUtl = "http://aquaman-battle.ingress-prod.flowgame.cn/replaysvr/public/getReplayTime?replay_time_cost=5000";
        const string  prod_replayDataUrl = "http://aquaman-battle.ingress-prod.flowgame.cn/replaysvr/public/getReplayDataPb?replay_id=";

        [MenuItem("MyTool/TestHttp")]
        static void getData()
        {
            try
            {
                Get(replayListUtl, (str) =>
                {
                    OnGetDat("pre_replay",str, replayDataUrl);
                });
                // Get(prod_replayListUtl, (str) =>
                // {
                //     OnGetDat("prod_replay",str, prod_replayDataUrl);
                // });
            }
            catch (Exception ex)
            {
                Console.WriteLine("错误：\n{0}", ex.Message);
            }
        }



        public static void OnGetDat(string root, string str, string replayDataUrl)
        {
            Debug.LogError(str);
            var data = JsonUtility.FromJson<ReplayData>(str);
            Dictionary<string, List<string>> paths = new Dictionary<string, List<string>>();
            foreach (var detail in data.replay_data_time)
            {
                string timeStep = (Mathf.Floor(detail.time / 1000f/ 5f) * 5).ToString();
                var filename = $"D:\\{root}\\{detail.lua_ver}\\{detail.lua_ver}_{detail.time}_{detail.replay_id}.data";
                if (!Directory.Exists(Path.GetDirectoryName(filename)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));
                }

                if (!File.Exists(filename))
                {
                    Debug.LogError($"new at {filename}");
                    var url = replayDataUrl + detail.replay_id;
                    System.Net.HttpWebRequest Myrq = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(url);
                    System.Net.HttpWebResponse myrp = (System.Net.HttpWebResponse)Myrq.GetResponse();
                    System.IO.Stream st = myrp.GetResponseStream();
                    System.IO.Stream so = new System.IO.FileStream(filename, System.IO.FileMode.Create);
                    byte[] by = new byte[1024];
                    int osize = st.Read(by, 0, (int)by.Length);
                    while (osize > 0)
                    {
                        so.Write(by, 0, osize);
                        osize = st.Read(by, 0, (int)by.Length);
                    }
                    so.Close();
                    st.Close();
                    myrp.Close();
                    Myrq.Abort();
                }
                if (!paths.ContainsKey(detail.lua_ver))
                {
                    paths.Add(detail.lua_ver,new List<string>());
                }
                paths[detail.lua_ver].Add(filename.Replace("\\","\\\\"));
            }

            var export = new Framework.Editor.CueEditor.DataToLuaHelper();
            export.ExportDataToLua(paths, "replays", Framework.Resource.ResConfig.EditorLuaClientDataPath);
        }
    }
}
