#region Header
/* ============================================ 
 *			    Strix Unity Library
 *		https://github.com/KorStrix/Unity_GetComponentAttribute
 *	============================================ 	
 *	�ۼ��� : Strix
 *	
 *	��� : 
 *	�������� �ҽ��ڵ��� ��� ������ - Inspector���� ���߸� ������ �Ǿ��µ�,
 *	����� SCManagerGetComponent.DoUpdateGetComponentAttribute �� ȣ���ϸ� �����ϵ��� �����Ͽ����ϴ�.
 *	Awake���� ȣ���Ͻø� �˴ϴ�.
 *
 *  GetComponentAttribute���� �ڵ�� �Ϻη� ���� ���Ͽ� �����ϰ� �ֽ��ϴ�.
 *  - Custom Package�� �������� �ʴ� ����Ƽ ���������� ��ġ�ϱ� �����ϱ� ����
 *
 *	�������� �ҽ� ��ũ
 *	https://openlevel.postype.com/post/683269
   ============================================ */
#endregion Header

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using GetComponentAttributeCore;
using Object = System.Object;

namespace GetComponentAttributeCore
{
    /// <summary>
    /// �� ���ӽ����̽��� Root Base Class
    /// <para>��ǥ�� <see cref="GetComponentAttributeSetter"/>�� <see cref="MonoBehaviour"/>�� ����/������Ƽ�� �� Attribute�� ���� �Ҵ��ϴ� ���Դϴ�.</para>
    /// </summary>
    public interface IGetComponentAttribute
    {
        object GetComponent(MonoBehaviour pMono, Type pElementType);
        bool bIsPrint_OnNotFound_GetComponent { get; }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public abstract class GetComponentAttributeBase : PropertyAttribute, IGetComponentAttribute
    {
        public bool bIsPrint_OnNotFound_GetComponent => bIsPrint_OnNotFound;
        public bool bIsPrint_OnNotFound;

        public abstract object GetComponent(MonoBehaviour pMono, Type pElementType);
    }

    public interface IGetComponentChildrenAttribute : IGetComponentAttribute
    {
        bool bSearch_By_ComponentName_ForGetComponent { get; }
        string strComponentName_ForGetComponent { get; }
    }

    public static class CoreLogic
    {
        #region zero array�� �ʿ�ø��� new�� �����ϱ� ���� ĳ���ؼ� ���

        static readonly UnityEngine.Object[] _EmptyUnityObjectArray = new UnityEngine.Object[0];
        static readonly GameObject[] _EmptyGameObjectArray = new GameObject[0];
        static readonly Object[] _EmptyObjectArray = new Object[0];
        static readonly Type[] _EmptyTypeArray = new Type[0];
        static readonly Type[] _BoolTypeArray = new[] { typeof(bool) };

        #endregion

        public static object Event_GetComponent(MonoBehaviour pMono, Type pElementType)
        {
            // ReSharper disable once PossibleNullReferenceException
            MethodInfo getter = typeof(MonoBehaviour)
                     .GetMethod("GetComponents", _EmptyTypeArray)
                     .MakeGenericMethod(pElementType);

            return getter.Invoke(pMono, null);
        }

        public static object Event_GetComponentInChildren(MonoBehaviour pMono, Type pElementType, bool bInclude_DeActive, bool bSearch_By_ComponentName, string strComponentName)
        {
            MethodInfo pGetMethod = typeof(MonoBehaviour).GetMethod("GetComponentsInChildren", _BoolTypeArray);

            if (pElementType.HasElementType)
                pElementType = pElementType.GetElementType();

            object pObjectReturn;
            if (pElementType == typeof(GameObject))
            {
                pElementType = typeof(Transform);
                // ReSharper disable once PossibleNullReferenceException
                pGetMethod = pGetMethod.MakeGenericMethod(pElementType);
                pObjectReturn = Convert_TransformArray_To_GameObjectArray(pGetMethod.Invoke(pMono, new object[] { bInclude_DeActive }));
            }
            else
            {
                // ReSharper disable once PossibleNullReferenceException
                pGetMethod = pGetMethod.MakeGenericMethod(pElementType);
                pObjectReturn = pGetMethod.Invoke(pMono, new object[] { bInclude_DeActive });
            }

