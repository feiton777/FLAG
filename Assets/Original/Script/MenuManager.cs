using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance = null;

    public List<GameObject> m_MenuList = new List<GameObject>();

    private void Awake()
    {
        if( Instance == null )
        {
            Instance = this;
        }
        else
        {
            Debug.Log( "MenuManager multi instance" );
            gameObject.SetActive( false );
        }
    }

	public void OnDestroy()
	{
        Instance = null;
    }

    public void OpenMainMenu()
    {
        OpenMenu( "MainMenu" );
    }

    public void OpenAccountInfo()
    {
        OpenMenu( "AccountInfo" );
    }

    public void OpenCreateMosaicMenu()
    {
        OpenMenu( "CreateMosaicMenu" );
    }

    public void OpenMapView()
    {
        OpenMenu( "MapView" );
    }

    public void OpenSetFlagMenu()
    {
        OpenMenu( "SetFlagMenu" );
    }

    private void OpenMenu( string menu )
    {
        foreach( var obj in m_MenuList )
        {
            if( obj.name.Equals( menu ) )
            {
                obj.SetActive( true );
            }
            else
            {
                obj.SetActive( false );
            }
        }
    }
}
