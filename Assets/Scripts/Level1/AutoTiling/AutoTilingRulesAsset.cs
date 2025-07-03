using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "New AutoTiling Rules", menuName = "AutoTiling/Rules Asset")]
public class AutoTilingRulesAsset : ScriptableObject
{
    [Header("Auto-Tiling Configuration")]
    public string rulesName = "Default Path Rules";

    [Header("All Connectivity Rules")]
    public List<TileConnectivityRule> rules = new List<TileConnectivityRule>();

    [Header("Quick Setup")]
    //[SerializeField] private bool useQuickSetup = false;
    [SerializeField] private TileBase[] quickSetupTiles = new TileBase[47];

    /*[Space]
    [Header("Quick Setup Guide")]
    [TextArea(8, 15)]
    public string quickSetupGuide =
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
        "16-24: Diagonal combinations...";

    [ContextMenu("Generate Basic 29 Rules")]*/
    public void GenerateBasicRules()
    {
        rules.Clear();

        if (quickSetupTiles.Length < 29)
        {
            Debug.LogError("Need exactly 29 tiles for quick setup");
            return;
        }

        //0:Isolated
        /* AddRule("No Connections", quickSetupTiles[0], 100,
         RequiredConnections: new bool[8] { false, false, false, false, false, false, false, false },
         ForbidenConnections: new bool[8] { false, true, false, true, true, false, true, false });

         //1-4: Single connections
         AddRule("Only Top", quickSetupTiles[1], 90,
         RequiredConnections: new bool[8] { false, true, false, false, false, false, false, false },
         ForbidenConnections: new bool[8] { false, false, false, true, true, false, true, false });
         AddRule("Only Bottom", quickSetupTiles[2], 90,
         RequiredConnections: new bool[8] { false, false, false, false, false, false, true, false },
         ForbidenConnections: new bool[8] { false, true, false, true, true, false, false, false });
         AddRule("Only Right", quickSetupTiles[3], 90,
         RequiredConnections: new bool[8] { false, false, false, false, true, false, false, false },
         ForbidenConnections: new bool[8] { false, true, false, true, false, false, true, false });
         AddRule("Only Left", quickSetupTiles[4], 90,
         RequiredConnections: new bool[8] { false, false, false, true, false, false, false, false },
         ForbidenConnections: new bool[8] { false, true, false, false, true, false, true, false });

         //5-8: Corner connections
         AddRule("Top + Right", quickSetupTiles[5], 80,
         RequiredConnections: new bool[8] { false, true, false, false, true, false, false, false },
         ForbidenConnections: new bool[8] { false, false, false, true, false, false, true, false });
         AddRule("Top + Left", quickSetupTiles[6], 80,
         RequiredConnections: new bool[8] { false, true, false, true, false, false, false, false },
         ForbidenConnections: new bool[8] { false, false, false, false, true, false, true, false });
         AddRule("Bottom + Right", quickSetupTiles[7], 80,
         RequiredConnections: new bool[8] { false, false, false, false, true, false, true, false },
         ForbidenConnections: new bool[8] { false, true, false, true, false, false, false, false });
         AddRule("Bottom + Left", quickSetupTiles[8], 80,
         RequiredConnections: new bool[8] { false, false, false, true, false, false, true, false },
         ForbidenConnections: new bool[8] { false, true, false, false, true, false, false, false });

         //9-10: Straight lines
         AddRule("Top + Bottom", quickSetupTiles[9], 70,
         RequiredConnections: new bool[8] { false, true, false, false, false, false, true, false },
         ForbidenConnections: new bool[8] { false, false, false, true, true, false, false, false });
         AddRule("Right + Left", quickSetupTiles[10], 70,
         RequiredConnections: new bool[8] { false, false, false, true, true, false, false, false },
         ForbidenConnections: new bool[8] { false, true, false, false, false, false, true, false });

         //11-14: T-Connections
         AddRule("Top + Right + Bottom", quickSetupTiles[11], 60,
         RequiredConnections: new bool[8] { false, true, false, false, true, false, true, false },
         ForbidenConnections: new bool[8] { false, false, false, true, false, false, false, false });
         AddRule("Top + Left + Bottom", quickSetupTiles[12], 60,
         RequiredConnections: new bool[8] { false, true, false, true, false, false, true, false },
         ForbidenConnections: new bool[8] { false, false, false, false, true, false, false, false });
         AddRule("Right + Bottom + Left", quickSetupTiles[13], 60,
         RequiredConnections: new bool[8] { false, false, false, true, true, false, true, false },
         ForbidenConnections: new bool[8] { false, true, false, false, false, false, false, false });
         AddRule("Right + Top + Left", quickSetupTiles[14], 60,
         RequiredConnections: new bool[8] { false, true, false, true, true, false, false, false },
         ForbidenConnections: new bool[8] { false, false, false, false, false, false, true, false });

         //15: Cross
         AddRule("Cross", quickSetupTiles[15], 50,
         RequiredConnections: new bool[8] { false, true, false, true, true, false, true, false }, exactMatch: true);

         //16-28: Diagonals combinations
         if (quickSetupTiles.Length > 16){
             //16: Surrounded
             AddRule("Surrounded", quickSetupTiles[16], 30,
             RequiredConnections: new bool[8] { true, true, true, true, true, true, true, true }, exactMatch: true);

             if (quickSetupTiles.Length > 20){
                 //17-20: BorderLines(?
                 AddRule("Left + Right + Bottom + Bottom-Left + Bottom-Right", quickSetupTiles[17], 65,
                 RequiredConnections: new bool[8] { false, false, false, true, true, true, true, true },
                 ForbidenConnections: new bool[8] { false, true, false, false, false, false, false, false });
                 AddRule("Left + Right + Top + Top-Left + Top-Right", quickSetupTiles[18], 65,
                 RequiredConnections: new bool[8] { true, true, true, true, true, false, false, false },
                 ForbidenConnections: new bool[8] { false, false, false, false, false, false, true, false });
                 AddRule("Top + Bottom + Top-Left + Left + Bottom-Left", quickSetupTiles[19], 65,
                 RequiredConnections: new bool[8] { true, true, false, true, false, true, true, false },
                 ForbidenConnections: new bool[8] { false, false, false, false, true, false, false, false });
                 AddRule("Top + Bottom + Top-Right + Right + Bottom-Right", quickSetupTiles[20], 65,
                 RequiredConnections: new bool[8] { false, true, true, false, true, false, true, true },
                 ForbidenConnections: new bool[8] { false, false, false, true, false, false, false, false });

                 if (quickSetupTiles.Length > 24){
                     //21-24: Box Corners
                     AddRule("Bottom + Right + Bottom-Right", quickSetupTiles[21], 75,
                     RequiredConnections: new bool[8] { false, false, false, false, true, false, true, true },
                     ForbidenConnections: new bool[8] { false, true, false, true, false, false, false, false });
                     AddRule("Bottom + Left + Bottom-Left", quickSetupTiles[22], 75,
                     RequiredConnections: new bool[8] { false, false, false, true, false, true, true, false },
                     ForbidenConnections: new bool[8] { false, true, false, false, true, false, false, false });
                     AddRule("Top + Right + Top-Right", quickSetupTiles[23], 75,
                     RequiredConnections: new bool[8] { false, true, true, false, true, false, false, false },
                     ForbidenConnections: new bool[8] { false, false, false, true, false, false, true, false });
                     AddRule("Top + Left + Top-Left", quickSetupTiles[24], 75,
                     RequiredConnections: new bool[8] { true, true, false, true, false, false, false, false },
                     ForbidenConnections: new bool[8] { false, false, false, false, true, false, true, false });

                     if (quickSetupTiles.Length > 28){
                         //25-28: Inside Corners
                         AddRule("Top + Right + Left + Bottom + Top-Left, Top-Right + Bottom-Right", quickSetupTiles[25], 40,
                         RequiredConnections: new bool[8] { true, true, true, true, true, false, true, true });
                         AddRule("Top + Right + Left + Bottom + Top-Left + Top-Right + Bottom-Left", quickSetupTiles[26], 40,
                         RequiredConnections: new bool[8] { true, true, true, true, true, true, true, false });
                         AddRule("Top + Right + Left + Bottom + Top-Right + Bottom-Left + Bottom-Right", quickSetupTiles[27], 40,
                         RequiredConnections: new bool[8] { false, true, true, true, true, true, true, true });
                         AddRule("Top + Right + Left + Bottom + Top-Left + Bottom-Left + Bottom-Right", quickSetupTiles[28], 40,
                         RequiredConnections: new bool[8] { true, true, false, true, true, true, true, true });
                     }
                 }
             }
         }*/

        CreateRule("Isolated", 0, 100, required: "00000000", forbidden: "11111111");

        CreateRule("Dead End Top", 1, 90, required: "01000000", forbidden: "10101010");
        CreateRule("Dead End Bottom", 2, 90, required: "00000100", forbidden: "10101010");
        CreateRule("Dead End Right", 3, 90, required: "00001000", forbidden: "01010101");
        CreateRule("Dead End Left", 4, 90, required: "00010000", forbidden: "01010101");

        CreateRule("Corner Top-Right", 5, 85, required: "01001000", forbidden: "10100100");
        CreateRule("Corner Top-Left", 6, 85, required: "01010000", forbidden: "10001010");
        CreateRule("Corner Bottom-Right", 7, 85, required: "00001100", forbidden: "01010001");
        CreateRule("Corner Bottom-Left", 8, 85, required: "00010100", forbidden: "01001010");

        CreateRule("Vertical Line", 9, 80, required: "01000100", forbidden: "10111011");
        CreateRule("Horizontal Line", 10, 80, required: "00011000", forbidden: "11100111");

        CreateRule("T-Junction Left", 11, 75, required: "01001100", forbidden: "10100000");
        CreateRule("T-Junction Right", 12, 75, required: "01010100", forbidden: "10001000");
        CreateRule("T-Junction Up", 13, 75, required: "00011100", forbidden: "01100000");
        CreateRule("T-Junction Down", 14, 75, required: "01011000", forbidden: "00100100");

        CreateRule("Cross", 15, 70, required: "01011100", forbidden: "10100000");

        CreateRule("Surronded", 16, 70, required: "11111111", exactMatch: true);

        CreateRule("Horizontal Border Bottom", 17, 65, required: "00011111", forbidden: "01100000");
        CreateRule("Horizontal Border Top", 18, 65, required: "11111000", forbidden: "00000110");
        CreateRule("Vertical Border Left", 19, 65, required: "10110110", forbidden: "01001001");
        CreateRule("Vertical Border Right", 20, 65, required: "01101101", forbidden: "10010010");

        CreateRule("Outer Corner Bottom-Right", 21, 88, required: "00001101", forbidden: "01110000");
        CreateRule("Outer Corner Bottom-Left", 22, 88, required: "00010110", forbidden: "01101000");
        CreateRule("Outer Corner Top-Right", 23, 88, required: "01001011", forbidden: "10110000");
        CreateRule("Outer Corner Top-Left", 24, 88, required: "11010000", forbidden: "00101100");

        CreateRule("Inner Corner Missing Bottom-Left", 25, 50, required: "11111101", forbidden: "00000010");
        CreateRule("Inner Corner Missing Bottom-Right", 26, 50, required: "11111110", forbidden: "00000001");
        CreateRule("Inner Corner Missing Top-Left", 27, 50, required: "01111111", forbidden: "10000000");
        CreateRule("Inner Corner Missing Top-Right", 28, 50, required: "10111111", forbidden: "01000000");
        Debug.Log($"Generated {rules.Count} auto-tiling rules succesfully!");
    }

