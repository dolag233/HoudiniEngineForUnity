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
        public struct GPUInstanceData{
            public int[] proto_index;
            public string[] proto;
        }
        private GPUInstanceData _gpuinstanceData = new GPUInstanceData();
        public GPUInstanceData gpuinstanceData{get{return _gpuinstanceData;}}
        private List<HEU_GeoNode> _geoNodes{get; set;}
        private HEU_GetAttributeOutputData _getData;
        public HEU_GetGPUInstanceData(HEU_SessionBase session,List<HEU_GeoNode> geoNodes){
            this.session = session;
            this._geoNodes = geoNodes;
            _getData = new HEU_GetAttributeOutputData(session, geoNodes);
            GetGPUInstanceData();
            // Debug.Log(string.Format("success! PC num is {0} and the value of the first point is {1}!", _gpuinstanceData.proto_index.Length, _gpuinstanceData.proto[0]));
        }
        public void GetGPUInstanceData(){
            _getData.GetGeoNodesOutputData<int>(HEU_Defines.HAPI_ATTRIB_DOLAG_GPUINSTANCE_PROTOTYPE_INDEX);
            // get prototype in detail
            List<string> proto = new List<string>();
            int i = 0;
            while(true){
                string proto_name = string.Format("{0}{1}",HEU_Defines.HAPI_ATTRIB_DOLAG_GPUINSTANCE_PROTOTYPE, i);
                if(!_getData.GetGeoNodesOutputData<string>(proto_name, false))
                    break;
                
                if(_getData[proto_name]._stringValues == null){
                    Debug.LogWarning(string.Format("Invalid attribute \"{0}\".", proto_name));
                    return;
                }

                proto.Add(_getData[proto_name]._stringValues[0]);
                i++;
            }
            _gpuinstanceData.proto_index = new int[_getData[HEU_Defines.HAPI_ATTRIB_DOLAG_GPUINSTANCE_PROTOTYPE_INDEX]._attributeInfo.count];
            _gpuinstanceData.proto_index = _getData[HEU_Defines.HAPI_ATTRIB_DOLAG_GPUINSTANCE_PROTOTYPE_INDEX]._intValues;
            _gpuinstanceData.proto = new string[proto.Count];
            _gpuinstanceData.proto = proto.ToArray();
        }
    }
}