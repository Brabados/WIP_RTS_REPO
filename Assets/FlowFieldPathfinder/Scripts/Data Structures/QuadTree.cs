using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class QuadTree
    {
        private int maxObjectsPerQuad = 15;
        public static int maxDepthLevel = 5;

        private int level;
        public List<Seeker> objects;
        public Rect bounds;
        public QuadTree[] nodes;
        private Pathfinder pathfinder;

        public bool nodesInUse = false; //if false, the child nodes are not used

        public QuadTree(int _level, Rect _bounds, Pathfinder _pathfinder)
        {
            level = _level;
            objects = new List<Seeker>(maxObjectsPerQuad + 1);
            bounds = _bounds;
            nodes = new QuadTree[4];
            pathfinder = _pathfinder;
        }

        public void Setup()
        {       
            if (level < maxDepthLevel)
            {
                split();
                for (int i = 0; i < nodes.Length; i++)
                    nodes[i].Setup();
            }
        }

        public void clear()
        {    
            objects.Clear();
            if (nodesInUse)
            {
                for (int i = 0; i < nodes.Length; i++)
                    nodes[i].clear();
            }

            nodesInUse = false;
        }


        private void split()
        {
            float subWidth = bounds.width / 2f;
            float subHeight = bounds.height / 2f;
            float x = bounds.xMin;
            float y = bounds.yMin;

            nodes[0] = new QuadTree(level + 1, new Rect(x + subWidth, y, subWidth, subHeight), pathfinder);
            nodes[1] = new QuadTree(level + 1, new Rect(x, y, subWidth, subHeight), pathfinder);
            nodes[2] = new QuadTree(level + 1, new Rect(x, y + subHeight, subWidth, subHeight), pathfinder);
            nodes[3] = new QuadTree(level + 1, new Rect(x + subWidth, y + subHeight, subWidth, subHeight), pathfinder);
        }


        private int getIndex(Seeker seeker)
        {
            int index = -1;
            float verticalMidpoint = bounds.xMin + (bounds.width / 2f);
            float horizontalMidpoint = bounds.yMin + (bounds.height / 2f);

            // Seeker is inside the top half
            bool topQuadrant = pathfinder.worldStart.z - seeker.transform.position.z < horizontalMidpoint;
            // Seeker is inside the bottom half
            bool bottomQuadrant = pathfinder.worldStart.z - seeker.transform.position.z >= horizontalMidpoint;

            // Seeker is inside the left half
            if (seeker.transform.position.x - pathfinder.worldStart.x < verticalMidpoint)
            {
                if (topQuadrant)
                    index = 1;
                else if (bottomQuadrant)
                    index = 2;
            }

            // Seeker is inside the right half
            else if (seeker.transform.position.x - pathfinder.worldStart.x >= verticalMidpoint)
            {
                if (topQuadrant)
                    index = 0;
                else if (bottomQuadrant)
                    index = 3;
            }

            return index;
        }


        public void insert(Seeker seeker)
        {
            if (nodesInUse)
            {
                int index = getIndex(seeker);

                if (index != -1)
                {
                    nodes[index].insert(seeker);

                    return;
                }
            }

            objects.Add(seeker);

            if (objects.Count > maxObjectsPerQuad && level < maxDepthLevel)
            {
                nodesInUse = true;

                int i = 0;
                while (i < objects.Count)
                {
                    Seeker obj = objects[i];

                    int index = getIndex(obj);
                    if (index != -1)
                    {
                        nodes[index].insert(obj);
                        objects.Remove(obj);
                    }
                    else
                        i++;
                }
            }
        }

    }
}