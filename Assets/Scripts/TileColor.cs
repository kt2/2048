using System;
using UnityEngine;

namespace game2048
{
    [Serializable]
    public class TileColor
    {
        public int value;
        public Color fgColor = Color.black;
        public Color bgColor = Color.white;
    }
}