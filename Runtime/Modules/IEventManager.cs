﻿using System;
using UnityEngine;
using UnityEngine.Events;

namespace SwiftFramework.Core
{
    [ModuleGroup(ModuleGroups.Core)]
    public interface IEventManager : IModule
    {
        bool IsPointerHandledByUI { get; }
        void Register(IPhysics2DClick physics2DClick);
        bool IsPhysics2DClicked();
    }

    public interface IPhysics2DClick
    {
        bool IsPointerDown { get; set; }
        void OnClick(Vector3 point);
        void OnPointerDown(Vector3 point);
        void OnPointerUp(Vector3 point);

        GameObject gameObject { get; }
    }

    public interface IButton
    {
        bool Interactable { get; set; }
        void AddListener(UnityAction action);
    }

    public interface IAffordableHandler
    {
        void SetAfforfable(bool affordable);
    }

    public interface IImage
    {
        void SetSprite(SpriteLink sprite);
        void SetAlpha(float alpha);
    }

    public interface IText
    {
        Color Color { get; set; }
        string Text { get; set; }
        void SetAsync<T>(IPromise<T> promise);
        void SetAsync<T>(IPromise<T> promise, string format);
    }


    [Serializable]
    public class GenericImage : InterfaceComponentField<IImage> { }

    [Serializable]
    public class GenericButton : InterfaceComponentField<IButton> { }

    [Serializable]
    public class GenericText : InterfaceComponentField<IText> { }

    [Serializable]
    public class AffordableHandler : InterfaceComponentField<IAffordableHandler> { }

}