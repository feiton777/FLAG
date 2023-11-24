using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPSWaitManager : MonoBehaviour
{
	private void Start()
	{
#if UNITY_EDITOR
        gameObject.SetActive( false );
#endif
    }


	// Update is called once per frame
	void Update()
    {
        if( 0 < LocationManager.LocationList.Count )
        {
            gameObject.SetActive( false );
        }
    }
}
