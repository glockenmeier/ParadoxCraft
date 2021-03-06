﻿// <auto-generated>
// Do not edit this file yourself!
//
// This code was generated by Paradox Shader Mixin Code Generator.
// To generate it yourself, please install SiliconStudio.Paradox.VisualStudio.Package .vsix
// and re-save the associated .pdxfx.
// </auto-generated>

using System;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Shaders;
using SiliconStudio.Core.Mathematics;
using Buffer = SiliconStudio.Paradox.Graphics.Buffer;

namespace SiliconStudio.Paradox.Effects
{
    public static partial class FogEffectKeys
    {
        public static readonly ParameterKey<Color4> FogColor = ParameterKeys.New<Color4>(new Color4(0,0,0,0));
        public static readonly ParameterKey<float> fogNearPlaneZ = ParameterKeys.New<float>(75.0f);
        public static readonly ParameterKey<float> fogFarPlaneZ = ParameterKeys.New<float>(275.0f);
        public static readonly ParameterKey<float> fogNearPlaneY = ParameterKeys.New<float>(0.0f);
        public static readonly ParameterKey<float> fogFarPlaneY = ParameterKeys.New<float>(20000.0f);
    }
}
