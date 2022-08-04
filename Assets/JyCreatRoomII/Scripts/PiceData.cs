using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JyModule
{
    public class PiceData : MonoBehaviour
    {
        public GameObject DrawObject;
        public Color OriColor;
        
        public Material mate;
        public Rigidbody rgd;
        public BoxCollider col;
        public MeshRenderer mrder;

        // Start is called before the first frame update
        void Start()
        {
            if (gameObject.TryGetComponent(out Rigidbody _rigd))
            {
                rgd = _rigd;
            }
            else
            {
                rgd = gameObject.AddComponent<Rigidbody>();
            }
            rgd.useGravity = false;

            if (gameObject.TryGetComponent(out BoxCollider _col))
            {
                col = _col;
            }

            if(gameObject.TryGetComponent(out MeshRenderer _mr))
            {
                mrder = _mr;
            }
        }

        // Update is called once per frame
        public void Instantiate_Material(Material _mat)
        {
            if (DrawObject == null)
                return;

            mate = Instantiate(_mat);
            gameObject.GetComponent<MeshRenderer>().material = mate;

            OriColor = mate.color;
        }

        public void ChangeColor(Color _color)
        {
            mate.color = _color;
        }

        public void ResetColor()
        {
            mate.color = OriColor;
        }

        public void UpMouse()
        {
            ResetColor();
            mrder.enabled = false;
        }

        public void DownMouse()
        {
             mrder.enabled = true;
        }

        public void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.name + " Object Trigger Enter");
        }
    }
}
