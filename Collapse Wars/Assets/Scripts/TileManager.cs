using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    private static readonly List<TileManager> selectedTiles = new();
    private SpriteRenderer spriteRenderer;
    private Color unselectedColor = Color.white;
    private Color selectedColor = Color.gray;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetUnselected();
    }

    void OnMouseDown()
    {
        if (PauseManager.isPaused || GameOverManager.isGameOver) return;

        Vector3 position = transform.position;

        ToggleSelection();
        
        if (selectedTiles.Count == 2)
        {
            TrySwapTiles();
            DeselectAllTiles();
        }
    }

    private void ToggleSelection()
    {
        if (selectedTiles.Contains(this))
        {
            SetUnselected();
            selectedTiles.Remove(this);
        }
        else
        {
            SetSelected();
            selectedTiles.Add(this);
        }
    }

    private void SetSelected()
    {
        spriteRenderer.color = selectedColor;
    }

    private void SetUnselected()
    {
        spriteRenderer.color = unselectedColor;
    }

    private void TrySwapTiles()
    {
        FindObjectOfType<GridManager>().ResetHintTimer();
        if (AreNeighbors(selectedTiles[0], selectedTiles[1]))
        {
            Sprite tempSprite = selectedTiles[0].spriteRenderer.sprite;
            selectedTiles[0].spriteRenderer.sprite = selectedTiles[1].spriteRenderer.sprite;
            selectedTiles[1].spriteRenderer.sprite = tempSprite;

            bool isMatch = FindObjectOfType<GridManager>().CheckForMatchesAfterSwap();

            if (!isMatch)
            {
                selectedTiles[1].spriteRenderer.sprite = selectedTiles[0].spriteRenderer.sprite;
                selectedTiles[0].spriteRenderer.sprite = tempSprite;
            }
        }
    }

    private bool AreNeighbors(TileManager tile1, TileManager tile2)
    {
        float distance = Vector2.Distance(tile1.transform.position, tile2.transform.position);
        if (Mathf.Approximately(distance, 1f))
        {
            int row1 = Mathf.RoundToInt(tile1.transform.position.x);
            int row2 = Mathf.RoundToInt(tile2.transform.position.x);

            // Blokowanie wymiany między sekcjami (wiersze 7 i 8)
            if ((row1 <= 7 && row2 >= 8) || (row1 >= 8 && row2 <= 7))
            {
                return false;
            }

            return true;
        }
        return false;
    }

    private void DeselectAllTiles()
    {
        foreach (TileManager tile in selectedTiles)
        {
            tile.SetUnselected();
        }
        selectedTiles.Clear();
    }

}
