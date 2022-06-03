using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class SeekerMovementManager : MonoBehaviour
    {
        public QuadTree quadTree;
        private List<QuadTree> openQuadTreeList;

        public OcTree octTree;
        private List<OcTree> openOctTreeList;

        private Rect searchQuad = new Rect(0, 0, 0, 0);
        private float[] searchOct = new float[6];


        private List<Seeker> allSeekers = new List<Seeker>();
        private Pathfinder pathfinder;
        private WorldData worldData;
        private IntVector2 key = new IntVector2();
        

        public static int maxNeighbourCount = 10;
        private int maxAmountOfTrees = 1;


       // quadTree
       //  /*
        public void Setup(Pathfinder _pathfinder, WorldData _worldData)
        {
            quadTree = new QuadTree(0, new Rect(0, 0, _pathfinder.worldWidth, _pathfinder.worldLength), _pathfinder);
            quadTree.Setup();
            worldData = _worldData;
            pathfinder = _pathfinder;

            int currentStep = 1;
            for (int i = 1; i < QuadTree.maxDepthLevel + 1; i++)
            {
                currentStep = currentStep * 4;
                maxAmountOfTrees += currentStep;
            }

            openQuadTreeList = new List<QuadTree>(maxAmountOfTrees);
        }

        // make seekers move and update their "neighbour" array  for proper steering forces
        void Update()
        {
            // empty tree and input all seekers
            quadTree.clear();
            for (int i = 0; i < allSeekers.Count; i++)
                quadTree.insert(allSeekers[i]);

            for (int i = 0; i < allSeekers.Count; i++)
            {
                // search box that exactly matches the search radius/circle
                SetNeighboursQuad(allSeekers[i], allSeekers[i].neighbourRadiusSquared);
                allSeekers[i].Tick();
            }
        }

        public void SetNeighboursQuad(Seeker seeker, float neighbourRadiusSquared)
        {
            SetSearchBoxQuad(seeker);
            RetrieveSeekerNeighboursQuad(seeker, neighbourRadiusSquared);
        }

        private void SetSearchBoxQuad(Seeker seeker)
        {
            searchQuad.xMin = seeker.transform.position.x - pathfinder.worldStart.x - seeker.neighbourRadius;
            searchQuad.yMin = pathfinder.worldStart.z - seeker.transform.position.z - seeker.neighbourRadius;
            searchQuad.width = searchQuad.height = seeker.neighbourRadius * 2f;
        }

        private void RetrieveSeekerNeighboursQuad(Seeker seeker, float radiusSquared)
        {
            for (int i = 0; i < maxNeighbourCount; i++)
                seeker.neighbours[i] = null;

            Vector3 directionToTarget;
            float dSqrToTarget;

            openQuadTreeList.Clear();
            openQuadTreeList.Add(quadTree);
            QuadTree current = null;

            int foundCount = 0;
            while (openQuadTreeList.Count > 0 && foundCount < maxNeighbourCount)
            {
                current = openQuadTreeList[0];

                if (current.bounds.Overlaps(searchQuad))
                {
                    for (int i = 0; i < current.objects.Count; i++)
                    {
                        if (current.objects[i] != seeker)
                        {
                            //vector3, sphere radius.   !use vector2 if you want circle radius!
                            directionToTarget = current.objects[i].transform.position - seeker.transform.position;
                            dSqrToTarget = directionToTarget.sqrMagnitude;

                            if (foundCount == maxNeighbourCount)
                                break;

                            if (dSqrToTarget < radiusSquared)
                            {
                                seeker.neighbours[foundCount] = current.objects[i];
                                foundCount++;
                            }
                        }
                    }

                    if (current.nodesInUse)
                    {
                        openQuadTreeList.Add(current.nodes[0]);
                        openQuadTreeList.Add(current.nodes[1]);
                        openQuadTreeList.Add(current.nodes[2]);
                        openQuadTreeList.Add(current.nodes[3]);
                    }
                }

                openQuadTreeList.Remove(current);
            }
        }
        //   */




        // octTree
        /*
        public void Setup(Pathfinder _pathfinder, WorldData _worldData)
        {
            octTree = new OctTree(0, new float[] { _pathfinder.worldStart.x + _pathfinder.worldWidth * 0.5f, _pathfinder.worldStart.z - _pathfinder.worldLength * 0.5f, _pathfinder.worldStart.y + _pathfinder.worldHeight * 0.5f, _pathfinder.worldWidth * 0.5f, _pathfinder.worldLength * 0.5f, _pathfinder.worldHeight * 0.5f }, _pathfinder);
            octTree.Setup();
            worldData = _worldData;
            pathfinder = _pathfinder;

            int currentStep = 1;
            for (int i = 1; i < OctTree.maxDepthLevel + 1; i++)
            {
                currentStep = currentStep * 4;
                maxAmountOfTrees += currentStep;
            }

            openOctTreeList = new List<OctTree>(maxAmountOfTrees);
        }

        // make seekers move and update their "neighbour" array  for proper steering forces
        void Update()
        {
            // empty tree and input all seekers
            octTree.clear();
            for (int i = 0; i < allSeekers.Count; i++)
                octTree.insert(allSeekers[i]);

            for (int i = 0; i < allSeekers.Count; i++)
            {
                // search box that exactly matches the search radius/circle
                SetNeighboursOct(allSeekers[i], allSeekers[i].neighbourRadiusSquared);
                allSeekers[i].Tick();
            }
        }

        public void SetNeighboursOct(Seeker seeker, float neighbourRadiusSquared)
        {
            SetSearchBoxOct(seeker);
            RetrieveSeekerNeighboursOct(seeker, neighbourRadiusSquared);
        }

        private void SetSearchBoxOct(Seeker seeker)
        {
            searchOct[0] = seeker.transform.position.x;
            searchOct[1] = seeker.transform.position.z;
            searchOct[2] = seeker.transform.position.y;

            searchOct[3] = searchOct[4] = searchOct[5] = seeker.neighbourRadius;
        }

        private void RetrieveSeekerNeighboursOct(Seeker seeker, float radiusSquared)
        {
            for (int i = 0; i < maxNeighbourCount; i++)
                seeker.neighbours[i] = null;

            Vector3 directionToTarget;
            float dSqrToTarget;

            openOctTreeList.Clear();
            openOctTreeList.Add(octTree);
            OctTree current = null;

            int foundCount = 0;
            while (openOctTreeList.Count > 0 && foundCount < maxNeighbourCount)
            {
                current = openOctTreeList[0];

                if (OctOverlaps(current.bounds, searchOct))//  current.bounds.Overlaps(searchBox))
                {
                    for (int i = 0; i < current.objects.Count; i++)
                    {
                        if (current.objects[i] != seeker)
                        {
                            //vector3, sphere radius
                            directionToTarget = current.objects[i].transform.position - seeker.transform.position;
                            dSqrToTarget = directionToTarget.sqrMagnitude;

                            if (foundCount == maxNeighbourCount)
                                break;

                            if (dSqrToTarget < radiusSquared)
                            {
                                seeker.neighbours[foundCount] = current.objects[i];
                                foundCount++;
                            }
                        }
                    }

                    if (current.nodesInUse)
                    {
                        openOctTreeList.Add(current.nodes[0]);
                        openOctTreeList.Add(current.nodes[1]);
                        openOctTreeList.Add(current.nodes[2]);
                        openOctTreeList.Add(current.nodes[3]);
                        openOctTreeList.Add(current.nodes[4]);
                        openOctTreeList.Add(current.nodes[5]);
                        openOctTreeList.Add(current.nodes[6]);
                        openOctTreeList.Add(current.nodes[7]);
                    }
                }

                openOctTreeList.Remove(current);
            }
        }

        private bool OctOverlaps(float[] currentBounds, float[] otherBounds)
        {
            if (Mathf.Abs(currentBounds[0] - otherBounds[0]) < (currentBounds[3] + otherBounds[3])) // x
            {
                if (Mathf.Abs(currentBounds[1] - otherBounds[1]) < (currentBounds[4] + otherBounds[4])) // z
                {
                    if (Mathf.Abs(currentBounds[2] - otherBounds[2]) < (currentBounds[5] + otherBounds[5])) // y
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        */




        // add seeker to manager, now the seeker's tick() will be called,  thus making him move
        public void AddSeeker(Seeker seeker)
        {
            allSeekers.Add(seeker);
        }

        // remove a seeker when you do not want him to move / or is dead/removed
        public void RemoveSeeker(Seeker seeker)
        {
            allSeekers.Remove(seeker);
        }




        public Vector2 FindflowValueFromPosition(Vector3 worldPosition, FlowField flowField, Seeker seeker)
        {
            seeker.currentWorldArea = null;
            seeker.currentTile = null;

            Vector2 vec = Vector2.zero;
            WorldArea area = worldData.tileManager.GetWorldAreaAtPosition(worldPosition);

            if (area != null)
            {
                int worldX = worldData.tileManager.locationToWorldGridX(worldPosition);
                int worldY = worldData.tileManager.locationToWorldGridY(worldPosition);

                Tile tile = area.tileGrid[worldX - area.leftOffset][worldY - area.topOffset];
                if (tile != null)
                {
                    seeker.currentWorldArea = area;
                    seeker.currentTile = tile;

                    key.x = area.index;
                    key.y = tile.sectorIndex;

                    if (flowField.field.ContainsKey(key))
                        vec = worldData.flowFieldManager.DirToVector(flowField.field[key][tile.indexWithinSector]);         
                }
            }

            return vec;
        }

        public void SetUnitAreaAndTile(Seeker seeker, Vector3 worldPosition)
        {
            WorldArea area = worldData.tileManager.GetWorldAreaAtPosition(worldPosition);

            if (area != null)
            {
                int worldX = worldData.tileManager.locationToWorldGridX(worldPosition);
                int worldY = worldData.tileManager.locationToWorldGridY(worldPosition);

                Tile tile = area.GetTileFromGrid(worldX - area.leftOffset, worldY - area.topOffset);
                if (tile != null)
                {
                    seeker.currentWorldArea = area;
                    seeker.currentTile = tile;
                }
            }
        }


        private int CheckIfMovingToOtherArea(Seeker seeker, int xDir, int yDir)
        {
            if (worldData.pathfinder.worldIsMultiLayered)
            {
                IntVector2 key = seeker.currentTile.gridPos;
                if (seeker.currentWorldArea.worldAreaTileConnections.ContainsKey(key)) // if our current tile connects to tile in another world area, outside of our own tilegrid
                {
                    List<IntVector3> values = seeker.currentWorldArea.worldAreaTileConnections[key];

                    for (int i = 0; i < values.Count; i++)
                    {
                        if (!worldData.worldAreas[values[i].z].tileGrid[values[i].x][values[i].y].blocked)
                        {
                            if (xDir != 0)
                            {
                                if (worldData.worldAreas[values[i].z].topOffset + values[i].y == seeker.currentWorldArea.topOffset + key.y) // allign on Z. its straight ahaed or behind
                                {
                                    if (xDir == 1 && (worldData.worldAreas[values[i].z].leftOffset + values[i].x > seeker.currentWorldArea.leftOffset + key.x)) // right
                                        return values[i].z;
                                    else if (xDir == -1 && (worldData.worldAreas[values[i].z].leftOffset + values[i].x < seeker.currentWorldArea.leftOffset + key.x)) // left
                                        return values[i].z;
                                }
                            }
                            else
                            {
                                if (worldData.worldAreas[values[i].z].leftOffset + values[i].x == seeker.currentWorldArea.leftOffset + key.x && !worldData.worldAreas[values[i].z].tileGrid[values[i].x][values[i].y].blocked) // allign on X. its straight ahaed or behind
                                {
                                    if (yDir == -1 && (worldData.worldAreas[values[i].z].topOffset + values[i].y < seeker.currentWorldArea.topOffset + key.y)) // forward
                                        return values[i].z;
                                    else if (yDir == 1 && (worldData.worldAreas[values[i].z].topOffset + values[i].y > seeker.currentWorldArea.topOffset + key.y)) // back
                                        return values[i].z;
                                }
                            }
                        }

                    }
                }
            }

            return -1;
        }


        public void CheckIfMovementLegit(Seeker seeker)
        {
            if (seeker.currentTile != null && seeker.currentWorldArea != null)
            {
                int xDir = 0;
                int yDir = 0;


                bool capXMovement = false;
                bool capZMovement = false;
                int areaXMovement = -1;
                int areaZMovement = -1;

                Tile destination = null;

                if (seeker.movement.x != 0)
                {
                    if (seeker.movement.x > 0)
                        xDir = 1;
                    else
                        xDir = -1;
                }

                if (seeker.movement.z != 0)
                {
                    if (seeker.movement.z > 0)
                        yDir = -1;
                    else
                        yDir = 1;
                }


                if (yDir == 0 && xDir == 0)
                {
                    // nothing changed
                }
                else
                {
                    if (seeker.currentTile.gridPos.x + xDir > -1 && seeker.currentTile.gridPos.x + xDir < seeker.currentWorldArea.gridWidth)
                    {
                        // inside world area
                        destination = seeker.currentWorldArea.tileGrid[seeker.currentTile.gridPos.x + xDir][seeker.currentTile.gridPos.y];
                        if (destination == null) // null, can we travel to another worldarea, or is there nothing?
                        {
                            areaXMovement = CheckIfMovingToOtherArea(seeker, xDir, 0);
                            capXMovement = true;
                        }
                        else if (destination.blocked)
                        {
                            capXMovement = true;
                        }
                    }
                    else
                    {
                        areaXMovement = CheckIfMovingToOtherArea(seeker, xDir, 0);
                        capXMovement = true;
                    }



                    if (seeker.currentTile.gridPos.y + yDir > -1 && seeker.currentTile.gridPos.y + yDir < seeker.currentWorldArea.gridLength)
                    {
                        // inside world area
                        destination = seeker.currentWorldArea.tileGrid[seeker.currentTile.gridPos.x][seeker.currentTile.gridPos.y + yDir];
                        if (destination == null) // null, can we travel to another worldarea, or is there nothing?
                        {
                            areaZMovement = CheckIfMovingToOtherArea(seeker, 0, yDir);
                            capZMovement = true;
                        }
                        else if (destination.blocked)
                        {
                            capZMovement = true;
                        }
                    }
                    else
                    {
                        // outside
                        areaZMovement = CheckIfMovingToOtherArea(seeker, 0, yDir);
                        capZMovement = true;
                    }


                    if (!capXMovement && !capZMovement) // digonal check
                    {
                        destination = seeker.currentWorldArea.tileGrid[seeker.currentTile.gridPos.x + xDir][seeker.currentTile.gridPos.y + yDir];
                        if (destination == null || destination.blocked)
                        {
                            if (seeker.desiredFlowValue == Vector2.zero)
                                seeker.movement = Vector3.zero;
                            else
                            {
                                if (seeker.desiredFlowValue.x == 0 || (seeker.desiredFlowValue.x > 0 && seeker.movement.x < 0) || (seeker.desiredFlowValue.x < 0 && seeker.movement.x > 0))
                                {
                                    float futureXPos = seeker.transform.position.x + seeker.movement.x;
                                    float border = worldData.tileManager.GetTileWorldPosition(seeker.currentTile, seeker.currentWorldArea).x + (worldData.pathfinder.tileSize * 0.49f * xDir);
                                    if (xDir == 1 && futureXPos > border)
                                        seeker.movement.x -= (futureXPos - border);
                                    else if (xDir == -1 && futureXPos < border)
                                        seeker.movement.x -= (futureXPos - border);
                                }

                                if (seeker.desiredFlowValue.y == 0 || (seeker.movement.z > 0 && seeker.desiredFlowValue.y < 0) || (seeker.movement.z < 0 && seeker.desiredFlowValue.y > 0))
                                {
                                    float futureZPos = seeker.transform.position.z + seeker.movement.z;
                                    float border = worldData.tileManager.GetTileWorldPosition(seeker.currentTile, seeker.currentWorldArea).z + (worldData.pathfinder.tileSize * 0.49f * -yDir);
                                    if (yDir == -1 && futureZPos > border)
                                        seeker.movement.z -= (futureZPos - border);
                                    else if (yDir == 1 && futureZPos < border)
                                        seeker.movement.z -= (futureZPos - border);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (areaXMovement != -1 && areaZMovement != -1)
                        {
                            if (seeker.desiredFlowValue.x == 0 || (seeker.desiredFlowValue.x > 0 && seeker.movement.x < 0) || (seeker.desiredFlowValue.x < 0 && seeker.movement.x > 0))
                                areaXMovement = -1;
                            else
                                areaZMovement = -1;
                        }
                    }


                    if (capXMovement)
                    {
                        if (areaXMovement == -1)
                        {
                            float futureXPos = seeker.transform.position.x + seeker.movement.x;
                            float border = worldData.tileManager.GetTileWorldPosition(seeker.currentTile, seeker.currentWorldArea).x + (worldData.pathfinder.tileSize * 0.49f * xDir);
                            if (xDir == 1 && futureXPos > border)
                                seeker.movement.x -= (futureXPos - border);
                            else if (xDir == -1 && futureXPos < border)
                                seeker.movement.x -= (futureXPos - border);
                        }
                        else
                        {
                            if (yDir != 0)
                            {
                                bool capZ = false;
                                int destinationAreaGridX = (seeker.currentTile.gridPos.x + xDir + seeker.currentWorldArea.leftOffset) - worldData.worldAreas[areaXMovement].leftOffset;
                                int destinationAreaGridY = (seeker.currentTile.gridPos.y + yDir + seeker.currentWorldArea.topOffset) - worldData.worldAreas[areaXMovement].topOffset;

                                if (destinationAreaGridY < worldData.worldAreas[areaXMovement].gridLength && destinationAreaGridY > -1)
                                {
                                    destination = worldData.worldAreas[areaXMovement].tileGrid[destinationAreaGridX][destinationAreaGridY];
                                    if (destination == null || destination.blocked)
                                        capZ = true;
                                }
                                else
                                    capZ = true;


                                if (capZ)
                                {
                                    float futureZPos = seeker.transform.position.z + seeker.movement.z;
                                    float border = worldData.tileManager.GetTileWorldPosition(seeker.currentTile, seeker.currentWorldArea).z + (worldData.pathfinder.tileSize * 0.49f * -yDir);
                                    if (yDir == -1 && futureZPos > border)
                                        seeker.movement.z -= (futureZPos - border);
                                    else if (yDir == 1 && futureZPos < border)
                                        seeker.movement.z -= (futureZPos - border);
                                }
                            }
                        }
                    }


                    if (capZMovement)
                    {
                        if (areaZMovement == -1)
                        {
                            float futureZPos = seeker.transform.position.z + seeker.movement.z;
                            float border = worldData.tileManager.GetTileWorldPosition(seeker.currentTile, seeker.currentWorldArea).z + (worldData.pathfinder.tileSize * 0.49f * -yDir);
                            if (yDir == -1 && futureZPos > border)
                                seeker.movement.z -= (futureZPos - border);
                            else if (yDir == 1 && futureZPos < border)
                                seeker.movement.z -= (futureZPos - border);
                        }
                        else
                        {
                            if (yDir != 0)
                            {
                                bool capX = false;
                                int destinationAreaGridX = (seeker.currentTile.gridPos.x + xDir + seeker.currentWorldArea.leftOffset) - worldData.worldAreas[areaZMovement].leftOffset;
                                int destinationAreaGridY = (seeker.currentTile.gridPos.y + yDir + seeker.currentWorldArea.topOffset) - worldData.worldAreas[areaZMovement].topOffset;

                                if (destinationAreaGridX < worldData.worldAreas[areaZMovement].gridWidth && destinationAreaGridX > -1)
                                {
                                    destination = worldData.worldAreas[areaZMovement].tileGrid[destinationAreaGridX][destinationAreaGridY];
                                    if (destination == null || destination.blocked)
                                        capX = true;
                                }
                                else
                                    capX = true;


                                if (capX)
                                {
                                    float futureXPos = seeker.transform.position.x + seeker.movement.x;
                                    float border = worldData.tileManager.GetTileWorldPosition(seeker.currentTile, seeker.currentWorldArea).x + (worldData.pathfinder.tileSize * 0.49f * xDir);
                                    if (xDir == 1 && futureXPos > border)
                                        seeker.movement.x -= (futureXPos - border);
                                    else if (xDir == -1 && futureXPos < border)
                                        seeker.movement.x -= (futureXPos - border);
                                }
                            }
                        }
                    }
                }
            }
        }



    }
}
