using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine.Rendering;
using UnityEngine;


namespace SVGPainterUnity{
    public class SVGPainterSystem : ComponentSystem
    {
        struct Group
        {
            [ReadOnly]
            public SharedComponentDataArray<SVGPainterData> Painter;
            public EntityArray                         Entity;
            public int                                 Length;
        }

        [Inject] Group m_Group;

        public List<Painter> painters = new List<Painter>();

        protected override void OnUpdate()
        {
            while (m_Group.Length != 0)
            {
                var painter = m_Group.Painter[0];
                var sourceEntity = m_Group.Entity[0];

                SVGDataParser parser = new SVGDataParser();

                List<string> paths = parser.Load (painter.svgFile);

                var entities = new NativeArray<Entity>(paths.Count, Allocator.Temp);
                EntityManager.Instantiate(painter.prefab, entities);

                var positions = new NativeArray<float3>(paths.Count, Allocator.Temp);

                ToPoints toPoints = new ToPoints();
                LineMesh lm = new LineMesh();

                Camera c = Camera.main;

                for(int i = 0; i<paths.Count; i++){
                    var position = new LocalPosition
                    {
                        Value = positions[i]
                    };

                    PathData data = toPoints.GetPointsFromPath(paths[i]);

                    Vector3 eulerAngles = new Vector3(0f, 180f, 180f);
                    Quaternion rotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
                    Matrix4x4 m = Matrix4x4.Rotate(rotation);

                    for (int j = 0; j < data.points.Count; j++)
                    {
                        Vector3 pos = new Vector3(data.points[j].x + ((Screen.width - parser.GetSize().x) * 0.5f), data.points[j].y + ((Screen.height - parser.GetSize().y) * 0.5f), c.nearClipPlane);
                        Vector3 p = c.ScreenToWorldPoint(pos);
                        p.y -= c.transform.localPosition.y * 2f;
                        p = m.MultiplyPoint3x4(p);
                        p.z = 0f;
                        data.points[j] = p;
                    }


                    var lineMesh = lm.CreateLine(data.points, painter.lineWidth, painter.lineColor);

                    Material mat = new Material(Shader.Find("Custom/SVGLine"));
                    mat.enableInstancing = true;
                    var meshRenderer = new MeshInstanceRenderer
                    {
                        mesh = lineMesh,
                        material = mat
                    };

                    var pObj = new Painter();
                    pObj.lineMat = mat;
                    painters.Add(pObj);

                    EntityManager.SetComponentData(entities[i], position);
                    EntityManager.AddSharedComponentData(entities[i], meshRenderer);

                    EntityManager.AddComponentData(entities[i], new TransformParent { Value = sourceEntity });
                }

                int pID = Shader.PropertyToID("_SVGLineMaskValue");
                for (int i = 0; i < painters.Count; i++)
                {
                    painters[i].sMaskValueID = pID;
                }

                entities.Dispose();
                positions.Dispose();

                EntityManager.RemoveComponent<SVGPainterData>(sourceEntity);

                // Instantiate & AddComponent & RemoveComponent calls invalidate the injected groups,
                // so before we get to the next spawner we have to reinject them  
                UpdateInjectedComponentGroups();

            }


        }



    }
}