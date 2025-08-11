using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using Plugins;
using TMPro;
using UnityEngine;

namespace Classic
{
    public class LevelPanel : MonoBehaviour
    {
        public TextMeshProUGUI LevelTest;

        public void SetLevel(string info)
        {
            LevelTest.text = info;
        }
    }
}

