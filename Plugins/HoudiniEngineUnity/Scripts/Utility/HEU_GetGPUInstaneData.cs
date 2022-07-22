/*
* Copyright (c) <2020> Side Effects Software Inc.
* All rights reserved.
*
* Redistribution and use in source and binary forms, with or without
* modification, are permitted provided that the following conditions are met:
*
* 1. Redistributions of source code must retain the above copyright notice,
*    this list of conditions and the following disclaimer.
*
* 2. The name of Side Effects Software may not be used to endorse or
*    promote products derived from this software without specific prior
*    written permission.
*
* THIS SOFTWARE IS PROVIDED BY SIDE EFFECTS SOFTWARE "AS IS" AND ANY EXPRESS
* OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
* OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.  IN
* NO EVENT SHALL SIDE EFFECTS SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT,
* INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
* LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
* OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
* LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
* NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
* EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace HoudiniEngineUnity{
    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // Typedefs (copy these from HEU_Common.cs)
    using HAPI_NodeId = System.Int32;
    using HAPI_PartId = System.Int32;
    using HAPI_StringHandle = System.Int32;
    public class HEU_GetGPUInstanceData
    {
        /// <summary> 
        /// Creates terrain from given volumeInfo for the given gameObject.
        /// If gameObject has a valid Terrain component, then it is reused.
        /// Similarly, if the Terrain component has a valid TerrainData, or if the given terrainData is valid, then it is used.
        /// Otherwise a new TerrainData is created and set to the Terrain.
        /// Populates the volumePositionOffset with the heightfield offset position.
        /// Returns true if successfully created the terrain, otherwise false.
        /// </summary>
        /// <param name="session">Houdini Engine session to query heightfield data from</param>
        /// <param name="volumeInfo">Volume info pertaining to the heightfield to generate the Terrain from</param>
        /// <param name="geoID">The geometry ID</param>
        /// <param name="partID">The part ID (height layer)</param>
        /// <param name="gameObject">The target GameObject containing the Terrain component</param>
        /// <param name="terrainData">A valid TerrainData to use, or if empty, a new one is created and populated</param>
        /// <param name="volumePositionOffset">Heightfield offset</param>
        /// <returns>True if successfully popupated the terrain</returns>
        public HEU_SessionBase session{get; set;}
        [System.Serializable]
        public struct GPUInstanceData{
            // below here is the data per instance point
            public List<HEU_InstancedPointData> points;
            // proto is the real mesh path per instance
            public string[] proto;
        }
        private GPUInstanceData _gpuinstanceData = new GPUInstanceData();
        public GPUInstanceData gpuinstanceData{get{return _gpuinstanceData;}}
        private List<HEU_GeoNode> _geoNodes{get; set;}
        private HEU_GetAttributeOutputData _getData;
        public HEU_GetGPUInstanceData(HEU_SessionBase session,List<HEU_GeoNode> geoNodes, int chunkSize){
            this.session = session;
            this._geoNodes = geoNodes;
            _getData = new HEU_GetAttributeOutputData(session, geoNodes);
            GetGPUInstanceData();
        }
        public void GetGPUInstanceData(){
            // get prototype index per point
            _getData.GetGeoNodesOutputData<int>(HEU_Defines.HAPI_ATTRIB_DOLAG_GPUINSTANCE_PROTOTYPE_INDEX);
            _getData.GetGeoNodesOutputData<Vector3>(HEU_Defines.HAPI_ATTRIB_POSITION);
            _getData.GetGeoNodesOutputData<Vector3>(HEU_Defines.HAPI_ATTRIB_DOLAG_GPUINSTANCE_N);
            _getData.GetGeoNodesOutputData<Vector3>(HEU_Defines.HAPI_ATTRIB_DOLAG_GPUINSTANCE_PSCALE);
            _getData.GetGeoNodesOutputData<Vector4>(HEU_Defines.HAPI_ATTRIB_DOLAG_GPUINSTANCE_ORIENT);
            int pcnum = _getData[HEU_Defines.HAPI_ATTRIB_POSITION]._attributeInfo.count;
            if(pcnum == 0){
                Debug.LogWarning("No point output in HDA.");
                return;
            }

            // get prototype owned by detail
            List<string> proto = new List<string>();
            int proto_i = 0;
            while(true){
                string proto_name = string.Format("{0}{1}",HEU_Defines.HAPI_ATTRIB_DOLAG_GPUINSTANCE_PROTOTYPE, proto_i);
                if(!_getData.GetGeoNodesOutputData<string>(proto_name, false))
                    break;
                
                if(_getData[proto_name]._stringValues == null){
                    Debug.LogWarning(string.Format("Invalid attribute \"{0}\".", proto_name));
                    return;
                }

                proto.Add(_getData[proto_name]._stringValues[0]);
                proto_i++;
            }

            // process model to world matrix
            Matrix4x4[] modelToWorldMatArray = new Matrix4x4[pcnum];
            HEU_InstancedPointData[] instance_points = new HEU_InstancedPointData[pcnum];
            for(int i = 0;i < pcnum; ++i){
                Quaternion rot = Quaternion.identity;
                Vector3 pos = Vector3.zero;
                Vector3 scale = Vector3.one;
                if(_getData[HEU_Defines.HAPI_ATTRIB_DOLAG_GPUINSTANCE_ORIENT]._vector4Values != null){
                    Vector4 orient = _getData[HEU_Defines.HAPI_ATTRIB_DOLAG_GPUINSTANCE_ORIENT]._vector4Values[i];
                    rot.x = orient.x;
                    rot.y = orient.y;
                    rot.z = orient.z;
                    rot.w = orient.w;
                    rot.y = -rot.y;
                    rot.z = -rot.z;
                }
                else if(_getData[HEU_Defines.HAPI_ATTRIB_DOLAG_GPUINSTANCE_N]._vector3Values != null){
                    Vector3 N = _getData[HEU_Defines.HAPI_ATTRIB_DOLAG_GPUINSTANCE_N]._vector3Values[i];
                    N.x = -N.x;
                    Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
                    rot = Quaternion.FromToRotation(up * Mathf.Rad2Deg, N.normalized * Mathf.Rad2Deg);
                }
                if(_getData[HEU_Defines.HAPI_ATTRIB_POSITION]._vector3Values != null){
                    pos = _getData[HEU_Defines.HAPI_ATTRIB_POSITION]._vector3Values[i];
                    pos.x = -pos.x;
                }
                if(_getData[HEU_Defines.HAPI_ATTRIB_DOLAG_GPUINSTANCE_PSCALE]._vector3Values != null){
                    scale = _getData[HEU_Defines.HAPI_ATTRIB_DOLAG_GPUINSTANCE_PSCALE]._vector3Values[i];
                }

                instance_points[i].modelToWorldMat = Matrix4x4.TRS(pos, rot, scale);
                instance_points[i].protoIndex = _getData[HEU_Defines.HAPI_ATTRIB_DOLAG_GPUINSTANCE_PROTOTYPE_INDEX]._intValues[i];
                instance_points[i].pos = pos;
                //instance_points[i].GetChunkHash();
            }
            _gpuinstanceData.points = new List<HEU_InstancedPointData>(instance_points);
            _gpuinstanceData.proto = proto.ToArray();
        }
        // return null if invalid data
        public string GetJsonData(){
            if(_gpuinstanceData.proto == null || _gpuinstanceData.proto.Length == 0 ||
             _gpuinstanceData.points == null || _gpuinstanceData.points.Count == 0){
                return null;
            }
            return JsonUtility.ToJson(_gpuinstanceData);
        }
        public void WriteJsonData(string asset_path = HEU_Defines.HAPI_GPUINSTANCE_JSON_TMP_PATH){
            // create folder
            string[] folder_token = asset_path.Split('/');
            string folder = "";
            for(int i = 0;i < folder_token.Length - 1; ++i){
                folder += folder_token[i] + "/";
                if(!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }
            
            StreamWriter writer = new StreamWriter(asset_path, false);
            string json_data = GetJsonData();
            if(json_data == null)
                Debug.LogWarning("Invalid instance data.");
            
            writer.Write(json_data);
            writer.Close();
        }
    }
}