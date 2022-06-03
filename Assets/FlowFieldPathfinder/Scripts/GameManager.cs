using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class GameManager : MonoBehaviour
    {
        private Dictionary<Tile, GameObject> obstacles = new Dictionary<Tile, GameObject>();

        public Pathfinder pathfinder;
        public GameObject unitHolder;

        private int controllGroup = 0;
        private List<List<Seeker>> selectedUnits = new List<List<Seeker>>();

        private bool gameHasStarted = false;

        void Start()
        {
            for (int i = 0; i < 3; i++)
                selectedUnits.Add(new List<Seeker>());

            bool aSwitch = true;
            foreach (Transform child in unitHolder.transform)
            {
                selectedUnits[0].Add(child.GetComponent<Seeker>());

                aSwitch = !aSwitch;
                if(aSwitch)
                    selectedUnits[1].Add(child.GetComponent<Seeker>());
                else
                    selectedUnits[2].Add(child.GetComponent<Seeker>());
            }
        }

        void Update()
        {
            if (gameHasStarted)
                Inputs();
            else
                gameHasStarted = true;
        }


        private void Inputs()
        {
            Tile tile = pathfinder.worldData.tileManager.GetTileFromPosition(pathfinder.GetMousePosition());
            if (tile != null)
            {
                if (Input.GetMouseButtonDown(0))
                    pathfinder.FindPath(tile, selectedUnits[controllGroup]);


                if (Input.GetMouseButton(1) && Input.GetKey("b"))
                {
                    if (!tile.blocked)
                    {
                        GameObject blockade = Resources.Load("Prefab/Obstacle") as GameObject;
                        GameObject b = Instantiate(blockade, pathfinder.worldData.tileManager.GetTileWorldPosition(tile, pathfinder.worldData.worldAreas[tile.worldAreaIndex]) + new Vector3(0, 0.1f, 0), Quaternion.identity) as GameObject;
                        b.transform.parent = transform;
                        obstacles.Add(tile, b);
                    }

                    pathfinder.worldData.worldManager.BlockTile(tile);
                }


                if (Input.GetMouseButton(1) && Input.GetKey("n"))
                {
                    if (tile.blocked)
                    {
                        Destroy(obstacles[tile]);
                        obstacles.Remove(tile);
                    }

                    pathfinder.worldData.worldManager.UnBlockTile(tile);
                }

                if (Input.GetMouseButton(1) && Input.GetKey("c"))
                    pathfinder.worldData.worldManager.SetTileCost(tile, 10);


                if (Input.GetKeyDown("0"))
                    controllGroup = 0;

                if (Input.GetKeyDown("1"))
                    controllGroup = 1;

                if (Input.GetKeyDown("2"))
                    controllGroup = 2;
            }


        }
    }
}
