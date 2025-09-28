using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderUIController : MonoBehaviour
{
    public CanvasGroup group;
    public Image recipeImage;
    public TMP_Text titleText;

    public void ShowOrder(IngredientPrefabDB.Entry entry)
    {
        if (recipeImage) recipeImage.sprite = entry.image;
        if (titleText) titleText.text = string.IsNullOrEmpty(entry.title) ? $"Receta {entry.id}" : entry.title;

        group.alpha = 1f;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    public void Hide()
    {
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }
}
