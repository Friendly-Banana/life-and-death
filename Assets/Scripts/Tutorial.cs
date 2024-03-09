using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoD
{
    public class Tutorial : MonoBehaviour
    {
        public static System.Action pendingSwap;
        public List<Sprite> images;
        public List<string> texts;
        public Image image;
        public TMP_Text text;
        public GameObject dotPrefab;
        public Transform dotParent;
        public Animator animator;
        List<Button> dots = new List<Button>();
        int position;

        // Start is called before the first frame update
        private void Start()
        {
            for (int i = 0; i < texts.Count; i++)
            {
                texts[i] = texts[i].Replace("\\n", "\n");
                int index = i;
                Button button = Instantiate(dotPrefab, dotParent).GetComponent<Button>();
                button.onClick.AddListener(() => AnimateSet(index));
                dots.Add(button);
            }
            Set(0);
        }

        void AnimateSet(int index)
        {
            if (index == position) return;
            animator.Play("FadeOut");
            pendingSwap = () => Set(index);
        }

        void Set(int index)
        {
            position = index;
            image.sprite = images[index];
            text.text = texts[index];
            dots[index].Select();
        }

        public void Next() => AnimateSet(Mathf.Min(texts.Count - 1, position + 1));

        public void Back() => AnimateSet(Mathf.Max(0, position - 1));
    }
}