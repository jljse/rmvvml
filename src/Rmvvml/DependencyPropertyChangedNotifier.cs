using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Rmvvml
{
    /// <summary>
    /// DependencyPropertyの変更通知を監視するためのオブジェクト
    /// 監視対象の参照は弱参照
    /// </summary>
    public class DependencyPropertyChangedNotifier : DependencyObject
    {
        WeakReference<DependencyObject> Target { get; set; }
        DependencyProperty Property { get; set; }

        /// <summary>
        /// DependencyPropertyの値変更を監視します
        /// </summary>
        /// <param name="target"></param>
        /// <param name="prop"></param>
        public DependencyPropertyChangedNotifier(DependencyObject target, DependencyProperty prop)
        {
            Target = new WeakReference<DependencyObject>(target);
            Property = prop;

            var binding = new Binding()
            {
                Source = target,
                Path = new PropertyPath(prop.Name),
            };
            BindingOperations.SetBinding(this, ValueProperty, binding);
        }

        #region Value
        object Value
        {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// DependencyPropertyの変更を検知するためのバインド用プロパティ
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(DependencyPropertyChangedNotifier), new PropertyMetadata(null, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = d as DependencyPropertyChangedNotifier;
            obj.OnValueChanged(e);
        }

        void OnValueChanged(DependencyPropertyChangedEventArgs e)
        {
            DependencyObject target;
            if (Target.TryGetTarget(out target))
            {
                ValueChanged?.Invoke(
                    this,  // これはthisにしないとWeakEventManagerで使えない
                    new DerivedDependencyPropertyChangedEventArgs(target, e.Property, e.OldValue, e.NewValue));
            }
        }
        #endregion

        /// <summary>
        /// 監視対象プロパティの値が変更された場合のイベント
        /// senderはこのオブジェクトを指す (監視対象のDependencyObjectではないので注意)
        /// </summary>
        public event EventHandler<DerivedDependencyPropertyChangedEventArgs> ValueChanged;
    }

    /// <summary>
    /// DependencyPropertyChangedEventArgsと同じ
    /// 違うのはEventArgsの派生クラスであること、変更された親オブジェクトの参照を持つこと
    /// </summary>
    public class DerivedDependencyPropertyChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 親オブジェクト
        /// </summary>
        public DependencyObject Target { get; private set; }
        /// <summary>
        /// 変更されたプロパティ
        /// </summary>
        public DependencyProperty Property { get; private set; }
        /// <summary>
        /// 変更前のプロパティの値
        /// </summary>
        public object OldValue { get; private set; }
        /// <summary>
        /// 変更後のプロパティの値
        /// </summary>
        public object NewValue { get; private set; }

        public DerivedDependencyPropertyChangedEventArgs(DependencyObject target, DependencyProperty property, object oldValue, object newValue)
        {
            Target = target;
            Property = property;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    /// <summary>
    /// DependencyPropertyChangedNotifierのイベントをWeakEventPatternで使用するためのクラス
    /// 以下のテンプレートをそのまま使用
    /// https://docs.microsoft.com/en-us/dotnet/framework/wpf/advanced/weak-event-patterns
    /// </summary>
    public class DependencyPropertyChangedNotifierEventManager : WeakEventManager
    {
        private DependencyPropertyChangedNotifierEventManager()
        {

        }

        /// <summary>
        /// Add a handler for the given source's event.
        /// </summary>
        public static void AddHandler(DependencyPropertyChangedNotifier source,
                                      EventHandler<DerivedDependencyPropertyChangedEventArgs> handler)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (handler == null)
                throw new ArgumentNullException("handler");

            CurrentManager.ProtectedAddHandler(source, handler);
        }

        /// <summary>
        /// Remove a handler for the given source's event.
        /// </summary>
        public static void RemoveHandler(DependencyPropertyChangedNotifier source,
                                         EventHandler<DerivedDependencyPropertyChangedEventArgs> handler)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (handler == null)
                throw new ArgumentNullException("handler");

            CurrentManager.ProtectedRemoveHandler(source, handler);
        }

        /// <summary>
        /// Get the event manager for the current thread.
        /// </summary>
        private static DependencyPropertyChangedNotifierEventManager CurrentManager
        {
            get
            {
                Type managerType = typeof(DependencyPropertyChangedNotifierEventManager);
                DependencyPropertyChangedNotifierEventManager manager =
                    (DependencyPropertyChangedNotifierEventManager)GetCurrentManager(managerType);

                // at first use, create and register a new manager
                if (manager == null)
                {
                    manager = new DependencyPropertyChangedNotifierEventManager();
                    SetCurrentManager(managerType, manager);
                }

                return manager;
            }
        }



        /// <summary>
        /// Return a new list to hold listeners to the event.
        /// </summary>
        protected override ListenerList NewListenerList()
        {
            return new ListenerList<DerivedDependencyPropertyChangedEventArgs>();
        }


        /// <summary>
        /// Listen to the given source for the event.
        /// </summary>
        protected override void StartListening(object source)
        {
            DependencyPropertyChangedNotifier typedSource = (DependencyPropertyChangedNotifier)source;
            typedSource.ValueChanged += new EventHandler<DerivedDependencyPropertyChangedEventArgs>(OnSomeEvent);
        }

        /// <summary>
        /// Stop listening to the given source for the event.
        /// </summary>
        protected override void StopListening(object source)
        {
            DependencyPropertyChangedNotifier typedSource = (DependencyPropertyChangedNotifier)source;
            typedSource.ValueChanged -= new EventHandler<DerivedDependencyPropertyChangedEventArgs>(OnSomeEvent);
        }

        /// <summary>
        /// Event handler for the SomeEvent event.
        /// </summary>
        void OnSomeEvent(object sender, DerivedDependencyPropertyChangedEventArgs e)
        {
            DeliverEvent(sender, e);
        }
    }
}
