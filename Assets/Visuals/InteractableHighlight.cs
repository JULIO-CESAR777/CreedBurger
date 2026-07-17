using CreedBurger.Interaction;
using UnityEngine;

namespace CreedBurger.Visuals
{
    public sealed class InteractableHighlight : MonoBehaviour, IHighlightable
    {
        [SerializeField] private Renderer[] targetRenderers;
        [SerializeField] private Color highlightColor = new Color(1f, 0.7f, 0f, 1f);
        [SerializeField, Min(0f)] private float highlightIntensity = 2.5f;

        private static readonly int HighlightActiveId =
            Shader.PropertyToID("_HighlightActive");

        private static readonly int HighlightColorId =
            Shader.PropertyToID("_HighlightColor");

        private static readonly int HighlightIntensityId =
            Shader.PropertyToID("_HighlightIntensity");

        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            if (targetRenderers == null || targetRenderers.Length == 0)
            {
                targetRenderers = GetComponentsInChildren<Renderer>(true);
            }

            propertyBlock = new MaterialPropertyBlock();
            SetHighlighted(false);
        }

        public void SetHighlighted(bool isHighlighted)
        {
            foreach (Renderer targetRenderer in targetRenderers)
            {
                if (targetRenderer == null)
                {
                    continue;
                }

                targetRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat(
                    HighlightActiveId,
                    isHighlighted ? 1f : 0f);
                propertyBlock.SetColor(HighlightColorId, highlightColor);
                propertyBlock.SetFloat(
                    HighlightIntensityId,
                    highlightIntensity);

                targetRenderer.SetPropertyBlock(propertyBlock);
            }
        }
    }
}