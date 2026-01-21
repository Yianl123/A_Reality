using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CreatureCommandPanel : MonoBehaviour
{
    [SerializeField] private Button walkButton;
    [SerializeField] private Button eatButton;
    [SerializeField] private Button sleepButton;
    [SerializeField] private Button lookAtMeButton;
    
    private CreatureBehavior selectedCreature;
    private bool waitingForWalkTarget = false;
    
    private void Start()
    {
        walkButton.onClick.AddListener(OnWalkButtonClicked);
        eatButton.onClick.AddListener(OnEatButtonClicked);
        sleepButton.onClick.AddListener(OnSleepButtonClicked);
        lookAtMeButton.onClick.AddListener(OnLookAtMeButtonClicked);
        
        gameObject.SetActive(false); // Hide until creature selected
    }
    
    public void SetSelectedCreature(GameObject creature)
    {
        if (creature == null)
        {
            gameObject.SetActive(false);
            selectedCreature = null;
            return;
        }
        
        selectedCreature = creature.GetComponent<CreatureBehavior>();
        if (selectedCreature == null)
        {
            selectedCreature = creature.AddComponent<CreatureBehavior>();
        }
        
        gameObject.SetActive(true);
    }
    
    private void OnWalkButtonClicked()
    {
        waitingForWalkTarget = true;
        Debug.Log("Tap on the ground where you want the animal to walk");
    }
    
    private void Update()
    {
        if (waitingForWalkTarget && Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                selectedCreature.CommandWalkTo(hit.point);
                waitingForWalkTarget = false;
            }
        }
    }
    
    private void OnEatButtonClicked()
    {
        selectedCreature?.CommandEat();
    }
    
    private void OnSleepButtonClicked()
    {
        selectedCreature?.CommandSleep();
    }
    
    private void OnLookAtMeButtonClicked()
    {
        selectedCreature?.CommandLookAtCamera();
    }
}