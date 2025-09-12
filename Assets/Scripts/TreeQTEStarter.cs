using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TreeQTEStarter : MonoBehaviour
{
    public QTEWatering qte;             // assign your QTEWatering
    public TreeGrowth tree;             // the tree for this zone
    public PlayerInteractor interactor; // player with PlayerInteractor
    public string playerTag = "Player";

    bool playerInside;

    void Reset() { GetComponent<Collider>().isTrigger = true; }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag)) playerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInside = false;
            if (qte) qte.StopQTE(false);    // hide QTE when leaving
        }
    }

    void Update()
    {
        if (!playerInside || qte == null || interactor == null) return;

        var held = interactor.CarriedItem;  // from PlayerInteractor getter
        if (held != null && held.itemKind == ItemKind.WateringCan)
        {
            if (Input.GetMouseButtonDown(0))   // LMB to initiate watering
            {
                qte.StartQTE(tree);
            }
        }
    }
}
