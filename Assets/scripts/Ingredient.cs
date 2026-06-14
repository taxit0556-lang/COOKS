using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Cooking/Ingredient Data")]
public class IngredientData : ScriptableObject
{
    public string ingredientName;
}

public class Ingredient : MonoBehaviour
{
    public IngredientData data;

    private bool isPickedUp = false;

    public bool IsPickedUp => isPickedUp;

    public void PickUp()
    {
        isPickedUp = true;
        gameObject.SetActive(false);
    }

    public void Drop(Vector3 position)
    {
        isPickedUp = false;
        transform.position = position;
        gameObject.SetActive(true);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
    }
}