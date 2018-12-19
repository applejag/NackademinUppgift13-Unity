using System.Globalization;
using BattleshipProtocol;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private BattleGame game;
    
    private void OnEnable()
    {
        CultureInfo.DefaultThreadCurrentCulture =
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");
    }
    
    private void OnDestroy()
    {
        game?.Disconnect();
        game?.Dispose();
    }
}
