using System;
using System.Collections.Generic;
using UnityEngine;

namespace Baracuda.Serialization
{
    [Serializable]
    public class ProfilePathData
    {
        [SerializeField] private List<string> paths = new();
        public List<string> Paths => paths;
    }
}