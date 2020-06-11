using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class WaypointManagerWindow : EditorWindow

{
  [MenuItem("Tool/Waypoint Editor")]
  public static void Open()
    {
        GetWindow < WaypointManagerWindow>();
    }

    public Transform waypointRoot;
    private void OnGUI()
    {
        SerializedObject obj = new SerializedObject(this);

        EditorGUILayout.PropertyField(obj.FindProperty("waypointRoot"));

        if(waypointRoot == null)
        {
            EditorGUILayout.HelpBox("Root transform must be selected. Please assign root transform", MessageType.Warning);

        }

        else
        {
            EditorGUILayout.BeginVertical("box");
            DrawButtons();
            EditorGUILayout.EndVertical();
        }

        obj.ApplyModifiedProperties();
    }


    void DrawButtons()
    {
        if(GUILayout.Button("Create Waypoiny"))
        {
            CreateWaypoint();
        }

        if(Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<WayPoint>())
        {
            if(GUILayout.Button("Create Waypoint Before"))
            {
                CreateWayPointBefore();
            }

            if(GUILayout.Button("Create Waypoint After"))
            {
                CreateWayPointAfter();
            }

            if(GUILayout.Button("Remove Waypoint"))
            {
                RemoveWaypoint();
            }
        }
    }

    void CreateWaypoint()
    {
        GameObject waypointObject = new GameObject("Waypoint " + waypointRoot.childCount, typeof(WayPoint));
        waypointObject.transform.SetParent(waypointRoot, false);

        WayPoint waypoint = waypointObject.GetComponent<WayPoint>();
        if(waypointRoot.childCount > 1)
        {
            waypoint.previousWaypoint = waypointRoot.GetChild(waypointRoot.childCount - 2).GetComponent<WayPoint>();
            waypoint.previousWaypoint.nextWaypoint = waypoint;

            waypoint.transform.position = waypoint.previousWaypoint.transform.position;
            waypoint.transform.forward = waypoint.previousWaypoint.transform.forward;


        }

        Selection.activeGameObject = waypoint.gameObject;
    }

    void CreateWayPointBefore()
    {
        GameObject wayPointObject = new GameObject("Waypoint " + waypointRoot.childCount, typeof(WayPoint));
        wayPointObject.transform.SetParent(waypointRoot, false);

        WayPoint newWayPoint = wayPointObject.GetComponent<WayPoint>();
        WayPoint selectedWayPoint = Selection.activeGameObject.GetComponent<WayPoint>();

        wayPointObject.transform.position = selectedWayPoint.transform.position;
        wayPointObject.transform.position = selectedWayPoint.transform.position;
        wayPointObject.transform.forward = selectedWayPoint.transform.forward;

        if(selectedWayPoint.previousWaypoint != null)
        {
            newWayPoint.previousWaypoint = selectedWayPoint.previousWaypoint;
            selectedWayPoint.previousWaypoint.nextWaypoint = newWayPoint;
        }

        newWayPoint.nextWaypoint = selectedWayPoint;
        selectedWayPoint.previousWaypoint = newWayPoint;
        newWayPoint.transform.SetSiblingIndex(selectedWayPoint.transform.GetSiblingIndex());

        Selection.activeGameObject = newWayPoint.gameObject;
    }


    void CreateWayPointAfter()
    {
        GameObject wayPointObject = new GameObject("Waypoint " + waypointRoot.childCount, typeof(WayPoint));
        wayPointObject.transform.SetParent(waypointRoot, false);

        WayPoint newWayPoint = wayPointObject.GetComponent<WayPoint>();
        WayPoint selectedWayPoint = Selection.activeGameObject.GetComponent<WayPoint>();

        wayPointObject.transform.position = selectedWayPoint.transform.position;
        wayPointObject.transform.position = selectedWayPoint.transform.position;
        wayPointObject.transform.forward = selectedWayPoint.transform.forward;

        newWayPoint.previousWaypoint = selectedWayPoint;

        if(selectedWayPoint.nextWaypoint != null)
        {
            selectedWayPoint.nextWaypoint.previousWaypoint = newWayPoint;
            newWayPoint.nextWaypoint = selectedWayPoint.nextWaypoint;
        
        }

        selectedWayPoint.nextWaypoint = newWayPoint;
        newWayPoint.transform.transform.SetSiblingIndex(selectedWayPoint.transform.GetSiblingIndex());
        Selection.activeGameObject = newWayPoint.gameObject;

    }


    void RemoveWaypoint()
    {
        WayPoint selectedWaypoint = Selection.activeGameObject.GetComponent<WayPoint>();

        if(selectedWaypoint.nextWaypoint != null)
        {
            selectedWaypoint.nextWaypoint.previousWaypoint = selectedWaypoint.previousWaypoint;
        }


        if (selectedWaypoint.previousWaypoint != null)
        {
            selectedWaypoint.previousWaypoint.nextWaypoint = selectedWaypoint.nextWaypoint;
        }

        DestroyImmediate(selectedWaypoint.gameObject);
    }
}
