﻿using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Threading;
using UnityEngine;

namespace ScriptTrainer
{
    [BepInPlugin("aoe.top.plugins.ScriptTrainer", "同舟共济 内置修改器", "1.0.0.0")]
    public class ScriptTrainer: BaseUnityPlugin
    {
        // 窗口相关
        MainWindow mw;

        // 启动按键
        private ConfigEntry<BepInEx.Configuration.KeyboardShortcut> ShowCounter { get; set; }
        private ConfigEntry<bool> IsDiscount { get; set; }
        public void Awake()
        {
            
        }

        public void Start()
        {
            #region[注入游戏补丁]
            var harmony = new Harmony("ScriptTrainer");
            harmony.PatchAll();
            #endregion

            ShowCounter = Config.Bind("修改器快捷键", "Key", new KeyboardShortcut(KeyCode.F9));
            IsDiscount = Config.Bind("是否启用打折功能", "Key", true);
            ScriptPatch.IsDiscount = IsDiscount.Value;
            Debug.Log("脚本已启动");
            mw = new MainWindow();
        }

        public void Update()
        {
            if (!MainWindow.initialized)
            {
                MainWindow.Initialize();
            }

            // 切换UI开关
            //if (ShowCounter.IsDown())
            // if (new KeyboardShortcut(KeyCode.F9).IsDown())

            if (ShowCounter.Value.IsDown())
            {
                if (!MainWindow.initialized)
                {
                    return;
                }

                MainWindow.optionToggle = !MainWindow.optionToggle;
                MainWindow.canvas.SetActive(MainWindow.optionToggle);
                UnityEngine.Event.current.Use();
            }
        }
        public void FixedUpdate()
        {
        }

        public void OnDestroy()
        {
            // 移除 MainWindow.testAssetBundle 加载时的资源
            AssetBundle.UnloadAllAssetBundles(true);

        }
    }
}