            if (bSearch_By_ComponentName)
                return ExtractSameNameArray(strComponentName, pObjectReturn as UnityEngine.Object[]);

            return pObjectReturn;
        }

        public static object Event_GetComponentInParents(MonoBehaviour pTargetMono, Type pElementType)
        {
            bool bTypeIsGameObject = pElementType == typeof(GameObject);
            if (bTypeIsGameObject)
                pElementType = typeof(Transform);

            // ReSharper disable once PossibleNullReferenceException
            MethodInfo pGetMethod = typeof(MonoBehaviour).
                GetMethod("GetComponentsInParent", _EmptyTypeArray).
                MakeGenericMethod(pElementType);

            if (bTypeIsGameObject)
                return Convert_TransformArray_To_GameObjectArray(pGetMethod.Invoke(pTargetMono, _EmptyObjectArray));
            return pGetMethod.Invoke(pTargetMono, _EmptyObjectArray);
        }

        public static UnityEngine.Object[] ExtractSameNameArray(string strObjectName, UnityEngine.Object[] arrComponentFind)
        {
            if (arrComponentFind == null)
                return _EmptyUnityObjectArray;

            return arrComponentFind.Where(p => p.name.Equals(strObjectName)).ToArray();
        }


        private static GameObject[] Convert_TransformArray_To_GameObjectArray(object pObject)
        {
            Transform[] arrTransform = pObject as Transform[];
            if (arrTransform == null)
                return _EmptyGameObjectArray;

            return arrTransform.Select(p => p.gameObject).ToArray();
        }
    }
}

/// <summary>
/// <see cref="GetComponentAttributeSetter.DoUpdate_GetComponentAttribute(MonoBehaviour)"/>�Լ� ȣ���� ���� 
/// <para><see cref="UnityEngine.Component.GetComponents(Type)"/>���� ����/������Ƽ�� �Ҵ��մϴ�.</para>
/// </summary>
public class GetComponentAttribute : GetComponentAttributeBase
{
    public override object GetComponent(MonoBehaviour pMono, Type pElementType)
    {
        return CoreLogic.Event_GetComponent(pMono, pElementType);
    }
}

/// <summary>
/// <see cref="GetComponentAttributeSetter.DoUpdate_GetComponentAttribute(MonoBehaviour)"/>�Լ� ȣ���� ���� 
/// <para><see cref="UnityEngine.Component.GetComponentInParent(Type)"/>���� ����/������Ƽ�� �Ҵ��մϴ�.</para>
/// </summary>
public class GetComponentInParentAttribute : GetComponentAttributeBase
{
    public override object GetComponent(MonoBehaviour pMono, Type pElementType)
    {
        return CoreLogic.Event_GetComponentInParents(pMono, pElementType);
    }
}


/// <summary>
/// <see cref="GetComponentAttributeSetter.DoUpdate_GetComponentAttribute(MonoBehaviour)"/>�Լ� ȣ���� ���� 
/// <para><see cref="UnityEngine.Component.GetComponentsInChildren(Type)"/>���� ����/������Ƽ�� �Ҵ��մϴ�.</para>
/// <remarks>������ �켱�Ѵٸ� <see cref="bInclude_DisableObject"/>���� <see langword="false"/>�� �ϼ���.</remarks>
/// </summary>
/// 
/// <example>
/// ������ ��� �����Դϴ�.
/// <code>
/// [GetComponentInChildren()]
/// public Rigidbody _rigidbody = null;
///
///
/// // Find ObjectName Component Example
/// enum SomeEnum { First, Second }
/// 
/// [GetComponentInChildren(SomeEnum.First)]
/// public Rigidbody _rigidbody = null;
///
/// [GetComponentInChildren("Second")]
/// public Rigidbody _rigidbody { get; private set; }
///
///
/// // List Example
/// [GetComponentInChildren]
/// public List(Rigidbody) _rigidbodies = null; // = { Rigidbody(First) }, { Rigidbody(Second) }
///
/// // Dictionary Example
/// [GetComponentInChildren]
/// public Dictionary(SomeEnum, Rigidbody) _rigidbodies {get; private set;} // = { First, Rigidbody } , { Second, Rigidbody }
/// </code>
/// </example>
public class GetComponentInChildrenAttribute : GetComponentAttributeBase, IGetComponentChildrenAttribute
{
    public bool bSearch_By_ComponentName_ForGetComponent => bSearch_By_ComponentName;
    public string strComponentName_ForGetComponent => strComponentName;


