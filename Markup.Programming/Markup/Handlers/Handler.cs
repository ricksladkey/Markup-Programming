﻿using Markup.Programming.Core;
using System.Windows;

namespace Markup.Programming
{
    public class Handler : HandlerBase
    {
        public string Path
        {
            get { return (string)GetValue(PathProperty); }
            set { SetValue(PathProperty, value); }
        }

        public static readonly DependencyProperty PathProperty =
            DependencyProperty.Register("Path", typeof(string), typeof(Handler), null);

        protected override void OnAttached()
        {
            new Engine().With(this, engine => RegisterHandler(engine, Path));
        }

        protected override void OnHandler(Engine engine)
        {
            Body.Execute(engine);
        }
    }
}
