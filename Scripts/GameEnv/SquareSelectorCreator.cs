using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareSelectorCreator : MonoBehaviour
{
    [SerializeField] private Material freeSquareMaterial;
    [SerializeField] private Material enemySquareMaterial;
    [SerializeField] private Material hoverSquareMaterial;
    [SerializeField] private GameObject selectorPrefab;
    private List<GameObject> instantiatedSelectors = new List<GameObject>();

    /// <summary>
    /// Show square selector based on passed squareData
    /// </summary>
    /// <param name="squareData"></param>
    public void ShowSelection(Dictionary<Vector3, bool> squareData)
    {
        ClearSelection();
        foreach (var data in squareData)
        {
            GameObject selector = Instantiate(selectorPrefab, data.Key, Quaternion.identity);
            instantiatedSelectors.Add(selector);

            foreach (var setter in selector.GetComponentsInChildren<MaterialSetter>())
            {
                setter.SetSingleMaterial(data.Value ? freeSquareMaterial : enemySquareMaterial);
            }
        }
    }

    /// <summary>
    /// Clear all selectors
    /// </summary>
    public void ClearSelection()
    {
        for (int i = 0; i < instantiatedSelectors.Count; i++)
        {
            Destroy(instantiatedSelectors[i]);
        }
    }

    /// <summary>
    /// Generated a selector based on mouse hoverPos
    /// </summary>
    /// <param name="hoverPos"></param>
    public void HoverSelector(Vector3 hoverPos)
    {
        hoverPos = new Vector3(Mathf.FloorToInt(hoverPos.x) + 0.2f, 0, Mathf.FloorToInt(hoverPos.z) + .5f);
        GameObject selector = Instantiate(selectorPrefab, hoverPos, Quaternion.identity);
        instantiatedSelectors.Add(selector);

        foreach (var setter in selector.GetComponentsInChildren<MaterialSetter>())
        {
            setter.SetSingleMaterial(hoverSquareMaterial);
        }


    }
}