    public bool bSearch_By_ComponentName;
    public bool bInclude_DisableObject;
    public string strComponentName;

    /// <summary>
    /// <see cref="UnityEngine.Component.GetComponentsInChildren(Type)"/>�� ȣ���Ͽ� �ڽ� ������Ʈ�� ã�� �Ҵ��մϴ�.
    /// </summary>
    /// <param name="bInclude_DisableObject">Disable �� ������Ʈ���� ������ ��</param>
    public GetComponentInChildrenAttribute(bool bInclude_DisableObject = true)
    {
        bSearch_By_ComponentName = false;
        this.bInclude_DisableObject = bInclude_DisableObject;
    }

    /// <summary>
    /// <see cref="UnityEngine.Component.GetComponentsInChildren(Type)"/>�� ȣ���Ͽ� �ڽ� ������Ʈ�� ã�� �Ҵ��մϴ�.
    /// </summary>
    /// <param name="bInclude_DisableObject">Disable �� ������Ʈ���� ������ ��</param>
    /// <param name="bIsPrint_OnNotFound">������Ʈ�� ��ã���� ��� <see cref="Debug.LogError(Object)"/>�� ����� ��</param>
    public GetComponentInChildrenAttribute(bool bInclude_DisableObject, bool bIsPrint_OnNotFound = true)
    {
        this.bInclude_DisableObject = bInclude_DisableObject;
        bSearch_By_ComponentName = false;
        this.bIsPrint_OnNotFound = bIsPrint_OnNotFound;
    }

    /// <summary>
    /// <see cref="UnityEngine.Component.GetComponentsInChildren(Type)"/>�� ȣ���Ͽ� �ڽ� ������Ʈ�� ã�� �Ҵ��մϴ�.
    /// </summary>
    /// <param name="pFindComponentName">���⿣ <see cref="string"/>Ȥ�� <see cref="System.Enum"/> Ÿ�Ը� ���� �մϴ�. ã���� �ϴ� ������Ʈ �̸��Դϴ�.</param>
    public GetComponentInChildrenAttribute(Object pFindComponentName)
    {
        bInclude_DisableObject = true;
        strComponentName = pFindComponentName.ToString();
        bSearch_By_ComponentName = true;
    }

    /// <summary>
    /// <see cref="UnityEngine.Component.GetComponentsInChildren(Type)"/>�� ȣ���Ͽ� �ڽ� ������Ʈ�� ã�� �Ҵ��մϴ�.
    /// </summary>
    /// <param name="pFindComponentName">���⿣ <see cref="string"/>Ȥ�� <see cref="System.Enum"/> Ÿ�Ը� ���� �մϴ�. ã���� �ϴ� ������Ʈ �̸��Դϴ�.</param>
    /// <param name="bInclude_DisableObject">Disable �� ������Ʈ���� ������ ��</param>
    public GetComponentInChildrenAttribute(Object pFindComponentName, bool bInclude_DisableObject)
    {
        strComponentName = pFindComponentName.ToString();
        bSearch_By_ComponentName = true;
        this.bInclude_DisableObject = bInclude_DisableObject;
    }

