using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemCleaner : MonoBehaviour{
    void OnEnable()
    {
        var existingEventSystems = FindFirstObjectByType<EventSystem>();
        if (existingEventSystems)
        {
            Destroy(gameObject);
        }
    }
}
