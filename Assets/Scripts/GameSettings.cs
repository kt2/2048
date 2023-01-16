using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace game2048
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "ScriptableObjects/GameSettings", order = 0)]
    public class GameSettings : ScriptableObject
    {
        public float AnimationDuration;
        public TileColor[] TileColors;
        void Reset()
        {
            AnimationDuration = 0.2f;
            TileColors = new TileColor[] {
            new TileColor { fgColor = new Color32(118, 110, 102, 255), bgColor = new Color32(237, 228, 219, 255), value = 2},
            new TileColor { fgColor = new Color32(118, 110, 102, 255), bgColor = new Color32(235, 224, 203, 255), value = 4},
            new TileColor { fgColor = new Color32(249, 246, 242, 255), bgColor = new Color32(234, 179, 129, 255), value = 8},
            new TileColor { fgColor = new Color32(249, 246, 242, 255), bgColor = new Color32(234, 152, 108, 255), value = 16},
            new TileColor { fgColor = new Color32(249, 246, 242, 255), bgColor = new Color32(233, 129, 103, 255), value = 32},
            new TileColor { fgColor = new Color32(249, 246, 242, 255), bgColor = new Color32(231, 101, 71, 255), value = 64},
            new TileColor { fgColor = new Color32(249, 246, 242, 255), bgColor = new Color32(233, 207, 128, 255), value = 128},
            new TileColor { fgColor = new Color32(249, 246, 242, 255), bgColor = new Color32(233, 204, 115, 255), value = 256},
            new TileColor { fgColor = new Color32(249, 246, 242, 255), bgColor = new Color32(233, 200, 101, 255), value = 512},
            new TileColor { fgColor = new Color32(249, 246, 242, 255), bgColor = new Color32(232, 197, 89, 255), value = 1024},
            new TileColor { fgColor = new Color32(249, 246, 242, 255), bgColor = new Color32(231, 195, 79, 255), value = 2048}
        };
        }
    }
}
