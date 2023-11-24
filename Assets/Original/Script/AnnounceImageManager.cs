using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnounceImageManager : MonoBehaviour
{
    [SerializeField]
    TMPro.TMP_Text m_AnnounceText;


    public static AnnounceImageManager Instance = null;

    private void Awake()
    {
        if( Instance == null )
        {
            Instance = this;
            Instance.gameObject.SetActive( false );
        }
        else
        {
            Destroy( gameObject );
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void CloseAnnounceImage()
	{
        this.gameObject.SetActive( false );
    }

    public void OpenAnnounceImage( string announce )
    {
        m_AnnounceText.text = announce;
        this.gameObject.SetActive( true );
    }
}
