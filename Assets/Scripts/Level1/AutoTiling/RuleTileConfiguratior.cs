using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
[CreateAssetMenu(fileName = "new Rule Tile Configurator", menuName = "AutoTiling/Rule Tile Configurator")]
public class RuleTileConfiguratior : ScriptableObject
{
    [Header("Rule Tile configuratoion")]
    public RuleTile targetRuleTile;

    [Header("Sprites for Auto-Tiling")]
    public TileBase[] autoTilingSprtes = new TileBase[47];

    [Header("Automatic Configuration")]
    public bool autoCreateRules = true;

    [Space]
    [TextArea(8, 15)]
    [Tooltip("Guide top order the tiles")]
    public string spriteOrderGuide = 
        "Quick Setup tiles order (0-46):\n" +
        "0: No connections \n" +
        "1: Only top \n" +
        "2: Only bottom \n" +
        "3: Only Right \n" +
        "4: Only Left \n" +
        "5: Top + Right \n" +
        "6: Top + Left \n" +
        "7: Bottom + Right \n" +
        "8: Bottom + Left \n" +
        "9: Top + Bottom \n" +
        "10: Left + Right \n" +
        "11: Top + Right + Bottom \n" +
        "12: Top + Left + Bottom \n" +
        "13: Right + Bottom + Left \n" +
        "14: Right + Top + Left \n" +
        "15: Top + Right + Left + Bottom \n" +
        "16: Completely surrounded \n" +
        "17-46: Diagonal combinations...";

        [ContextMenu("Configure Rule Tile")]
        public void ConfigureRuleTile(){
            if(targetRuleTile == null){
                Debug.LogError("Target Rule Tile is not assigned.");
                return;
            }

            if(autoCreateRules){
                CreateBasicRules();
            }
            else{
                AssignTilesToRuleTile();
            }

            EditorUtility.SetDirty(targetRuleTile);
            AssetDatabase.SaveAssets();

            Debug.Log("Rule Tile configuration completed.");
        }

        private void CreateBasicRules(){
            targetRuleTile.m_TilingRules.Clear();

            CreateRule("Isolated", 0, GetPattern("00000000"));
            CreateRule("Dead End Top", 1,  GetPattern( "01000000"));
            CreateRule("Dead End Bottom", 2,  GetPattern( "00000100"));
            CreateRule("Dead End Right", 3,  GetPattern( "00001000"));
            CreateRule("Dead End Left", 4,  GetPattern( "00010000"));

            CreateRule("Corner Top-Right", 5,  GetPattern( "01001000"));
            CreateRule("Corner Top-Left", 6, GetPattern( "01010000"));
            CreateRule("Corner Bottom-Right", 7,  GetPattern( "00001100"));
            CreateRule("Corner Bottom-Left", 8,  GetPattern( "00010100"));

            CreateRule("Vertical Line", 9,  GetPattern( "01000100"));
            CreateRule("Horizontal Line", 10,  GetPattern( "00011000"));

            CreateRule("T-Junction Left", 11,  GetPattern( "01001100"));
            CreateRule("T-Junction Right", 12,  GetPattern( "01010100"));
            CreateRule("T-Junction Up", 13,  GetPattern( "00011100"));
            CreateRule("T-Junction Down", 14,  GetPattern( "01011000"));

            CreateRule("Cross", 15, GetPattern("01011100"));
        }

