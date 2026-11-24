using System.Collections.Generic;
using UnityEngine;

namespace TheseusCore.Grid
{
    /// <summary>One material requirement of a recipe.</summary>
    [System.Serializable]
    public struct CraftingIngredient
    {
        /// <summary>Material identifier.</summary>
        public string materialId;

        /// <summary>Required amount.</summary>
        public int amount;

        /// <summary>Creates an ingredient.</summary>
        public CraftingIngredient(string id, int amount)
        {
            materialId = id;
            this.amount = amount;
        }
    }

    /// <summary>
    /// Data-only recipe asset. Contains no art, localization text or scene references.
    /// </summary>
    [CreateAssetMenu(menuName = "Theseus Core/Crafting Recipe", fileName = "CraftingRecipe")]
    public sealed class CraftingRecipe : ScriptableObject, ICraftingRecipe
    {
        [SerializeField] private PlaceableObject _resultPrefab;
        [SerializeField] private int _resultCount = 1;
        [SerializeField] private List<CraftingIngredient> _requiredIngredients = new List<CraftingIngredient>();

        /// <summary>Placeable produced by this recipe.</summary>
        public PlaceableObject ResultPrefab => _resultPrefab;

        /// <summary>Number produced per craft.</summary>
        public int ResultCount => Mathf.Max(1, _resultCount);

        /// <summary>Required materials.</summary>
        public IReadOnlyList<CraftingIngredient> RequiredIngredients => _requiredIngredients;
    }
}
