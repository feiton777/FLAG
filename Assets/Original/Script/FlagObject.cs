using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class FlagObject : MonoBehaviour
{
    [SerializeField]
    MeshRenderer m_FlagRenderer;

    public string HASH { private set; get; } = "";
    public string MOSAIC_ID { private set; get; } = "";
    public Vector2 LOCATION { private set; get; } = Vector2.zero;
    public string MESSAGE { private set; get; } = "";

    public async UniTask<int> SetData( string mosaicId, string setMessage )
    {
        MESSAGE = setMessage;
        await Test_MosaicManager.LoadMosaicAsync( mosaicId, m_FlagRenderer );
        return 0;
    }
}
