using UnityEngine;

namespace ECS.UI
{
    public class BuildingScreenSingleton : MonoBehaviour
    {
        [SerializeField] private BuildingScreenPresenter defaultInstance;
        
        public static BuildingScreenPresenter Instance;

        private void Awake()
        {
            Instance = defaultInstance;
        }
    }
}