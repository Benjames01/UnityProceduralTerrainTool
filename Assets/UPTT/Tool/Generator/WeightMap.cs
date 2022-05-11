using UnityEngine;

namespace UPTT.Tool.Generator
{
    [System.Serializable]
    public class WeightMap : IDeletable
    {
        [SerializeField]
        private Texture2D diffuseTexture;
        [SerializeField]
        private Texture2D normalsTexture;
        
        [SerializeField] private Vector2 heights = new Vector2(0, 0.2f); // Range of heights, min:0 max:1
        [SerializeField] private Vector2 gradients = new Vector2(0, 90); // Range of gradient min: 0 max:90

        
        [SerializeField]
        private float metallic; 
        [SerializeField]
        private float smoothness;
        [SerializeField]
        private Color specular; 

        [SerializeField] private Vector2 offset = new Vector2(0, 0); // Offset for texture starting point
        [SerializeField] private Vector2 size = new Vector2(25, 25); // How large the tilesize is
        public bool canDelete = false;
        public bool ToRemove
        {
            get => canDelete;
            set => canDelete = value;
        }

        public Texture2D DiffuseTexture => diffuseTexture;

        public float Metallic => metallic;

        public float Smoothness => smoothness;

        public Color Specular => specular;

        public Vector2 Offset => offset;

        public Texture2D NormalsTexture => normalsTexture;

        public float MINWeight => heights.x;

        public float MAXWeight => heights.y;
        
        public float MINGradient => gradients.x;

        public float MAXGradient => gradients.y;

        public Vector2 Size
        {
            get => size;
            set => size = value;
        }
    }
}
