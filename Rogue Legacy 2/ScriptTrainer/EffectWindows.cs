﻿using Sigtrap.Relays;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityGameUI;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;
using Object = UnityEngine.Object;

namespace ScriptTrainer
{
    internal class TraitWindow : MonoBehaviour
    {
        private static GameObject Panel;
        private static int initialX;
        private static int initialY;
        private static int elementX;
        private static int elementY;

        #region[数据分页相关]
        private static GameObject ItemPanel;
        private static List<GameObject> ItemButtons = new List<GameObject>();
        private static int page = 1;
        private static int maxPage = 1;
        private static int conunt = 15;
        private static string searchText = "";
        private static GameObject uiText;
        private static string uiText_text
        {
            get
            {
                return $"{page} / {maxPage}";
            }
        }
        #endregion

        public TraitWindow(GameObject panel, int x, int y)
        {
            Panel = panel;
            initialX = elementX = x + 50;
            initialY = elementY = y;
            Initialize();
        }
        public void Initialize()
        {
            //创建搜索框
            SearchBar(Panel);
            elementX += 300;
            elementY += 10;
            hr();

            //创建物品列表
            if (false)
            {
                container();
            }
            else
            {
                //elementX += 200;
                elementY = 125 - 60 * 5;
            }
            //创建分页

            PageBar(Panel);
        }

        #region[创建详细]
        //搜索框
        private void SearchBar(GameObject panel)
        {
            elementY += 10;
            elementX = -MainWindow.width / 2 + 120;
            //label
            Sprite txtBgSprite = UIControls.createSpriteFrmTexture(UIControls.createDefaultTexture("#7AB900FF"));
            GameObject uiText = UIControls.createUIText(panel, txtBgSprite, "#FFFFFFFF");
            uiText.GetComponent<Text>().text = "搜索";
            uiText.GetComponent<RectTransform>().localPosition = new Vector3(elementX, elementY, 0);
            uiText.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 30);
            uiText.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

            //坐标偏移
            elementX += 60;

            //输入框
            int w = 260;
            Sprite inputFieldSprite = UIControls.createSpriteFrmTexture(UIControls.createDefaultTexture("#212121FF"));
            GameObject uiInputField = UIControls.createUIInputField(panel, inputFieldSprite, "#FFFFFFFF");
            uiInputField.GetComponent<InputField>().text = searchText;
            uiInputField.GetComponent<RectTransform>().localPosition = new Vector3(elementX + 100, elementY, 0);
            uiInputField.GetComponent<RectTransform>().sizeDelta = new Vector2(w, 30);


            //文本框失去焦点时触发方法
            uiInputField.GetComponent<InputField>().onEndEdit.AddListener((string text) =>
            {
                //Debug.Log(text);
                page = 1;
                searchText = text;
                container();
                TraitWindow.uiText.GetComponent<Text>().text = uiText_text;
                //Destroy(ItemPanel);
            });
        }
        //分页
        private void PageBar(GameObject panel)
        {
            //背景
            GameObject pageObj = UIControls.createUIPanel(panel, "40", "500");
            pageObj.GetComponent<Image>().color = UIControls.HTMLString2Color("#424242FF");
            pageObj.GetComponent<RectTransform>().localPosition = new Vector3(0, elementY, 0);

            //当前页数 / 总页数
            Sprite txtBgSprite = UIControls.createSpriteFrmTexture(UIControls.createDefaultTexture("#7AB900FF"));

            if (uiText == null)
            {
                uiText = UIControls.createUIText(pageObj, txtBgSprite, "#ffFFFFFF");
                uiText.GetComponent<Text>().text = uiText_text;
                uiText.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                //设置字体
                uiText.GetComponent<Text>().fontSize = 20;
                uiText.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
            }


            //上一页
            string backgroundColor = "#8C9EFFFF";
            GameObject prevBtn = UIControls.createUIButton(pageObj, backgroundColor, "上一页", () =>
            {

                page--;
                if (page <= 0) page = 1;
                container();
                uiText.GetComponent<Text>().text = uiText_text;
            }, new Vector3());
            prevBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 20);
            prevBtn.GetComponent<RectTransform>().localPosition = new Vector3(-100, 0, 0);

