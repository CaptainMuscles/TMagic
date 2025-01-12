﻿using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using RimWorld;

namespace TorannMagic
{
    public class TM_WeatherEvent_MeshGeneric : WeatherEvent
    {
        private int age = 0;
        private int durationSolid;
        private int fadeOutTicks = 20;
        private int fadeInTicks = 10;

        private float angle = 0;
        private int boltMaxCount = 25;
        private float meshContortionMagnitude = 12f;

        private Vector2 shadowVector; //currently unused
        private Vector3 meshStart = default(Vector3);
        private Vector3 meshEnd = default(Vector3);
        private bool startIsVec = false;
        private Mesh boltMesh = null;
        private static List<Mesh> boltMeshes = new List<Mesh>();
        private static readonly SkyColorSet MeshSkyColors = new SkyColorSet(new Color(0.9f, 0.95f, 1f), new Color(0.784313738f, 0.8235294f, 0.847058833f), new Color(0.9f, 0.95f, 1f), 1.15f);
        private Material meshMat;
        private AltitudeLayer altitudeLayer;

        public override bool Expired
        {
            get
            {
                return this.age > (this.durationSolid + fadeOutTicks);
            }
        }

        public TM_WeatherEvent_MeshGeneric(Map map, Material meshMat, IntVec3 meshStart, IntVec3 meshEnd, float meshContortionMagnitude, AltitudeLayer altitudeLayer, int durationSolid, int fadeOutTicks, int fadeInTicks) : base(map)
        {
            this.map = map;
            this.meshMat = meshMat;
            this.meshStart = meshStart.ToVector3ShiftedWithAltitude(altitudeLayer);
            this.meshEnd = meshEnd.ToVector3ShiftedWithAltitude(altitudeLayer);
            this.meshContortionMagnitude = meshContortionMagnitude;
            this.altitudeLayer = altitudeLayer;
            this.durationSolid = durationSolid;
            this.fadeOutTicks = fadeOutTicks;
            this.fadeInTicks = fadeInTicks;
            //this.shadowVector = new Vector2(Rand.Range(-5f, 5f), Rand.Range(-5f, 0f));
        }

        public TM_WeatherEvent_MeshGeneric(Map map, Material meshMat, Vector3 meshStart, Vector3 meshEnd, float meshContortionMagnitude, AltitudeLayer altitudeLayer, int durationSolid, int fadeOutTicks, int fadeInTicks) : base(map)
        {
            this.map = map;
            this.meshMat = meshMat;
            this.meshStart = meshStart;
            this.meshEnd = meshEnd;
            this.meshContortionMagnitude = meshContortionMagnitude;
            this.altitudeLayer = altitudeLayer;
            this.durationSolid = durationSolid;
            this.fadeOutTicks = fadeOutTicks;
            this.fadeInTicks = fadeInTicks;
            //this.shadowVector = new Vector2(Rand.Range(-5f, 5f), Rand.Range(-5f, 0f));
        }

        public override void FireEvent()
        {
            if (this.meshStart != default(Vector3))
            {
                GetVector(this.meshStart, this.meshEnd);
            }
            this.boltMesh = this.RandomBoltMesh;
        }

        public override void WeatherEventDraw()
        {
            if (this.meshStart != default(Vector3))
            {
                Graphics.DrawMesh(this.boltMesh, this.meshStart, Quaternion.Euler(0f, this.angle, 0f), FadedMaterialPool.FadedVersionOf(this.meshMat, this.MeshBrightness), 0);
            }
            else
            {
                Graphics.DrawMesh(this.boltMesh, this.meshEnd, Quaternion.identity, FadedMaterialPool.FadedVersionOf(this.meshMat, this.MeshBrightness), 0);
            }
        }

        public override void WeatherEventTick()
        {
            this.age++;
        }

        public Mesh RandomBoltMesh
        {
            get
            {
                Mesh result;
                if (TM_WeatherEvent_MeshGeneric.boltMeshes.Count < this.boltMaxCount)
                {
                    Mesh mesh;
                    if (this.meshStart != default(Vector3))
                    {
                        mesh = TM_MeshMaker.NewBoltMesh(Vector3.Distance(this.meshStart, this.meshEnd), this.meshContortionMagnitude);
                    }
                    else
                    {
                        mesh = TM_MeshMaker.NewBoltMesh(200, this.meshContortionMagnitude);
                    }
                    TM_WeatherEvent_MeshGeneric.boltMeshes.Add(mesh);
                    result = mesh;
                }
                else
                {
                    result = TM_WeatherEvent_MeshGeneric.boltMeshes.RandomElement<Mesh>();
                }
                return result;
            }
        }

        protected float MeshBrightness
        {
            get
            {
                float result;
                if (this.age <= this.fadeInTicks)
                {
                    result = (float)this.age / this.fadeInTicks;
                }
                else if (this.age < durationSolid)
                {
                    result = 1f;
                }
                else
                {
                    result = 1f - (float)(this.age - this.durationSolid) / (float)this.fadeOutTicks;
                }
                return result;
            }
        }

        public Vector3 GetVector(Vector3 start, Vector3 end)
        {
            Vector3 heading = (end - start);
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;
            this.angle = (Quaternion.AngleAxis(90, Vector3.up) * direction).ToAngleFlat();
            return direction;
        }

        public override SkyTarget SkyTarget
        {
            get
            {
                return new SkyTarget(1f, TM_WeatherEvent_MeshGeneric.MeshSkyColors, 1f, 1f);
            }
        }

        public override Vector2? OverrideShadowVector
        {
            get
            {
                return new Vector2?(this.shadowVector);
            }
        }
    }
}
