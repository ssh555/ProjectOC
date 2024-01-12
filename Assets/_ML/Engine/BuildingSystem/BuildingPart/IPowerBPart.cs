using UnityEngine;

namespace ML.Engine.BuildingSystem.BuildingPart
{
    public interface IPowerBPart : IBuildingPart
    {
        public int PowerCount { get; set; }

        //��������   xz����
        public Vector2 Pos { get; set; }
        public bool Inpower { get; set; }
    }
}