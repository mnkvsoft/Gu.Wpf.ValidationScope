﻿namespace Gu.Wpf.ValidationScope
{
    using System;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Windows;
    using System.Windows.Controls;

    public static partial class Scope
    {
#pragma warning disable SA1202 // Elements must be ordered by access
        private static readonly ReadOnlyObservableCollection<ValidationError> EmptyErrorsCollection = new ReadOnlyObservableCollection<ValidationError>(new ObservableBatchCollection<ValidationError>());

        /// <summary>The error event is raised even if the bindings does not notify.</summary>
        public static readonly RoutedEvent ErrorEvent = EventManager.RegisterRoutedEvent(
            "ValidationError",
            RoutingStrategy.Bubble,
            typeof(EventHandler<ValidationErrorEventArgs>),
            typeof(Scope));

        public static readonly DependencyProperty ForInputTypesProperty = DependencyProperty.RegisterAttached(
            "ForInputTypes",
            typeof(InputTypeCollection),
            typeof(Scope),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.Inherits,
                OnScopeForChanged));

        private static readonly DependencyPropertyKey HasErrorPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "HasError",
            typeof(bool),
            typeof(Scope),
            new PropertyMetadata(BooleanBoxes.False, OnHasErrorChanged));

        public static readonly DependencyProperty HasErrorProperty = HasErrorPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ErrorsPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "Errors",
            typeof(ReadOnlyObservableCollection<ValidationError>),
            typeof(Scope),
            new PropertyMetadata(EmptyErrorsCollection, OnErrorsChanged));

        public static readonly DependencyProperty ErrorsProperty = ErrorsPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey NodePropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "Node",
            typeof(IErrorNode),
            typeof(Scope),
            new PropertyMetadata(default(IErrorNode), OnNodeChanged));

        public static readonly DependencyProperty NodeProperty = NodePropertyKey.DependencyProperty;

        /// <summary>
        ///     Adds a handler for the ValidationError attached event
        /// </summary>
        /// <param name="element">UIElement or ContentElement that listens to this event</param>
        /// <param name="handler">Event Handler to be added</param>
        public static void AddErrorHandler(DependencyObject element, EventHandler<ValidationErrorEventArgs> handler)
        {
            (element as UIElement)?.AddHandler(ErrorEvent, handler);
            (element as ContentElement)?.AddHandler(ErrorEvent, handler);
        }

        /// <summary>
        ///     Removes a handler for the ValidationError attached event
        /// </summary>
        /// <param name="element">UIElement or ContentElement that listens to this event</param>
        /// <param name="handler">Event Handler to be removed</param>
        public static void RemoveErrorHandler(DependencyObject element, EventHandler<ValidationErrorEventArgs> handler)
        {
            (element as UIElement)?.RemoveHandler(ErrorEvent, handler);
            (element as ContentElement)?.RemoveHandler(ErrorEvent, handler);
        }

        public static void SetForInputTypes(this UIElement element, InputTypeCollection value) => element.SetValue(ForInputTypesProperty, value);

        [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static InputTypeCollection GetForInputTypes(DependencyObject element) => (InputTypeCollection)element.GetValue(ForInputTypesProperty);

        private static void SetHasErrors(DependencyObject element, bool value) => element.SetValue(HasErrorPropertyKey, BooleanBoxes.Box(value));

        [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static bool GetHasError(UIElement element) => (bool)element.GetValue(HasErrorProperty);

        private static void SetErrors(this DependencyObject element, ReadOnlyObservableCollection<ValidationError> value)
        {
            element.SetValue(ErrorsPropertyKey, value);
        }

        [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static ReadOnlyObservableCollection<ValidationError> GetErrors(this DependencyObject element)
        {
            return (ReadOnlyObservableCollection<ValidationError>)element.GetValue(ErrorsProperty);
        }

        internal static void SetNode(DependencyObject element, IErrorNode value) => element.SetValue(NodePropertyKey, value);

        [AttachedPropertyBrowsableForChildren(IncludeDescendants = false)]
        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static IErrorNode GetNode(DependencyObject element) => (IErrorNode)element.GetValue(NodeProperty);

#pragma warning restore SA1202 // Elements must be ordered by access

        private static void OnScopeForChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (((InputTypeCollection)e.NewValue)?.IsInputType(d) == true)
            {
                if (GetNode(d) == null)
                {
                    SetNode(d, ErrorNode.CreateFor(d));
                }
            }
            else if (((InputTypeCollection)e.OldValue)?.IsInputType(d) == true)
            {
                d.ClearValue(NodePropertyKey);
            }
        }

        private static void OnNodeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var oldNode = e.OldValue as IErrorNode;
            if (oldNode != null)
            {
                oldNode.Dispose();
                CollectionChangedEventManager.RemoveHandler(oldNode, OnNodeErrorsChanged);
            }

            var newNode = (IErrorNode)e.NewValue;
            if (newNode != null)
            {
                if (newNode.HasErrors)
                {
                    SetErrors(d, newNode.Errors);
                    SetHasErrors(d, newNode.HasErrors);
                }
                else
                {
                    SetErrors(d, EmptyErrorsCollection);
                    SetHasErrors(d, newNode.HasErrors);
                }

                CollectionChangedEventManager.AddHandler(newNode, OnNodeErrorsChanged);
                (e.NewValue as ErrorNode)?.BindToSourceErrors();
            }
            else
            {
                SetHasErrors(d, false);
                SetErrors(d, EmptyErrorsCollection);
            }

            d.SetCurrentValue(NodeProxyProperty, e.NewValue);
        }

        private static void OnNodeErrorsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

        private static void OnHasErrorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetCurrentValue(HasErrorProxyProperty, e.NewValue);
        }

        private static void OnErrorsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            d.SetCurrentValue(ErrorsProxyProperty, e.NewValue);
        }
    }
}
