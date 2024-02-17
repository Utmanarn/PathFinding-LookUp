using UnityEngine;

namespace BoardGame {
    public abstract class TileParent : MonoBehaviour {
        private const uint 
            Blocked = 1u,
            Obstacle = 2u,
            CheckPoint = 4u,
            Portal = 8u,
            StartPoint = 16u;
        
        [SerializeField, HideInInspector] internal Vector2Int coordinate;
        [SerializeField, HideInInspector] internal uint bitmask;
        [SerializeField, HideInInspector] internal Vector2Int portalTarget;
        [SerializeField, HideInInspector] internal int movementPenalty;
        
        /// <summary>
        /// Returns the (discrete) coordinate of this tile.
        /// </summary>
        public Vector2Int Coordinate => coordinate;
        
        /// <summary>
        /// Returns true if this tile has the 'blocked' property set.
        /// </summary>
        /// <returns>true if 'blocked'</returns>
        public bool IsBlocked  => (bitmask & Blocked) > 0u;
        
        /// <summary>
        /// Returns true if this tile has the 'checkpoint' property set.
        /// </summary>
        /// <returns>true if 'checkpoint'</returns>
        public bool IsCheckPoint => (bitmask & CheckPoint) > 0u;
        
        /// <summary>
        /// Returns true if this tile has the 'start point' property set.
        /// </summary>
        /// <returns>true if 'start point'</returns>
        public bool IsStartPoint => (bitmask & StartPoint) > 0u;

        /// <summary>
        /// Returns true if this tile has the 'portal' property set, and if so
        /// also returns the portal target.
        /// </summary>
        /// <param name="target">the target coordinate for the portal</param>
        /// <returns>true if 'checkpoint'</returns>
        public bool IsPortal(out Vector2Int target) {
            if ((bitmask & Portal) > 0u) {
                target = portalTarget;
                return true;
            }

            target = default;
            return false;
        }
        
        /// <summary>
        /// Returns true if this tile has the 'obstacle' property set, and if so
        /// also returns the movement penalty.
        /// </summary>
        /// <param name="penalty">the movement penalty for the obstacle</param>
        /// <returns>true if 'obstacle'</returns>
        public bool IsObstacle(out int penalty) {
            if ((bitmask & Obstacle) > 0u) {
                penalty = movementPenalty;
                return true;
            }

            penalty = default;
            return false;
        }

        public void SetBlocked(bool blocked) {
            if (blocked) bitmask |= Blocked;
            else bitmask &= ~Blocked;
        }
        
        public void SetStartPoint(bool startPoint) {
            if (startPoint) bitmask |= StartPoint;
            else bitmask &= ~StartPoint;
        }
        
        public void SetCheckPoint(bool checkPoint) {
            if (checkPoint) bitmask |= CheckPoint;
            else bitmask &= ~CheckPoint;
        }
        
        public void AddObstacle(int penalty) {
            bitmask |= Obstacle;
            movementPenalty = penalty;
        }
        
        public void AddPortal(Vector2Int target) {
            bitmask |= Portal;
            portalTarget = target;
        }

        public void RemoveObstacle() => bitmask &= ~Obstacle;
        
        public void RemovePortal() => bitmask &= ~Portal;

        /// <summary>
        /// Executed after all tiles has been created.
        /// </summary>
        /// <param name="board">the board</param>
        public abstract void OnSetup(Board board);

        /// <summary>
        /// Called during the update step but also passes a reference to the
        /// board.
        /// </summary>
        /// <param name="board">the board</param>
        public abstract void OnUpdate(Board board);
        
        /// <summary>
        /// Copies everything from the specified tile into this.
        /// </summary>
        /// <param name="other">the tile to copy from</param>
        internal void CopyFrom(TileParent other) {
            coordinate      = other.coordinate;
            bitmask         = other.bitmask;
            portalTarget    = other.portalTarget;
            movementPenalty = other.movementPenalty;
        }
    }
}