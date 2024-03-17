using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ApplyPalette : EditorWindow
{
    const string COLORS_KEY = "PaletteColors";
    public ColorBlock colors = new ColorBlock { colorMultiplier = 1, fadeDuration = 0.1f };
    public List<Color> palette = new List<Color>();
    public List<GameObject> convertList = new List<GameObject>();
    SerializedObject serialObj;
    SerializedProperty listProp;
    SerializedProperty paletteProp;
    SerializedProperty colorsProp;
    int lastEditedCount;

    [MenuItem("Utils/ApplyPalette")]
    static void Init()
    {
        ApplyPalette window = (ApplyPalette)GetWindowWithRect(typeof(ApplyPalette), new Rect(0, 0, 200, 100));
        window.Show();
    }

    private void OnEnable()
    {
        if (palette.Count == 0)
        {
            foreach (string color in PlayerPrefs.GetString(COLORS_KEY, "").Split(";"))
            {
                ColorUtility.TryParseHtmlString("#" + color, out Color lastColor);
                palette.Add(lastColor);
            }
        }
        serialObj = new SerializedObject(this);
        listProp = serialObj.FindProperty("convertList");
        paletteProp = serialObj.FindProperty("palette");
        colorsProp = serialObj.FindProperty("colors");
    }

    private void OnDisable()
    {
        PlayerPrefs.SetString(COLORS_KEY, string.Join(";", palette.Select(x => ColorUtility.ToHtmlStringRGB(x))));
    }

    void OnGUI()
    {
        serialObj.Update();
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.HelpBox(new GUIContent("Button | Text | Slider, Toggle Background | Image"));
        EditorGUILayout.PropertyField(paletteProp);
        EditorGUILayout.HelpBox("Any Colors here will be overriden by the palette", MessageType.Warning);
        EditorGUILayout.PropertyField(colorsProp);
        EditorGUILayout.PropertyField(listProp);
        serialObj.ApplyModifiedProperties();
        if (EditorGUI.EndChangeCheck())
        {
            colors.normalColor = palette[0];
            colors.highlightedColor = palette[1];
            colors.pressedColor = palette[2];
            colors.selectedColor = palette[3];
            colors.disabledColor = palette[4];
        }

        foreach (GameObject go in convertList)
        {
            if (go == null) continue;
            ScrollRect sv = go.GetComponentInChildren<ScrollRect>();
            Slider s = go.GetComponentInChildren<Slider>();
            Toggle t = go.GetComponentInChildren<Toggle>();
            Button bt = go.GetComponent<Button>();
            TMP_InputField input = go.GetComponentInChildren<TMP_InputField>();
            TMP_Text te = go.GetComponentInChildren<TMP_Text>();
            Image i = go.GetComponentInChildren<Image>();

            if (input != null)
            {
                input.colors = colors;
                // Background
                i.color = palette[0];
                input.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>().color = palette[4];
                input.transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>().color = palette[5];
                continue;
            }

            if (te != null)
                te.color = palette[5];

            if (sv != null)
            {
                // Scrollbars
                if (sv.horizontalScrollbar)
                    sv.horizontalScrollbar.colors = colors;
                if (sv.verticalScrollbar)
                    sv.verticalScrollbar.colors = colors;
            }
            else if (t != null)
            {
                t.colors = colors;
                // Background, Checkmark
                i.color = palette[6];
                t.transform.GetChild(0).GetChild(0).GetComponent<Image>().color = palette[0];
            }
            else if (bt != null)
            {
                bt.colors = colors;
            }
            else if (s != null)
            {
                s.colors = colors;
                // Background, Fill
                i.color = palette[6];
                s.transform.GetChild(1).GetComponentInChildren<Image>().color = palette[1];
            }
            else if (i != null)
                i.color = palette[7];
            EditorUtility.SetDirty(go);
        }
        if (convertList.Count != 0)
            lastEditedCount = convertList.Count;
        EditorGUILayout.HelpBox(new GUIContent($"Edited {lastEditedCount} objects"));
        convertList.Clear();
    }
}