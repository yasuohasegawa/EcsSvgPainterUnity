using System;
using Unity.Entities;
using UnityEngine;

namespace SVGPainterUnity{
	[Serializable]
	public struct SVGPainterData : ISharedComponentData
	{
	    public GameObject prefab;
        public TextAsset svgFile;
        public float lineWidth;
        public Color lineColor;
	}

    public class SVGPainterComponent : SharedComponentDataWrapper<SVGPainterData> { }
}