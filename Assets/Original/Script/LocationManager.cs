using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using System;
using System.Device.Location;

public class LocationManager : MonoBehaviour
{
    [SerializeField]
    TMPro.TMP_Text m_DebugText;

    [SerializeField]
    Transform m_Camera;

    [SerializeField]
    Transform m_Object;

    public static LocationManager Instance { set; get; } = null;

    public float Latitude { private set; get; } = 0.0f; // 緯度
    public float Longitude { private set; get; } = 0.0f; // 経度
    public float Altitude { private set; get; } = 0.0f; // 高度

    public static List<Vector2> LocationList { private set; get; } = new List<Vector2>();

    private bool m_IsPlay = false;

    private void Awake()
    {
        if( Instance == null )
        {
            Instance = this;
            DontDestroyOnLoad( gameObject );
        }
        else
        {
            Destroy( gameObject );
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if( !m_IsPlay )
        {
            m_IsPlay = true;
            StartLocationServiceAsync().Forget();
        }
    }

    private async UniTask<int> StartLocationServiceAsync()
    {

#if UNITY_ANDROID && !UNITY_EDITOR
        if( !Input.location.isEnabledByUser )
        {
            UnityEngine.Android.Permission.RequestUserPermission( UnityEngine.Android.Permission.FineLocation );
            //CheckPermission( "android.permission.ACCESS_FINE_LOCATION" );
            await UniTask.Delay( 10 * 1000 );
        }
#endif

		// First, check if user has location service enabled
		if( !Input.location.isEnabledByUser )
        {
            Debug.Log( "GPS not enabled" );
            return 1;
        }

        // Start service before querying location
        Input.compass.enabled = true;
        Input.location.Start( 5.0f );

        // Wait until service initializes
        int maxWait = 20;
        while( Input.location.status == LocationServiceStatus.Initializing && maxWait > 0 )
        {
            await UniTask.Delay( 1000 );
            //yield return new WaitForSeconds( 1 );
            maxWait--;
        }

        // Service didn't initialize in 20 seconds
        if( maxWait <= 0 )
        {
            Debug.Log( "Timed out" );
            return 1;
        }

        // Connection has failed
        if( Input.location.status == LocationServiceStatus.Failed )
        {
            Debug.Log( "Unable to determine device location" );
            return 1;
        }

		{
            var setData = new Vector2( Input.location.lastData.latitude, Input.location.lastData.longitude );
            LocationList.Add( setData );
        }

        // Set locational infomations
        while( true )
        {
            LocationInfo lastData = Input.location.lastData;
            //GeoCoordinateUtility.GeoCoordinateConverter.LatLon2Coordinate();
            //var result = CoordConv.BLH2XYZ( lastData.longitude, lastData.longitude, 0.0 );
            var result = CoordConv.BLH2XYZ( 89.0, 179.0, 0.0 );
            m_DebugText.text = $"Latitude : {lastData.latitude.ToString( "R" )}, Longitude : {lastData.longitude.ToString( "R" )}\r\n Accuracy : {lastData.horizontalAccuracy.ToString( "R" )}\r\n x : {result.x.ToString( "R" )}, y : {result.y.ToString( "R" )}, z : {result.z.ToString( "R" )}\r\n Count : {LocationList.Count}\r\n x : {m_Camera.localPosition.x}, y : {m_Camera.localPosition.y}, z : {m_Camera.localPosition.z}";


            var xy = Degrees2meters( lastData.latitude, lastData.longitude );
            var unit = 2 * GEO_R * Math.PI / Math.Pow( 2, 18 );
            var xtile = Math.Floor( ( xy.x - orgX ) / unit );
            var ytile = Math.Floor( ( orgY - xy.y ) / unit );
            m_DebugText.text = $"Latitude : {lastData.latitude.ToString( "R" )}, Longitude : {lastData.longitude.ToString( "R" )}\r\n Accuracy : {lastData.horizontalAccuracy.ToString( "R" )}\r\n x : {xtile.ToString( "R" )}, y : {ytile.ToString( "R" )}\r\n Count : {LocationList.Count}\r\n x : {m_Camera.localPosition.x}, y : {m_Camera.localPosition.y}, z : {m_Camera.localPosition.z}";


            if( lastData.horizontalAccuracy < 0.0f || lastData.horizontalAccuracy > 20.0f )
			{
                //
			}
            else
			{
                if( LocationList.Count > 100 )
				{
                    LocationList.RemoveAt( 0 );
                }
                var setData = new Vector2( Input.location.lastData.latitude, Input.location.lastData.longitude );
                LocationList.Add( setData );
            }

            if( 0 < LocationList.Count )
            {
                Vector2 setData2 = GetLocation();

                Latitude = setData2.x;
                Longitude = setData2.y;
                Altitude = Input.location.lastData.altitude;
                //m_DebugText.text = $"Latitude : {Latitude}, Longitude : {Longitude} ";
                
                //var sub = m_Object.position - m_Camera.position;
                //m_DebugText.text = $"x : {sub.x}, y : {sub.y}, z : {sub.z}";

                //m_DebugText.text = $"x : {m_Camera.localPosition.x}, y : {m_Camera.localPosition.y}, z : {m_Camera.localPosition.z}, rotx : {m_Camera.localRotation.x}, roty : {m_Camera.localRotation.y}, rotz : {m_Camera.localRotation.z}";
                //yield return new WaitForSeconds( 10 );

                //UpdateObjectPos( m_Camera, m_Object, 35.174223f, 136.859547f, 1.0f );
            }

            //await UniTask.Delay( 1 * 100 );
            await UniTask.Yield( PlayerLoopTiming.Update );
        }
        return 0;
    }

    public static Vector2 GetLocation()
	{
        if( LocationList.Count == 0 ) return Vector2.zero;

        Vector2 setData = Vector2.zero;
        foreach( var oneData in LocationList )
        {
            setData += oneData;
        }
        setData = setData / LocationList.Count;
        return setData;
    }

    static bool CheckPermission( string permission )
    {
        using( var unityPlayer = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" ) )
        using( var activity = unityPlayer.GetStatic<AndroidJavaObject>( "currentActivity" ) )
        using( var compat = new AndroidJavaClass( "android.support.v4.app.ActivityCompat" ) )
        {
            var check = compat.CallStatic<int>( "checkSelfPermission", activity, permission );

            if( check == 0 ) return true;

            int REQUEST_CODE = 1;
            compat.CallStatic( "requestPermissions", activity, new string[] {
                    permission
                }, REQUEST_CODE );

            //再チェック
            check = compat.CallStatic<int>( "checkSelfPermission", activity, permission );
            if( check == 0 ) return true;

            // "設定からパーミッションを許可してください。機能が使用できません。";
        }
        return false;
    }

    public static bool CheckDistance( float targetLatitude, float targetLongitude )
	{
        // ロケーション情報
        if( !Input.location.isEnabledByUser || Input.location.status != LocationServiceStatus.Running )
        {
            return false;
        }

        if( LocationList.Count == 0 ) return false;
        Vector2 setData = GetLocation();
        GeoCoordinate currentCoordinate = new GeoCoordinate( setData.x, setData.y );
        float currentHeading = Input.compass.trueHeading;

        GeoCoordinate targetCoordinate = new GeoCoordinate( targetLatitude, targetLongitude );
        double distance = currentCoordinate.GetDistanceTo( targetCoordinate ); // 現在位置と目標位置の距離

        if( 10.0f * 1000.0f < distance )
        {
            return false;
        }

        return true;
    }

    public static bool UpdateObjectPos( Transform cameraTransform, Transform targetTransform, float targetLatitude, float targetLongitude, float targetAltitude )
    {
        // ロケーション情報
        if( !Input.location.isEnabledByUser || Input.location.status != LocationServiceStatus.Running )
        {
            return false;
        }

        //LocationInfo lastData = Input.location.lastData;
        //if( lastData.horizontalAccuracy < 0.0f || lastData.horizontalAccuracy > 10.0f ) return false; // 誤差

        //GeoCoordinate currentCoordinate = new GeoCoordinate( lastData.latitude, lastData.longitude );

        if( LocationList.Count == 0 ) return false;
        Vector2 setData = GetLocation();
        GeoCoordinate currentCoordinate = new GeoCoordinate( setData.x, setData.y );
        float currentHeading = Input.compass.trueHeading;

        GeoCoordinate targetCoordinate = new GeoCoordinate( targetLatitude, targetLongitude );
        double distance = currentCoordinate.GetDistanceTo( targetCoordinate ); // 現在位置と目標位置の距離
        double bearing = CalculateBearing( currentCoordinate, targetCoordinate ); // 現在位置から目標位置の方角

        
        if( Mathf.Approximately( currentHeading, (float)bearing ) ) // tan(90)
        {
            targetTransform.position = new Vector3( 0, (float)targetAltitude, (float)distance );
            //return new Vector3( 0, (float)targetAltitude, (float)distance );
            
        }
        else if( Mathf.Approximately( currentHeading, (float)-bearing ) ) // tan(-90)
        {
            targetTransform.position = new Vector3( 0, (float)targetAltitude, (float)-distance );
            //return new Vector3( 0, (float)targetAltitude, (float)-distance );
        }
        else
        {
            double angleInRadian = ToRadian( bearing - currentHeading ); // 端末の方向から目標位置の方角
            targetTransform.position = new Vector3(
                (float)( Math.Sin( angleInRadian ) * distance ),
                (float)targetAltitude,
                (float)( Math.Cos( angleInRadian ) * distance )
            );
            //return new Vector3(
            //    (float)( Math.Sin( angleInRadian ) * distance ),
            //    (float)targetAltitude,
            //    (float)( Math.Cos( angleInRadian ) * distance )
            //);
        }

        targetTransform.position += cameraTransform.position;
        //targetTransform.position = Vector3.forward / 2.0f;

        return true;
    }

    private static double CalculateBearing( GeoCoordinate origin, GeoCoordinate target )
    {
        double φ1 = ToRadian( origin.Latitude );
        double φ2 = ToRadian( target.Latitude );
        double λ1 = ToRadian( origin.Longitude );
        double λ2 = ToRadian( target.Longitude );

        double y = Math.Sin( λ2 - λ1 ) * Math.Cos( φ2 );
        double x = Math.Cos( φ1 ) * Math.Sin( φ2 ) - Math.Sin( φ1 ) * Math.Cos( φ2 ) * Math.Cos( λ2 - λ1 );
        double θ = Math.Atan2( y, x );
        double bearing = ( ToDegree( θ ) + 360 ) % 360;

        return bearing;
    }

    private static double ToRadian( double degree )
    {
        return degree * Math.PI / 180;
    }

    private static double ToDegree( double radian )
    {
        return radian * 180 / Math.PI;
    }



    public const double GEO_R = 6378137.0;
    public const double orgX = -1 * ( 2 * GEO_R * Math.PI / 2 );
    public const double orgY = ( 2 * GEO_R * Math.PI / 2 );
    public static (double x, double y) Degrees2meters( double latitude, double Longitude )
    {
        var x = Longitude * 20037508.34 / 180.0;
        var y = Math.Log( Math.Tan( ( 90.0 + latitude ) * Math.PI / 360.0 ) ) / ( Math.PI / 180.0 );
        y = y * 20037508.34 / 180.0;
        return (x, y);
    }


}
