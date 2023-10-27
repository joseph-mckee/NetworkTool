using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;

namespace NetworkTool.WPF.Behaviors;

public class AutoScrollBehavior : Behavior<ListBox>
{
    private INotifyCollectionChanged oldCollection;

    protected override void OnAttached()
    {
        base.OnAttached();
        DependencyPropertyDescriptor itemsSourceProperty = DependencyPropertyDescriptor.FromProperty(ListBox.ItemsSourceProperty, typeof(ListBox));
        itemsSourceProperty.AddValueChanged(AssociatedObject, ItemsSourceChanged);
        AttachCollectionChanged(AssociatedObject.ItemsSource as INotifyCollectionChanged);
    }

    private void ItemsSourceChanged(object sender, EventArgs e)
    {
        AttachCollectionChanged(AssociatedObject.ItemsSource as INotifyCollectionChanged);
    }

    private void AttachCollectionChanged(INotifyCollectionChanged newCollection)
    {
        if (oldCollection != null)
        {
            oldCollection.CollectionChanged -= OnCollectionChanged;
        }

        if (newCollection != null)
        {
            newCollection.CollectionChanged += OnCollectionChanged;
        }

        oldCollection = newCollection;
    }

    protected override void OnDetaching()
    {
        DependencyPropertyDescriptor itemsSourceProperty = DependencyPropertyDescriptor.FromProperty(ListBox.ItemsSourceProperty, typeof(ListBox));
        itemsSourceProperty.RemoveValueChanged(AssociatedObject, ItemsSourceChanged);

        if (oldCollection != null)
        {
            oldCollection.CollectionChanged -= OnCollectionChanged;
        }

        base.OnDetaching();
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            var newItem = e.NewItems[0];
            AssociatedObject.ScrollIntoView(newItem);
        }
    }
}
