using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MhyTool
{
    public class BatRunSettings : ScriptableObject
    {
        [LabelText("执行文件路径")] public string[] batPaths;
    }
}