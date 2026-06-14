using System.Collections;
using UnityEngine;

public class CookingStation : MonoBehaviour
{
    [SerializeField] private float interactDistance = 2.5f;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject cheesecakePrefab;

    private IngredientPickup playerPickup;
    private bool isCooking = false;

    private enum CookingPhase
    {
        NeedGrahamCrackers,
        CrushingCrackers,
        NeedButter,
        MixingCrust,
        NeedCreamCheese,
        BeatingCheese,
        NeedPowderedSugar,
        NeedVanillaExtract,
        NeedSalt,
        MixingFilling,
        NeedCoolWhip,
        FoldingCoolWhip,
        Refrigerating,
        Done
    }

    private CookingPhase phase = CookingPhase.NeedGrahamCrackers;

    void Start()
    {
        if (playerTransform != null)
            playerPickup = playerTransform.GetComponent<IngredientPickup>();

        Debug.Log("Bring Graham Crackers to start cooking!");
    }

    void Update()
    {
        if (isCooking) return;
        if (phase == CookingPhase.Done) return;

        bool isMinigamePhase =
            phase == CookingPhase.CrushingCrackers ||
            phase == CookingPhase.MixingCrust ||
            phase == CookingPhase.BeatingCheese ||
            phase == CookingPhase.MixingFilling ||
            phase == CookingPhase.FoldingCoolWhip ||
            phase == CookingPhase.Refrigerating;

        if (isMinigamePhase) return;

        float distance = Vector3.Distance(playerTransform.position, transform.position);

        if (distance <= interactDistance && Input.GetKeyDown(KeyCode.E))
            HandlePhase();
    }

    void HandlePhase()
    {
        switch (phase)
        {
            case CookingPhase.NeedGrahamCrackers:
                TryUseIngredient("GrahamCrackers", () =>
                {
                    phase = CookingPhase.CrushingCrackers;
                    isCooking = true;
                    CookingMinigame.Instance.StartMinigame(MinigameType.Crush, () =>
                    {
                        isCooking = false;
                        phase = CookingPhase.NeedButter;
                        Debug.Log("Crackers crushed! Now bring Butter.");
                    });
                });
                break;

            case CookingPhase.NeedButter:
                TryUseIngredient("Butter", () =>
                {
                    phase = CookingPhase.MixingCrust;
                    isCooking = true;
                    CookingMinigame.Instance.StartMinigame(MinigameType.CircularMix, () =>
                    {
                        isCooking = false;
                        phase = CookingPhase.NeedCreamCheese;
                        Debug.Log("Crust mixed and set! Now bring Cream Cheese.");
                    });
                });
                break;

            case CookingPhase.NeedCreamCheese:
                TryUseIngredient("CreamCheese", () =>
                {
                    phase = CookingPhase.BeatingCheese;
                    isCooking = true;
                    CookingMinigame.Instance.StartMinigame(MinigameType.Beat, () =>
                    {
                        isCooking = false;
                        phase = CookingPhase.NeedPowderedSugar;
                        Debug.Log("Cream cheese beaten smooth! Now bring Powdered Sugar.");
                    });
                });
                break;

            case CookingPhase.NeedPowderedSugar:
                TryUseIngredient("PowderedSugar", () =>
                {
                    phase = CookingPhase.NeedVanillaExtract;
                    isCooking = true;
                    CookingMinigame.Instance.StartMinigame(MinigameType.Pour, () =>
                    {
                        isCooking = false;
                        Debug.Log("Powdered sugar added! Now bring Vanilla Extract.");
                    });
                });
                break;

            case CookingPhase.NeedVanillaExtract:
                TryUseIngredient("VanillaExtract", () =>
                {
                    phase = CookingPhase.NeedSalt;
                    isCooking = true;
                    CookingMinigame.Instance.StartMinigame(MinigameType.Pour, () =>
                    {
                        isCooking = false;
                        Debug.Log("Vanilla added! Now bring Salt.");
                    });
                });
                break;

            case CookingPhase.NeedSalt:
                TryUseIngredient("Salt", () =>
                {
                    phase = CookingPhase.MixingFilling;
                    isCooking = true;
                    CookingMinigame.Instance.StartMinigame(MinigameType.Pour, () =>
                    {
                        isCooking = false;
                        CookingMinigame.Instance.StartMinigame(MinigameType.Beat, () =>
                        {
                            isCooking = false;
                            phase = CookingPhase.NeedCoolWhip;
                            Debug.Log("Filling mixed! Now bring Cool Whip.");
                        });
                    });
                });
                break;

            case CookingPhase.NeedCoolWhip:
                TryUseIngredient("CoolWhip", () =>
                {
                    phase = CookingPhase.FoldingCoolWhip;
                    isCooking = true;
                    CookingMinigame.Instance.StartMinigame(MinigameType.Fold, () =>
                    {
                        phase = CookingPhase.Refrigerating;
                        CookingMinigame.Instance.StartMinigame(MinigameType.Wait, () =>
                        {
                            isCooking = false;
                            phase = CookingPhase.Done;

                            if (cheesecakePrefab != null && spawnPoint != null)
                                UnityEngine.Object.Instantiate(cheesecakePrefab, spawnPoint.position, spawnPoint.rotation);

                            Debug.Log("No-Bake Cheesecake is ready!");
                        });
                    });
                });
                break;
        }
    }

    void TryUseIngredient(string ingredientName, System.Action onSuccess)
    {
        if (playerPickup == null || playerPickup.HeldIngredient == null)
        {
            Debug.Log("Not holding anything.");
            return;
        }

        string held = playerPickup.HeldIngredient.data.ingredientName;

        if (held != ingredientName)
        {
            Debug.Log($"Wrong ingredient. Need: {ingredientName}, holding: {held}");
            return;
        }

        Ingredient taken = playerPickup.TakeIngredient(ingredientName);
        Destroy(taken.gameObject);
        onSuccess?.Invoke();
    }

    public bool IsCooking => isCooking;
}