    /// <summary>
    /// <see cref="UnityEngine.Component.GetComponentsInChildren(Type)"/>�� ȣ���Ͽ� �ڽ� ������Ʈ�� ã�� �Ҵ��մϴ�.
    /// </summary>
    /// <param name="pFindComponentName">���⿣ <see cref="string"/>Ȥ�� <see cref="System.Enum"/> Ÿ�Ը� ���� �մϴ�. ã���� �ϴ� ������Ʈ �̸��Դϴ�.</param>
    /// <param name="bInclude_DisableObject">Disable �� ������Ʈ���� ������ ��</param>
    /// <param name="bIsPrint_OnNotFound">������Ʈ�� ��ã���� ��� <see cref="Debug.LogError(Object)"/>�� ����� ��</param>
    public GetComponentInChildrenAttribute(Object pFindComponentName, bool bInclude_DisableObject, bool bIsPrint_OnNotFound = true)
    {
        this.bInclude_DisableObject = bInclude_DisableObject;

        strComponentName = pFindComponentName.ToString();
        bSearch_By_ComponentName = true;
        this.bIsPrint_OnNotFound = bIsPrint_OnNotFound;
    }

    public override object GetComponent(MonoBehaviour pMono, Type pElementType)
    {
        return CoreLogic.Event_GetComponentInChildren(pMono, pElementType, bInclude_DisableObject, bSearch_By_ComponentName, strComponentName);
    }
}


/// <summary>
/// <see cref="GetComponentAttribute"/>�� �ִ� �ʵ� / ������Ƽ�� �Ҵ�����ִ� <see langword="static"/> class
/// </summary>
public static class GetComponentAttributeSetter
{
    /// <summary>
    /// ������ ȣ���� ������ new�� ����⺸��
    /// Static���� �ϳ� ���� �� ���� Clear�ϴ� ������..
    /// </summary>
    private static List<MemberInfo> _listMemberTemp = new List<MemberInfo>();


    /// <summary>
    /// �Ű������� �ִ� ���������� <see cref="GetComponentAttribute"/>�� ���� �ʵ�/������Ƽ�� ��� ã�� �Ҵ��մϴ�.
    /// </summary>
    /// <param name="pMonoTarget"><see cref="GetComponentAttribute"/>�� ������ ���</param>
    public static void DoUpdate_GetComponentAttribute(MonoBehaviour pMonoTarget) => DoUpdate_GetComponentAttribute(pMonoTarget, pMonoTarget);

    /// <summary>
    /// �Ű������� �ִ� Ŭ����(�ƹ� ��ӹ��� ���� class�� ����)����
    /// <para><see cref="GetComponentAttribute"/>�� ���� �ʵ�/������Ƽ�� �Ű������� ���� �������� �������� ã�� �Ҵ��մϴ�.</para>
    /// </summary>
    /// <param name="pMonoTarget"><see cref="GetComponentAttribute"/>�� ã�� ���� ���</param>
    /// <param name="pClass_AttributeOwner"><see cref="GetComponentAttribute"/>�� ������ ���</param>
    public static void DoUpdate_GetComponentAttribute(MonoBehaviour pMonoTarget, object pClass_AttributeOwner)
    {
        Type pType = pClass_AttributeOwner.GetType();
        // BindingFlags�� ������ ��� �� �����Ѵ�..
        // _listMemberTemp.AddRange(pType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
        // _listMemberTemp.AddRange(pType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance));
        // _listMemberTemp.AddRange(pType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
        // _listMemberTemp.AddRange(pType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance));

        // �����Ҵ� ����� ���µ� �ٽ� �̰ŷ��ϴϱ� �ߵ�;
        // �����ս� ���������� �ִ��� ���̴°� ����
        _listMemberTemp.AddRange(pType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
        _listMemberTemp.AddRange(pType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));

        foreach (var pMember in _listMemberTemp)
            DoUpdate_GetComponentAttribute(pMonoTarget, pClass_AttributeOwner, pMember);

        _listMemberTemp.Clear();
    }

