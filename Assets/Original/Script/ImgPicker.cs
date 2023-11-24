using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImgPicker : MonoBehaviour
{
    [SerializeField]
    Image m_Image;

    public delegate void ImageDelegate( string path );

    public delegate void ErrorDelegate( string message );

    public event ImageDelegate Completed;

    public event ErrorDelegate Failed;

    private IPicker m_Picker =
#if UNITY_ANDROID && !UNITY_EDITOR
            new PickerAndroid();
#else
            new PickerEditor();
#endif

	public void Awake()
	{
        Completed += ( string path ) => { StartCoroutine( LoadImage( path, m_Image ) ); };
    }


    [Obsolete( "Resizing is deprecated. Use Show(title, outputFileName)" )]
    public void Show( string title, string outputFileName, int maxSize )
    {
        Show( title, outputFileName );
    }

    public void Show( string title, string outputFileName )
    {
        m_Picker.Show( title, outputFileName );
    }

    public void ClickShowPicker()
    {
        Show( "Select Image", "unimgpicker" );
    }

    private void OnComplete( string path )
    {
        var handler = Completed;
        if( handler != null )
        {
            handler( path );
        }
    }

    private void OnFailure( string message )
    {
        var handler = Failed;
        if( handler != null )
        {
            handler( message );
        }
    }

    private void OnLog( string message )
    {
        Debug.Log( message );
    }

    private IEnumerator LoadImage( string path, Image output )
    {
        var url = "file://" + path;
        Debug.Log( url );
        var unityWebRequestTexture = UnityWebRequestTexture.GetTexture( url );
        yield return unityWebRequestTexture.SendWebRequest();

        var texture = ( (DownloadHandlerTexture)unityWebRequestTexture.downloadHandler ).texture;
        if( texture == null )
        {
            Debug.LogError( "Failed to load texture url:" + url );
        }

        output.sprite = Sprite.Create( texture, new Rect( 0, 0, texture.width, texture.height ), Vector2.zero );
        RectTransform rectTransform = output.gameObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2( texture.width, texture.height );
        output.color = Color.white;
        var wRate = 1080.0f / texture.width;
        var hRate = 1980.0f / texture.height;
        var minRate = Mathf.Min( wRate, hRate );
        output.transform.localScale = new Vector3( minRate, minRate, minRate );

        Test_MosaicManager.Instance.m_ExplanationText.SetActive( false );
        Test_MosaicManager.Instance.m_LoadImageButton.SetActive( false );
        Test_MosaicManager.Instance.m_CreateFlagButton.SetActive( true );
    }


    //private IEnumerator LoadImage( string path, MeshRenderer output )
    //{
    //    var url = "file://" + path;
    //    var unityWebRequestTexture = UnityWebRequestTexture.GetTexture( url );
    //    yield return unityWebRequestTexture.SendWebRequest();
    //
    //    var texture = ( (DownloadHandlerTexture)unityWebRequestTexture.downloadHandler ).texture;
    //    if( texture == null )
    //    {
    //        Debug.LogError( "Failed to load texture url:" + url );
    //    }
    //
    //    output.material.mainTexture = texture;
    //}
}
