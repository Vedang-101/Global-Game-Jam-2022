using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    public GameObject LightSourcePrefab;
    public LayerMask LightHittable;

    public Material MirrorLightMaterial;
    public Material SplitterLightMaterial;

    List<Tuple<List<Vector3>, bool>> m_lightRays;
    List<LineRenderer> m_lightRenderers;

    int m_bounceCount;
    int m_maxBounceCount = 100;

    void Start()
    {
        m_lightRays = new List<Tuple<List<Vector3>, bool>>();
        m_lightRenderers = new List<LineRenderer>();

        m_lightRenderers.Add(GetComponent<LineRenderer>());
    }

    public void LightUpdate()
    {
        // Clear all Line Renderers and Lists
        foreach (LineRenderer renderer in m_lightRenderers)
            renderer.positionCount = 0;

        foreach (var light in m_lightRays)
            light.Item1.Clear();
        m_lightRays.Clear();

        // Update List with new rays
        m_bounceCount = 0;

        List<Vector3> line = new List<Vector3>();
        line.Add(transform.position);

        traceLight(transform.position, transform.forward, line, true);

        // Update Renderers with this newly populated rays
        for(int i=0; i<m_lightRays.Count; i++)
        {
            LineRenderer lr;
            // New lightRenderer needs to be created
            if(i>=m_lightRenderers.Count)
            {
                GameObject lineRendererPrefab = Instantiate(LightSourcePrefab, transform.position, Quaternion.identity, transform);
                lr = lineRendererPrefab.GetComponent<LineRenderer>();
                m_lightRenderers.Add(lr);
            }
            else
            {
                lr = m_lightRenderers[i];
            }

            //Line rendering mirror
            if (m_lightRays[i].Item2 == true)
                lr.material = MirrorLightMaterial;
            else
                lr.material = SplitterLightMaterial;

            lr.positionCount = m_lightRays[i].Item1.Count;
            for(int j=0; j<m_lightRays[i].Item1.Count; j++)
            {
                lr.SetPosition(j, m_lightRays[i].Item1[j]);
            }
        }
    }

    void traceLight(Vector3 startPos, Vector3 startDir, List<Vector3> list, bool isMirrorLight)
    {
        m_bounceCount += 1;
        if(m_bounceCount>m_maxBounceCount)
        {
            list.Add(startPos);
            m_lightRays.Add(new Tuple<List<Vector3>, bool>(list, isMirrorLight));
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(startPos, startDir, out hit, Mathf.Infinity, LightHittable))
        {
            if(hit.collider.transform.tag == "Mirror")
            {
                if(hit.collider.transform.parent.GetComponentInChildren<ColorableObject>() != null)
                    hit.collider.transform.parent.GetComponentInChildren<ColorableObject>().IsInColor = true;

                List<Vector3> currList = list;

                Vector3 newPoint = hit.point;
                newPoint.y = startPos.y;
                currList.Add(newPoint);

                if (!isMirrorLight)
                {
                    m_lightRays.Add(new Tuple<List<Vector3>, bool>(currList, isMirrorLight));
                    
                    currList = new List<Vector3>();
                    currList.Add(newPoint);
                }

                Vector3 newDir = Vector3.Reflect(startDir, hit.normal);
                newDir.y = 0;
                newDir.Normalize();

                traceLight(newPoint, newDir, currList, true);
            }
            else if(hit.collider.transform.tag == "Splitter")
            {
                List<Vector3> currList = list;
                List<Vector3> splittedList = new List<Vector3>();

                Vector3 newPoint = hit.collider.transform.position;

                SplitterData data = hit.collider.transform.GetComponent<SplitterData>();
                Vector3 newDirA = data.DirA.position - newPoint;
                newDirA.Normalize();
                newDirA.y = 0;

                Vector3 newDirB = data.DirB.position - newPoint;
                newDirB.Normalize();
                newDirB.y = 0;

                newPoint.y = startPos.y;
                currList.Add(newPoint);
                
                if (isMirrorLight)
                {
                    m_lightRays.Add(new Tuple<List<Vector3>, bool>(currList, isMirrorLight));

                    currList = new List<Vector3>();
                    currList.Add(newPoint);
                }
                splittedList.Add(newPoint);

                traceLight(newPoint, newDirA, currList, false);
                traceLight(newPoint, newDirB, splittedList, false);
            }
            else if(hit.collider.transform.tag == "ColorableObject")
            {
                Vector3 endPoint = hit.point;
                endPoint.y = startPos.y;
                list.Add(endPoint);
                m_lightRays.Add((new Tuple<List<Vector3>, bool>(list, isMirrorLight)));

                hit.transform.GetComponent<ColorableObject>().IsInColor = true;
            }
            else
            {
                Vector3 endPoint = hit.point;
                endPoint.y = startPos.y;
                list.Add(endPoint);
                m_lightRays.Add((new Tuple<List<Vector3>, bool>(list, isMirrorLight)));
            }
        }
        else
        {
            Vector3 endPoint = startPos + (startDir * 100);
            list.Add(endPoint);
            m_lightRays.Add((new Tuple<List<Vector3>, bool>(list, isMirrorLight)));
            return;
        }
    }
}
