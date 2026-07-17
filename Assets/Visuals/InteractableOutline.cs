using CreedBurger.Interaction;
using UnityEngine;
using UnityEngine.Rendering;

namespace CreedBurger.Visuals
{
    public sealed class InteractableOutline : MonoBehaviour, IHighlightable
    {
        [SerializeField] private MeshFilter sourceMeshFilter;
        [SerializeField] private Material outlineMaterial;

        private GameObject outlineObject;

        private void Awake()
        {
            if (sourceMeshFilter == null)
            {
                sourceMeshFilter = GetComponentInChildren<MeshFilter>();
            }

            if (sourceMeshFilter == null || outlineMaterial == null)
            {
                Debug.LogWarning(
                    $"{name}: falta Mesh Filter o material de outline.");
                enabled = false;
                return;
            }

            CreateOutlineObject();
            SetHighlighted(false);
        }

        public void SetHighlighted(bool isHighlighted)
        {
            if (outlineObject != null)
            {
                outlineObject.SetActive(isHighlighted);
            }
        }

        private void CreateOutlineObject()
        {
            outlineObject = new GameObject("__InteractableOutline");
            outlineObject.layer = sourceMeshFilter.gameObject.layer;

            outlineObject.transform.SetParent(
                sourceMeshFilter.transform,
                false);

            MeshFilter outlineMeshFilter =
                outlineObject.AddComponent<MeshFilter>();

            outlineMeshFilter.sharedMesh = sourceMeshFilter.sharedMesh;

            MeshRenderer outlineRenderer =
                outlineObject.AddComponent<MeshRenderer>();

            outlineRenderer.sharedMaterial = outlineMaterial;
            outlineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            outlineRenderer.receiveShadows = false;
            outlineRenderer.lightProbeUsage =
                LightProbeUsage.Off;
            outlineRenderer.reflectionProbeUsage =
                ReflectionProbeUsage.Off;
        }

        private void OnDisable()
        {
            if (outlineObject != null)
            {
                outlineObject.SetActive(false);
            }
        }
    }
}