    /// <summary>
    /// �Ű������� �ִ� Ŭ����(�ƹ� ��ӹ��� ���� class�� ����)����
    /// <para><see cref="GetComponentAttribute"/>�� ���� �ʵ�/������Ƽ�� �Ű������� ���� �������� �������� ã�� �Ҵ��մϴ�.</para>
    /// </summary>
    public static void DoUpdate_GetComponentAttribute(MonoBehaviour pMonoTarget, object pClass_AttributeOwner, MemberInfo pMemberInfo)
    {
        if (pMemberInfo == null)
            return;


        Type pMemberType = pMemberInfo.MemberType();
        IEnumerable<IGetComponentAttribute> arrCustomAttributes = pMemberInfo.GetCustomAttributes(true).OfType<IGetComponentAttribute>();
        foreach (var pGetComponentAttribute in arrCustomAttributes)
        {
            object pComponent = SetMember_FromGetComponent(pMonoTarget, pClass_AttributeOwner, pMemberInfo, pMemberType, pGetComponentAttribute);
            if (pComponent != null)
                continue;

            if (pGetComponentAttribute.bIsPrint_OnNotFound_GetComponent)
            {
                if (pGetComponentAttribute is GetComponentInChildrenAttribute pAttribute && pAttribute.bSearch_By_ComponentName)
                    Debug.LogError(pMonoTarget.name + string.Format(".{0}<{1}>({2}) Result == null", pGetComponentAttribute.GetType().Name, pMemberType, pAttribute.strComponentName), pMonoTarget);
                else
                    Debug.LogError(pMonoTarget.name + string.Format(".{0}<{1}> Result == null", pGetComponentAttribute.GetType().Name, pMemberType), pMonoTarget);
            }
        }
    }

    // ====================================================================================================================

    private static object SetMember_FromGetComponent(MonoBehaviour pMono, object pMemberOwner, MemberInfo pMemberInfo, Type pMemberType, IGetComponentAttribute iGetComponentAttribute)
    {
        var pComponent = GetComponent(pMono, pMemberType, iGetComponentAttribute);
        if (pComponent == null)
            return null;

        if (pMemberType.IsGenericType)
        {
            pMemberInfo.SetValue_Extension(pMemberOwner, pComponent);
            return pComponent;
        }


        if (pMemberType.HasElementType == false)
        {
            if (pComponent is Array arrComponent)
                pMemberInfo.SetValue_Extension(pMemberOwner, arrComponent.Length != 0 ? arrComponent.GetValue(0) : null);
        }
        else
        {
            if (pComponent is Array arrComponent)
            {
                if (pMemberType.GetElementType() == typeof(GameObject))
                {
                    pMemberInfo.SetValue_Extension(pMemberOwner, arrComponent.Cast<GameObject>().ToArray());
                }
                else
                {
                    // Object[]�� Type[]�� NoneGeneric�ϰ� �ٲ����..;
                    Array ConvertedArray = Array.CreateInstance(pMemberType.GetElementType(), arrComponent.Length);
                    Array.Copy(arrComponent, ConvertedArray, arrComponent.Length);

                    pMemberInfo.SetValue_Extension(pMemberOwner, ConvertedArray);
                }
            }
            else
            {
                if (pMemberType == typeof(GameObject))
                    pMemberInfo.SetValue_Extension(pMemberOwner, ((Component)pComponent).gameObject);
                else
                    pMemberInfo.SetValue_Extension(pMemberOwner, pComponent);
            }
        }

        return pComponent;
    }

    private static object GetComponent(MonoBehaviour pMono, Type pMemberType, IGetComponentAttribute iGetComponentAttribute)
    {
        return pMemberType.IsGenericType ?
            GetComponent_OnGeneric(iGetComponentAttribute, pMono, pMemberType) :
            iGetComponentAttribute.GetComponent(pMono, pMemberType.HasElementType ? pMemberType.GetElementType() : pMemberType);
    }

    private static object GetComponent_OnGeneric(IGetComponentAttribute iGetComponentAttribute, MonoBehaviour pMono, Type pTypeField)
    {
        Type pTypeField_Generic = pTypeField.GetGenericTypeDefinition();
        Type[] arrArgumentsType = pTypeField.GetGenericArguments();

