using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowPathfinding
{
    public class Seeker : MonoBehaviour
    {
        private Pathfinder pathfinder;

        [HideInInspector]
        public WorldArea currentWorldArea;
        [HideInInspector]
        public Tile currentTile;
        [HideInInspector]
        public Vector2 desiredFlowValue = Vector2.zero; 

        float flowWeight = 0.9f;
        float sepWeight = 2.5f;
        float sepWeightIdle = 2.75f;
        float alignWeight = 0.3f;
        float cohWeight = 0f;

        float maxForce = 4; // maximun magnitude of the (combined) force vector that is applied each tick
        float maxMoveSpeed = 4; // maximun magnitude of the (combined) velocity vector
        float maxIdleSpeed = 0.8f; // maximun magnitude of the (combined) velocity vector

        public static float neighbourRadiusMoving = 2.2f;
        private float neighbourRadiusSquaredMoving = 0;
        public static float neighbourRadiusIdle = 0.95f;
        private float neighbourRadiusSquaredIdle = 0;

        public float neighbourRadius = 0;
        public float neighbourRadiusSquared = 0;
    

        [HideInInspector]
        public Vector3 movement = new Vector3();
        [HideInInspector]
        public Vector2 velocity = Vector2.zero;
        public Seeker[] neighbours;

        private Vector2[] combinedForces = new Vector2[4];

        [HideInInspector]
        public FlowFieldPath flowFieldPath = null;

        private CharacterController controller;

        private enum State
        {
            Idle,
            Moving
        }

        private State seekerState = State.Moving;


        // Use this for initialization
        void Start()
        {
            neighbourRadiusSquaredMoving = neighbourRadiusMoving * neighbourRadiusMoving;
            neighbourRadiusSquaredIdle = neighbourRadiusIdle * neighbourRadiusIdle;
            SetNeighbourRadius(seekerState);

            neighbours = new Seeker[SeekerMovementManager.maxNeighbourCount]; 
            pathfinder = GameObject.Find("Pathfinder").GetComponent<Pathfinder>() as Pathfinder;
            controller = GetComponent<CharacterController>();

            pathfinder.seekerManager.AddSeeker(this);
            pathfinder.seekerManager.SetUnitAreaAndTile(this, transform.position - (Vector3.up * 0.57f));
        }

        private void SetNeighbourRadius(State currentState)
        {
            switch (seekerState)
            {
                case State.Idle:
                    {
                        neighbourRadius = neighbourRadiusIdle;
                        neighbourRadiusSquared = neighbourRadiusSquaredIdle;
                    }
                    break;
                case State.Moving:
                    {
                        neighbourRadius = neighbourRadiusMoving;
                        neighbourRadiusSquared = neighbourRadiusSquaredMoving;
                    }
                    break;
            }
        }

        // Update is called once per frame
        public void Tick()
        {
            switch (seekerState)
            {
                case State.Idle:
                    Idle();
                    break;
                case State.Moving:
                    Move();
                    break;
            }

            // make sure Seeker will not fall in null area, offmap or go into a blocked of tile
            pathfinder.seekerManager.CheckIfMovementLegit(this);

            //movement
            controller.Move(movement);

            // rotation
            movement.y = 0;
            transform.LookAt(transform.position + movement);
        }

        private void Idle()
        {
            if (neighbours[0] == null)
            {
                velocity = Vector2.zero;
                movement = Vector3.zero;
            }
            else
            {
                Vector2 netForce = Vector2.zero;

                // 4 steering Vectors in order: Flow, separation, alignment, cohesion
                // adjusted with user defined weights
                FlowFieldFollow();

                combinedForces[0] = sepWeightIdle * Separation(neighbourRadiusSquaredIdle); // seperation
                combinedForces[1] = Vector2.zero;
                combinedForces[2] = Vector2.zero;
                combinedForces[3] = Vector2.zero;

                // calculate the combined force, but dont go over the maximum force
                netForce = CombineForces(maxForce, combinedForces);
                // velocity gets adjusted by the calculated force
                velocity += netForce * Time.deltaTime;

                // dont go over the maximum movement speed possible
                if (velocity.magnitude > maxIdleSpeed)
                    velocity = (velocity / velocity.magnitude) * maxIdleSpeed;

                // move
                movement = new Vector3(velocity.x * Time.deltaTime, -9.8f * Time.deltaTime, velocity.y * Time.deltaTime);
            }
        }


        private void Move()
        {
            if (flowFieldPath != null)
            {
                if (currentTile == flowFieldPath.destination)
                    ReachedDestination();
                else
                {
                    Vector2 netForce = Vector2.zero;

                    // 4 steering Vectors in order: Flow, separation, alignment, cohesion
                    // adjusted with user defined weights
                    combinedForces[0] = flowWeight * FlowFieldFollow();
                    combinedForces[1] = sepWeight * Separation(neighbourRadiusSquaredMoving);
                    combinedForces[2] = alignWeight * Alignment();
                    combinedForces[3] = cohWeight * Cohesion();


                    // calculate the combined force, but dont go over the maximum force
                    netForce = CombineForces(maxForce, combinedForces);

                    // velocity gets adjusted by the calculated force
                    velocity += netForce * Time.deltaTime;
                    // dont go over the maximum movement speed possible
                    if (velocity.magnitude > maxMoveSpeed)
                        velocity = (velocity / velocity.magnitude) * maxMoveSpeed;

                    // move
                    movement = new Vector3(velocity.x * Time.deltaTime, -9.8f * Time.deltaTime, velocity.y * Time.deltaTime);
                }
            }
        }


        // become idle, and the charcaters around you
        private void ReachedDestination()
        {
            velocity = Vector2.zero;
            movement = Vector3.zero;
            seekerState = State.Idle;
            SetNeighbourRadius(seekerState);

            // quad or oct, depends on what you are using in  SeekerMovementManager.cs
            if(pathfinder.seekerManager.quadTree != null)
                pathfinder.seekerManager.SetNeighboursQuad(this, 5f);
            //else
            //    pathfinder.seekerManager.SetNeighboursOct(this, 5f);
            
            for (int i = 0; i < neighbours.Length; i ++ )
            {
                if (neighbours[i] == null)
                    break;

                neighbours[i].seekerState = State.Idle;
                neighbours[i].SetNeighbourRadius(neighbours[i].seekerState);
                neighbours[i].velocity = Vector2.zero;
                neighbours[i].movement = Vector3.zero;
            }
        }


        Vector2 CombineForces(float maxForce, Vector2[] forces)
        {
            Vector2 force = Vector2.zero;

            for (int i = 0; i < forces.Length; i++)
            {
                Vector2 newForce = force + forces[i];

                if (newForce.magnitude > maxForce)
                {
                    float amountNeeded = maxForce - force.magnitude;
                    float amountAdded = forces[i].magnitude;
                    float division = amountNeeded / amountAdded;

                    force += division * forces[i];

                    return force;
                }
                else
                    force = newForce;
            }

            return force;
        }


        private Vector2 FlowFieldFollow()
        {   
            desiredFlowValue = pathfinder.seekerManager.FindflowValueFromPosition(transform.position - (Vector3.up * 0.57f), flowFieldPath.flowField, this);
            if (desiredFlowValue == Vector2.zero && currentWorldArea != null && currentTile != null && currentTile != flowFieldPath.destination && !pathfinder.WorkingOnPathAdjustment(flowFieldPath.key, currentWorldArea.index, currentTile.sectorIndex))
                pathfinder.AddSectorInPath(currentWorldArea, currentTile, flowFieldPath);

            // return the velocity we desire to go to
            desiredFlowValue *= maxMoveSpeed; // we desire this velocity
            desiredFlowValue -= velocity;

            return desiredFlowValue * (maxForce / maxMoveSpeed);

        }

        private Vector2 Separation(float squaredRadius)
        {
            if (neighbours[0] == null)
                return Vector2.zero;

            Vector2 totalForce = Vector2.zero;

            int neighbourAmount = 0;
            // get avarge push force away from neighbours
            for (int i = 0; i < neighbours.Length; i++)
            {
                if (neighbours[i] == null)
                    break;
                
                Vector2 pushforce = new Vector2(transform.position.x - neighbours[i].transform.position.x, transform.position.z - neighbours[i].transform.position.z);
                totalForce += pushforce.normalized * Mathf.Max(0.05f,(squaredRadius - pushforce.magnitude));
                neighbourAmount++;
            }

            totalForce /= neighbourAmount;//neighbours.Count;
            totalForce *= maxForce;

            return totalForce;
        }

        private Vector2 Cohesion()
        {
            if (neighbours[0] == null)
                return Vector2.zero;

            Vector2 pos = new Vector2(transform.position.x, transform.position.z);
            Vector2 centerOfMass = pos;

            int neighbourAmount = 0;
            for (int i = 0; i < neighbours.Length; i++)
            {
                if (neighbours[i] == null)
                    break;
                
                centerOfMass += new Vector2(neighbours[i].transform.position.x, neighbours[i].transform.position.z);
                neighbourAmount++;
            }

            centerOfMass /= neighbourAmount;

            Vector2 desired = centerOfMass - pos;
            desired *= (maxMoveSpeed / desired.magnitude);

            Vector2 force = desired - velocity;
            return force * (maxForce / maxMoveSpeed);
        }


        private Vector2 Alignment()
        {
            if (neighbours[0] == null)
                return Vector2.zero;

            // get avarge velocity from neighbours
            Vector2 averageHeading = velocity.normalized;

            int neighbourAmount = 0;
            for (int i = 0; i < neighbours.Length; i++)
            {
                if (neighbours[i] == null)
                    break;
                
                averageHeading += neighbours[i].velocity.normalized;
                neighbourAmount++;
            }

            averageHeading /= neighbourAmount;

            Vector2 desired = averageHeading * maxMoveSpeed;

            Vector2 force = desired - velocity;
            return force * (maxForce / maxMoveSpeed);
        }


        public void SetNeighbours(Seeker[] foundNeighbours)
        {
            neighbours = foundNeighbours;
        }


        public void SetFlowField(FlowFieldPath _flowFieldPath, bool pathEdit)
        {
            if (!pathEdit)
            {
                seekerState = State.Moving;
                SetNeighbourRadius(seekerState);
            }

            flowFieldPath = _flowFieldPath;
        }

    }
}
