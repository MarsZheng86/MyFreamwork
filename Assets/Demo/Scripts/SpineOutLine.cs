using System.Collections.Generic;
using UnityEngine;

public class SpineOutLine : MonoBehaviour {
    #region Public Member Variable
    public float m_fMaterialAPos;
    public float m_fMaterialBPos;
    public Material m_materialA;
    public Material m_materialB;
    #endregion

    #region Private Member Variable
    private Mesh m_mesh;
    private MeshFilter m_meshFilter;
    private MeshRenderer m_meshRenderer;
    private List<Matrix4x4> m_listM4A;
    private List<Matrix4x4> m_listM4B;
    private GameObject m_goOutLineA;
    private MeshFilter m_meshFilterA;
    private MeshRenderer m_meshRendererA;
    private GameObject m_goOutLineB;
    private MeshFilter m_meshFilterB;
    private MeshRenderer m_meshRendererB;
    private int m_iSortingOrder;
    private bool m_bLoop = false;
    private Color m_cDefaultColor = new Color(1, 1, 1, 1);
    private bool m_bRefresh = false;
    #endregion

    #region Inherit Function

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (false == m_bLoop)
        {
            return;
        }

        if (false == m_bRefresh)
        {
            return;
        }

        m_meshFilter = gameObject.GetComponent<MeshFilter>();
        m_mesh = m_meshFilter.mesh;
        m_meshRenderer = gameObject.GetComponent<MeshRenderer>();
        m_iSortingOrder = m_meshRenderer.sortingOrder;
        RefreshListA();
        if (m_meshRendererA.sortingOrder != m_iSortingOrder)
        {
            m_meshRendererA.sortingOrder = m_iSortingOrder;
        }


        RefreshListB();

        if (m_meshRendererB.sortingOrder != m_iSortingOrder)
        {
            m_meshRendererB.sortingOrder = m_iSortingOrder;
        }

