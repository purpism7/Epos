using System.Collections;
using System.Collections.Generic;
using Creature;
using UnityEngine;


public class Field : Part, IField
{
    [SerializeField] 
    private FieldPoint[] fieldPoints = null;

    #region IField
    void IField.Initialize()
    {
        Initialize();
    }
    
    void IField.Activate()
    {
        ActivateFieldPoints();
    }

    void IField.Deactivate()
    {
        DeactivateFieldPoints();
    }
    
    void IField.ChainUpdate()
    {
        if (fieldPoints == null)
            return;

        foreach (var fieldPoint in fieldPoints)
        {
            fieldPoint?.ChainUpdate();
        }
    }
    #endregion

    private void Initialize()
    {
        if (fieldPoints == null)
            return;

        foreach (var fieldPoint in fieldPoints)
        {
            fieldPoint?.Initialize();
        }
    }
    
    private void ActivateFieldPoints()
    {
        if (fieldPoints == null)
            return;

        foreach (var fieldPoint in fieldPoints)
        {
            fieldPoint?.Activate();
        }
    }
    
    private void DeactivateFieldPoints()
    {
        if (fieldPoints == null)
            return;

        foreach (var fieldPoint in fieldPoints)
        {
            fieldPoint?.Deactivate();
        }
    }
}

public interface IField
{
    void Initialize();
    void Activate();
    void Deactivate();
    void ChainUpdate();
}