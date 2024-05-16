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
                // �R�[���h�X�^�[�g�� Application.absoluteURL �� null �ł͂���܂���B���̂��� Deep Link ���������܂�
                onDeepLinkActivated( Application.absoluteURL );
            }
            // DeepLink Manager �O���[�o���ϐ���������
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
        // DeepLink Manager �O���[�o���ϐ����A�b�v�f�[�g�B���̂��߁AURL �͂ǂ�����ł��A�N�Z�X�\�ł�
        deeplinkURL = url;
        Debug.Log( "deeplinkURL" + deeplinkURL );

        /*
        // URL ���f�R�[�h���ē�������肵�܂� 
        // ���̗�ł́A�����N���ȉ��̂悤�Ƀt�H�[�}�b�g����邱�Ƃ�O��Ƃ��Ă��܂�
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
