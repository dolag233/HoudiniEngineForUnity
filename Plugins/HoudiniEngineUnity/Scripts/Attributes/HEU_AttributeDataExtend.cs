using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoudiniEngineUnity{
    // extend vector
    [System.Serializable]
    public class HEU_AttributeDataExtend : HEU_AttributeData{
        public new enum AttributeType{
        UNDEFINED = -1,
	    BOOL,
	    INT,
	    FLOAT,
	    STRING,
	    MAX,
        // new type
        VECTOR3,
        VECTOR4
        }
        public new AttributeType _attributeType;
        public Vector3[] _vector3Values = null;
        public Vector4[] _vector4Values = null;
        public void CopyValuesTo(HEU_AttributeDataExtend destAttrData){
            base.CopyValuesTo(destAttrData);
            if (this._vector3Values == null)
            {
            destAttrData._vector3Values = null;
            }
            else
            {
            int arraySize = this._vector3Values.Length;
            System.Array.Resize<Vector3>(ref destAttrData._vector3Values, arraySize);
            System.Array.Copy(this._vector3Values, destAttrData._vector3Values, arraySize);
            }
            
            if (this._vector4Values == null)
            {
            destAttrData._vector4Values = null;
            }
            else
            {
            int arraySize = this._vector4Values.Length;
            System.Array.Resize<Vector4>(ref destAttrData._vector4Values, arraySize);
            System.Array.Copy(this._vector4Values, destAttrData._vector4Values, arraySize);
            }
        } 
    }
}