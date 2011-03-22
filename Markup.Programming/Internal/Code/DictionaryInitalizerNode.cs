﻿using System.Collections;
using System.Collections.Generic;

namespace Markup.Programming.Core
{
    public class DictionaryInitializerNode : ExpressionNode
    {
        public ExpressionNode Context { get; set; }
        public ExpressionNode Dictionary { get; set; }
        public IList<ExpressionNode> Items { get; set; }
        protected override object OnEvaluate(Engine engine)
        {
            var dictionary = Dictionary.Evaluate(engine) as IDictionary;
            foreach (var item in Items)
            {
                var entry = (DictionaryEntry)item.Evaluate(engine);
                dictionary.Add(entry.Key, entry.Value);
            }
            return Context == Dictionary ? dictionary : Context.Evaluate(engine);
        }
    }
}