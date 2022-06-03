using UnityEngine;
using System.Collections;

namespace FlowPathfinding
{
    // contains easy arrays for debugging
    public class ColorLists
    {
        public Color[] pathCostColors;

        // Use this for initialization
        public void Setup()
        {
            SetupPathColors();
        }

        void SetupPathColors()
        {
            Color[] colors = new Color[] { Color.white, Color.yellow, Color.green, Color.cyan, Color.blue, Color.magenta, Color.red, Color.grey, Color.black };
            int length = 450;
            pathCostColors = new Color[length];
            int step = length / (colors.Length - 1);

            for (int i = 0; i < colors.Length; i++)
                pathCostColors[i * step] = colors[i];

            for (int i = 0; i < colors.Length - 1; i++)
            {
                int min = i * step;
                int max = (i + 1) * step;
                int amount = max - min;

                Color start = pathCostColors[min];
                Color end = pathCostColors[max];

                for (int j = min; j < max; j++)
                {
                    Color one = start * (max - j);
                    Color two = end * (j - min);

                    pathCostColors[j] = (one + two) / amount;
                }
            }
        }

    }
}
