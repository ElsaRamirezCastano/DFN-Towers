using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PathNode
{
    public Transform transform;
    public Vector2 position;
    public List<PathNode> connections = new List<PathNode>();
    public bool isActive = true;

    public PathNode(Transform t)
    {
        transform = t;
        position = t.position;
    }
}

public class DynamicPathSystem : MonoBehaviour
{
    public static DynamicPathSystem Instance;

    [Header("Path Configuration")]
    public Transform startPoint;
    public Transform endPoint;
    public List<Transform> availableWaypoints = new List<Transform>();

    private Dictionary<Transform, PathNode> nodeMap = new Dictionary<Transform, PathNode>();

    private List<PathNode> allNodes = new List<PathNode>();
    public System.Action OnPathChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePathSystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializePathSystem()
    {
        nodeMap.Clear();
        allNodes.Clear();

        if (startPoint != null)
        {
            PathNode startNode = new PathNode(startPoint);
            nodeMap[startPoint] = startNode;
            allNodes.Add(startNode);
        }

        if (endPoint != null)
        {
            PathNode endNode = new PathNode(endPoint);
            nodeMap[endPoint] = endNode;
            allNodes.Add(endNode);
        }

        foreach (Transform waypoint in availableWaypoints)
        {
            if (waypoint != null)
            {
                PathNode node = new PathNode(waypoint);
                nodeMap[waypoint] = node;
                allNodes.Add(node);
            }
        }

        UpdateAllConnections();
    }

    void UpdateAllConnections()
    {
        foreach (PathNode node in allNodes)
        {
            node.connections.Clear();

            if (!node.isActive && node.transform != startPoint && node.transform != endPoint) continue;

            foreach (PathNode otherNode in allNodes)
            {
                if (otherNode == node || (!otherNode.isActive && otherNode.transform != endPoint)) continue;

                float distance = Vector2.Distance(node.position, otherNode.position);

                if (distance <= GetMaxConnectionDistance() && CanConnect(node, otherNode))
                {
                    node.connections.Add(otherNode);
                }
            }
        }
    }

    float GetMaxConnectionDistance()
    {
        return 5f;
    }

    bool CanConnect(PathNode from, PathNode to)
    {
        return true;
    }

    public void SetWaypointActive(Transform waypoint, bool active)
    {
        if (nodeMap.ContainsKey(waypoint))
        {
            nodeMap[waypoint].isActive = active;
            UpdateAllConnections();
            OnPathChanged?.Invoke();
        }
    }

    public void AddWaypoint(Transform waypoint)
    {
        if (!nodeMap.ContainsKey(waypoint))
        {
            PathNode newNode = new PathNode(waypoint);
            nodeMap[waypoint] = newNode;
            allNodes.Add(newNode);
            availableWaypoints.Add(waypoint);
            UpdateAllConnections();
            OnPathChanged?.Invoke();
        }
    }

    public void RemoveWaypoint(Transform waypoint)
    {
        if (nodeMap.ContainsKey(waypoint) && waypoint != startPoint && waypoint != endPoint)
        {
            PathNode nodeToRemove = nodeMap[waypoint];
            allNodes.Remove(nodeToRemove);
            nodeMap.Remove(waypoint);
            availableWaypoints.Remove(waypoint);
            UpdateAllConnections();
            OnPathChanged?.Invoke();
        }
    }

    public List<Transform> FindPath()
    {
        if (startPoint == null || endPoint == null)
        {
            Debug.Log("Start or End point not set");
            return new List<Transform>();
        }

        PathNode startNode = nodeMap[startPoint];
        PathNode endNode = nodeMap[endPoint];

        return AStar(startNode, endNode);
    }

    List<Transform> AStar(PathNode start, PathNode goal)
    {
        Dictionary<PathNode, float> gScore = new Dictionary<PathNode, float>();
        Dictionary<PathNode, float> fScore = new Dictionary<PathNode, float>();
        Dictionary<PathNode, PathNode> cameFrom = new Dictionary<PathNode, PathNode>();

        List<PathNode> openSet = new List<PathNode>();
        HashSet<PathNode> closedSet = new HashSet<PathNode>();

        gScore[start] = 0;
        fScore[start] = Heuristic(start, goal);
        openSet.Add(start);

        while (openSet.Count > 0)
        {
            PathNode current = openSet.OrderBy(node => fScore.ContainsKey(node) ? fScore[node] : float.MaxValue).First();

            if (current == goal)
            {
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (PathNode neighbor in current.connections)
            {
                if (closedSet.Contains(neighbor)) continue;

                float tentativeGScore = gScore[current] + Vector2.Distance(current.position, neighbor.position);

                if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                else if (tentativeGScore >= (gScore.ContainsKey(neighbor) ? gScore[neighbor] : float.MaxValue)) continue;

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, goal);
            }
        }

        Debug.Log("No path found");
        return new List<Transform>();
    }

    float Heuristic(PathNode a, PathNode b)
    {
        return Vector2.Distance(a.position, b.position);
    }

    List<Transform> ReconstructPath(Dictionary<PathNode, PathNode> cameFrom, PathNode current)
    {
        List<Transform> path = new List<Transform>();
        path.Add(current.transform);

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current.transform);
        }
        return path;
    }
}
