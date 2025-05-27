using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;

public class DynamicEventSystemHandler : MonoBehaviour{

   [Header("References")]
   public List<Selectable> Selectables = new List<Selectable>();

   [Header("Controls")]
   [SerializeField] protected InputActionReference navigateReference;

   [Header("Animations")]
   [SerializeField] protected float selectedAnimationScale = 1.1f;
   [SerializeField] protected float scaleDuration = 0.25f;

   /*[Header("Sounds")]
    [SerializeField] protected UnityEvent SoundEvent;
   */

   protected Dictionary<Selectable, Vector3> scales = new Dictionary<Selectable, Vector3>();

   protected Selectable lastSelected;

   protected Tween scaleUpTween;
    protected Tween scaleDownTween;

    public virtual void OnEnable(){
        navigateReference.action.performed += OnNavigate;
    }

    protected virtual IEnumerator SelectAfterDelay(){
        yield return null;
        EventSystem.current.SetSelectedGameObject(Selectables[0].gameObject);
    }

    public virtual void OnDisable(){
        navigateReference.action.performed -= OnNavigate;
        scaleUpTween.Kill(true);
        scaleDownTween.Kill(true);
    }

   protected virtual void AddSelectionListeners(Selectable selectable){
        EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();

        if(trigger == null){
            trigger = selectable.gameObject.AddComponent<EventTrigger>();
        }


        EventTrigger.Entry SelectedEntry = new EventTrigger.Entry{
            eventID = EventTriggerType.Select
        };
        SelectedEntry.callback.AddListener(OnSelect);
        trigger.triggers.Add(SelectedEntry);


        EventTrigger.Entry DeselectEntry = new EventTrigger.Entry{
            eventID = EventTriggerType.Deselect
        };
        DeselectEntry.callback.AddListener(OnDeselect);
        trigger.triggers.Add(DeselectEntry);


        EventTrigger.Entry PointerEnter = new EventTrigger.Entry{
            eventID = EventTriggerType.PointerEnter
        };
        PointerEnter.callback.AddListener(OnPointerEnter);
        trigger.triggers.Add(PointerEnter);


        EventTrigger.Entry PointerExit = new EventTrigger.Entry{
            eventID = EventTriggerType.PointerExit
        };
        PointerExit.callback.AddListener(OnPointerExit);
        trigger.triggers.Add(PointerExit);
   }

   public virtual void OnSelect(BaseEventData eventData){
    //   SoundEvent?.Invoke();
        lastSelected = eventData.selectedObject.GetComponent<Selectable>();

        Vector3 newScale = eventData.selectedObject.transform.localScale * selectedAnimationScale;
        scaleUpTween = eventData.selectedObject.transform.DOScale(newScale, scaleDuration);
   }

   public virtual void OnDeselect(BaseEventData eventData){        
        Selectable sel = eventData.selectedObject.GetComponent<Selectable>();
        scaleDownTween = eventData.selectedObject.transform.DOScale(scales[sel], scaleDuration);
   }

   public virtual void OnPointerEnter(BaseEventData eventData){
        PointerEventData pointerEventData = eventData as PointerEventData;
        if(pointerEventData != null){
            Selectable sel = pointerEventData.pointerEnter.GetComponentInParent<Selectable>();
            if(sel == null){
                sel = pointerEventData.pointerEnter.GetComponentInChildren<Selectable>();
            }
            pointerEventData.selectedObject = sel.gameObject;
        }
   }

   public virtual void OnPointerExit(BaseEventData eventData){
        PointerEventData pointerEventData = eventData as PointerEventData;
        if(pointerEventData != null){
            pointerEventData.selectedObject = null;
        }
   }

   protected virtual void OnNavigate(InputAction.CallbackContext context){
        if(EventSystem.current.currentSelectedGameObject == null && lastSelected != null){
            EventSystem.current.SetSelectedGameObject(lastSelected.gameObject);
        }
   }

#region Helper methods
   public void AddSelectable(Selectable selectable){
        Selectables.Add(selectable);
   }

   public void InitSelectables(){
        foreach(var selectable in Selectables){
            AddSelectionListeners(selectable);
            scales.TryAdd(selectable, selectable.transform.localScale);
        }
   }

   public void SetFirstSelected(){
        StartCoroutine(SelectAfterDelay());
   }
#endregion
}
