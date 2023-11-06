using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;

namespace NetworkTool.WPF.Behaviors;

public class AutoScrollBehavior : Behavior<ListBox>
{
    private INotifyCollectionChanged? _oldCollection;

    protected override void OnAttached()
    {
        base.OnAttached();
        var itemsSourceProperty =
            DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ListBox));
        itemsSourceProperty.AddValueChanged(AssociatedObject, ItemsSourceChanged!);
        AttachCollectionChanged(AssociatedObject.ItemsSource as INotifyCollectionChanged);
    }

    private void ItemsSourceChanged(object sender, EventArgs e)
    {
        AttachCollectionChanged(AssociatedObject.ItemsSource as INotifyCollectionChanged);
    }

    private void AttachCollectionChanged(INotifyCollectionChanged? newCollection)
    {
        if (_oldCollection != null) _oldCollection.CollectionChanged -= OnCollectionChanged!;

        if (newCollection != null) newCollection.CollectionChanged += OnCollectionChanged!;

        _oldCollection = newCollection;
    }

    protected override void OnDetaching()
    {
        var itemsSourceProperty =
            DependencyPropertyDescriptor.FromProperty(ItemsControl.ItemsSourceProperty, typeof(ListBox));
        itemsSourceProperty.RemoveValueChanged(AssociatedObject, ItemsSourceChanged!);

        if (_oldCollection != null) _oldCollection.CollectionChanged -= OnCollectionChanged!;

        base.OnDetaching();
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Add) return;
        var newItem = e.NewItems?[0];
        if (newItem != null) AssociatedObject.ScrollIntoView(newItem);
    }
}