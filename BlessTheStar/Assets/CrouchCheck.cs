using UnityEngine;
using System.Collections;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof(RigidbodyFirstPersonController))]
    public class CrouchCheck : MonoBehaviour
    {
        private RigidbodyFirstPersonController RigidBodyFPSController;
        private CapsuleCollider capsuleCollider;
        private float height;
        private float distance;
        private int platLayer;
        

        // Use this for initialization
        void Start()
        {
            RigidBodyFPSController = GetComponent<RigidbodyFirstPersonController>();
            capsuleCollider = RigidBodyFPSController.GetComponent<CapsuleCollider>();

            distance = capsuleCollider.radius + 0.1f; // add a little distance so it doesn't clip through roof
            height = capsuleCollider.height;

            platLayer = LayerMask.NameToLayer("Platf");
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        private void FixedUpdate()
        {
            RaycastHit hit;

            //Bottom of controller. Slightly above ground so it doesn't bump into slanted platforms. (Adjust to your needs)
            Vector3 p1 = transform.position + Vector3.up * 0.1f;
            //Top of controller
            Vector3 p2 = p1 + Vector3.up * height; 

            //Check around the character in a 360, 10 times (increase if more accuracy is needed)
            for (int i = 0; i < 360; i += 36)
            {
                //Check if anything with the platform layer touches this object
                if (Physics.CapsuleCast(p1, p2, 0, new Vector3(Mathf.Cos(i), 0, Mathf.Sin(i)), out hit, distance, 1 << platLayer))
                {
                    //If the object is touched by a platform, move the object away from it
                   
               //     RigidBodyFPSController.GetComponent<Rigidbody>().MovePosition(new Vector3(hit.normal.x * (distance - hit.distance),0f,0f));
                    // hit.normal.y * (distance - hit.distance), hit.normal.z * (distance - hit.distance))
                    
                }
            }

            //[Optional] Check the players feet and push them up if something clips through their feet.
            //(Useful for vertical moving platforms)
            if (Physics.Raycast(transform.position + Vector3.up, -Vector3.up, out hit, 1, 1 << platLayer))
            {
                RigidBodyFPSController.GetComponent<Rigidbody>().MovePosition(Vector3.up * (1 - hit.distance));
            }
        }
    }

}
