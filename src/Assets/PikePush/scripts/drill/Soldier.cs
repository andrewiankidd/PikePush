using UnityEngine;

namespace PikePush.Drill
{
    public class Soldier : MonoBehaviour
    {
        [SerializeField] float positionLerpRate = 6f;
        [SerializeField] float rotationLerpRate = 6f;

        Block block;
        int rankIndex;
        int fileIndex;

        public void Configure(Block block, int rank, int file)
        {
            this.block = block;
            this.rankIndex = rank;
            this.fileIndex = file;
        }

        void Update()
        {
            if (block == null) return;

            Vector3 worldSlot = block.transform.TransformPoint(block.LocalSlot(rankIndex, fileIndex));
            transform.position = Vector3.Lerp(transform.position, worldSlot, Time.deltaTime * positionLerpRate);
            transform.rotation = Quaternion.Slerp(transform.rotation, block.transform.rotation, Time.deltaTime * rotationLerpRate);
        }
    }
}
