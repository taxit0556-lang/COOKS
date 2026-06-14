using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FridgeInventory : MonoBehaviour
{
    [System.Serializable]
    public class FridgeSlot
    {
        public GameObject ingredientPrefab;
        public int quantity;
    }

    [SerializeField] private List<FridgeSlot> slots = new List<FridgeSlot>();
    [SerializeField] private Transform slotContainer;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private IngredientPickup playerPickup;
    [SerializeField] private Button closeButton;
    [SerializeField] private Camera previewCamera;

    private void OnEnable()
    {
        RefreshUI();

        if (closeButton != null)
            closeButton.onClick.AddListener(OnClosePressed);
    }

    private void OnDisable()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnClosePressed);
    }

    void OnClosePressed()
    {
        FridgeController.Instance.CloseFridge();
    }

    public void RefreshUI()
    {
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        foreach (FridgeSlot slot in slots)
        {
            if (slot.ingredientPrefab == null || slot.quantity <= 0) continue;

            Ingredient ingredientRef = slot.ingredientPrefab.GetComponent<Ingredient>();
            if (ingredientRef == null) continue;

            GameObject go = Instantiate(slotPrefab, slotContainer);

            RawImage preview = go.transform.Find("Preview")?.GetComponent<RawImage>();
            TextMeshProUGUI label = go.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI qty = go.transform.Find("Quantity")?.GetComponent<TextMeshProUGUI>();
            Button btn = go.GetComponent<Button>();

            if (preview != null)
                preview.texture = GeneratePreview(slot.ingredientPrefab);

            if (label != null)
                label.text = ingredientRef.data.ingredientName;

            if (qty != null)
                qty.text = $"x{slot.quantity}";

            FridgeSlot captured = slot;

            if (btn != null)
                btn.onClick.AddListener(() => TakeIngredient(captured));
        }
    }

    RenderTexture GeneratePreview(GameObject prefab)
    {
        RenderTexture rt = new RenderTexture(128, 128, 16);

        GameObject previewObj = Instantiate(prefab, new Vector3(9999, 9999, 9999), Quaternion.identity);
        previewObj.SetActive(true);

        Renderer[] renderers = previewObj.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
            r.enabled = true;

        if (previewCamera == null)
        {
            GameObject camObj = new GameObject("PreviewCam_Temp");
            previewCamera = camObj.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0, 0, 0, 0);
            previewCamera.cullingMask = ~0;
        }

        Bounds bounds = new Bounds(previewObj.transform.position, Vector3.zero);
        foreach (Renderer r in renderers)
            bounds.Encapsulate(r.bounds);

        previewCamera.transform.position = bounds.center + new Vector3(0, bounds.size.y * 0.5f, -bounds.size.magnitude * 1.5f);
        previewCamera.transform.LookAt(bounds.center);
        previewCamera.targetTexture = rt;
        previewCamera.Render();
        previewCamera.targetTexture = null;

        Destroy(previewObj);

        return rt;
    }

    void TakeIngredient(FridgeSlot slot)
    {
        if (slot.quantity <= 0) return;

        if (playerPickup.Inventory.Count >= 4)
        {
            Debug.Log("Inventory full.");
            return;
        }

        slot.quantity--;

        GameObject instance = Instantiate(slot.ingredientPrefab);
        instance.SetActive(false);

        Ingredient ingredient = instance.GetComponent<Ingredient>();

        playerPickup.GiveIngredient(ingredient);

        Debug.Log($"Took {ingredient.data.ingredientName} from fridge.");

        RefreshUI();
    }

    public void RestockSlot(string ingredientName, int amount = 1)
    {
        FridgeSlot slot = slots.Find(s =>
        {
            Ingredient i = s.ingredientPrefab?.GetComponent<Ingredient>();
            return i != null && i.data.ingredientName == ingredientName;
        });

        if (slot != null)
        {
            slot.quantity += amount;
            RefreshUI();
        }
    }
}