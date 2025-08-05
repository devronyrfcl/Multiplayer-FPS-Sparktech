using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ForceCodeFPS
{
    public class r_BulletTrailFade : MonoBehaviour
    {
        #region Public variables
        [Header("Bullet Trail Settings")]
        [SerializeField] private Color m_Color;
        [SerializeField] private float m_Speed = 10f;
        #endregion

        #region Private variables
        [HideInInspector] public LineRenderer m_LineRenderer;
        #endregion

        #region Functions
        private void Start() => this.m_LineRenderer = this.gameObject.GetComponent<LineRenderer>();

        private void Update() => HandleBulletTrail();
        #endregion

        #region Handling
        private void HandleBulletTrail()
        {
            // move towards zero
            this.m_Color.a = Mathf.Lerp(this.m_Color.a, 0, Time.deltaTime * this.m_Speed);

            // update color
            this.m_LineRenderer.startColor = this.m_Color;
            this.m_LineRenderer.endColor = this.m_Color;
        }
        #endregion
    }
}