        object pComponent = null;
        if (pTypeField_Generic == typeof(List<>))
            pComponent = GetComponent_OnList(iGetComponentAttribute, pMono, pTypeField, arrArgumentsType[0]);
        else if (pTypeField_Generic == typeof(Dictionary<,>))
            pComponent = GetComponent_OnDictionary(iGetComponentAttribute as IGetComponentChildrenAttribute, pMono, pTypeField, arrArgumentsType[0], arrArgumentsType[1]);

        return pComponent;
    }

    private static object GetComponent_OnList(IGetComponentAttribute iGetComponentAttribute, MonoBehaviour pMono, Type pTypeMember, Type pElementType)
    {
        Array arrComponent = iGetComponentAttribute.GetComponent(pMono, pElementType) as Array;
        if (arrComponent == null || arrComponent.Length == 0)
            return null;

        return Create_GenericList(pTypeMember, arrComponent);
    }

    private static object Create_GenericList(Type pTypeMember, IEnumerable arrComponent)
    {
        var pInstanceList = Activator.CreateInstance(pTypeMember);
        if (arrComponent == null)
            return pInstanceList;

        var Method_Add = pTypeMember.GetMethod("Add");
        var pIter = arrComponent.GetEnumerator();

        // ReSharper disable once PossibleNullReferenceException
        while (pIter.MoveNext())
            Method_Add.Invoke(pInstanceList, new[] { pIter.Current });

        return pInstanceList;
    }

    private static object GetComponent_OnDictionary(IGetComponentChildrenAttribute pAttributeInChildren, MonoBehaviour pMono, Type pMemberType, Type pType_DictionaryKey, Type pType_DictionaryValue)
    {
        if (pAttributeInChildren == null)
        {
            Debug.LogError($"Dictionary Field Type Not Support Non-IGetComponentChildrenAttribute - {pType_DictionaryKey.Name}");
            return null;
        }

        if (pType_DictionaryKey != typeof(string) && pType_DictionaryKey.IsEnum == false)
        {
            Debug.LogError($"Not Support Dictionary Key - {pType_DictionaryKey.Name} - pType_DictionaryKey != typeof(string) && pType_DictionaryKey.IsEnum == false");
            return null;
        }

        object pComponent = null;
        bool bValue_Is_Collection = pType_DictionaryValue.IsGenericType || pType_DictionaryValue.HasElementType;
        Type pTypeChild_OnValueIsCollection = null;
        if (bValue_Is_Collection)
        {
            // Dictionary�� ValueŸ���� �׻� �������ڶ�� �����Ϸ� ����
            pTypeChild_OnValueIsCollection = pType_DictionaryValue.IsGenericType ? pType_DictionaryValue.GenericTypeArguments[0] : pType_DictionaryValue.GetElementType();
            pComponent = GetComponent(pMono, pType_DictionaryValue, pAttributeInChildren);
        }
        else
            pComponent = pAttributeInChildren.GetComponent(pMono, pType_DictionaryValue);

        IEnumerable arrChildrenComponent = pComponent as IEnumerable;
        if (arrChildrenComponent == null)
            return null;


        MethodInfo Method_Add = pMemberType.GetMethod("Add", new[] {
                                pType_DictionaryKey, pType_DictionaryValue });

        Object pInstanceDictionary = Activator.CreateInstance(pMemberType);
        if (pType_DictionaryKey == typeof(string))
        {
            if (bValue_Is_Collection)
            {
                arrChildrenComponent.OfType<UnityEngine.Object>().
                    GroupBy(p => p.name).
                    ToList().
                    ForEach(pGroup =>
                    AddDictionary_OnValueIsCollection(pMono, pType_DictionaryValue, pGroup, pTypeChild_OnValueIsCollection, Method_Add, pInstanceDictionary, (key) => key));
            }
            else
            {
                arrChildrenComponent.OfType<UnityEngine.Object>().
                    ToList().
                    ForEach(pUnityObject => AddDictionary(pMono, Method_Add, pInstanceDictionary, pUnityObject, (key) => key));
            }
        }
        else if (pType_DictionaryKey.IsEnum)
        {
            HashSet<string> setEnumName = new HashSet<string>(Enum.GetNames(pType_DictionaryKey));

            if (bValue_Is_Collection)
            {
                arrChildrenComponent.OfType<UnityEngine.Object>().
                    GroupBy(p => p.name).
                    Where(p => setEnumName.Contains(p.Key)).
                    ToList().
                    ForEach(pGroup =>
                    AddDictionary_OnValueIsCollection(pMono, pType_DictionaryValue, pGroup, pTypeChild_OnValueIsCollection, Method_Add, pInstanceDictionary, (key) => Enum.Parse(pType_DictionaryKey, key, true)));
            }
            else
            {
                arrChildrenComponent.OfType<UnityEngine.Object>().
                    Where(p => setEnumName.Contains(p.name)).
                    ToList().
                    ForEach(pUnityObject => AddDictionary(pMono, Method_Add, pInstanceDictionary, pUnityObject, (key) => Enum.Parse(pType_DictionaryKey, pUnityObject.name, true)));
            }
        }


