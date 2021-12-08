using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace Enemies
{
    public class RagdollController : MonoBehaviour
    {
        [SerializeField] private Transform boneParentTransform;

        private List<Transform> bones;
    
        void Awake()
        {
            bones = boneParentTransform.GetComponentsInChildren<Transform>().ToList();
        }

        private void Start()
        {
        }

        public void CopyBoneTransform(Transform[] sourceBones)
        {
            foreach (var bone in bones)
            {
                var sameTransform = sourceBones.First(x => x.name == bone.name);
                bone.position = sameTransform.position;
                bone.rotation = sameTransform.rotation;
            }
        }

        public void AddForceAllBones(Vector3 force)
        {
            var boneRbs = bones.Where(x => x.GetComponent<Rigidbody>() != null).Select(x => x.GetComponent<Rigidbody>()).ToList();
        
            foreach (var rb in boneRbs)
            {
                rb.AddForce(force, ForceMode.Impulse);
            }
        }
    }
}
