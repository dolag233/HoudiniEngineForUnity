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
    using HEU_FloatData = KeyValuePair<string, float[]>;
    
    [System.Serializable]
    public class HEU_GetAttributeOutputData
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
        private Dictionary<string, HEU_AttributeDataExtend> _attribDataDict = new Dictionary<string, HEU_AttributeDataExtend>();
        public Dictionary<string, HEU_AttributeDataExtend> floatDataDict{get{return _attribDataDict;}}
        private List<HEU_GeoNode> _geoNodes;
        public HEU_GetAttributeOutputData(HEU_SessionBase session,List<HEU_GeoNode> geoNodes){
            this.session = session;
            this._geoNodes = geoNodes;
        }
        // access like dict
        public HEU_AttributeDataExtend this[string key]{
            get{
                // invalid key
                if(!_attribDataDict.ContainsKey(key))
                    return new HEU_AttributeDataExtend();
                else
                    return _attribDataDict[key];
            }
        }
        // get float data of output geos
        // @TODO support duplicated name of the different owners
        public bool GetGeoNodesOutputData<T>(string attribName, bool enableLog = true){
            // if already has the same name attrib in attribute dict
            if(_attribDataDict.ContainsKey(attribName))
                return false;

            HAPI_AttributeInfo attrib_info = new HAPI_AttributeInfo();
            float[] attrib_float_array = new float[0];
            int[] attrib_int_array = new int[0];
            string[] attrib_string_array = new string[0];
            Vector3[] attrib_vector3_array = new Vector3[0];
            Vector4[] attrib_vector4_array = new Vector4[0];
            HEU_AttributeDataExtend.AttributeType attrib_type = HEU_AttributeDataExtend.AttributeType.FLOAT;

            // get attribute data
            foreach(var geoNode in _geoNodes){
                foreach(var part in geoNode.GetParts()){
                    bool flag = false;
                    if(typeof(T) == typeof(float)){
                        flag = GetFloatData(session, geoNode.GeoID, part.PartID, attribName, ref attrib_info, ref attrib_float_array);
                        attrib_type = HEU_AttributeDataExtend.AttributeType.FLOAT;
                    }
                    else if(typeof(T) == typeof(int)){
                        flag = GetIntData(session, geoNode.GeoID, part.PartID, attribName, ref attrib_info, ref attrib_int_array);
                        attrib_type = HEU_AttributeDataExtend.AttributeType.INT;
                        //Debug.LogWarning(string.Format("float num {0}", attrib_info.count));
                    }
                    else if(typeof(T) == typeof(string)){
                        flag = GetStringData(session, geoNode.GeoID, part.PartID, attribName, ref attrib_info, ref attrib_string_array);
                        attrib_type = HEU_AttributeDataExtend.AttributeType.STRING;
                    }
                    else if(typeof(T) == typeof(Vector3)){
                        flag = GetVector3Data(session, geoNode.GeoID, part.PartID, attribName, ref attrib_info, ref attrib_vector3_array);
                        attrib_type = HEU_AttributeDataExtend.AttributeType.VECTOR3;
                        //Debug.Log(attrib_vector3_array[11]);
                    }
                    else if(typeof(T) == typeof(Vector4)){
                        flag = GetVector4Data(session, geoNode.GeoID, part.PartID, attribName, ref attrib_info, ref attrib_vector4_array);
                        attrib_type = HEU_AttributeDataExtend.AttributeType.VECTOR4;
                        //Debug.Log(attrib_vector4_array[15]);
                    }
                    else
                        return false;
                    
                    if (!flag)
                    {
                        if(enableLog)
                            Debug.LogWarning(string.Format("Missing attribute \"{0}\"!", attribName));
                        return false;
                    }
                    else
                        // just for one attribute
                        break;
                }
            }

            // insert into attribute dictionary
            if(attrib_info.tupleSize > 0){
                if(attrib_info.count > 0){
                    // if not empty
                    HEU_AttributeDataExtend attrib_data = new HEU_AttributeDataExtend();
                    attrib_data._attributeInfo = attrib_info;
                    attrib_data._name = attribName;
                    attrib_data._attributeType = attrib_type;
                    attrib_data._attributeState = HEU_AttributeDataExtend.AttributeState.SYNCED;
                    // set attribute real data
                    if(typeof(T) == typeof(float)){
                        attrib_data._floatValues = new float[attrib_info.count];
                        attrib_data._floatValues = attrib_float_array;
                    }
                    else if(typeof(T) == typeof(string)){
                        attrib_data._stringValues = new string[attrib_info.count];
                        attrib_data._stringValues = attrib_string_array;
                    }
                    else if(typeof(T) == typeof(int)){
                        attrib_data._intValues = new int[attrib_info.count];
                        attrib_data._intValues = attrib_int_array;
                    }
                    else if(typeof(T) == typeof(Vector3)){
                        attrib_data._vector3Values = new Vector3[attrib_info.count];
                        attrib_data._vector3Values = attrib_vector3_array;
                    }
                    else if(typeof(T) == typeof(Vector4)){
                        attrib_data._vector4Values = new Vector4[attrib_info.count];
                        attrib_data._vector4Values = attrib_vector4_array;
                    }
                    else
                        return false;
                    _attribDataDict.TryAdd(attribName, attrib_data);
                }
            }
            return true;
        }

        // get float data of specific part
        public static bool GetFloatData(HEU_SessionBase session, HAPI_NodeId geoID, HAPI_PartId partID, string attribName, ref HAPI_AttributeInfo attrib_info, ref float[] attrib_array){
            return HEU_GeneralUtility.GetAttribute(session, geoID, partID, attribName, ref attrib_info, ref attrib_array, session.GetAttributeFloatData);
        }
        // get vetor3 data of specific part
        public static bool GetVector3Data(HEU_SessionBase session, HAPI_NodeId geoID, HAPI_PartId partID, string attribName, ref HAPI_AttributeInfo attrib_info, ref Vector3[] attrib_array){
            float[] data = new float[0];
            bool flag = HEU_GeneralUtility.GetAttribute(session, geoID, partID, attribName, ref attrib_info, ref data, session.GetAttributeFloatData);

            if(!flag)
                return false;
            
            // note that attrib_info.count is the count of points
            attrib_array = new Vector3[attrib_info.count];
            for(int i = 0;i < attrib_info.count; ++i){
                attrib_array[i] = new Vector3(data[i * attrib_info.tupleSize],
                                            data[i * attrib_info.tupleSize + 1],
                                            data[i * attrib_info.tupleSize + 2]);
            }
            return flag;
        }
        public static bool GetVector4Data(HEU_SessionBase session, HAPI_NodeId geoID, HAPI_PartId partID, string attribName, ref HAPI_AttributeInfo attrib_info, ref Vector4[] attrib_array){
            float[] data = new float[0];
            bool flag = HEU_GeneralUtility.GetAttribute(session, geoID, partID, attribName, ref attrib_info, ref data, session.GetAttributeFloatData);

            if(!flag)
                return false;
            
            attrib_array = new Vector4[attrib_info.count];
            for(int i = 0;i < attrib_info.count; ++i){
                attrib_array[i] = new Vector4(data[i * attrib_info.tupleSize],
                                            data[i * attrib_info.tupleSize + 1],
                                            data[i * attrib_info.tupleSize + 2],
                                            data[i * attrib_info.tupleSize + 3]);
            }
            return flag;
        }
        // get int data of specific part
        public static bool GetIntData(HEU_SessionBase session, HAPI_NodeId geoID, HAPI_PartId partID, string attribName, ref HAPI_AttributeInfo attrib_info, ref int[] attrib_array){
            return HEU_GeneralUtility.GetAttribute(session, geoID, partID, attribName, ref attrib_info, ref attrib_array, session.GetAttributeIntData);
        }
        // get string data of specific part
        public static bool GetStringData(HEU_SessionBase session, HAPI_NodeId geoID, HAPI_PartId partID, string attribName, ref HAPI_AttributeInfo attrib_info, ref string[] attrib_array){
            attrib_array = HEU_GeneralUtility.GetAttributeStringData(session, geoID, partID, attribName, ref attrib_info);
            return attrib_array != null;
        }
        
    }
}