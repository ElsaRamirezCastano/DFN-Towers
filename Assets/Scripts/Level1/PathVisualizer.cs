using UnityEngine;
using UnityEngine.Tilemaps;

public class PathVisualizer : MonoBehaviour{
    private Tilemap previewTilemap;
    private TileBase previewTile;
    private Vector3Int lastPreviewPosition = Vector3Int.zero;
    private bool showingPreview = false;

    public void Initialize(Tilemap tilemap, TileBase tile){
        previewTilemap = tilemap;
        previewTile = tile;
    }

    public void UpdatePreview(Vector3Int position, bool canPlace){
        if (showingPreview){
            previewTilemap.SetTile(lastPreviewPosition, null);
        }

        if (canPlace){
            previewTilemap.SetTile(position, previewTile);
            lastPreviewPosition = position;
            showingPreview = true;
        }
        else{
            showingPreview = false;
        }
    }

    public void ClearPreview(){
        if (showingPreview){
            previewTilemap.SetTile(lastPreviewPosition, null);
            showingPreview = false;
        }
    }
}
