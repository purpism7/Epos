using UnityEngine;

using VContainer;
using VContainer.Unity;

using Battle;
using Common;
using Creature;
using GameSystem;

namespace Creator
{
    public class ProjectileCreator
    {
        [Inject] private IObjectResolver _iResolver = null;
        [Inject] private ObjectPooler _objectPooler = null;

        public Battle.IProjectile Create(GameObject projectilePrefab, Projectile.Param projectileParam, Quaternion rotation)
        {
            var projectile = _objectPooler?.Get<Projectile>(projectilePrefab);
            if (projectile == null)
            {
                var projectileGameObj = _iResolver?.Instantiate(projectilePrefab, projectileParam.StartPosition.Value, rotation);
                if (!projectileGameObj)
                    return null;
                
                projectile = projectileGameObj.GetComponent<Battle.Projectile>();
                if (projectile == null)
                    return null;

                _objectPooler?.Add(projectile);

                projectile?.InitializeAsync(projectileParam);
            }

            projectile?.ActivateAsync(projectileParam);

            return projectile;
        }
    }
}
