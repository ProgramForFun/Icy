using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kamgam.UGUIWorldImage
{
    public interface IPrefabInstantiatorForWorldImagePrefabSource
    {
        List<PrefabInstantiatorForWorldImage.PrefabHandle> GetPrefabHandles();
    }
}