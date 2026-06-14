using UnityEngine;
using TMPro;

public class FridgePickup : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private IngredientPickup playerPickup;
    [SerializeField] private float clickRange = 3f;
    [SerializeField] private TextMeshProUGUI hoverLabel;

    private FridgeIngredient hoveredIngredient;

    private void Update()
    {
        if (!FridgeController.Instance.IsOpen()) return;

        HandleHover();

        if (Input.GetMouseButtonDown(0) && hoveredIngredient != null)
            TryPickup(hoveredIngredient);
    }

    void HandleHover()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, clickRange))
        {
            FridgeIngredient fi = hit.collider.GetComponentInParent<FridgeIngredient>();

            if (fi != null && fi.Quantity > 0)
            {
                hoveredIngredient = fi;

                if (hoverLabel != null)
                {
                    hoverLabel.gameObject.SetActive(true);
                    Ingredient ing = hit.collider.GetComponentInParent<Ingredient>();
                    string name = ing != null ? ing.data.ingredientName : fi.name;
                    hoverLabel.text = $"{name} (x{fi.Quantity}) — Click to take";
                }

                return;
            }
        }

        hoveredIngredient = null;

        if (hoverLabel != null)
            hoverLabel.gameObject.SetActive(false);
    }

    void TryPickup(FridgeIngredient fridgeIngredient)
    {
        if (playerPickup.Inventory.Count >= 4)
        {
            Debug.Log("Inventory full.");
            return;
        }

        if (fridgeIngredient.TryTake(out GameObject takenObj))
        {
            Ingredient ingredient = takenObj.GetComponent<Ingredient>();

            if (ingredient != null)
            {
                Rigidbody rb = takenObj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }

                ingredient.PickUp();
                playerPickup.GiveIngredient(ingredient);
                Debug.Log($"Picked up {ingredient.data.ingredientName}");
            }
        }
    }
}