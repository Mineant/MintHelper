using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant
{
    public class ExampleMVCController : MVCController<MVCWeapon>
    {
        public void AddAmmoClick()
        {

        }

        public void ReduceAmmoClick()
        {
            
        }

        public void SetAmmo(int change)
        {

        }

        public override void ListenToData()
        {
            _data.OnChange += UpdateView;
        }
    }

    public class MVCWeapon
    {
        public int Ammo;
        public Action OnChange;

        public void SetAmmo(int ammo)
        {
            Ammo = ammo;
            OnChange?.Invoke();
        }
    }
}