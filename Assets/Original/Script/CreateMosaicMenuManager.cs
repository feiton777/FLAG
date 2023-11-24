using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateMosaicMenuManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        Test_MosaicManager.Instance.m_Image.color = Color.clear;
        Test_MosaicManager.Instance.m_ExplanationText.SetActive( true );
        Test_MosaicManager.Instance.m_LoadImageButton.SetActive( true );
        Test_MosaicManager.Instance.m_CreateFlagButton.SetActive( false );
    }
}
