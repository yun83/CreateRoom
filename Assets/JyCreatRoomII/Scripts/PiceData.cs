using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JyModule
{
    public class PiceData : MonoBehaviour
    {
        public Material mate;
        public Rigidbody rgd;
        public BoxCollider col;

        public GameObject DrawObject;

        // Start is called before the first frame update
        void Start()
        {
            rgd = gameObject.AddComponent<Rigidbody>();
            rgd.useGravity = false;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Instantiate_Material()
        {
            if (DrawObject == null)
                return;

            mate = Instantiate(DrawObject.GetComponent<MeshRenderer>().material);
            DrawObject.GetComponent<MeshRenderer>().material = mate;
        }
    }
}