        m_bRefresh = false;
    }

    private void OnDestroy()
    {
        if (null != m_listM4A)
        {
            m_listM4A.Clear();
            m_listM4A = null;
        }

        if (null != m_listM4B)
        {
            m_listM4B.Clear();
            m_listM4B = null;
        }

        m_materialA = null;
        m_materialB = null;
        m_mesh = null;
        m_meshFilter = null;
        m_meshRenderer = null;

        if (null != m_goOutLineA)
        {
            UnityEngine.Object.DestroyImmediate(m_goOutLineA);
        }

        if (null != m_goOutLineB)
        {
            UnityEngine.Object.DestroyImmediate(m_goOutLineB);
        }

        m_meshFilterA = null;
        m_meshRendererA = null;
        m_meshFilterB = null;
        m_meshRendererB = null;
    }
    #endregion

    #region Private Function
    private void RefreshListA() {
        m_listM4A.Clear();

        Matrix4x4 _mA1 = Matrix4x4.identity;
        _mA1.m03 = - m_fMaterialAPos;
        _mA1.m13 = 0;
        _mA1.m23 = 0;

        Matrix4x4 _mA2 = Matrix4x4.identity;
        _mA2.m03 =   + m_fMaterialAPos;
        _mA2.m13 = 0;
        _mA2.m23 = 0;

        Matrix4x4 _mA3 = Matrix4x4.identity;
        _mA3.m03 = 0;
        _mA3.m13 = 0 + m_fMaterialAPos;
        _mA3.m23 = 0;

        Matrix4x4 _mA4 = Matrix4x4.identity;
        _mA4.m03 = 0;
        _mA4.m13 = 0 - m_fMaterialAPos;
        _mA4.m23 = 0;

        Matrix4x4 _mA5 = Matrix4x4.identity;
        _mA5.m03 = - m_fMaterialAPos / 2;
        _mA5.m13 = 0 + m_fMaterialAPos;
        _mA5.m23 = 0;

        Matrix4x4 _mA6 = Matrix4x4.identity;
        _mA6.m03 = - m_fMaterialAPos;
        _mA6.m13 = 0 + m_fMaterialAPos / 2;
        _mA6.m23 = 0;

        Matrix4x4 _mA7 = Matrix4x4.identity;
        _mA7.m03 = + m_fMaterialAPos / 2;
        _mA7.m13 = 0 + m_fMaterialAPos;
        _mA7.m23 = 0;

        Matrix4x4 _mA8 = Matrix4x4.identity;
        _mA8.m03 = + m_fMaterialAPos;
        _mA8.m13 = 0 + m_fMaterialAPos / 2;
        _mA8.m23 = 0;

        Matrix4x4 _mA9 = Matrix4x4.identity;
        _mA9.m03 = + m_fMaterialAPos / 2;
        _mA9.m13 = 0 - m_fMaterialAPos;
        _mA9.m23 = 0;

        Matrix4x4 _mA10 = Matrix4x4.identity;
        _mA10.m03 = + m_fMaterialAPos;
        _mA10.m13 = 0 - m_fMaterialAPos / 2;
        _mA10.m23 = 0;

        Matrix4x4 _mA11 = Matrix4x4.identity;
        _mA11.m03 = - m_fMaterialAPos / 2;
        _mA11.m13 = 0 - m_fMaterialAPos;
        _mA11.m23 = 0;

        Matrix4x4 _mA12 = Matrix4x4.identity;
        _mA12.m03 = - m_fMaterialAPos;
        _mA12.m13 = 0 - m_fMaterialAPos / 2;
        _mA12.m23 = 0; 

        m_listM4A.Add(_mA1);
        m_listM4A.Add(_mA2);
        m_listM4A.Add(_mA3);
        m_listM4A.Add(_mA4);
        m_listM4A.Add(_mA5);
        m_listM4A.Add(_mA6);
        m_listM4A.Add(_mA7);
        m_listM4A.Add(_mA8);
        m_listM4A.Add(_mA9);
        m_listM4A.Add(_mA10);
        m_listM4A.Add(_mA11);
        m_listM4A.Add(_mA12);

        
        CombineInstance[] _combine = new CombineInstance[12];
        for (int i = 0; i < 12; i++)
        {
            _combine[i].mesh = m_mesh;
            _combine[i].transform = m_listM4A[i];
        }

        m_meshFilterA.sharedMesh = new Mesh();
        m_meshFilterA.sharedMesh.CombineMeshes(_combine);
        m_meshRendererA.sharedMaterial = m_materialB;
    }

    private void RefreshListB() {
        m_listM4B.Clear();

        Matrix4x4 _mB1 = Matrix4x4.identity;
        _mB1.m03 = - m_fMaterialBPos;
        _mB1.m13 = 0;
        _mB1.m23 = 0;

        Matrix4x4 _mB2 = Matrix4x4.identity;
        _mB2.m03 = + m_fMaterialBPos;
        _mB2.m13 = 0;
        _mB2.m23 = 0;

        Matrix4x4 _mB3 = Matrix4x4.identity;
        _mB3.m03 = 0;
        _mB3.m13 = 0 + m_fMaterialBPos;
        _mB3.m23 = 0;

        Matrix4x4 _mB4 = Matrix4x4.identity;
        _mB4.m03 = 0;
        _mB4.m13 = 0 - m_fMaterialBPos;
        _mB4.m23 = 0;

        Matrix4x4 _mB5 = Matrix4x4.identity;
        _mB5.m03 = - m_fMaterialBPos;
        _mB5.m13 = 0 + m_fMaterialBPos / 3;
        _mB5.m23 = 0;

        Matrix4x4 _mB6 = Matrix4x4.identity;
        _mB6.m03 = -m_fMaterialBPos;
        _mB6.m13 = 0 + (m_fMaterialBPos / 3 * 2);
        _mB6.m23 = 0;

        Matrix4x4 _mB7 = Matrix4x4.identity;
        _mB7.m03 = - m_fMaterialBPos / 3;
        _mB7.m13 = 0 + m_fMaterialBPos;
        _mB7.m23 = 0;

        Matrix4x4 _mB8 = Matrix4x4.identity;
        _mB8.m03 = -(m_fMaterialBPos / 3 * 2);
        _mB8.m13 = 0 + m_fMaterialBPos;
        _mB8.m23 = 0;

        Matrix4x4 _mB9 = Matrix4x4.identity;
        _mB9.m03 = + m_fMaterialBPos;
        _mB9.m13 = 0 + m_fMaterialBPos / 3;
        _mB9.m23 = 0;

        Matrix4x4 _mB10 = Matrix4x4.identity;
        _mB10.m03 = +m_fMaterialBPos;
        _mB10.m13 = 0 + (m_fMaterialBPos / 3 * 2);
        _mB10.m23 = 0;

        Matrix4x4 _mB11 = Matrix4x4.identity;
        _mB11.m03 = + m_fMaterialBPos / 3;
        _mB11.m13 = 0 + m_fMaterialBPos;
        _mB11.m23 = 0;

        Matrix4x4 _mB12 = Matrix4x4.identity;
        _mB12.m03 = +(m_fMaterialBPos / 3 * 2);
        _mB12.m13 = 0 + m_fMaterialBPos;
        _mB12.m23 = 0;

        Matrix4x4 _mB13 = Matrix4x4.identity;
        _mB13.m03 = + m_fMaterialBPos / 3;
        _mB13.m13 = 0 - m_fMaterialBPos;
        _mB13.m23 = 0;

        Matrix4x4 _mB14 = Matrix4x4.identity;
        _mB14.m03 = +m_fMaterialBPos / 3 * 2;
        _mB14.m13 = 0 - m_fMaterialBPos;
        _mB14.m23 = 0;

        Matrix4x4 _mB15 = Matrix4x4.identity;
        _mB15.m03 = + m_fMaterialBPos;
        _mB15.m13 = 0 - m_fMaterialBPos / 3;
        _mB15.m23 = 0;

        Matrix4x4 _mB16 = Matrix4x4.identity;
        _mB16.m03 = +m_fMaterialBPos;
        _mB16.m13 = 0 - (m_fMaterialBPos / 3 * 2);
        _mB16.m23 = 0;

        Matrix4x4 _mB17 = Matrix4x4.identity;
        _mB17.m03 = - m_fMaterialBPos / 3;
        _mB17.m13 = 0 - m_fMaterialBPos;
        _mB17.m23 = 0;

        Matrix4x4 _mB18 = Matrix4x4.identity;
        _mB18.m03 = -m_fMaterialBPos / 3 * 2;
        _mB18.m13 = 0 - m_fMaterialBPos;
        _mB18.m23 = 0;

        Matrix4x4 _mB19 = Matrix4x4.identity;
        _mB19.m03 = - m_fMaterialBPos;
        _mB19.m13 = 0 - m_fMaterialBPos / 3;
        _mB19.m23 = 0;

        Matrix4x4 _mB20 = Matrix4x4.identity;
        _mB20.m03 = -m_fMaterialBPos;
        _mB20.m13 = 0 - m_fMaterialBPos / 3 * 2;
        _mB20.m23 = 0;

        m_listM4B.Add(_mB1);
        m_listM4B.Add(_mB2);
        m_listM4B.Add(_mB3);
        m_listM4B.Add(_mB4);
        m_listM4B.Add(_mB5);
        m_listM4B.Add(_mB6);
        m_listM4B.Add(_mB7);
        m_listM4B.Add(_mB8);
        m_listM4B.Add(_mB9);
        m_listM4B.Add(_mB10);
        m_listM4B.Add(_mB11);
        m_listM4B.Add(_mB12);
        m_listM4B.Add(_mB13);
        m_listM4B.Add(_mB14);
        m_listM4B.Add(_mB15);
        m_listM4B.Add(_mB16);
        m_listM4B.Add(_mB17);
        m_listM4B.Add(_mB18);
        m_listM4B.Add(_mB19);
        m_listM4B.Add(_mB20);

        CombineInstance[] _combine = new CombineInstance[20];
        for (int i = 0; i < 20; i++)
        {
            _combine[i].mesh = m_mesh;
            _combine[i].transform = m_listM4B[i];
        }

        m_meshFilterB.sharedMesh = new Mesh();
        m_meshFilterB.sharedMesh.CombineMeshes(_combine);
        m_meshRendererB.sharedMaterial = m_materialA;
    }
    #endregion

    #region Public Function
    public void ShowOutLine(bool bShow, Color colorA, Color colorB) {
        m_materialA.color = colorA;
        m_materialB.color = colorB;

        if (null == m_meshFilter)
        {
            m_meshFilter = gameObject.GetComponent<MeshFilter>();
        }

        if (null == m_mesh)
        {
            m_mesh = m_meshFilter.mesh;
        }

        if (null == m_meshRenderer)
        {
            m_meshRenderer = gameObject.GetComponent<MeshRenderer>();
        }

        if (m_listM4A == null)
        {
            m_listM4A = new List<Matrix4x4>();
        }

        if (m_listM4B == null)
        {
            m_listM4B = new List<Matrix4x4>();
        }

        if (null == m_goOutLineA)
        {
            m_goOutLineA = new GameObject("outlineA");
            m_goOutLineA.transform.parent = transform;
            m_goOutLineA.transform.localPosition = new Vector3(0, 0, 0.1f); ;
            m_goOutLineA.transform.localScale = Vector3.one;
            if (null == m_meshFilterA)
            {
                m_meshFilterA = m_goOutLineA.AddComponent<MeshFilter>();
            }

            if (null == m_meshRendererA)
            {
                m_meshRendererA = m_goOutLineA.AddComponent<MeshRenderer>();
                m_meshRendererA.sortingLayerName = m_meshRenderer.sortingLayerName;
            }
            
            m_goOutLineA.layer = LayerMask.NameToLayer("map");
        }

        if (null == m_goOutLineB)
        {
            m_goOutLineB = new GameObject("outlineB");
            m_goOutLineB.transform.parent = transform;
            m_goOutLineB.transform.localPosition = new Vector3(0, 0, 0.2f);
            m_goOutLineB.transform.localScale = Vector3.one;
            if (null == m_meshFilterB)
            {
                m_meshFilterB = m_goOutLineB.AddComponent<MeshFilter>();
            }

            if (null == m_meshRendererB)
            {
                m_meshRendererB = m_goOutLineB.AddComponent<MeshRenderer>();
                m_meshRendererB.sortingLayerName = m_meshRenderer.sortingLayerName;
            }

            m_goOutLineB.layer = LayerMask.NameToLayer("map");
        }

        //m_iSortingOrder = m_meshRenderer.sortingOrder;
        //RefreshListA();
        //if (m_meshRendererA.sortingOrder != m_iSortingOrder)
        //{
        //    m_meshRendererA.sortingOrder = m_iSortingOrder;
        //}
        

        //RefreshListB();

        //if (m_meshRendererB.sortingOrder != m_iSortingOrder)
        //{
        //    m_meshRendererB.sortingOrder = m_iSortingOrder;
        //}

        m_goOutLineA.SetActive(bShow);
        m_goOutLineB.SetActive(bShow);

        m_bLoop = bShow;
        m_bRefresh = true;

        if (bShow == false)
        {
            m_materialA.color = m_cDefaultColor;
            m_materialB.color = m_cDefaultColor;
        }
    }

    public void RefreshSortingOrder()
    {
        m_meshRenderer = gameObject.GetComponent<MeshRenderer>();
        m_iSortingOrder = m_meshRenderer.sortingOrder;

        if (m_meshRendererA.sortingOrder != m_iSortingOrder)
        {
            m_meshRendererA.sortingOrder = m_iSortingOrder;
        }

        if (m_meshRendererB.sortingOrder != m_iSortingOrder)
        {
            m_meshRendererB.sortingOrder = m_iSortingOrder;
        }
    }
    #endregion
}
