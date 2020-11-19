using UnityEngine;

namespace Lights
{
    internal class MyLight
    {
        internal float range;
        internal float intensity;
        internal float bounceIntensity;
        internal Color color;

        public MyLight(Light light)
        {
            color = light.color;
            range = light.range;
            intensity = light.intensity;
            bounceIntensity = light.bounceIntensity;
            color = light.color;
        }
    }
}