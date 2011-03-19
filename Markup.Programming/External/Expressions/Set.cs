﻿using System.Windows;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// The Set statement sets something to a value.  For all cases the value
    /// that is set is either Value or VariableName, optionally converted to
    /// Type.  The set value is always further modified by the optionally
    /// AssignmentOperator.  What remains is what gets set.  There are
    /// three modes:
    /// 
    /// - bare target
    /// - context with instance member (PropertyName, FieldName or DependencyProperty)
    /// - type with static member (StaticPropertyName or StaticFieldName)
    /// 
    /// If a bare target is specified and is assignable it will be set.
    /// If a target with an instance member is specified the target is an
    /// instance and the instance's member will be set.
    /// if a target with a static member is specified the target is is a
    /// type and the type's static member will be set.
    /// 
    /// There are three types of instance members:
    /// 
    /// - PropertyName: ordinary property
    /// - DependencyProperty: dependency property
    /// - FieldName: field
    /// 
    /// You only need to use DependencyProperty for attached properties
    /// since normal DependencyProperties are usually also normal
    /// properties.  The advantage is that you get completion; the
    /// disadvantage is that it doesn't know the target type so you
    /// have to specify the type and property.
    /// 
    /// The target itself specified by either Target or TargetParameterName.
    /// In the case of a bare target specified by Target, it must have a
    /// two-way binding to have any useful effect.  If it is a bare target
    /// and TargetParameterName is specified, it is as if VariableName
    /// were specified with the Let statement.  This is useful to
    /// assign the value of one parameter to another.
    /// </summary>
    public class Set : ValueExpression
    {
        public object Target
        {
            get { return (object)GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }

        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(object), typeof(Set), null);

        public string TargetPath { get; set; }

        private PathExpression targetPathExpression = new PathExpression();
        protected PathExpression TargetPathExpression { get { return targetPathExpression; } }

        public string Var { get; set; }

        public string PropertyName { get; set; }

        public DependencyProperty DependencyProperty { get; set; }

        public string FieldName { get; set; }

        public string StaticPropertyName { get; set; }

        public string StaticFieldName { get; set; }

        public AssignmentOp Op { get; set; }

        public string ValuePath { get; set; }

        private PathExpression valuePathExpression = new PathExpression();
        protected PathExpression ValuePathExpression { get { return valuePathExpression; } }

        private PathExpression setPathExpression = new PathExpression();
        protected PathExpression SetPathExpression { get { return setPathExpression; } }

        protected override object OnEvaluate(Engine engine)
        {
            var value = engine.Evaluate(ValueProperty, ValuePath, ValuePathExpression);
            if (Var != null)
            {
                var variable = "$" + Var;
                if (Op != AssignmentOp.Assign)
                {
                    var oldValue = engine.LookupVariable(variable);
                    value = engine.Evaluate(Op, oldValue, value);
                }
                engine.DefineVariableInParentScope(variable, value);
                return value;
            }
            if (IsBareTarget)
            {
                var target = engine.Evaluate(TargetProperty, TargetPath, TargetPathExpression);
                target = engine.Evaluate(Op, target, value);
                Target = value;
                return value;
            }
            var context = engine.Context;
            var type = engine.EvaluateType(TypeProperty, TypeName);
            if (Op != AssignmentOp.Assign)
            {
                object oldValue = null;
                if (DependencyProperty != null)
                    oldValue = PathHelper.GetDependencyProperty(engine, context as DependencyObject, DependencyProperty);
                else if (PropertyName != null)
                    oldValue = PathHelper.GetProperty(engine, context, PropertyName);
                else if (FieldName != null)
                    oldValue = PathHelper.GetField(engine, context, FieldName);
                else if (StaticPropertyName != null)
                    oldValue = PathHelper.GetStaticProperty(engine, type, StaticPropertyName);
                else if (StaticFieldName != null)
                    oldValue = PathHelper.GetStaticField(engine, type, StaticFieldName);
                else if (Path != null)
                    oldValue = engine.GetPath(Path, PathExpression);
                else
                    oldValue = context;
                value = engine.Evaluate(Op, oldValue, value);
            }
            if (DependencyProperty != null)
                PathHelper.SetDependencyProperty(engine, context as DependencyObject, DependencyProperty, value);
            else if (PropertyName != null)
                PathHelper.SetProperty(engine, context, PropertyName, value);
            else if (FieldName != null)
                PathHelper.SetField(engine, context, FieldName, value);
            else if (StaticPropertyName != null)
                PathHelper.SetStaticProperty(engine, type, StaticPropertyName, value);
            else if (StaticFieldName != null)
                PathHelper.SetStaticField(engine, type, StaticFieldName, value);
            else if (Path != null)
                engine.SetPath(Path, SetPathExpression, value);
            else
                Context = value;
            return value;
        }

        private bool IsBareTarget
        {
            get
            {
                if (DependencyProperty != null) return false;
                if (PropertyName != null) return false;
                if (StaticPropertyName != null) return false;
                if (FieldName != null) return false;
                if (StaticFieldName != null) return false;
                if (Path != null) return false;
                return true;
            }
        }
    }
}