Markup.Programming extends XAML to a full programming language in markup: control flow, expressions, functions, objects, commands, converters, events, and more. Write an MVVM prototype in markup-only or use it for enhanced interactivity.

Features for WPF4 and Silverlight4:

- Expressions: operators, CLR methods/properties/fields, conversions, bindings
- Parameters: define, store and reference scoped values by name
- Control flow: If/Then/Else, For, ForEach, While, When, and Break
- Modularity: Function, Call and Return, Module and Import complete with arguments
- Instantiation: New with constructor arguments including generic type parameters
- MarkupObjects: like a dynamic data store but with no code-behind
- Commands and CommandBindings: defined and referenced all in markup
- Converters and MultiConverters: all in markup

This means you can:

- create an instance of ObservableCollection<string>()
- call a method on Keyboard.FocusedElement
- move an item from one collection to another
- mark an event as handled
- or even simply create an object with two properties called Name and Address that implements INotifyPropertyChanged
