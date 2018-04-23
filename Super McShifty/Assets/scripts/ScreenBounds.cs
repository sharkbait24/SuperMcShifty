using UnityEngine;

/********************************************************************************************
 * Provides some useful functionality related to the visible screen space that are used by 
 * movement classes.
 * 
 * Copyright (c) 2018, Joe Coleman, All rights reserved.
 ********************************************************************************************/

namespace SuperMcShifty
{
    public class ScreenBounds
    {
        float halfWidth;                // Width of camera from center
        float halfHeight;               // Height of camera from center
        Vector3 center;                 // Current center position used (used to determine if camera moved)
        Transform camera;               // Position of camera this is defining bounds for

        float left;                     // Position.x of left edge of game screen
        float right;                    // Position.x of right edge
        float top;                      // Position.y of top edge
        float bottom;                   // Position.y of bottom edge

        public float Left
        {
            get { return left; }
        }

        public float Right
        {
            get { return right; }
        }

        public float Top
        {
            get { return top; }
        }

        public float Bottom
        {
            get { return bottom; }
        }

        /********************************************************************
         * Initialize on camera to construct bounds off of.
         * 
         * @param camera      Camera bounds are derived from
         ********************************************************************/
        public ScreenBounds(Camera camera)
        {
            ChangeCamera(camera);
        }

        /********************************************************************
         * Change camera focused on and rebuild the screen bounds of it.
         * 
         * @param camera      New camera to derive bounds from
         ********************************************************************/
        public void ChangeCamera(Camera camera)
        {
            halfHeight = camera.orthographicSize;
            halfWidth = camera.aspect * halfHeight;
            this.camera = camera.transform;
            UpdateBounds();
        }

        /********************************************************************
         * Check if camera has moved since last update, and updates bounds if so.
         ********************************************************************/
        public void CheckBounds()
        {
            if (!camera.position.Equals(center))
                UpdateBounds();
        }

        /********************************************************************
         * Returns if the position is within the camera frame.
         * 
         * @param position      Position to be checked
         * @return              If the position is in frame
         ********************************************************************/
        public bool IsInCameraFrame(Vector3 position)
        {
            if (position.x > right || position.x < left ||
                position.y > top || position.y < bottom)
                return false;
            return true;
        }

        /********************************************************************
         * Takes the current position and returns the position after wrap around
         * is applied.  i.e. If they go off the left side of the screen, make
         * them appear on the right at the same height.
         * 
         * @param   currentPosition     Current position of object
         * @return                      A new position with the wrap around applied
         ********************************************************************/
        public Vector3 PositionAfterWrapAround(Vector3 currentPosition)
        {
            Vector3 newPosition = currentPosition;

            if (currentPosition.x < left)
                newPosition.x = right - (left - currentPosition.x);
            else if (currentPosition.x > right)
                newPosition.x = left - (right - currentPosition.x);

            if (currentPosition.y < bottom)
                newPosition.y = top - (bottom - currentPosition.y);
            else if (currentPosition.y > top)
                newPosition.y = bottom - (top - currentPosition.y);

            return newPosition;
        }

        /********************************************************************
         * Update center, top, left, right and bottom values.
         ********************************************************************/
        private void UpdateBounds()
        {
            center = camera.transform.position;
            left = center.x - halfWidth;
            right = center.x + halfWidth;
            top = center.y + halfHeight;
            bottom = center.y - halfHeight;
        }
    }
}