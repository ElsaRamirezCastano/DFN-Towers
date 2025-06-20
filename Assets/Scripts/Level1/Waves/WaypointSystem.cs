using UnityEngine;
using System.Collections.Generic;

public class WaypointSystem : MonoBehaviour{

    public Transform[] waypoints;

    public Transform[] GetWaypoints(){
        return waypoints;
    }

    public Transform GetWaypoint(int index){
        if(index >= 0 && index < waypoints.Length){
            return waypoints[index];
        }
        return null;
    }

    //This will get the last waypoint of the list as the end
    public Transform GetEndPoint(){
        if(waypoints.Length > 0){
            return waypoints[waypoints.Length - 1];
        }
        return null;
    }

     //This will get the first waypoint of the list as the start
    public Transform GetStartPoint(){
        if(waypoints.Length > 0){
            return waypoints[0];
        }
        return null;
    }
}