    /*private void AddRule(string name, TileBase tile, int priority, bool[] RequiredConnections = null, bool[] ForbidenConnections = null, bool exactMatch = false){
        if (tile == null) return;

        TileConnectivityRule rule = new TileConnectivityRule{
            ruleName = name,
            tileToPlace = tile,
            priority = priority,
            exactMatch = exactMatch,
            ignoreUnmarkedConnections = !exactMatch,
        };

        if (RequiredConnections != null && RequiredConnections.Length == 8){
            rule.topLeft = RequiredConnections[0];
            rule.top = RequiredConnections[1];
            rule.topRight = RequiredConnections[2];
            rule.left = RequiredConnections[3];
            rule.right = RequiredConnections[4];
            rule.bottomLeft = RequiredConnections[5];
            rule.bottom = RequiredConnections[6];
            rule.bottomRight = RequiredConnections[7];
        }

        if (ForbidenConnections != null && ForbidenConnections.Length == 8){
            rule.forbiddenTopLeft = ForbidenConnections[0];
            rule.forbiddenTop = ForbidenConnections[1];
            rule.forbiddenTopRight = ForbidenConnections[2];
            rule.forbiddenLeft = ForbidenConnections[3];
            rule.forbiddenRight = ForbidenConnections[4];
            rule.forbiddenBottomLeft = ForbidenConnections[5];
            rule.forbiddenBottom = ForbidenConnections[6];
            rule.forbiddenBottomRight = ForbidenConnections[7];
        }
        rules.Add(rule);
    }*/

