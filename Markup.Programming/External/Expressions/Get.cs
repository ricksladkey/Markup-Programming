using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using Markup.Programming.Core;

namespace Markup.Programming
{
    /// <summary>
    /// The Get expression has four modes:
    /// 
    /// - a parameter ParamterName
    /// - a bare source Source
    /// - a PropertyName, DependencyProperty, or FieldName of the context
    /// - a StaticPropertyName or StaticFieldName of type Source
    /// </summary>
    [ContentProperty("Context")]
    public class Get : TypedExpession
    {
        public object Source
        {
            get { return (object)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(Get), null);

        public string Var { get; set; }

        public string PropertyName { get; set; }

        public string FieldName { get; set; }

        public DependencyProperty DependencyProperty { get; set; }

        public string StaticPropertyName { get; set; }

        public string StaticFieldName { get; set; }

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ContextProperty);
        }

        protected override object OnEvaluate(Engine engine)
        {
            if (Var != null) return engine.LookupVariable("$" + Var);
            var context = engine.GetContext(Path, PathExpression);
            var type = engine.EvaluateType(TypeProperty, TypeName);
            if (PropertyName != null)
                return PathHelper.GetProperty(engine, context, PropertyName);
            if (FieldName != null)
                return PathHelper.GetField(engine, context, FieldName);
            if (DependencyProperty != null)
                return PathHelper.GetDependencyProperty(engine, context as DependencyObject, DependencyProperty);
            if (StaticPropertyName != null)
                return PathHelper.GetStaticProperty(engine, type, StaticPropertyName);
            if (StaticFieldName != null)
                return PathHelper.GetStaticField(engine, type, StaticFieldName);
            if (Path != null)
                return context;
            return engine.Evaluate(SourceProperty);
        }
    }
}
