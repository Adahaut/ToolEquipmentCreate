using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Equipment
{
    [Serializable]
    public class DataEquipment : ScriptableObject
    {
        public string Name;
        public int Damage;
        public Texture2D Icone;
        public int Durability;
        public Color Rarity = Color.gray;
    }
}
