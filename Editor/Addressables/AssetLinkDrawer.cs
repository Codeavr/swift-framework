﻿using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;

namespace SwiftFramework.Core.Editor
{
    internal class AssetLinkDrawer : BaseLinkDrawer
    {
        public AssetLinkDrawer(System.Type type, FieldInfo fieldInfo = null) : base(type, fieldInfo)
        {

        }

        protected override bool CanCreate => type.IsAbstract == false 
            && type.IsGenericType == false 
            && (typeof(ScriptableObject).IsAssignableFrom(type) || typeof(MonoBehaviour).IsAssignableFrom(type));

        protected override IPromise<string> OnCreate()
        {
            return Promise<string>.Resolved(CreateAsset(type, fieldInfo.GetChildValueType()));
        }

        private static readonly Sorter sorter = new Sorter();

        private class Sorter : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                return CompareName(x, y);
            }
        }

        protected override void Reload()
        {
            assets.Clear();

            assets.AddRange(AddrHelper.GetAssets(type));

            if (fieldInfo != null)
            {
                LinkFilterAttribute interfaceFilter = fieldInfo.GetCustomAttribute<LinkFilterAttribute>();

                if (interfaceFilter != null)
                {
                    assets.RemoveAll(t =>
                    {
                        Object asset = AssetDatabase.LoadAssetAtPath(t.AssetPath, typeof(Object));
                        return interfaceFilter.interfaceType.IsAssignableFrom(asset.GetType()) == false;
                    });
                }

                LinkTypeFilterAttribute typeFilter = fieldInfo.GetCustomAttribute<LinkTypeFilterAttribute>();

                if (typeFilter != null)
                {
                    assets.RemoveAll(t =>
                    {
                        Object asset = AssetDatabase.LoadAssetAtPath(t.AssetPath, typeof(Object));
                        return asset.GetType() != typeFilter.type;
                    });
                }
            }

            
        }

    }

}