        return pInstanceDictionary;
    }

    private static void AddDictionary(MonoBehaviour pMono, MethodInfo Method_Add, object pInstanceDictionary, UnityEngine.Object pUnityObject, Func<string, object> OnSelectDictionaryKey)
    {
        try
        {
            Method_Add.Invoke(pInstanceDictionary, new[] { OnSelectDictionaryKey(pUnityObject.name), pUnityObject });
        }
        catch (Exception e)
        {
            Debug.LogError(pUnityObject.name + " GetComponent - GetComponent_OnDictionary - Overlap Key MonoType : " + pMono.GetType() + e, pMono);
        }
    }

    private static void AddDictionary_OnValueIsCollection(MonoBehaviour pMono, Type pType_DictionaryValue, IGrouping<string, UnityEngine.Object> pGroup, Type pTypeChild_OnValueIsCollection, MethodInfo Method_Add, object pInstanceDictionary, Func<string, object> OnSelectDictionaryKey)
    {
#if UNITY_EDITOR
        try
#endif
        {
            var arrChildrenObject = pGroup.ToArray();
            if (pType_DictionaryValue.IsArray)
            {
                Array ConvertedArray = Array.CreateInstance(pTypeChild_OnValueIsCollection, arrChildrenObject.Length);
                Array.Copy(arrChildrenObject, ConvertedArray, arrChildrenObject.Length);
                Method_Add.Invoke(pInstanceDictionary, new[] { OnSelectDictionaryKey(pGroup.Key), ConvertedArray });
            }
            else
            {
                if (pType_DictionaryValue.IsGenericType)
                    Method_Add.Invoke(pInstanceDictionary, new[] { OnSelectDictionaryKey(pGroup.Key), Create_GenericList(pType_DictionaryValue, arrChildrenObject) });
                else
                    Method_Add.Invoke(pInstanceDictionary, new[] { OnSelectDictionaryKey(pGroup.Key), arrChildrenObject });
            }
        }
#if UNITY_EDITOR
        catch (Exception e)
        {
            Debug.LogError(e, pMono);
        }
#endif
    }
}


#region Extension

/// <summary>
/// <see cref="GetComponentAttributeSetter"/>���� ���ϰ� ����ϱ� ���� <see cref="MemberInfo"/>�� Ȯ�� Ŭ����
/// </summary>
public static class MemberInfo_Extension
{
    public static Type MemberType(this MemberInfo pMemberInfo)
    {
        FieldInfo pFieldInfo = pMemberInfo as FieldInfo;
        if (pFieldInfo != null)
            return pFieldInfo.FieldType;

        PropertyInfo pProperty = pMemberInfo as PropertyInfo;
        if (pProperty != null)
            return pProperty.PropertyType;

        return null;
    }

    public static void SetValue_Extension(this MemberInfo pMemberInfo, object pTarget, object pValue)
    {
        FieldInfo pFieldInfo = pMemberInfo as FieldInfo;
        if (pFieldInfo != null)
            pFieldInfo.SetValue(pTarget, pValue);

        PropertyInfo pProperty = pMemberInfo as PropertyInfo;
        if (pProperty != null)
            pProperty.SetValue(pTarget, pValue, null);
    }
}

#endregion