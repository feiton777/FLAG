using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using UnityEngine.UI;
using UnityEngine.Networking;

public class MapViewManager : MonoBehaviour
{
    [SerializeField]
    TMPro.TMP_Dropdown m_ZoomDropdown;

    [SerializeField]
    Transform m_Marker;

    [SerializeField]
    List<RawImage> m_MapImageList = new List<RawImage>();
    
    [SerializeField]
    List<Transform> m_TargetMarkerList = new List<Transform>();

    int m_ZoomLevel = 18;

    // Start is called before the first frame update
    void Start()
    {
        //LoadMapAsync().Forget();
    }

	private void OnEnable()
	{
        LoadMapAsync().Forget();
    }

	async UniTask<int> LoadMapAsync()
	{
        if( LocationManager.LocationList.Count == 0 ) return 0;

        var setData = LocationManager.GetLocation();
        //var level = 18; // 5:“ú–{‘S‘Ì

        var xy = LocationManager.Degrees2meters( setData.x, setData.y );
        var unit = 2 * LocationManager.GEO_R * Math.PI / Math.Pow( 2, m_ZoomLevel );
        var xtile = Math.Floor( ( xy.x - LocationManager.orgX ) / unit );
        var ytile = Math.Floor( ( LocationManager.orgY - xy.y ) / unit );

        var xPos = ( ( ( xy.x - LocationManager.orgX ) % unit ) / unit ) * 256.0 - 128.0;
        var yPos = -( ( ( LocationManager.orgY - xy.y ) % unit ) / unit ) * 256.0 + 128.0;
        m_Marker.localPosition = new Vector3( (float)xPos, (float)yPos, 0.0f );
        m_Marker.localScale = Vector3.one;

        //Debug.Log($"");

        var leftXTile = xtile - 1;
        var upYTile = ytile - 1;
        for( int i = 0; i < m_MapImageList.Count; i++ )
        {
            var xIndex = i % 3;
            var yIndex = i / 3;

            //string _uri = @"https://a.tile.openstreetmap.org/0/0/0.png";
            //string _uri = $"https://a.tile.openstreetmap.org/{m_ZoomLevel}/{leftXTile+xIndex}/{upYTile+yIndex}.png";
            string _uri = $"https://tile.openstreetmap.org/{m_ZoomLevel}/{leftXTile + xIndex}/{upYTile + yIndex}.png";

            UnityWebRequest www = UnityWebRequestTexture.GetTexture( _uri );
            await www.SendWebRequest();
            if( www.result != UnityWebRequest.Result.Success )
            {
                Debug.Log( www.error );
            }
            else
            {
                m_MapImageList[ i ].texture = ( (DownloadHandlerTexture)www.downloadHandler ).texture;
            }
        }

        // 
        if( FlagObjectsManager.FlagDataList == null ) return 1;

        for( int i = m_TargetMarkerList.Count - 1; 0 <= i; i-- )
        {
            Destroy( m_TargetMarkerList[ i ].gameObject );
        }
        m_TargetMarkerList.Clear();

        for( int i = 0; i < FlagObjectsManager.FlagDataList.Count; i++ )
        {
            string[] delComma = { "," };
            string[] splitData = FlagObjectsManager.FlagDataList[ i ].Split( delComma, StringSplitOptions.None );

            // hash,TANYEISCFJ7ZYLPIRXNQZCZUOM3UIDN4QOI6N5Q,32543886000,Test_Set_flag,1,298182EF0E364B30,-90.000000,-180.000000,‚ ‚¢‚¤‚¦‚¨‚©‚«‚­‚¯‚±‚³‚µ‚·‚¹‚»‚½‚¿‚Â‚Ä‚Æ‚È‚É‚Ê‚Ë‚Ì‚Í‚Ð‚Ó‚Ö‚Ù‚Ü‚Ý‚Þ‚ß‚à‚â‚ä‚æ‚ç‚è‚é‚ê‚ë‚í‚ð‚ñ
            if( splitData.Length < 9 ) continue;

            var flagXY = LocationManager.Degrees2meters( float.Parse( splitData[ 6 ] ), float.Parse( splitData[ 7 ] ) );
            var flagXTile = Math.Floor( ( flagXY.x - LocationManager.orgX ) / unit );
            var flagYTile = Math.Floor( ( LocationManager.orgY - flagXY.y ) / unit );
            var flagXPos = ( ( ( flagXY.x - LocationManager.orgX ) % unit ) / unit ) * 256.0 - 128.0 + ( flagXTile - xtile ) * 256.0;
            var flagYPos = -( ( ( LocationManager.orgY - flagXY.y ) % unit ) / unit ) * 256.0 + 128.0 - ( flagYTile - ytile ) * 256.0;

            var flagObject = Instantiate( m_Marker.gameObject );
            flagObject.transform.SetParent( m_Marker.parent );
            flagObject.transform.localScale = Vector3.one;
            flagObject.transform.localPosition = new Vector3( (float)flagXPos, (float)flagYPos, 0.0f );
            if( splitData[ 1 ].Contains( Test_AccountManager.Instance.AliceAddress.ToString() ) )
            {
                flagObject.GetComponent<Image>().color = Color.blue;
            }
            else
			{
                flagObject.GetComponent<Image>().color = Color.green;
            }
            m_TargetMarkerList.Add( flagObject.transform );
        }

        return 0;
	}

    public void ChangeZoomDropdown()
	{
        switch( m_ZoomDropdown.value )
		{
            default:
            case 0: // x18
                m_ZoomLevel = 18;
                break;
            case 1: // x17
                m_ZoomLevel = 17;
                break;
            case 2: // x16
                m_ZoomLevel = 16;
                break;
            case 3: // x14
                m_ZoomLevel = 14;
                break;
            case 4: // x12
                m_ZoomLevel = 12;
                break;
            case 5: // x10
                m_ZoomLevel = 10;
                break;
            case 6: // x8
                m_ZoomLevel = 8;
                break;
            case 7: // x5
                m_ZoomLevel = 5;
                break;
        }
        LoadMapAsync().Forget();
    }

}
