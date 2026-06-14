using UnityEngine;

public class FridgeIngredient : MonoBehaviour
{
    [SerializeField] private IngredientData data;
    [SerializeField] private int quantity = 3;
    [SerializeField] private Transform[] spawnSlots;

    private GameObject[] spawnedItems;

    private void Start()
    {
        spawnedItems = new GameObject[spawnSlots.Length];
        RefreshSlots();
        Destroy(gameObject);
    }

    void RefreshSlots()
    {
        for (int i = 0; i < spawnSlots.Length; i++)
        {
            if (spawnedItems[i] != null)
                Destroy(spawnedItems[i]);

            if (i < quantity)
            {
                GameObject instance = Instantiate(gameObject, spawnSlots[i].position, spawnSlots[i].rotation, spawnSlots[i]);

                FridgeIngredient fi = instance.GetComponent<FridgeIngredient>();
                if (fi != null) Destroy(fi);

                Rigidbody rb = instance.GetComponent<Rigidbody>();
                if (rb == null) rb = instance.AddComponent<Rigidbody>();
                rb.isKinematic = true;
                rb.useGravity = false;

                Ingredient ing = instance.GetComponent<Ingredient>();
                if (ing == null) ing = instance.AddComponent<Ingredient>();
                ing.data = data;

                spawnedItems[i] = instance;
            }
        }
    }

    public bool TryTake(out GameObject takenObject)
    {
        takenObject = null;

        if (quantity <= 0)
            return false;

        quantity--;

        for (int i = spawnedItems.Length - 1; i >= 0; i--)
        {
            if (spawnedItems[i] != null)
            {
                takenObject = spawnedItems[i];
                spawnedItems[i] = null;
                takenObject.transform.SetParent(null);
                return true;
            }
        }

        return false;
    }

    public int Quantity => quantity;
}