    private void CreateRule(string name, int tileIndex, int priority, string required = null, string forbidden = null, bool exactMatch = false)
    {
        if (tileIndex >= quickSetupTiles.Length || quickSetupTiles[tileIndex] == null)
        {
            Debug.Log($"Tile{tileIndex} is null or out of range for rule {name}");
            return;
        }

        TileConnectivityRule rule = new TileConnectivityRule
        {
            ruleName = name,
            tileToPlace = quickSetupTiles[tileIndex],
            priority = priority,
            exactMatch = exactMatch
        };

        if (!string.IsNullOrEmpty(required) && required.Length == 8)
        {
            rule.topLeft = required[0] == '1';
            rule.top = required[1] == '1';
            rule.topRight = required[2] == '1';
            rule.left = required[3] == '1';
            rule.right = required[4] == '1';
            rule.bottomLeft = required[5] == '1';
            rule.bottom = required[6] == '1';
            rule.bottomRight = required[7] == '1';
        }

        if (!string.IsNullOrEmpty(forbidden) && forbidden.Length == 8)
        {
            rule.forbiddenTopLeft = forbidden[0] == '1';
            rule.forbiddenTop = forbidden[1] == '1';
            rule.forbiddenTopRight = forbidden[2] == '1';
            rule.forbiddenLeft = forbidden[3] == '1';
            rule.forbiddenRight = forbidden[4] == '1';
            rule.forbiddenBottomLeft = forbidden[5] == '1';
            rule.forbiddenBottom = forbidden[6] == '1';
            rule.forbiddenBottomRight = forbidden[7] == '1';
        }
        rules.Add(rule);

        if (Application.isEditor)
        {
            Debug.Log($"Created rule: {name}, tile: {quickSetupTiles[tileIndex].name}, priority: {priority}");
        }
    }

