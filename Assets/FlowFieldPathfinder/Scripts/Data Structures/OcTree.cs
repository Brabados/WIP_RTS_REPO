using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class OcTree
    {
        private int maxObjectsPerQuad = 15;
        public static int maxDepthLevel = 4;

        public int level;
        public List<Seeker> objects;
        public float[] bounds = new float[6]; // !quad is diffrent!   oct = !center/pivot position: x,y,z  -  half:width,length,height
        public OcTree[] nodes;
        private Pathfinder pathfinder;

        public bool nodesInUse = false; //if false, the child nodes are not used

        public OcTree(int _level, float[] _bounds, Pathfinder _pathfinder)
        {
            level = _level;
            objects = new List<Seeker>(maxObjectsPerQuad + 1);
            bounds = _bounds;
            nodes = new OcTree[8];
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
            float subWidth = bounds[3] * 0.5f;
            float subLength = bounds[4] * 0.5f;
            float subHeight = bounds[5] * 0.5f;

            float x = bounds[0];
            float y = bounds[1];
            float z = bounds[2];

            // bottom half
            nodes[0] = new OcTree(level + 1, new float[] { x + subWidth, y + subLength, z - subHeight, subWidth, subLength, subHeight }, pathfinder);
            nodes[1] = new OcTree(level + 1, new float[] { x - subWidth, y + subLength, z - subHeight, subWidth, subLength, subHeight }, pathfinder);
            nodes[2] = new OcTree(level + 1, new float[] { x - subWidth, y - subLength, z - subHeight, subWidth, subLength, subHeight }, pathfinder);
            nodes[3] = new OcTree(level + 1, new float[] { x + subWidth, y - subLength, z - subHeight, subWidth, subLength, subHeight }, pathfinder);

            // top half
            nodes[4] = new OcTree(level + 1, new float[] { x + subWidth, y + subLength, z + subHeight, subWidth, subLength, subHeight }, pathfinder);
            nodes[5] = new OcTree(level + 1, new float[] { x - subWidth, y + subLength, z + subHeight, subWidth, subLength, subHeight }, pathfinder);
            nodes[6] = new OcTree(level + 1, new float[] { x - subWidth, y - subLength, z + subHeight, subWidth, subLength, subHeight }, pathfinder);
            nodes[7] = new OcTree(level + 1, new float[] { x + subWidth, y - subLength, z + subHeight, subWidth, subLength, subHeight }, pathfinder);
        }


        private int getIndex(Seeker seeker)
        {
            int index = -1;

            // Seeker is inside the top half // 2dview
            bool topQuadrant = seeker.transform.position.z > bounds[1];
            // Seeker is inside the bottom half // 2dview
            bool bottomQuadrant = seeker.transform.position.z <= bounds[1];
            // Seeker is inside the top height half
            bool topWorldY_Quadrant = seeker.transform.position.y >= bounds[2];

            // Seeker is inside the left half
            if (seeker.transform.position.x < bounds[0])
            {
                if (topWorldY_Quadrant)
                {
                    if (topQuadrant)
                        index = 5;
                    else if (bottomQuadrant)
                        index = 6;
                }
                else
                {
                    if (topQuadrant)
                        index = 1;
                    else if (bottomQuadrant)
                        index = 2;
                }
            }

            // Seeker is inside the right half
            else if (seeker.transform.position.x >= bounds[0])
            {
                if (topWorldY_Quadrant)
                {
                    if (topQuadrant)
                        index = 4;
                    else if (bottomQuadrant)
                        index = 7;
                }
                else
                {
                    if (topQuadrant)
                        index = 0;
                    else if (bottomQuadrant)
                        index = 3;
                }
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