using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rmvvml.Sample
{
    class INPCListenerTest
    {
        public void Test()
        {
            A a = new A();

            //var y = new INPCListener(nameof(A.Y)) { Target = a, };
            //y.Subscribe(v => System.Diagnostics.Debug.WriteLine(v));

            //var z = new INPCListener(nameof(A.Z)) { Target = a, };
            //z.Subscribe(v => System.Diagnostics.Debug.WriteLine(v));
            var z = Observable.Return(this).Property(nameof(A.Z)).Subscribe(v => System.Diagnostics.Debug.WriteLine(v));

            a.Y = "Y1";
            a.Y = "Y2";

            //var x = new INPCListener(nameof(A.Model)) { Target = a, };
            //x.Child = new INPCListener(nameof(B.X));
            //x.Subscribe(v => System.Diagnostics.Debug.WriteLine(v));
            var x = Observable.Return(this)
                .Property(nameof(A.Model)).Property(nameof(B.X))
                .Subscribe(v => System.Diagnostics.Debug.WriteLine(v));

            var b1 = new B();
            a.Model = b1;
            b1.X = "X1";
            b1.X = "X2";

            var b2 = new B();
            a.Model = b2;
            b1.X = "X1";
            b1.X = "X2";
            b2.X = "X3";
            b2.X = "X4";

            var b3 = new B() { X = "X5" };
            a.Model = b3;


        }
    }

    class A : ViewModelBase
    {
        INPCListener _ZHandler = null;

        public A()
        {
            _ZHandler = new INPCListener(nameof(Model)) { Target = this, Child = new INPCListener(nameof(B.X)), };
            _ZHandler
                .Throttle(TimeSpan.FromMilliseconds(200))
                .Subscribe((v) => Z = "[" + Model?.X + "]");
        }

        #region Y
        string _Y;
        public string Y
        {
            get { return _Y; }
            set { Set(nameof(Y), ref _Y, value); }
        }
        #endregion

        #region Z
        string _Z;
        public string Z
        {
            get { return _Z; }
            set { Set(nameof(Z), ref _Z, value); }
        }
        #endregion

        #region Model
        B _Model;
        public B Model
        {
            get { return _Model; }
            set { Set(nameof(Model), ref _Model, value); }
        }
        #endregion
    }

    class B : ViewModelBase
    {
        #region X
        string _X;
        public string X
        {
            get { return _X; }
            set { Set(nameof(X), ref _X, value); }
        }
        #endregion
    }

    public static class ReactiveExtension
    {
        public static IObservable<object> Property(this IObservable<object> o, string propName)
        {
            var x = new INPCListener2(propName);
            o.Subscribe(x);
            return x;
        }
    }

    public class INPCListener2 : IObservable<object>, IObserver<object>
    {
        public string PropertyName { get; set; }
        public INotifyPropertyChanged Target { get; set; }

        List<IObserver<object>> Observers { get; } = new List<IObserver<object>>();

        public INPCListener2(string propName)
        {
            PropertyName = propName;
        }

        public void OnCompleted()
        {
            //throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            //throw new NotImplementedException();
        }

        public void OnNext(object value)
        {
            if (Target != null)
            {
                Target.PropertyChanged -= Target_PropertyChanged;
            }
            Target = value as INotifyPropertyChanged;
            if (Target != null)
            {
                Target.PropertyChanged += Target_PropertyChanged;
            }

            Target_PropertyChanged(Target, new PropertyChangedEventArgs(PropertyName));
        }

        private void Target_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == PropertyName)
            {
                object val = null;
                if (Target != null)
                {
                    val = Target.GetType().GetProperty(PropertyName).GetValue(Target);
                }
                foreach (var observer in Observers)
                {
                    observer.OnNext(val);
                }
            }
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            Observers.Add(observer);
            return new INPCListener2Disposer(this, observer);
        }

        public void Unsubscribe(IObserver<object> observer)
        {
            Observers.Remove(observer);
        }
    }

    public class INPCListener : ObservableObject, IObservable<object>, IObserver<object>
    {
        public INPCListener(string propName)
        {
            PropertyName = propName;
        }

        public string PropertyName { get; private set; }
        List<IObserver<object>> Observers { get; } = new List<IObserver<object>>();

        #region Target
        INotifyPropertyChanged _Target;
        public INotifyPropertyChanged Target
        {
            get { return _Target; }
            set
            {
                var old = _Target;
                if (Set(nameof(Target), ref _Target, value))
                {
                    if (old != null)
                    {
                        old.PropertyChanged -= Target_PropertyChanged;
                    }
                    if (_Target != null)
                    {
                        _Target.PropertyChanged += Target_PropertyChanged;
                    }
                    if (!string.IsNullOrEmpty(PropertyName))
                    {
                        Target_PropertyChanged(_Target, new PropertyChangedEventArgs(PropertyName));
                    }
                }
            }
        }
        #endregion

        #region Child
        INPCListener _Child;
        public INPCListener Child
        {
            get { return _Child; }
            set
            {
                var old = _Child;
                if (Set(nameof(Child), ref _Child, value))
                {
                    if (old != null)
                    {
                        old.Unsubscribe(this);
                    }
                    if (_Child != null)
                    {
                        _Child.Subscribe(this);
                    }
                }
            }
        }
        #endregion

        private void Target_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == PropertyName)
            {
                if (Target == null)
                {
                    OnNext(null);
                }

                var value = Target.GetType().GetProperty(PropertyName).GetValue(Target);
                if (Child == null)
                {
                    OnNext(value);
                }
                else
                {
                    Child.Target = value as INotifyPropertyChanged;
                }
            }
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(object value)
        {
            foreach (var observer in Observers)
            {
                observer.OnNext(value);
            }
        }

        public IDisposable Subscribe(Action<object> onNext)
        {
            var observer = new ObserverAction(onNext);
            Observers.Add(observer);
            return new INPCListenerDisposer(this, observer);
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            Observers.Add(observer);
            return new INPCListenerDisposer(this, observer);
        }

        public void Unsubscribe(IObserver<object> observer)
        {
            Observers.Remove(observer);
        }
    }

    public class INPCListenerDisposer : IDisposable
    {
        public INPCListenerDisposer(INPCListener listener, IObserver<object> observer)
        {
            Listener = listener;
            Observer = observer;
        }

        INPCListener Listener { get; set; }
        IObserver<object> Observer { get; set; }

        public void Dispose()
        {
            Listener.Unsubscribe(Observer);
        }
    }

    public class INPCListener2Disposer : IDisposable
    {
        public INPCListener2Disposer(INPCListener2 listener, IObserver<object> observer)
        {
            Listener = listener;
            Observer = observer;
        }

        INPCListener2 Listener { get; set; }
        IObserver<object> Observer { get; set; }

        public void Dispose()
        {
            Listener.Unsubscribe(Observer);
        }
    }

    public class ObserverAction : IObserver<object>
    {
        Action<object> OnNextAction { get; set; }

        public ObserverAction(Action<object> onNext)
        {
            OnNextAction = onNext;
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(object value)
        {
            OnNextAction?.Invoke(value);
        }
    }
}