    [ContextMenu("Apply Rules to AutoTiling System")]
    public void ApplyRulesToSystem()
    {
        AutoTilingSystem system = FindFirstObjectByType<AutoTilingSystem>();

        if (system != null)
        {
            system.ClearConnectivityRules();
            foreach (var rule in rules)
            {
                system.AddConnectivityRule(rule);
            }
            Debug.Log($"Applied {rules.Count}  rules to AutoTiling System");
        }
        else
        {
            Debug.LogError("AutoTilingSystem not found in scene!");
        }
    }

    [ContextMenu("Validate rules")]
    public void ValidateRules()
    {
        int validRules = 0;
        int invalidRules = 0;

        foreach (var rule in rules)
        {
            if (rule.tileToPlace == null)
            {
                Debug.Log($"Rule {rule.ruleName} has no tile assigned");
                invalidRules++;
            }
            else
            {
                validRules++;
            }
        }

        Debug.Log($"Rule validation complete: {validRules} valid, {invalidRules} invalid rules");
    }

    [ContextMenu("Debug Rule Patterns")]
    /*public void DebugTilePatterns(){
        foreach (var rule in rules){
            if (rule.tileToPlace != null){
                bool[] required = rule.GetConnectionPattern();
                bool[] forbidden = rule.GetForbiddenPattern();

                string reqStr = string.Join(", ", required);
                string forbStr = string.Join(", ", forbidden);

                Debug.Log($"Rule: {rule.ruleName} | priority: {rule.priority} | required: [{reqStr}] | Forbidden: [{forbStr}]");
            }
        }
    }*/

    public void DebugAllRules()
    {
        foreach (var rule in rules.OrderByDescending(r => r.priority))
        {
            if (rule.tileToPlace != null)
            {
                bool[] required = rule.GetConnectionPattern();
                bool[] forbidden = rule.GetForbiddenPattern();

                string reqStr = "";
                string forbStr = "";

                for (int i = 0; i < 8; i++)
                {
                    reqStr += required[i] ? "1" : "0";
                    forbStr += forbidden[i] ? "1" : "0";
                }

                Debug.Log($"Rule: {rule.ruleName}, priority:{rule.priority}, tile:{rule.tileToPlace.name} \n" +
                $"Required: {reqStr}, forbidden: {forbStr}, exact: {rule.exactMatch}");
            }
        }
    }

    [ContextMenu("Test Specific Pattern")]
    public void TestSpecificPattern()
    {
        
    }
}
