using UnityEngine;

namespace SalvageSprint
{
    public sealed class SalvagePickup : MonoBehaviour
    {
        [SerializeField] private int id;
        public int Id => id;
        public void Configure(int value) { id = value; }
    }
}
