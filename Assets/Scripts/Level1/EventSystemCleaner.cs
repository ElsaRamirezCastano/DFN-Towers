using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemCleaner : MonoBehaviour{
    void OnEnable()
    {
        var existingEventSystems = FindObjectsOfType<EventSystem>();
        if (existingEventSystems.Length > 1)
        {
            Destroy(gameObject);
        }
    }
}
