using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    static Stack<BackButton> active = new Stack<BackButton>();
    Button button;

    private void OnValidate()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        active.Push(this);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && active.Peek() == this)
        {
            button.onClick.Invoke();
        }
    }

    private void OnDisable()
    {
        active.Pop();
    }
}