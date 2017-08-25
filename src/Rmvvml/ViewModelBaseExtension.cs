using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rmvvml
{
    /// <summary>
    /// プロパティが依存する他のプロパティを指定します
    /// PropertyChangedが連動して発生するようになります
    /// </summary>
    public class DependsOnAttribute : Attribute
    {
        public string PropertyName;
        public string NestedPropertyName;

        /// <summary>
        /// 他のプロパティに依存して変化する
        /// </summary>
        /// <param name="propertyName"></param>
        public DependsOnAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        /// 入れ子になった propertyName.nestedPropertyName に依存して変化する
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="nestedPropertyName"></param>
        public DependsOnAttribute(string propertyName, string nestedPropertyName)
        {
            PropertyName = propertyName;
            NestedPropertyName = nestedPropertyName;
        }
    }

    /// <summary>
    /// コマンド実行可否が依存する他のプロパティを指定します
    /// CanExecuteChangedが連動して発生するようになります
    /// </summary>
    public class CanExecuteDependsOnAttribute : Attribute
    {
        public string PropertyName;
        public string NestedPropertyName;

        public CanExecuteDependsOnAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        public CanExecuteDependsOnAttribute(string propertyName, string nestedPropertyName)
        {
            PropertyName = propertyName;
            NestedPropertyName = nestedPropertyName;
        }
    }

    /// <summary>
    /// 1プロパティに関連した依存の情報
    /// </summary>
    class DependencyForProperty
    {
        /// <summary>
        /// 変更元に連動して変化するプロパティ
        /// </summary>
        public List<string> Dependencies { get; } = new List<string>();
        /// <summary>
        /// 変更元に連動して変化するコマンド実行可否
        /// </summary>
        public List<string> CanExeDependencies { get; } = new List<string>();

        /// <summary>
        /// 入れ子になっている依存
        /// NestedDependencies["XXX"] は、変更元.XXX に対して依存しているプロパティ名
        /// </summary>
        public Dictionary<string, DependencyForProperty> NestedDependencies { get; } = new Dictionary<string, DependencyForProperty>();
    }

    /// <summary>
    /// プロパティ間の依存関係の情報
    /// </summary>
    class WirePropertyChangedInfo
    {
        // VMの型ごとにインスタンスをキャッシュ
        static Dictionary<Type, WirePropertyChangedInfo> WirePropertyChangedCache { get; } = new Dictionary<Type, WirePropertyChangedInfo>();

        /// <summary>
        /// 指定した型に対する依存関係を取得
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static WirePropertyChangedInfo GetCacheFor(Type type)
        {
            if (!WirePropertyChangedInfo.WirePropertyChangedCache.ContainsKey(type))
            {
                var cache = new WirePropertyChangedInfo();
                cache.MakeCacheFor(type);
                WirePropertyChangedCache[type] = cache;
            }
            return WirePropertyChangedCache[type];
        }

        /// <summary>
        /// 依存関係の情報
        /// </summary>
        Dictionary<string, DependencyForProperty> DependencyInfo { get; } = new Dictionary<string, DependencyForProperty>();

        void MakeCacheFor(Type type)
        {
            // DependsOnの処理
            foreach (var prop in type.GetProperties())
            {
                foreach (var attr in prop.GetCustomAttributes(typeof(DependsOnAttribute), true).OfType<DependsOnAttribute>())
                {
                    // 同階層のプロパティ間の依存関係を保存
                    if (!DependencyInfo.ContainsKey(attr.PropertyName))
                    {
                        DependencyInfo[attr.PropertyName] = new DependencyForProperty();
                    }
                    var dep = DependencyInfo[attr.PropertyName];
                    // プロパティ[attr.PropertyName] 変更時、プロパティ[prop.Name] も変わる
                    dep.Dependencies.Add(prop.Name);

                    // 入れ子になったプロパティとの依存関係を保存
                    if (!string.IsNullOrEmpty(attr.NestedPropertyName))
                    {
                        var nestedDeps = dep.NestedDependencies;
                        if (!nestedDeps.ContainsKey(attr.NestedPropertyName))
                        {
                            nestedDeps[attr.NestedPropertyName] = new DependencyForProperty();
                        }
                        // プロパティ[attr.PropertyName].[attr.NestedPropertyName] 変更時、プロパティ[prop.Name]も変わる
                        nestedDeps[attr.NestedPropertyName].Dependencies.Add(prop.Name);
                    }
                }
            }

            // CanExecuteDependsOnの処理
            foreach (var prop in type.GetProperties())
            {
                foreach (var attr in prop.GetCustomAttributes(typeof(CanExecuteDependsOnAttribute), true).OfType<CanExecuteDependsOnAttribute>())
                {
                    if (!prop.PropertyType.IsAssignableFrom(typeof(RelayCommand)))
                    {
                        // 型がおかしい
                        throw new ArgumentException("CanExecuteDependsOn must be on RelayCommand property");
                    }

                    // 同階層のプロパティ間の依存関係を保存
                    if (!DependencyInfo.ContainsKey(attr.PropertyName))
                    {
                        DependencyInfo[attr.PropertyName] = new DependencyForProperty();
                    }
                    var dep = DependencyInfo[attr.PropertyName];
                    // プロパティ[attr.PropertyName] 変更時、コマンド[prop.Name] のCanExecuteも変わる
                    dep.CanExeDependencies.Add(prop.Name);

                    // 入れ子になったプロパティとの依存関係を保存
                    if (!string.IsNullOrEmpty(attr.NestedPropertyName))
                    {
                        var nestedDeps = dep.NestedDependencies;
                        if (!nestedDeps.ContainsKey(attr.NestedPropertyName))
                        {
                            nestedDeps[attr.NestedPropertyName] = new DependencyForProperty();
                        }
                        // プロパティ[attr.PropertyName].[attr.NestedPropertyName] 変更時、コマンド[prop.Name] のCanExecuteも変わる
                        nestedDeps[attr.NestedPropertyName].CanExeDependencies.Add(prop.Name);
                    }
                }
            }
        }

        /// <summary>
        /// 依存関係の存在しないクラスならtrue
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return DependencyInfo.Count == 0;
        }

        /// <summary>
        /// 依存関係に従ってプロパティ変更通知を発行する
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="propertyName"></param>
        /// <param name="nestedPropertyName"></param>
        public void ResolveDependency(ViewModelBase vm, string propertyName, string nestedPropertyName)
        {
            // プロパティ値が変わった場合、そのプロパティに依存している他のプロパティに対しても変更通知を発生させる
            if (DependencyInfo.ContainsKey(propertyName))
            {
                // プロパティ(propertyName)に連動して変わるプロパティの情報
                var dep = DependencyInfo[propertyName];

                foreach (var propName in dep.Dependencies)
                {
                    vm.RaisePropertyChanged(propName);
                }
                foreach (var propName in dep.CanExeDependencies)
                {
                    var command = vm.GetType().GetProperty(propName).GetValue(vm) as RelayCommand;
                    command.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// maybeNest配下のプロパティへの入れ子になった依存があるならtrue
        /// </summary>
        /// <param name="maybeNest"></param>
        /// <returns></returns>
        public bool IsNestedDependency(string maybeNest)
        {
            if(DependencyInfo.ContainsKey(maybeNest))
            {
                return DependencyInfo[maybeNest].NestedDependencies.Count > 0;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 入れ子になった依存関係を内部に持つプロパティ名のリスト
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetNestPropertyNames()
        {
            return DependencyInfo.Keys.Where(propName => IsNestedDependency(propName));
        }
    }

    /// <summary>
    /// 1オブジェクトに対して1つ存在する、INPCの通知を受けて処理するためのオブジェクト
    /// コンストラクタに渡したVMへイベントハンドラを登録するため、参照を残す必要はない
    /// </summary>
    class WirePropertyChangedListener
    {
        // Targetのクラス用の依存情報
        WirePropertyChangedInfo WireInfo { get; set; }
        // INPCで主に監視している対象オブジェクト
        ViewModelBase Target { get; set; }
        // Target配下のプロパティ値の中で、INPCを監視しているオブジェクト
        Dictionary<string, INotifyPropertyChanged> NestHolder { get; } = new Dictionary<string, INotifyPropertyChanged>();
        // 逆引き
        Dictionary<INotifyPropertyChanged, string> NestHolderBack { get; } = new Dictionary<INotifyPropertyChanged, string>();

        public WirePropertyChangedListener(ViewModelBase target)
        {
            Target = target;
            var type = target.GetType();
            WireInfo = WirePropertyChangedInfo.GetCacheFor(type);

            Target.PropertyChanged += Target_PropertyChanged;
            ReconnectNested();
        }

        // Target直下のプロパティが変更された場合のイベントハンドラ
        private void Target_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // 依存関係を解決してINPC発行
            WireInfo.ResolveDependency(Target, e.PropertyName, null);

            if(WireInfo.IsNestedDependency(e.PropertyName))
            {
                // 今差し変わったオブジェクト配下のプロパティへ依存があるなら、
                // 新しい方のオブジェクトのINPCを購読する
                ReconnectNested(e.PropertyName);
            }
        }

        // 依存関係が存在する入れ子オブジェクトを監視する
        void ReconnectNested()
        {
            foreach(var nest in WireInfo.GetNestPropertyNames())
            {
                ReconnectNested(nest);
            }
        }

        // [propName]プロパティに入っているオブジェクトを監視するようにする
        void ReconnectNested(string propName)
        {
            if(NestHolder.ContainsKey(propName))
            {
                // 古いオブジェクトへの監視を外す
                var old = NestHolder[propName];
                NestHolder[propName] = null;
                if(old != null)
                {
                    NestHolderBack.Remove(old);
                    PropertyChangedEventManager.RemoveHandler(old, Nested_PropertyChanged, string.Empty);
                }
            }
            // 新しいオブジェクトを監視
            var val = (INotifyPropertyChanged)(Target.GetType().GetProperty(propName).GetValue(Target));
            if(val != null)
            {
                NestHolder[propName] = val;
                NestHolderBack[val] = propName;
                // 子オブジェクトの寿命とは関係ないので弱参照にする
                PropertyChangedEventManager.AddHandler(val, Nested_PropertyChanged, string.Empty);
            }
        }

        // Targetに入れ子になったオブジェクト内のプロパティが変更された場合のイベントハンドラ
        private void Nested_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // どのプロパティのオブジェクトからの通知なのか判断
            var nestedPropName = NestHolderBack[sender as INotifyPropertyChanged];
            // 依存関係を解決してINPC発行
            WireInfo.ResolveDependency(Target, nestedPropName, e.PropertyName);
        }
    }

    /// <summary>
    /// MVVM LightのViewModel基底クラス拡張
    /// </summary>
    public static class ViewModelBaseExtension
    {
        /// <summary>
        /// プロパティに属性[DependsOn("XXX")]を指定すると、XXXが変更された際に連動してPropertyChangedイベントが発生します
        /// コマンドに属性[CanExecuteDependsOn("XXX")]を指定すると、XXXが変更された際に連動してCanExecuteChangedイベントが発生します
        /// </summary>
        /// <param name="vm"></param>
        public static void WirePropertyChanged(ViewModelBase vm)
        {
            if (vm == null) return;
            new WirePropertyChangedListener(vm);
        }

    }
}
