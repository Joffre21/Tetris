using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }
    public Piece heldPiece { get; private set; }
    private bool changePiece;
    public Piece nextPiece { get; private set; }
    public TetrominoData[] tetrominoes;
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);
    public Vector3Int holdPosition { get; private set; }
    public Vector3Int previewPosition = new Vector3Int(7, 6, 0);
    public Vector2Int boardSize = new Vector2Int(10, 20);

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-this.boardSize.x / 2 , -this.boardSize.y / 2);
            return new RectInt(position, this.boardSize);
        }
    }

    private void Awake()
    {
        this.tilemap = GetComponentInChildren<Tilemap>();
        this.activePiece = GetComponentInChildren<Piece>();

        this.heldPiece = gameObject.AddComponent<Piece>();
        this.heldPiece.enabled = false;
        this.changePiece = true;

        this.nextPiece = gameObject.AddComponent<Piece>();
        this.nextPiece.enabled = false;

        for (int i = 0; i < this.tetrominoes.Length; i++)
        {
            this.tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        SetNextPiece();
        SpawnPiece();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SavePiece();
        }
    }

    private void SetNextPiece()
    {
        if (nextPiece.cells != null)
        {
            Clear(this.nextPiece);
        }
        int random = Random.Range(0, this.tetrominoes.Length);
        TetrominoData data = this.tetrominoes[random];

        switch (data.tetromino.ToString())
        {
            case "I":
            case "O":
                this.previewPosition = new Vector3Int(8, 3, 0);
                break;
            case "Z":
            case "L":
                this.previewPosition = new Vector3Int(8, 4 , 0);
                break;
            case "T":
            case "S":
            case "J":
                this.previewPosition = new Vector3Int(9, 4 , 0);
                break;
        }

        this.nextPiece.Initialize(this, this.previewPosition, data);
        Set(this.nextPiece);

        switch (this.nextPiece.data.tetromino.ToString())
        {
            case "T":
            case "S":
            case "J":
                Clear(this.nextPiece);
                this.nextPiece.ApplyRotationMatrix(-1);
                Set(this.nextPiece);
                break;
            case "Z":
            case "L":
                Clear(this.nextPiece);
                this.nextPiece.ApplyRotationMatrix(1);
                Set(this.nextPiece);
                break;
        }
    }

    private void SavePiece()
    {
        if (this.changePiece)
        {
            switch (this.activePiece.data.tetromino.ToString())
            {
                case "I":
                case "O":
                    this.holdPosition = new Vector3Int(-10, 3, 0);
                    break;
                case "Z":
                case "L":
                    this.holdPosition = new Vector3Int(-10, 4 , 0);
                    break;
                case "T":
                case "S":
                case "J":
                    this.holdPosition = new Vector3Int(-9, 4 , 0);
                    break;
            }

            TetrominoData savedPieceData = this.heldPiece.data;

            if (savedPieceData.cells != null)
            {
                Clear(this.heldPiece);
            }

            this.heldPiece.Initialize(this, this.holdPosition, activePiece.data);

            Set(this.heldPiece);
            Clear(this.activePiece);

            if (savedPieceData.cells != null)
            {
                this.activePiece.Initialize(this, this.spawnPosition, savedPieceData);
                Set(this.nextPiece);
            }
            else
            {
                this.activePiece.Initialize(this, this.spawnPosition, this.nextPiece.data);
            }

            switch (this.heldPiece.data.tetromino.ToString())
            {
                case "T":
                case "S":
                case "J":
                    Clear(this.heldPiece);
                    this.heldPiece.ApplyRotationMatrix(-1);
                    Set(this.heldPiece);
                    break;
                case "Z":
                case "L":
                    Clear(this.heldPiece);
                    this.heldPiece.ApplyRotationMatrix(1);
                    Set(this.heldPiece);
                    break;
            }

            this.changePiece = false;
        }
    }

    public void SpawnPiece()
    {
        this.activePiece.Initialize(this, this.spawnPosition, this.nextPiece.data);

        if (IsValidPosition(this.activePiece, this.spawnPosition))
        {
            this.changePiece = true;
            Set(this.activePiece);
        }
        else
        {
            GameOver();
        }

        SetNextPiece();
    }

    private void GameOver()
    {
        this.tilemap.ClearAllTiles();
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            this.tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = this.Bounds;

        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            if (this.tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }
        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = this.Bounds;
        int row = bounds.yMin;

        while (row < bounds.yMax)
        {
            if(IsLineFull(row))
            {
                LineClear(row);
            } 
            else
            {
                row++;
            }
        }
    }

    private bool IsLineFull(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            if (!this.tilemap.HasTile(position))
            {
                return false;
            }
        }
        return true;
    }

    private void LineClear(int row)
    {
        RectInt bounds = this.Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            this.tilemap.SetTile(position, null);
        }

        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = this.tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(position, above);
            }
            row++;
        }
    }
}
