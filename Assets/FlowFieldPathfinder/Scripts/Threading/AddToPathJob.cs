using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class AddToPathJob : ThreadedJob
    {
        Pathfinder pathfinder;
        FlowFieldPath path;
        WorldArea area;
        Tile tile;

        public AddToPathJob(WorldArea _area, Tile _tile, FlowFieldPath _path, Pathfinder _pathfinder)
        {
            path = _path;
            area = _area;
            tile = _tile;
            pathfinder = _pathfinder;
        }

        protected override void ThreadFunction()
        {
            pathfinder.worldData.intergrationFieldManager.CreateExtraField(area, tile, path, this);

        }
        protected override void OnFinished()
        {
            pathfinder.PathAdjusted(path, area, tile);

        }
    }
}
