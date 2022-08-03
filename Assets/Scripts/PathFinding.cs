using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;


/// <summary>
/// Calculates the path form seeker to target
/// </summary>
public class PathFinding : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    PathRequestManager requestManager;

    /// <summary>
    /// Grid plane positions
    /// </summary>
    GridPlane gridPlane;

    #region Unity Methods

    void Awake()
    {
        gridPlane = GetComponent<GridPlane>();
        requestManager = GetComponent<PathRequestManager>();
    }


    #endregion Unity Methods

    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    /// <summary>
    /// Calculates the route to target position
    /// </summary>
    /// <param name="startPos">start position</param>
    /// <param name="targetPos">end position</param>
    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Vector3[] wayPoints = new Vector3[0];
        bool pathSucess = false;

        Node startNode = gridPlane.NodeFromWorldPoint(startPos);
        Node targetNode = gridPlane.NodeFromWorldPoint(targetPos);

        if (startNode.walkable && targetNode.walkable)
        {

            Heap<Node> openSet = new Heap<Node>(gridPlane.MaxSize);
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {

                Node currentNode = openSet.RemoveFirst();

                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    pathSucess = true;
                    break;
                }

                foreach (Node neigbour in gridPlane.GetNeighbours(currentNode))
                {
                    if (!neigbour.walkable || closedSet.Contains(neigbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neigbour);

                    if (newMovementCostToNeighbour < neigbour.gCost || !openSet.Contains(neigbour))
                    {
                        neigbour.gCost = newMovementCostToNeighbour;
                        neigbour.hCost = GetDistance(neigbour, targetNode);
                        neigbour.parent = currentNode;

                        if (!openSet.Contains(neigbour))
                        {
                            openSet.Add(neigbour);
						}
                    }
                }
            }
        }

        yield return null;

        if (pathSucess)
        {
            wayPoints = RetracePath(startNode, targetNode);
        }

        requestManager.FinishedProcessingPath(wayPoints, pathSucess);
    }

    /// <summary>
    /// Add the path to the parent 
    /// </summary>
    /// <param name="startNode">start position</param>
    /// <param name="endNode">end position</param>
    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode!= startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        
        Vector3[] waypoints = SimplifyPath(path);
        Array.Reverse(waypoints);

        return waypoints;

       // gridPlane.path = path;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
            if (directionNew!= directionOld)
            {
                waypoints.Add(path[i].worldPosition);
            }
            directionOld = directionNew;
        }

        return waypoints.ToArray();
    }

    /// <summary>
    /// Gets the distance from one to another point
    /// </summary>
    /// <param name="nodeA">seeker</param>
    /// <param name="nodeB">target</param>
    /// <returns></returns>
    int GetDistance(Node nodeA, Node nodeB)
    {
        int distX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (distX>distY)
        {
            return 14 * distY + 10 * (distX - distY);
        }

        return 14 * distX + 10 * (distY - distX);
    }
}
