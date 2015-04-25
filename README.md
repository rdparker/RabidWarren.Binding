# RabidWarren.Binding (RWB)

Provides a property binding system implemented as a Portable Class Library.
It is meant to be independent of WPF, Xamarin and other user-interface
frameworks, while working with them in a portable manner.

## Basic Binding

Properties may be bound using either text names or expressions such as

    _view.Bind("Text", _viewModel, "Text");

or

    _view.Bind(v => v.Text, _viewModel, vm => vm.Text);

Using expressions provides a safer way of passing property names without
risking typos in the names.

## CanRead and CanWrite Pseudo-Properties

There are two pseudo-properties implemented in RWB, CanRead and CanWrite.  They
allow for checking whether or not a property is readable or writeable.  This
can be used to adjust the state of control depending upon a properties
accessibility.  For example,

    _view.Bind(this, "Text.IsEnabled", _viewModel, "Text.CanWrite");

could be used to enable a text field only if its View Model property is
writeable.  When using expressions, the CanWrite pseudo-property is accessed
using the CanWrite() function.

    _view.Bind(v => v.Text.IsEnabled, _viewModel, vm => vm.Text.CanWrite());

This is because

    vm => vm.Text.CanWrite

is a syntax error at compile time.  Also, when using expressions, there is no
CanRead property.  Attempting to access a write-only property would also
generate a compile-time error, making CanRead() unnecessary.

## Expression Composition

RabidWarren.Binding also supports expression composition, which can simplify
binding many similar items.  For example, it would be nice to be able to write
something like

```c#
void InitializeBindings()
{
	Bind(mw => mw.NumericTextBox, vm => vm.Number);
	Bind(mw => mw.StringTextBox, vm => vm.Text);
	Bind(mw => mw.ComputedTextBox, vm => vm.Computed);
}
```

to bind a set of WPF TextBoxes to various types of ViewModel properties.

This could be accomplished using this Bind function:

```c#
void Bind<T>(Expression<Func<MainWindow, TextBox>> target, Expression<Func<ViewModel, T>> source)
{
	var textBox = target.Compile().Invoke(this);
	var textProperty = textBox.Name + ".Text";
	
	_binding.Bind(this, target.Compose(tb => tb.Text), _viewModel, source);
	_binding.Bind(this, target.Compose(tb => tb.IsEnabled), _viewModel, source.Compose(s => s.CanWrite()));

	textBox.TextChanged +=
		(object sender, TextChangedEventArgs e) => OnPropertyChangedEvent(textProperty);
}
```

The Invoke gets the actual TextBox which is being bound.  It is then used later
to fire the correct OnPropertyChangedEvent when the TextBoxes Text changes.

The two _binding.Bind calls use composition to access sub-properties of both
the source and target. In this case they bind various types of source
properties to the appropriate TextBox Text property and also set the TextBox
IsEnabled property in accordance with whether or not the source property can
be written to.

Internally RabidWarren.Binding will generate composed expressions from the
supplied expression fragments.  For example, given

```c#
	mw => mw.NumericTextBox
```
	
and

```c#
	tb => tb.Text
```

RWB automatically produces and uses

```c#
	mw => mw.NumericTextBox.Text
```

in binding to the source properties.  Composing with CanWrite() results in a
constant expression rather than an actual function call, since the read-only
status of a property does not change at run time.

## TODO

Implement an add-on library for automatically binding to DependencyProperties
and similar framework types.