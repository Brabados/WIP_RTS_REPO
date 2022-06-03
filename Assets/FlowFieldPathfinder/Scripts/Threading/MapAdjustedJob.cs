using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class MapAdjustedJob : ThreadedJob
    {
        Pathfinder pathfinder;

        public MapAdjustedJob(Pathfinder _pathfinder)
        {
            pathfinder = _pathfinder;
        }

        protected override void ThreadFunction()
        {
            pathfinder.worldData.worldManager.InputChanges();
        }



        protected override void OnFinished()
        {

        }

    }
}
