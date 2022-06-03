using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace FlowPathfinding
{
    public class MultiLevelSector
    {
        // connections for visual debugging
        public List<Tile> connections = new List<Tile>();
        // connection Lengths for visual debugging
        public List<int> connectionLengths = new List<int>();

        // level depth.  0 = first layer of abstraction,  1 = second, etc.
        public int level = 0;

        public int ID = 0;

        // index world area this sector is in
        public int worldAreaIndex;

        public int gridX = 0;
        public int gridY = 0;

        public int top = 0;
        public int bottom = 0;
        public int left = 0;
        public int right = 0;

        public int tilesInWidth = 0;
        public int tilesInHeight = 0;

        // list of higher nodes, orderned by edge  up 0, down 1, left 2, right 3
        public List<AbstractNode>[] sectorNodesOnEdge = new List<AbstractNode>[4];
        // nodes that have a direct connection to a node on a diffrent World Area
        public Dictionary<AbstractNode, int> worldAreaNodes = new Dictionary<AbstractNode, int>();

        public void Setup()
        {
            for (int i = 0; i < sectorNodesOnEdge.Length; i++)
                sectorNodesOnEdge[i] = new List<AbstractNode>();
        }

        // set lists for visual debugging
        public void SearchConnections()
        {
            connections.Clear();
            connectionLengths.Clear();

            foreach (List<AbstractNode> list in sectorNodesOnEdge)
            {
                foreach (AbstractNode sectorNode in list)
                {
                    foreach (AbstractNode connection in sectorNode.connections.Keys)
                        TryToAddConnection(sectorNode.tileConnection, connection.tileConnection, sectorNode.connections[connection]);
                }
            }
        }

        // set lists for visual debugging
        private void TryToAddConnection(Tile sectorTile, Tile connectedTile, int distance)
        {
            bool alreadyOnList = false;

            for (int i = 0; i < connections.Count; i += 2)
            {
                if ((connections[i] == sectorTile && connections[i + 1] == connectedTile) || (connections[i] == connectedTile && connections[i + 1] == sectorTile))
                {
                    alreadyOnList = true;
                    break;
                }
            }

            if (!alreadyOnList)
            {
                connections.Add(sectorTile);
                connections.Add(connectedTile);
                connectionLengths.Add(distance);
            }
        }

    }
}
