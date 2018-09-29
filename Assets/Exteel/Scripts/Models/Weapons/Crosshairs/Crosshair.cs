using UnityEngine;
using Weapons.Crosshairs.Data;

namespace Weapons.Crosshairs {
    public abstract class Crosshair {
        protected CrosshairData CrosshairData;
        protected GameObject crosshair, redCrosshair;
        protected RectTransform crosshairRect, crosshairRedRect;

        protected Camera Cam;

        protected float Radius, OrgRadius, RadiusCoeff = 22, ShakingAmount = 1.33f;
        protected bool IsShaking = false;

        public virtual void Init(Transform parent, Camera Cam, int hand) {
            this.Cam = Cam;

            GetCrosshairData();

            if (CrosshairData.CrosshairPrefab != null) {
                crosshair = Object.Instantiate(CrosshairData.CrosshairPrefab, parent);
                crosshairRect = crosshair.GetComponent<RectTransform>();
            }

            if (CrosshairData.RedCrosshairPrefab != null) {
                redCrosshair = Object.Instantiate(CrosshairData.RedCrosshairPrefab, parent);
                crosshairRedRect = redCrosshair.GetComponent<RectTransform>();
            }
        }

        protected abstract void GetCrosshairData();

        public virtual void OnShootAction() {
        }

        public virtual void Update() {
        }

        public virtual void SetRadius(float _radius) {
        }

        protected virtual void SetOffset(float _radius) {
        }

        public virtual void OnTarget(bool onTarget) {
            crosshair.SetActive(!onTarget);
            redCrosshair.SetActive(onTarget);
        }

        public virtual void EnableCrosshair(bool b) {
            crosshair.SetActive(b);
            redCrosshair.SetActive(false);
        }

        public virtual void ShakingEffect() {
            IsShaking = true;
        }

        public virtual void Destroy() {
            if (crosshair != null) Object.Destroy(crosshair);
            if (redCrosshair != null) Object.Destroy(redCrosshair);
        }

        public abstract void MarkTarget(Transform target);
    }
}