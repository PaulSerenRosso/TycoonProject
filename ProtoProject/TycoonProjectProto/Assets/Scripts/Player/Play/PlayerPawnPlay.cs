using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerPawnPlay : PlayerPawn
    {
        [SerializeField] private Car carPrefab;
        protected Car car;

        protected override void Start()
        {
            base.Start();
            car = Instantiate<Car>(carPrefab, transform.position, transform.rotation);
        }
    }
}