            //下一页
            GameObject nextBtn = UIControls.createUIButton(pageObj, backgroundColor, "下一页", () =>
            {
                page++;
                if (page >= maxPage) page = maxPage;
                container();
                uiText.GetComponent<Text>().text = uiText_text;
            });
            nextBtn.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 20);
            nextBtn.GetComponent<RectTransform>().localPosition = new Vector3(100, 0, 0);
        }
        private static void container()
        {
            elementX = -200;
            elementY = 125;

            //先清空旧的 ItemPanel
            foreach (var item in ItemButtons)
            {
                UnityEngine.Object.Destroy(item);
            }
            ItemButtons.Clear();

            ItemPanel = UIControls.createUIPanel(Panel, "300", "600");
            ItemPanel.GetComponent<Image>().color = UIControls.HTMLString2Color("#424242FF");
            ItemPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(10, 0);

            int num = 0;
            foreach (RelicDrop item in GetItemData())
            {
                var btn = CreateItemButton("获得", GetItemName(item), GetItemIcon(item), ItemPanel, () =>
                {
                    SpawnDescriptionInputDialog("获得", GetItemDescription(item), () =>
                    {
                        SpawnItem(item);
                    });
                });
                ItemButtons.Add(btn);
                num++;
                if (num % 3 == 0)
                {
                    hr();
                }
            }
        }
        public static void SpawnDescriptionInputDialog(string ButtonText, string Description, UnityAction onFinish)
        {
            //创建画布
            GameObject canvas = UIControls.createUICanvas();
            Object.DontDestroyOnLoad(canvas);
            //设置置顶显示
            canvas.GetComponent<Canvas>().overrideSorting = true;
            canvas.GetComponent<Canvas>().sortingOrder = 100;
            //分割物品介绍，设置界面大小
            List<string> outlines = Description.GetSeparateSubString(18);
            int size = outlines.Count * 20 + 20 + 40;
            // 创建面板
            GameObject uiPanel = UIControls.createUIPanel(canvas, size.ToString(), "300", null);
            uiPanel.GetComponent<Image>().color = UIControls.HTMLString2Color("#37474FFF");
            //创建物品介绍
            Sprite txtBgSprite = UIControls.createSpriteFrmTexture(UIControls.createDefaultTexture("#7AB900FF"));
            string DefaultColor = "#FFFFFFFF";
            for (int i = 0; i < outlines.Count; i++)
            {
                GameObject uiText = UIControls.createUIText(uiPanel, txtBgSprite, DefaultColor);
                uiText.GetComponent<Text>().text = outlines[i];
                uiText.GetComponent<RectTransform>().localPosition = new Vector2(0, size / 2 - 20 - 20 * i);
                uiText.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 20);
                uiText.GetComponent<Text>().fontSize = 14;
            }
            //创建确定按钮
            GameObject uiButton = UIControls.createUIButton(uiPanel, "#8C9EFFFF", ButtonText, () =>
            {
                onFinish();
                Object.Destroy(canvas);
            }, new Vector3(100, -size / 2 + 30, 0));

            //创建关闭按钮
            GameObject closeButton = UIControls.createUIButton(uiPanel, "#B71C1CFF", "X", () =>
            {
                Object.Destroy(canvas);
            }, new Vector3(350 / 2 - 10, size / 2 - 10, 0));
            //设置closeButton宽高
            closeButton.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 20);
            //字体颜色为白色
            closeButton.GetComponentInChildren<Text>().color = UIControls.HTMLString2Color("#FFFFFFFF");
        }
        public static void SpawnEquipmentInputDialog(string ButtonText, string Description, UnityAction onFinish)
        {
            Debug.Log($"ZG:{Description}");
            //创建画布
            GameObject canvas = UIControls.createUICanvas();
            Object.DontDestroyOnLoad(canvas);
            //设置置顶显示
            canvas.GetComponent<Canvas>().overrideSorting = true;
            canvas.GetComponent<Canvas>().sortingOrder = 100;
            //分割物品介绍，设置界面大小
            List<string> outlines = Description.GetSeparateSubString(18);
            int size = outlines.Count * 20 + 20 + 40;
            // 创建面板
            GameObject uiPanel = UIControls.createUIPanel(canvas, size.ToString(), "300", null);
            uiPanel.GetComponent<Image>().color = UIControls.HTMLString2Color("#37474FFF");
            //创建物品介绍
            Sprite txtBgSprite = UIControls.createSpriteFrmTexture(UIControls.createDefaultTexture("#7AB900FF"));
            string DefaultColor = "#FFFFFFFF";
            for (int i = 0; i < outlines.Count; i++)
            {
                GameObject uiText = UIControls.createUIText(uiPanel, txtBgSprite, DefaultColor);
                uiText.GetComponent<Text>().text = outlines[i];
                uiText.GetComponent<RectTransform>().localPosition = new Vector2(0, size / 2 - 20 - 20 * i);
                uiText.GetComponent<RectTransform>().sizeDelta = new Vector2(280, 20);
                uiText.GetComponent<Text>().fontSize = 14;
            }
            //创建确定按钮
            GameObject uiButton = UIControls.createUIButton(uiPanel, "#8C9EFFFF", ButtonText, () =>
            {
                onFinish();
                Object.Destroy(canvas);
            }, new Vector3(100, -size / 2 + 30, 0));

            //创建关闭按钮
            GameObject closeButton = UIControls.createUIButton(uiPanel, "#B71C1CFF", "X", () =>
            {
                Object.Destroy(canvas);
            }, new Vector3(350 / 2 - 10, size / 2 - 10, 0));
            //设置closeButton宽高
            closeButton.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 20);
            //字体颜色为白色
            closeButton.GetComponentInChildren<Text>().color = UIControls.HTMLString2Color("#FFFFFFFF");
        }
        private static GameObject CreateItemButton(string ButtonText, string ItemName, Sprite ItemIcon, GameObject panel, UnityAction action)
        {
            //按钮宽 200 高 50
            int buttonWidth = 190;
            int buttonHeight = 50;

            //根据品质设置背景颜色
            string qualityColor = "#FFFFFFFF";

            //创建一个背景
            GameObject background = UIControls.createUIPanel(panel, buttonHeight.ToString(), buttonWidth.ToString(), null);
            background.GetComponent<Image>().color = UIControls.HTMLString2Color("#455A64FF");
            background.GetComponent<RectTransform>().localPosition = new Vector3(elementX, elementY, 0);


            GameObject background_icon = UIControls.createUIPanel(background, buttonHeight.ToString(), "50", null);
            background_icon.GetComponent<Image>().color = UIControls.HTMLString2Color(qualityColor);
            background_icon.GetComponent<RectTransform>().anchoredPosition = new Vector2(70, 0);

            GameObject icon = UIControls.createUIImage(background_icon, ItemIcon);
            icon.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
            icon.GetComponent<RectTransform>().localPosition = new Vector2(0, 0);

            //创建文字
            Sprite txtBgSprite = UIControls.createSpriteFrmTexture(UIControls.createDefaultTexture("#7AB900FF"));
            GameObject uiText = UIControls.createUIText(background, txtBgSprite, ColorUtility.ToHtmlStringRGBA(Color.white));
            uiText.GetComponent<Text>().text = ItemName;
            uiText.GetComponent<RectTransform>().localPosition = new Vector3(0, 5, 0);

            //创建按钮
            string backgroundColor_btn = "#8C9EFFFF";
            GameObject button = UIControls.createUIButton(background, backgroundColor_btn, ButtonText, action, new Vector3());
            button.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 20);
            button.GetComponent<RectTransform>().localPosition = new Vector3(-50, -10, 0);

            elementX += 200;

            return button;
        }
        public static GameObject AddToggle(string Text, int width, GameObject panel, UnityAction<bool> action)
        {
            //计算x轴偏移
            elementX += width / 2 - 30;

            Sprite toggleBgSprite = UIControls.createSpriteFrmTexture(UIControls.createDefaultTexture(ColorUtility.ToHtmlStringRGBA(Color.white)));
            Sprite toggleSprite = UIControls.createSpriteFrmTexture(UIControls.createDefaultTexture("#18FFFFFF"));
            GameObject uiToggle = UIControls.createUIToggle(panel, toggleBgSprite, toggleSprite);
            uiToggle.GetComponentInChildren<Text>().color = Color.white;
            uiToggle.GetComponentInChildren<Toggle>().isOn = false;
            uiToggle.GetComponent<RectTransform>().localPosition = new Vector3(elementX, elementY, 0);

            uiToggle.GetComponentInChildren<Text>().text = Text;
            uiToggle.GetComponentInChildren<Toggle>().onValueChanged.AddListener(action);


            elementX += width / 2 + 10;

            return uiToggle;
        }
        private static void hr()
        {
            elementX = initialX;
            elementY -= 60;
        }

        #endregion

        public static void SpawnItem(RelicDrop item)
        {
            ItemDropManager.DropSpecialItem(item);
        }


        #region[获取数据相关函数]
        private static List<RelicDrop> GetItemData()
        {
            List<RelicDrop> ItemData = new List<RelicDrop>();

            foreach (RelicType type in Enum.GetValues(typeof(RelicType)))
            {
                if (type != RelicType.None && !GetItemName(new RelicDrop(type, RelicModType.None)).StartsWith("LOC_ID"))
                    ItemData.Add(new RelicDrop(type, RelicModType.None));
            }

            //var xx = MainController.Instance.Settings.effects.ToList<EffectModel>();
            //foreach (EffectModel item in xx)
            //{
            //    Debug.Log($"ZG;{item.regularEffect.Name};{item.Name};{item.upgradedEffect.Name}");
            //}
            Debug.Log($"ZG:全物品数量:{ItemData.Count}");
            if (searchText != "")
            {
                ItemData = FilterItemData(ItemData);
            }
            //Curse诅咒 Magazine
            //对 DataList 进行分页
            List<RelicDrop> list = new List<RelicDrop>();
            int start = (page - 1) * conunt;
            int end = start + conunt;
            for (int i = start; i < end; i++)
            {
                if (i < ItemData.Count)
                {
                    list.Add(ItemData[i]);
                }
            }
            if (ItemData.Count % conunt != 0)
                maxPage = ItemData.Count / conunt + 1;
            else
                maxPage = ItemData.Count / conunt;

            return list;
        }
        //搜索过滤
        private static List<RelicDrop> FilterItemData(List<RelicDrop> dataList)
        {
            if (searchText == "")
            {
                return dataList;
            }
            List<RelicDrop> list = new List<RelicDrop>();

            foreach (var item in dataList)
            {
                string text = GetItemName(item);
                string text2 = GetItemDescription(item);
                if (text.Contains(searchText.Replace(" ", "")) || text2.Contains(searchText.Replace(" ", "")))
                {
                    list.Add(item);
                }
            }

            return list;
        }
        private static string GetItemName(RelicDrop item)
        {
            return LocalizationManager.GetString(RelicLibrary.GetRelicData(item.RelicType).Title, false, false);
        }
        private static string GetItemDescription(RelicDrop item)
        {
            bool flag;
            return LocalizationManager.GetString(RelicLibrary.GetRelicData(item.RelicType).Description, false, out flag, false);
        }
        private static Sprite GetItemIcon(RelicDrop item)
        {
            Sprite sprite = null;
            try
            {
                sprite = IconLibrary.GetRelicSprite(item.RelicType, true);
                Debug.Log(sprite);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            return sprite;
        }
        #endregion
    }

}
