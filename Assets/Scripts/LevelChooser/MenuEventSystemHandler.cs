using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;

public class MenuEventSystemHandler : MonoBehaviour
{
   [Header("References")]
   public List<Selectable> Selectables = new List<Selectable>();
   [SerializeField] protected Selectable firstSelected;

   [Header("Controls")]
   [SerializeField] protected InputActionReference navigateReference;

   [Header("Animations")]
   [SerializeField] protected float selectedAnimationScale = 1.1f;
   [SerializeField] protected float scaleDuration = 0.25f;
   [SerializeField] protected List<GameObject> animationExclusions = new List<GameObject>();

   protected Dictionary<Selectable, Vector3> scales = new Dictionary<Selectable, Vector3>();

   protected Selectable lastSelected;

   protected Tween scaleUpTween;
    protected Tween scaleDownTween;

    public virtual void Awake(){
        foreach(var selectable in Selectables){
            AddSelectionListeners(selectable);
            scales.Add(selectable, selectable.transform.localScale);
        }
    }

    public virtual void OnEnable(){
        navigateReference.action.performed += OnNavigate;
        for(int i = 0; i < Selectables.Count; i++){
            Selectables[i].transform.localScale = scales[Selectables[i]];
        }
        StartCoroutine(SelectAfterDelay());
    }

    protected virtual IEnumerator SelectAfterDelay(){
        yield return null;
        EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
    }

    public virtual void OnDisable(){
        navigateReference.action.performed -= OnNavigate;
        if(scaleUpTween != null) scaleUpTween.Kill(true);
        if(scaleDownTween != null) scaleDownTween.Kill(true);
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

   public void OnSelect(BaseEventData eventData){
    //   SoundEvent?.Invoke();
        lastSelected = eventData.selectedObject.GetComponent<Selectable>();

        if(animationExclusions.Contains(eventData.selectedObject)){
            return;
        }

        Vector3 newScale = eventData.selectedObject.transform.localScale * selectedAnimationScale;
        scaleUpTween = eventData.selectedObject.transform.DOScale(newScale, scaleDuration);
   }

   public void OnDeselect(BaseEventData eventData){
    if(animationExclusions.Contains(eventData.selectedObject)){
            return;
        }
        
        Selectable sel = eventData.selectedObject.GetComponent<Selectable>();
        scaleDownTween = eventData.selectedObject.transform.DOScale(scales[sel], scaleDuration);
   }

   public void OnPointerEnter(BaseEventData eventData){
        PointerEventData pointerEventData = eventData as PointerEventData;
        if(pointerEventData != null){
            Selectable sel = pointerEventData.pointerEnter.GetComponentInParent<Selectable>();
            if(sel == null){
                sel = pointerEventData.pointerEnter.GetComponentInChildren<Selectable>();
            }
            pointerEventData.selectedObject = sel.gameObject;
        }
   }

   public void OnPointerExit(BaseEventData eventData){
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
}
