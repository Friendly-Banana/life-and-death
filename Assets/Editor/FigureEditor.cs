using UnityEditor;
using UnityEngine;

namespace LoD
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Figure))]
    class FigurEditor : Editor
    {
        bool showLookSettings = true;
        bool zeroBasedIndex = true;
        int cellWidth = 20;
        Color cellColor = Color.white;
        Color bgColor = Color.black;

        SerializedProperty categoryProp;

        private void OnEnable()
        {
            categoryProp = serializedObject.FindProperty("category");
        }

        public override void OnInspectorGUI()
        {
            showLookSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showLookSettings, "Editor Look");
            if (showLookSettings)
            {
                cellColor = EditorGUILayout.ColorField("Cell Color", cellColor);
                bgColor = EditorGUILayout.ColorField("Background Color", bgColor);
                cellWidth = Mathf.Max(5, EditorGUILayout.DelayedIntField("Cell Width", cellWidth));
                zeroBasedIndex = EditorGUILayout.Toggle("Zero Based Grid Numbers", zeroBasedIndex);
                EditorGUILayout.Space(20);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUI.BeginChangeCheck();

            Figure figure = (Figure)target;
            string name = EditorGUILayout.DelayedTextField("Figure Name", figure.name);
            if (name != figure.name)
            {
                Undo.RecordObject(figure, $"Rename {name} from {figure.name}.");
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(figure), name);
            }

            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();
            EditorGUILayout.PropertyField(categoryProp, new GUIContent("Category"));
            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();

            int width = Mathf.Max(1, EditorGUILayout.DelayedIntField("Width", figure.width));
            int height = Mathf.Max(1, EditorGUILayout.DelayedIntField("Height", figure.height));
            if (width != figure.width || height != figure.height)
            {
                // init new array
                ArrayWrapper[] newCells = new ArrayWrapper[width];
                for (int i = 0; i < width; i++)
                    newCells[i] = new ArrayWrapper(height);
                // copy old values
                int minX = Mathf.Min(figure.width, width);
                int minY = Mathf.Min(figure.height, height);
                for (int i = 0; i < minX; i++)
                    System.Array.Copy(figure.cells[i].array, newCells[i].array, minY);

                Undo.RecordObject(figure, "Change size of figure");
                figure.cells = newCells;
                figure.width = width;
                figure.height = height;
            }


            EditorGUILayout.BeginVertical();
            HorizontalNumbers(width);
            for (int y = 0; y < figure.height; y++)
            {
                GUILayout.BeginHorizontal();
                VerticalNumber(y);
                for (int x = 0; x < figure.width; x++)
                {
                    GUI.color = figure.cells[x][y] ? cellColor : bgColor;
                    if (GUILayout.Button(figure.cells[x][y] ? "X" : " ", GUILayout.Width(cellWidth)))
                    {
                        Undo.RecordObject(figure, "Edit figure");
                        figure.cells[x][y] = !figure.cells[x][y];
                    }
                }
                VerticalNumber(y);
                GUILayout.EndHorizontal();
            }
            HorizontalNumbers(width);
            EditorGUILayout.EndVertical();
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(figure);
                AssetDatabase.SaveAssetIfDirty(AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(figure)));
            }
        }

        private void VerticalNumber(int y)
        {
            GUI.color = Color.white;
            EditorGUILayout.LabelField(((zeroBasedIndex ? 0 : 1) + y).ToString(), GUILayout.Width(cellWidth));
        }

        private void HorizontalNumbers(int length)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(cellWidth));
            for (int x = 0; x < length; x++)
            {
                EditorGUILayout.LabelField(((zeroBasedIndex ? 0 : 1) + x).ToString(), GUILayout.Width(cellWidth));
            }
            GUILayout.EndHorizontal();
        }
    }
}