using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GridManager : MonoBehaviour
{
    [Header("Grid Configuration")]
    public int rows = 16;
    public int columns = 8;
    public GameObject tilePrefab;
    public List<Sprite> tileSprites;
    public int minNewTiles;

    [Header("Dependencies")]
    private GameObject[,] gridArray;
    public ScoreManager scoreManager;
    private int destroyedTileCount = 0;
    private const float CheckInterval = 0.5f;
    private const int RightBoardOffsetZ = -14;
    private float hintTimer = 5f;
    private float timeSinceLastMove = 0f;
    private List<Vector2Int> hintPositions = new();

    [Header("UI Elements")]
    public GameObject ShuffleText;

    [Header("Audio")]
    public AudioClip destroyTileSound;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        ShuffleText.SetActive(false);
        gridArray = new GameObject[rows, columns];
        GenerateGrid();
        InvokeRepeating(nameof(CheckAndRefresh), CheckInterval, CheckInterval);
    }


    private void Update()
    {
        timeSinceLastMove += Time.deltaTime;

        if (timeSinceLastMove >= hintTimer && hintPositions.Count == 0)
        {
            hintPositions = FindFirstAvailableMove();
            if (hintPositions.Count > 0)
            {
                HighlightHintTiles(hintPositions);
            }
        }
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns / 2; y++)
            {
                Vector3 position = new(x, y, x < 8 ? 0 : RightBoardOffsetZ);
                GameObject newTile = Instantiate(tilePrefab, position, Quaternion.identity, transform);
                SpriteRenderer renderer = newTile.GetComponent<SpriteRenderer>();
                renderer.sprite = GetValidSprite(x, y);
                gridArray[x, y] = newTile;
            }
        }
    }

    private Sprite GetValidSprite(int x, int y)
    {
        List<Sprite> possibleSprites = new(tileSprites);

        void RemoveIfMatch(int x1, int y1, int x2, int y2)
        {
            if (x1 >= 0 && x2 >= 0 && x2 < rows && y1 >= 0 && y2 >= 0 && y2 < columns &&
                gridArray[x1, y1]?.GetComponent<SpriteRenderer>().sprite == gridArray[x2, y2]?.GetComponent<SpriteRenderer>().sprite &&
                AreTilesInSameSection(x1, x2))
            {
                possibleSprites.Remove(gridArray[x1, y1].GetComponent<SpriteRenderer>().sprite);
            }
        }

        RemoveIfMatch(x - 1, y, x - 2, y); // Check horizontal match
        RemoveIfMatch(x, y - 1, x, y - 2); // Check vertical match

        return possibleSprites[Random.Range(0, possibleSprites.Count)];
    }

    private void CheckAndRefresh()
    {
        if (CheckMatches())
        {
            CollapseGrid();
        }
        else if (!AreMovesAvailable())
        {
            Debug.Log("No moves available. Shuffling the board...");
            ShuffleBoard();
        }
        CheckGameOver();
    }

    private bool AreMovesAvailable()
    {
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                if (gridArray[x, y] == null) continue;

                if (IsPotentialMatch(x, y, x + 1, y) || IsPotentialMatch(x, y, x, y + 1))
                    return true;
            }
        }
        return false;
    }

    private bool IsPotentialMatch(int x1, int y1, int x2, int y2)
    {
        if (!IsInBounds(x2, y2) || gridArray[x2, y2] == null) return false;

        // Sprawdzenie czy oba wiersze należą do tej samej sekcji
        if (!AreTilesInSameSection(x1, x2)) return false;

        SwapTiles(x1, y1, x2, y2);
        bool match = HasMatchAt(x1, y1) || HasMatchAt(x2, y2);
        SwapTiles(x1, y1, x2, y2); // Cofnij zamianę
        return match;
    }

    private void SwapTiles(int x1, int y1, int x2, int y2)
    {
        var temp = gridArray[x1, y1]?.GetComponent<SpriteRenderer>().sprite;
        gridArray[x1, y1].GetComponent<SpriteRenderer>().sprite = gridArray[x2, y2]?.GetComponent<SpriteRenderer>().sprite;
        gridArray[x2, y2].GetComponent<SpriteRenderer>().sprite = temp;
    }

    private bool HasMatchAt(int x, int y)
    {
        if (!IsInBounds(x, y)) return false;
        Sprite currentSprite = gridArray[x, y]?.GetComponent<SpriteRenderer>().sprite;

        return IsLineMatch(x, y, -1, 0) || IsLineMatch(x, y, 0, -1);
    }

    private bool IsLineMatch(int x, int y, int xOffset, int yOffset)
    {
        Sprite currentSprite = gridArray[x, y]?.GetComponent<SpriteRenderer>().sprite;

        int x1 = x + xOffset, y1 = y + yOffset;
        int x2 = x + 2 * xOffset, y2 = y + 2 * yOffset;
  
        if (!IsInBounds(x1, y1) || !IsInBounds(x2, y2) || 
            !AreTilesInSameSection(x, x1) || !AreTilesInSameSection(x, x2)) 
            return false;

        return gridArray[x1, y1]?.GetComponent<SpriteRenderer>().sprite == currentSprite &&
            gridArray[x2, y2]?.GetComponent<SpriteRenderer>().sprite == currentSprite;
    }

    private bool IsInBounds(int x, int y) => x >= 0 && y >= 0 && x < rows && y < columns;

    private void ShuffleBoard()
    {
        const int MaxShuffleAttempts = 100;
        int attempts = 0;

        if (ShuffleText != null)
        {
            ShuffleText.SetActive(true); // Włącz tekst przetasowania
        }

        while (attempts++ < MaxShuffleAttempts)
        {
            List<Sprite> allSprites = CollectAllSprites();
            ShuffleList(allSprites);
            ApplySpritesToGrid(allSprites);

            if (AreMovesAvailable())
            {
                if (ShuffleText != null)
                {
                    StartCoroutine(HideShuffleTextAfterDelay(1.5f)); // Ukryj tekst po opóźnieniu
                }
                return;
            }
        }

        Debug.LogError("Failed to shuffle board with valid moves.");
    }

    private IEnumerator HideShuffleTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ShuffleText != null)
        {
            ShuffleText.SetActive(false);
        }
    }
        
    private List<Sprite> CollectAllSprites()
    {
        List<Sprite> sprites = new();
        foreach (var tile in gridArray)
        {
            if (tile) sprites.Add(tile.GetComponent<SpriteRenderer>().sprite);
        }
        return sprites;
    }

    private void ShuffleList(List<Sprite> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(0, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    private void ApplySpritesToGrid(List<Sprite> sprites)
    {
        int index = 0;

        // Przypisz kafelki do lewej sekcji
        for (int x = 0; x <= 7; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                if (gridArray[x, y] != null)
                {
                    gridArray[x, y].GetComponent<SpriteRenderer>().sprite = sprites[index++];
                }
            }
        }

        // Przypisz kafelki do prawej sekcji
        for (int x = 8; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                if (gridArray[x, y] != null)
                {
                    gridArray[x, y].GetComponent<SpriteRenderer>().sprite = sprites[index++];
                }
            }
        }
    }

    private void CheckGameOver()
    {
        if (IsBoardFull(0, 7) || IsBoardFull(8, 15)) GameOver();
    }

    private bool IsBoardFull(int startRow, int endRow)
    {
        for (int x = startRow; x <= endRow; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                if (!gridArray[x, y]) return false;
            }
        }
        return true;
    }

    private void GameOver()
    {
        string difficulty = SceneManager.GetActiveScene().name;
        scoreManager.SaveBestScore(difficulty);
        FindObjectOfType<GameOverManager>().ShowGameOverScreen();
    }

    public bool CheckForMatchesAfterSwap()
    {
        if (CheckMatches())
        {
            CollapseGrid();
            return true;
        }
        return false;
    }

    private bool CheckMatches()
    {
        List<GameObject> matchedTiles = new();

        // Sprawdzanie lewej planszy
        matchedTiles.AddRange(CheckSectionMatches(gridArray, 0, 7));

        // Sprawdzanie prawej planszy
        matchedTiles.AddRange(CheckSectionMatches(gridArray, 8, 15));

        matchedTiles = matchedTiles.Distinct().ToList();

        foreach (GameObject tile in matchedTiles)
        {
            Vector2Int pos = FindTilePosition(tile);
            DestroyTile(pos.x, pos.y);
        }

        if (matchedTiles.Count > 0)
        {
            GenerateNewTiles();
        }

        return matchedTiles.Count > 0;
    }

    private Vector2Int FindTilePosition(GameObject tile)
    {
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                if (gridArray[x, y] == tile)
                {
                    return new Vector2Int(x, y);
                }
            }
        }
        return Vector2Int.zero;
    }

    private List<Vector2Int> GetMatchCoordinates(int x, int y)
    {
        List<Vector2Int> matchedCoordinates = new();
        Sprite currentSprite = gridArray[x, y]?.GetComponent<SpriteRenderer>().sprite;

        for (int offsetX = -2; offsetX <= 0; offsetX++)
        {
            int x1 = x + offsetX, x2 = x + offsetX + 2;
            if (IsInBounds(x1, y) && IsInBounds(x2, y) && AreTilesInSameSection(x1, x2))
            {
                if (gridArray[x1, y]?.GetComponent<SpriteRenderer>().sprite == currentSprite &&
                    gridArray[x1 + 1, y]?.GetComponent<SpriteRenderer>().sprite == currentSprite &&
                    gridArray[x2, y]?.GetComponent<SpriteRenderer>().sprite == currentSprite)
                {
                    matchedCoordinates.Add(new Vector2Int(x1, y));
                    matchedCoordinates.Add(new Vector2Int(x1 + 1, y));
                    matchedCoordinates.Add(new Vector2Int(x2, y));
                }
            }
        }

        for (int offsetY = -2; offsetY <= 0; offsetY++)
        {
            int y1 = y + offsetY, y2 = y + offsetY + 2;
            if (IsInBounds(x, y1) && IsInBounds(x, y2) && AreTilesInSameSection(x, x))
            {
                if (gridArray[x, y1]?.GetComponent<SpriteRenderer>().sprite == currentSprite &&
                    gridArray[x, y1 + 1]?.GetComponent<SpriteRenderer>().sprite == currentSprite &&
                    gridArray[x, y2]?.GetComponent<SpriteRenderer>().sprite == currentSprite)
                {
                    matchedCoordinates.Add(new Vector2Int(x, y1));
                    matchedCoordinates.Add(new Vector2Int(x, y1 + 1));
                    matchedCoordinates.Add(new Vector2Int(x, y2));
                }
            }
        }

        return matchedCoordinates;
    }

    private bool AreTilesInSameSection(int rowStart, int rowEnd)
    {
        return (rowStart <= 7 && rowEnd <= 7) || (rowStart >= 8 && rowEnd >= 8);
    }

    private void DestroyTile(int x, int y)
    {
        GameObject tile = gridArray[x, y];
        if (tile == null) return;

        gridArray[x, y] = null;
        destroyedTileCount++;

        tile.transform.DOScale(Vector3.zero, 0.3f).OnComplete(() =>
        {
            Destroy(tile);
            scoreManager.AddScore(10);
            if (destroyTileSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(destroyTileSound, MusicManager.instance.SFXVolume);
            }
        });
    }

    List<GameObject> CheckSectionMatches(GameObject[,] gridArray, int startRow, int endRow)
    {
        List<GameObject> matchedTiles = new();

        // Sprawdzanie poziome (w obrębie sekcji)
        for (int x = startRow; x <= endRow; x++)
        {
            for (int y = 0; y < columns - 2; y++)
            {
                if (gridArray[x, y] != null && gridArray[x, y + 1] != null && gridArray[x, y + 2] != null)
                {
                    SpriteRenderer sr1 = gridArray[x, y].GetComponent<SpriteRenderer>();
                    SpriteRenderer sr2 = gridArray[x, y + 1].GetComponent<SpriteRenderer>();
                    SpriteRenderer sr3 = gridArray[x, y + 2].GetComponent<SpriteRenderer>();

                    if (sr1.sprite == sr2.sprite && sr2.sprite == sr3.sprite)
                    {
                        matchedTiles.Add(gridArray[x, y]);
                        matchedTiles.Add(gridArray[x, y + 1]);
                        matchedTiles.Add(gridArray[x, y + 2]);
                    }
                }
            }
        }

        // Sprawdzanie pionowe (w obrębie sekcji)
        for (int x = startRow; x < endRow - 1; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                if (gridArray[x, y] != null && gridArray[x + 1, y] != null && gridArray[x + 2, y] != null)
                {
                    SpriteRenderer sr1 = gridArray[x, y].GetComponent<SpriteRenderer>();
                    SpriteRenderer sr2 = gridArray[x + 1, y].GetComponent<SpriteRenderer>();
                    SpriteRenderer sr3 = gridArray[x + 2, y].GetComponent<SpriteRenderer>();

                    if (sr1.sprite == sr2.sprite && sr2.sprite == sr3.sprite)
                    {
                        matchedTiles.Add(gridArray[x, y]);
                        matchedTiles.Add(gridArray[x + 1, y]);
                        matchedTiles.Add(gridArray[x + 2, y]);
                    }
                }
            }
        }

        return matchedTiles;
    }


    private void CollapseGrid()
    {
        for (int x = 0; x < rows; x++) // Iterate through all columns
        {
            for (int y = 0; y < columns; y++)
            {
                if (gridArray[x, y] == null)
                {
                    ShiftColumnDown(x);
                }
            }
        }
    }

    private void ShiftColumnDown(int startX)
    {
        int emptyRow = -1;

        // Zacznij od dołu, żeby znaleźć pierwsze puste pole
        for (int y = 0; y < columns; y++)
        {
            if (gridArray[startX, y] == null)
            {
                // Zapisz pierwsze puste miejsce, do którego kafelki mają spaść
                emptyRow = y;
                break;
            }
        }

        if (emptyRow == -1) return;  // Jeśli nie ma pustego miejsca w tej kolumnie, nie rób nic

        // Przesuń kafelki w dół
        for (int y = emptyRow + 1; y < columns; y++)
        {
            if (gridArray[startX, y] != null)
            {
                // Przenieś kafelek na pierwsze puste miejsce
                gridArray[startX, emptyRow] = gridArray[startX, y];
                gridArray[startX, y] = null;

                // Przemieszczamy kafelek
                Vector3 targetPos = new Vector3(startX, emptyRow, startX < 8 ? 0 : RightBoardOffsetZ);
                gridArray[startX, emptyRow]?.transform.DOMove(targetPos, 0.3f);

                // Zaktualizuj pozycję pustego miejsca
                emptyRow++;
            }
        }
    }

    private void GenerateNewTiles()
    {
        var emptyPositions = GetEmptyTilePositions();
        
        float maxNewTiles = Mathf.FloorToInt(emptyPositions.Count * 0.2f);

        int numberOfNewTiles = Mathf.Clamp(
            Random.Range(minNewTiles, Mathf.FloorToInt(maxNewTiles)), 
            minNewTiles, 
            Mathf.FloorToInt(maxNewTiles) 
        );

        Sequence generateSequence = DOTween.Sequence();

        List<GameObject> newTiles = new();

        for (int i = 0; i < numberOfNewTiles && emptyPositions.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, emptyPositions.Count);
            Vector2Int position = emptyPositions[randomIndex];
            emptyPositions.RemoveAt(randomIndex);
            GameObject newTile = CreateTileAtPosition(position);
            gridArray[position.x, position.y] = newTile;
            newTiles.Add(newTile);
            newTile.transform.localScale = Vector3.zero;
        }

        foreach (var tile in newTiles)
        {
            generateSequence.Join(tile.transform.DOScale(new Vector3(0.8f, 0.8f, 0.8f), 0.5f).SetEase(Ease.OutBack));
        }

        generateSequence.Play();
    }

    private List<Vector2Int> GetEmptyTilePositions()
    {
        List<Vector2Int> emptyPositions = new();
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                if (gridArray[x, y] == null)
                    emptyPositions.Add(new Vector2Int(x, y));
            }
        }
        return emptyPositions;
    }

    private GameObject CreateTileAtPosition(Vector2Int position)
    {
        int ZOffset = position.x >= 8 ? RightBoardOffsetZ : 0;
        GameObject newTile = Instantiate(tilePrefab, new Vector3(position.x, position.y, ZOffset), Quaternion.identity, transform);
        newTile.GetComponent<SpriteRenderer>().sprite = tileSprites[Random.Range(0, tileSprites.Count)];
        return newTile;
    }

    private List<Vector2Int> FindFirstAvailableMove()
    {
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < columns; y++)
            {
                if (gridArray[x, y] == null) continue;

                if (x <= 7)
                {
                    if (IsPotentialMatch(x, y, x + 1, y) && x + 1 <= 7) return new() { new(x, y), new(x + 1, y) };
                    if (IsPotentialMatch(x, y, x, y + 1)) return new() { new(x, y), new(x, y + 1) };
                }
                else
                {
                    if (IsPotentialMatch(x, y, x + 1, y) && x + 1 >= 8) return new() { new(x, y), new(x + 1, y) };
                    if (IsPotentialMatch(x, y, x, y + 1)) return new() { new(x, y), new(x, y + 1) };
                }
            }
        }
        return new();
    }

    private void HighlightHintTiles(List<Vector2Int> positions)
    {
        foreach (var pos in positions)
        {
            if (IsInBounds(pos.x, pos.y) && gridArray[pos.x, pos.y] != null)
            {
                SpriteRenderer sr = gridArray[pos.x, pos.y].GetComponent<SpriteRenderer>();
                sr.color = Color.green;
            }
        }
    }

    private void ClearHints()
    {
        foreach (var pos in hintPositions)
        {
            if (IsInBounds(pos.x, pos.y) && gridArray[pos.x, pos.y] != null)
            {
                SpriteRenderer sr = gridArray[pos.x, pos.y].GetComponent<SpriteRenderer>();
                sr.color = Color.white; 
            }
        }
        hintPositions.Clear();
    }

    public void ResetHintTimer()
    {
        ClearHints();
        timeSinceLastMove = 0f;
    }

}
