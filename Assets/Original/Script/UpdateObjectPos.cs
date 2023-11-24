using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;
using System;
using System.Device.Location;

public class UpdateObjectPos : MonoBehaviour
{
    public float Latitude { set; get; } = 35.174223f; // ˆÜ“x
    public float Longitude { set; get; } = 136.859547f; // Œo“x

    // Start is called before the first frame update
    void Start()
    {
        UpdatePosAsync().Forget();
    }

    async UniTask<int> UpdatePosAsync()
	{
        while(!LocationManager.UpdateObjectPos( Camera.main.transform, gameObject.transform, Latitude, Longitude, 1.0f ) )
		{
            await UniTask.Yield( PlayerLoopTiming.Update );
        }

        return 0;
	}
}
