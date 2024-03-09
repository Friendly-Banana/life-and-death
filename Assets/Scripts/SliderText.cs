using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEventCallState = UnityEngine.Events.UnityEventCallState;

namespace LoD
{
    public class SliderText : MonoBehaviour
    {
        [SerializeField]
        private bool autoSave;
        [SerializeField]
        private string key;
        [SerializeField]
        private int decimals = 2;
        [SerializeField]
        private string prefix;
        [SerializeField]
        private TMP_Text text;
        [SerializeField]
        private Slider slider;

#if UNITY_EDITOR
        private void OnValidate()
        {
            slider = GetComponent<Slider>();
            text = GetComponentInChildren<TMP_Text>();
            List<int> methodRefs = new List<int>();
            for (int i = 0; i < slider.onValueChanged.GetPersistentEventCount(); i++)
            {
                if (slider.onValueChanged.GetPersistentMethodName(i) == nameof(UpdateText) || slider.onValueChanged.GetPersistentTarget(i) == this)
                    methodRefs.Add(i);
            }
            switch (methodRefs.Count)
            {
                case 0:
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(slider.onValueChanged, UpdateText);
                    slider.onValueChanged.SetPersistentListenerState(slider.onValueChanged.GetPersistentEventCount() - 1, UnityEventCallState.EditorAndRuntime);
                    break;
                case 1:
                    slider.onValueChanged.SetPersistentListenerState(methodRefs[0], UnityEventCallState.EditorAndRuntime);
                    break;
                default:
                    slider.onValueChanged.SetPersistentListenerState(methodRefs[0], UnityEventCallState.EditorAndRuntime);
                    methodRefs.RemoveAt(0);
                    methodRefs.Reverse();
                    for (int i = 0; i < methodRefs.Count; i++)
                    {
                        UnityEditor.Events.UnityEventTools.RemovePersistentListener(slider.onValueChanged, i);
                    }
                    break;
            }
        }
#endif

        private void Start()
        {
            if (autoSave)
            {
                slider.value = PlayerPrefs.GetFloat(key, slider.value);
                text.text = prefix + slider.value.ToString(slider.wholeNumbers ? "0" : "n" + decimals);
            }
            else
                UpdateText(slider.value);
        }

        private void UpdateText(float value)
        {
            text.text = prefix + value.ToString(slider.wholeNumbers ? "0" : "n" + decimals);
            if (autoSave)
                PlayerPrefs.SetFloat(key, value);
        }
    }
}
