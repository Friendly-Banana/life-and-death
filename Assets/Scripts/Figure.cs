using System;
using UnityEngine;

namespace LoD
{
    [Serializable]
    public class ArrayWrapper
    {
        public bool[] array;

        public ArrayWrapper() { array = new bool[1]; }
        public ArrayWrapper(int length) { array = new bool[length]; }

        public int Length => array.Length;

        public bool this[int index]
        {
            get => array[index];
            set => array[index] = value;
        }
    }

    public enum FigureCategory
    {
        StillLife, Oscillator, Spaceship, Other, Methuselah, Wick
    }

    [CreateAssetMenu(fileName = "Figure", menuName = "Figure")]
    public class Figure : ScriptableObject
    {
        public FigureCategory category;
        public int width;
        public int height;
        public ArrayWrapper[] cells;
    }
}