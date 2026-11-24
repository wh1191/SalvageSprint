using UnityEngine;

namespace SalvageSprint
{
    public sealed class RaceActor : MonoBehaviour
    {
        [SerializeField] private MatchController match;
        [SerializeField] private int team;
        [SerializeField] private Transform depot;
        [SerializeField] private GameObject cargoVisual;
        public int Team => team;
        public Vector3 DepotPosition => depot.position;
        public bool Carrying => match != null && match.State != null && match.State.HasCargo(team);

        public void Configure(MatchController owner, int teamIndex, Transform home, GameObject carried)
        { match = owner; team = teamIndex; depot = home; cargoVisual = carried; }

        private void LateUpdate()
        { if (cargoVisual != null) cargoVisual.SetActive(Carrying); }
    }
}
