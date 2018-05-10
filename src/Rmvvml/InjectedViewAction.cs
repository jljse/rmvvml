using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Rmvvml
{
    /// <summary>
    /// ViewModelからViewへ揮発性のイベントを通知するためのクラス
    /// 
    /// 使用方法
    /// 1. ViewにInjectedViewAction型の添付プロパティを定義します
    /// 2. プロパティ変更イベントの中で、InjectedViewActionHandler.Injectで実行したい処理を登録します
    /// 3. ViewModelにInjectedViewAction型のプロパティを定義します
    /// 4. XAMLで1の添付プロパティと3のプロパティをバインドします
    /// 5. ViewModelからInvokeメソッドを呼ぶことで、2の処理が実行されるようになります
    /// </summary>
    /// <typeparam name="TParam">通知パラメータの型</typeparam>
    public class InjectedViewAction<TParam>
    {
        // Viewで実行する処理の集合
        // 役割は単なるdelegateと同じですが、以下の点が違います
        // - Viewの強参照を持たない
        // - lambda式を登録してもremoveできる方法を提供している
        List<IInjectedViewActionHandler<TParam>> Handlers { get; } = new List<IInjectedViewActionHandler<TParam>>();

        /// <summary>
        /// View側からハンドラを登録する
        /// </summary>
        /// <param name="handler"></param>
        public void Add(IInjectedViewActionHandler<TParam> handler)
        {
            GC();
            // System.Diagnostics.Debug.WriteLine("InjectedViewAction.Add add handler");
            Handlers.Add(handler);
        }

        /// <summary>
        /// View側から指定したハンドラを取り除く
        /// </summary>
        /// <param name="attachedPropertyName"></param>
        /// <param name="receiverControl"></param>
        public void Remove(string attachedPropertyName, object obj)
        {
            for (int i = 0; i < Handlers.Count; ++i)
            {
                var handler = Handlers[i];
                if (handler.IsSameReceiver(attachedPropertyName, obj))
                {
                    // System.Diagnostics.Debug.WriteLine("InjectedViewAction.Remove remove handler");
                    Handlers.RemoveAt(i);
                }
            }
        }

        // 死んでいるハンドラを削除する
        void GC()
        {
            for(int i = Handlers.Count - 1; i >= 0; --i)
            {
                var handler = Handlers[i];
                if(!handler.IsAlive())
                {
                    // System.Diagnostics.Debug.WriteLine("InjectedViewAction.GC remove handler");
                    Handlers.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Viewでの操作を実行
        /// </summary>
        /// <param name="parameter"></param>
        public async Task Invoke(TParam parameter)
        {
            foreach(var handler in Handlers)
            {
                await handler.Invoke(parameter);
            }
        }
    }

    /// <summary>
    /// ViewModelからViewへの通知のために使用するクラス
    /// 通知引数が不要な場合用
    /// </summary>
    public class InjectedViewAction : InjectedViewAction<object>
    {
        /// <summary>
        /// Viewへの通知を実行します
        /// </summary>
        public async Task Invoke()
        {
            await Invoke(null);
        }
    }

    /// <summary>
    /// View側の通知受付インターフェイス
    /// </summary>
    /// <typeparam name="TParam"></typeparam>
    public interface IInjectedViewActionHandler<TParam>
    {
        /// <summary>
        /// 内部で紐づいている添付プロパティ名とコントロールが一致するかどうか
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        bool IsSameReceiver(string attachedPropertyName , object obj);

        /// <summary>
        /// 内部で紐づいているコントロールが生きているかどうか
        /// </summary>
        /// <returns></returns>
        bool IsAlive();

        /// <summary>
        /// Viewでの操作を実行
        /// </summary>
        /// <param name="parameter"></param>
        Task Invoke(TParam parameter);
    }

    /// <summary>
    /// ViewModelからViewへの通知を受け取り、処理を実行するクラス
    /// </summary>
    /// <typeparam name="TControl">Viewの型</typeparam>
    /// <typeparam name="TParam">通知パラメータ</typeparam>
    public class InjectedViewActionHandler<TControl, TParam> : IInjectedViewActionHandler<TParam>
        where TControl : FrameworkElement
    {
        InjectedViewActionHandler(string attachedPropertyName, TControl control, Action<TControl, TParam> action)
        {
            AttachedPropertyName = attachedPropertyName;
            View = new WeakReference(control);
            Action = action;
        }

        // コントロールの寿命を延ばしたくないので弱参照
        WeakReference View { get; set; }
        // 紐づいている処理
        Action<TControl, TParam> Action { get; set; }
        // 生成元の添付プロパティ名
        string AttachedPropertyName { get; set; }

        /// <summary>
        /// Viewでの操作を実行
        /// </summary>
        /// <param name="parameter"></param>
        public async Task Invoke(TParam parameter)
        {
            if (!View.IsAlive) return;
            var control = View.Target as TControl;
            if (control == null) return;
            if (Action == null) return;

            if (Application.Current.Dispatcher.Thread == Thread.CurrentThread)
            {
                // 元々UIスレッドなのでそのまま実行 (こっちの方がデバッガで追いやすいので)
                Action.Invoke(control, parameter);
            }
            else
            {
                // コントロールを触るのでUIスレッドを使う
                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    Action.Invoke(control, parameter);
                }));
            }
        }

        // このハンドラがまだ生きているかチェックする
        public bool IsAlive()
        {
            return View.IsAlive;
        }

        // 同じハンドラかどうかをチェックする
        public bool IsSameReceiver(string attachedPropertyName, object obj)
        {
            if (attachedPropertyName != AttachedPropertyName)
            {
                return false;
            }

            if(View.IsAlive)
            {
                return View.Target == obj;
            }
            else
            {
                // 同じオブジェクトなら、objの参照が生きているのにAliveでなくなるはずがない
                return false;
            }
        }

        /// <summary>
        /// AttachedPropertyActionに、通知時に実行するViewの処理を埋め込みます
        /// </summary>
        /// <param name="attachedProperty"></param>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <param name="action"></param>
        public static void Inject(DependencyProperty attachedProperty, DependencyObject d, DependencyPropertyChangedEventArgs e, Action<TControl, TParam> action)
        {
            var control = d as TControl;
            if (control == null) return;

            var oldVal = e.OldValue as InjectedViewAction<TParam>;
            if (oldVal != null)
            {
                // 古いハンドラを削除
                oldVal.Remove(attachedProperty.Name, control);
            }
            var newVal = e.NewValue as InjectedViewAction<TParam>;
            if (newVal != null)
            {
                // 新しいハンドラを登録
                var handler = new InjectedViewActionHandler<TControl, TParam>(attachedProperty.Name, control, action);
                newVal.Add(handler);
            }
        }
    }

}
