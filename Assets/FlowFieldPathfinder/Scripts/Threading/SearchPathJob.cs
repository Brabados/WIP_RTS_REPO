using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class SearchPathJob : ThreadedJob
    {
        FlowFieldPath path;
        int key;
        bool pathEdit;

        Tile destinationNode;
        List<Seeker> units;
        Pathfinder pathfinder;

        public SearchPathJob(Tile _destinationNode, List<Seeker> _units, Pathfinder _pathfinder)
        {
            destinationNode = _destinationNode;
            units = _units;
            pathfinder = _pathfinder;
        }

        protected override void ThreadFunction()
        {
            if (destinationNode != null)
            {
                WorldArea destinationArea = pathfinder.worldData.worldAreas[destinationNode.worldAreaIndex];
                List<Seeker> selectedUnits = new List<Seeker>(units);
                Dictionary<IntVector2, Tile> startingPoints = new Dictionary<IntVector2, Tile>();
                IntVector2 pointKey = new IntVector2(0, 0);

                foreach (Seeker unit in units)
                {
                    if (unit.currentWorldArea != null && unit.currentTile != null)
                    {
                        pointKey.x = unit.currentWorldArea.index;
                        pointKey.y = unit.currentTile.sectorIndex;
                        if (!startingPoints.ContainsKey(pointKey))
                            startingPoints.Add(pointKey, unit.currentTile);
                    }
                    else
                    {
                        selectedUnits.Remove(unit);
                        Debug.Log("A Selected Unit cannot be located,  is it to far above or below the ground? ");
                    }
                }

                if (startingPoints.Count > 0 && destinationNode != null && !destinationNode.blocked)
                {
                    List<List<int>> areasAndSectors = pathfinder.worldData.hierachalPathfinder.FindPaths(startingPoints, destinationNode, destinationArea);

                    pathfinder.KeepTrackOfUnitsInPaths(selectedUnits);
                    int key = pathfinder.GenerateKey(selectedUnits);

                    if (areasAndSectors != null)
                    {
                        pathfinder.worldData.intergrationFieldManager.StartIntegrationFieldCreation(destinationNode, areasAndSectors, null, null, key, false);
                        pathfinder.worldData.hierachalPathfinder.RemovePreviousSearch();
                    }
                }
            }
        }

        protected override void OnFinished()
        {
            pathfinder.PathCreated(path, key, pathEdit);
        }

        public void PathCreated(FlowFieldPath _path, bool _pathEdit)
        {
            path = _path;
            key = path.key;
            pathEdit = _pathEdit;
        }
    }
}
