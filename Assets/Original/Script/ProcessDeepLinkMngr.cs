using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcessDeepLinkMngr : MonoBehaviour
{
    public static ProcessDeepLinkMngr Instance { get; private set; }
    public string deeplinkURL;
    private void Awake()
    {
        if( Instance == null )
        {
            Instance = this;
            Application.deepLinkActivated += onDeepLinkActivated;
            if( !string.IsNullOrEmpty( Application.absoluteURL ) )
            {
                // コールドスタートと Application.absoluteURL は null ではありません。そのため Deep Link を処理します
                onDeepLinkActivated( Application.absoluteURL );
            }
            // DeepLink Manager グローバル変数を初期化
            else deeplinkURL = "[none]";
            DontDestroyOnLoad( gameObject );
        }
        else
        {
            Destroy( gameObject );
        }
    }

    private void onDeepLinkActivated( string url )
    {
        // DeepLink Manager グローバル変数をアップデート。そのため、URL はどこからでもアクセス可能です
        deeplinkURL = url;
        Debug.Log( "deeplinkURL" + deeplinkURL );

        /*
        // URL をデコードして動作を決定します 
        // この例では、リンクが以下のようにフォーマットされることを前提としています
        // unitydl://mylink?scene1
        string sceneName = url.Split( "?"[ 0 ] )[ 1 ];
        bool validScene;
        switch( sceneName )
        {
            case "scene1":
                validScene = true;
                break;
            case "scene2":
                validScene = true;
                break;
            default:
                validScene = false;
                break;
        }
        if( validScene ) UnityEngine.SceneManagement.SceneManager.LoadScene( sceneName );
        */
    }
}