    private void CreateRule(string ruleName, int tileIndex, int[] neighborPattern){
        if(tileIndex >= autoTilingSprtes.Length || autoTilingSprtes[tileIndex] == null){
            Debug.LogWarning($"Tile index {tileIndex} is out of bounds or sprite is not assigned.");
            return;
        }

        RuleTile.TilingRule newRule = new RuleTile.TilingRule();

        Sprite sprite = GetSpriteFromTile(autoTilingSprtes[tileIndex]);
        if(sprite != null){
            newRule.m_Sprites = new Sprite[] { sprite };
        }
        newRule.m_Output = RuleTile.TilingRule.OutputSprite.Single;

        newRule.m_Neighbors = new List<int>();
        newRule.m_NeighborPositions = new List<Vector3Int>();

        Vector3Int[] directions = new Vector3Int[]{
            new Vector3Int(-1, 1, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(1, 1, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, -1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(1, -1, 0),
        };

        for(int i = 0; i < directions.Length; i++){
            newRule.m_NeighborPositions.Add(directions[i]);

            if(neighborPattern != null && i < neighborPattern.Length){
                newRule.m_Neighbors.Add(neighborPattern[i]);
            }
        }
        targetRuleTile.m_TilingRules.Add(newRule);
    }

    private int[] GetPattern(string pattern){
        if(pattern.Length != 8) return null;

        int[] result = new int[8];
        for(int i = 0; i < 8; i++){
            switch(pattern[i]){
                case '0':
                 result[i] = RuleTile.TilingRule.Neighbor.NotThis;
                 break;
                 case '1':
                 result[i] = RuleTile.TilingRule.Neighbor.This;
                 break;
            }
        }
        return result;
    }

    private void AssignTilesToRuleTile(){
        if(targetRuleTile.m_TilingRules.Count == 0){
            Debug.LogWarning("No rules found in the target Rule Tile. Please create rules first.");
            return;
        }

        int tileIndex = 0;
        foreach(var rule in targetRuleTile.m_TilingRules){
            if(tileIndex < autoTilingSprtes.Length && autoTilingSprtes[tileIndex] != null){
                rule.m_Sprites = new Sprite[] { GetSpriteFromTile(autoTilingSprtes[tileIndex]) };
            } 
            tileIndex++;
        }
    }

    private Sprite GetSpriteFromTile(TileBase tile){
        if(tile == null) return null;

        if(tile is Tile regularTile){
            return regularTile.sprite;
        }

        if(tile is ScriptableObject so){
            var spriteProperty = so.GetType().GetField("sprite");

            if(spriteProperty != null){
                return spriteProperty.GetValue(so) as Sprite;
            }
        }
        return null;
    }

    [ContextMenu("Create New Rule Tile")]
    public void CreateNewRuleTile(){
        string path = EditorUtility.SaveFilePanelInProject(
            "Create New Rule Tile",
            "New Rule Tile",
            "asset",
            "Choose where to save the new Rule Tile"
        );

        if(!string.IsNullOrEmpty(path)){
            RuleTile newRuleTile = ScriptableObject.CreateInstance<RuleTile>();
            AssetDatabase.CreateAsset(newRuleTile, path);
            AssetDatabase.SaveAssets();

            targetRuleTile = newRuleTile;
            EditorUtility.SetDirty(this);

            Debug.Log("New Rule Tile created at: " + path);
        }
    }

    [ContextMenu("Validate Configuration")]
    public void ValidateConfiguration(){
        if(targetRuleTile == null){
            Debug.LogError("Target Rule Tile is not assigned.");
            return;
        }

        int validSprites = 0;
        int totalSprites = autoTilingSprtes.Length;

        for(int i = 0; i< autoTilingSprtes.Length; i++){
            if(autoTilingSprtes[i] != null){
                validSprites++;
            }
        }

        Debug.Log($"Configuration Validation: {validSprites} valid sprites out of {totalSprites} total sprites.");
        if(targetRuleTile.m_TilingRules != null){
            Debug.Log($"Rule Tile has {targetRuleTile.m_TilingRules.Count} rules defined.");
        } 
    }

    [ContextMenu("Clear All Rules")]
    public void ClearAllRules(){
        if(targetRuleTile == null){
            Debug.LogError("Target Rule Tile is not assigned.");
            return;
        }

        targetRuleTile.m_TilingRules.Clear();
        EditorUtility.SetDirty(targetRuleTile);
        Debug.Log("All rules cleared from the Rule Tile.");
    }

    [ContextMenu("Print Rule Summary")]
    public void PrintRuleSummary(){
        if(targetRuleTile == null){
            Debug.LogError("Target Rule Tile is not assigned.");
            return;
        }

        Debug.Log($"Rule Tile Summary for {targetRuleTile.name}");
        Debug.Log($"Total Rules: {targetRuleTile.m_TilingRules.Count}");

        for(int i = 0; i < targetRuleTile.m_TilingRules.Count; i++){
            var rule = targetRuleTile.m_TilingRules[i];
            string spriteInfo = rule.m_Sprites != null && rule.m_Sprites.Length > 0 && rule.m_Sprites[0] != null
                ? rule.m_Sprites[0].name
                : "No Sprite Assigned";
            Debug.Log($"Rule {i}: {spriteInfo}");
        }
    }
}
#endif
