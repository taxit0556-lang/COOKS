using System.Collections.Generic;
using UnityEngine;

public class IngredientPickup : MonoBehaviour
{
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float pickupRange = 2.5f;
    [SerializeField] private Transform holdPoint;

    private Ingredient heldIngredient;

    public Ingredient HeldIngredient => heldIngredient;

    public IReadOnlyList<Ingredient> Inventory => heldIngredient != null
        ? new List<Ingredient> { heldIngredient }
        : new List<Ingredient>();

    void Update()
    {
        if (FridgeController.Instance != null && FridgeController.Instance.IsOpen()) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (heldIngredient != null)
                Drop();
            else
                TryPickup();
        }

        if (heldIngredient != null)
            HoldItem();
    }

    void TryPickup()
    {
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            Ingredient ingredient = hit.collider.GetComponent<Ingredient>();

            if (ingredient != null && !ingredient.IsPickedUp)
            {
                Rigidbody rb = ingredient.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                }

                ingredient.PickUp();
                heldIngredient = ingredient;
                heldIngredient.gameObject.SetActive(true);
                Debug.Log($"Picked up: {ingredient.data.ingredientName}");
            }
        }
    }

    void HoldItem()
    {
        heldIngredient.transform.position = holdPoint.position;
        heldIngredient.transform.rotation = holdPoint.rotation;
    }

    void Drop()
    {
        if (heldIngredient == null) return;

        Rigidbody rb = heldIngredient.GetComponent<Rigidbody>();
        if (rb == null) rb = heldIngredient.gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;

        heldIngredient.transform.SetParent(null);
        heldIngredient.gameObject.SetActive(true);

        Vector3 dropPos = playerCamera.position + playerCamera.forward * 1.2f;
        heldIngredient.transform.position = dropPos;

        heldIngredient.gameObject.GetComponent<Ingredient>().Drop(dropPos);
        heldIngredient = null;
    }

    public bool HasIngredient(string ingredientName)
    {
        return heldIngredient != null && heldIngredient.data.ingredientName == ingredientName;
    }

    public Ingredient TakeIngredient(string ingredientName)
    {
        if (heldIngredient != null && heldIngredient.data.ingredientName == ingredientName)
        {
            Ingredient taken = heldIngredient;
            heldIngredient = null;
            return taken;
        }
        return null;
    }

    public List<Ingredient> TakeAll()
    {
        List<Ingredient> taken = new List<Ingredient>();
        if (heldIngredient != null)
        {
            taken.Add(heldIngredient);
            heldIngredient = null;
        }
        return taken;
    }

    public void GiveIngredient(Ingredient ingredient)
    {
        if (heldIngredient != null) return;
        heldIngredient = ingredient;
        heldIngredient.gameObject.SetActive(true);
    }
}