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

        public string SourcePath { get; set; }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(Get), null);

        public string ParameterName
        {
            get { return (string)GetValue(ParameterNameProperty); }
            set { SetValue(ParameterNameProperty, value); }
        }

        public static readonly DependencyProperty ParameterNameProperty =
            DependencyProperty.Register("ParameterName", typeof(string), typeof(Get), null);

        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }

        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.Register("PropertyName", typeof(string), typeof(Get), null);

        public string FieldName
        {
            get { return (string)GetValue(FieldNameProperty); }
            set { SetValue(FieldNameProperty, value); }
        }

        public static readonly DependencyProperty FieldNameProperty =
            DependencyProperty.Register("FieldName", typeof(string), typeof(Get), null);

        public DependencyProperty DependencyProperty
        {
            get { return (DependencyProperty)GetValue(DependencyPropertyProperty); }
            set { SetValue(DependencyPropertyProperty, value); }
        }

        public static readonly DependencyProperty DependencyPropertyProperty =
            DependencyProperty.Register("DependencyProperty", typeof(DependencyProperty), typeof(Get), null);

        public string StaticPropertyName
        {
            get { return (string)GetValue(StaticPropertyNameProperty); }
            set { SetValue(StaticPropertyNameProperty, value); }
        }

        public static readonly DependencyProperty StaticPropertyNameProperty =
            DependencyProperty.Register("StaticPropertyName", typeof(string), typeof(Get), null);

        public string StaticFieldName
        {
            get { return (string)GetValue(StaticFieldNameProperty); }
            set { SetValue(StaticFieldNameProperty, value); }
        }

        public static readonly DependencyProperty StaticFieldNameProperty =
            DependencyProperty.Register("StaticFieldName", typeof(string), typeof(Get), null);

        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(Get), null);

        protected override void OnAttached()
        {
            base.OnAttached();
            Attach(ContextProperty);
        }

        protected override object OnEvaluate(Engine engine)
        {
            if (ParameterName != null) return engine.LookupParameter(ParameterName);
            var context = engine.GetContext(Path);
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
            if (Source != null)
                return engine.Evaluate(SourceProperty, SourcePath);
            return engine.Throw("nothing to get");
        }
    }
}
