using UnityEngine;
using UnityEngine.UI;
public class Star : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Image YellowStar;

    private void Awake()
    {
        YellowStar = GetComponent<Image>();
        YellowStar.transform.localScale= Vector3.zero;
        
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
